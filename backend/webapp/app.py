import base64
import json
import logging

import lucene
import requests
from flask import Flask, request, send_file, jsonify, render_template, abort
from google.protobuf import json_format

import generated.search_pb2 as searchpb
from search import DocumentFactory
from search.indexer import IndexService
from search.searcher import SearchService

from flask_mongoengine import MongoEngine
import enum
import datetime

lucene.initVM()
app = Flask(__name__)
app.config.from_pyfile('config.py')

searcher = SearchService()
log = logging.getLogger("my-logger")

gcloud_key = 'xxx'
gspeech_uri = "https://speech.googleapis.com/v1/speech:recognize?key=%s" % gcloud_key

db = MongoEngine(app)


class ArchiveMetadata(db.EmbeddedDocument):
    bytes = db.IntField()
    files = db.ListField(default=[])
    name = db.StringField(max_length=255)
    primary_obj = db.StringField(required=False)
    primary_mtl = db.StringField(required=False)


class User(db.Document):
    email = db.StringField(max_length=300, required=True)
    password = db.StringField(max_length=300, required=True)
    timestamp = db.DateTimeField(default=datetime.datetime.now())

    def __init__(self, email):
        self.email = email


class JobStatus(enum.Enum):
    QUEUED = 'queued'
    PENDING = 'pending'
    SUCCESSFUL = 'successful'
    FAILURE = 'failure'


class CrawlJob(db.Document):
    source = db.StringField(max_length=300, required=True)
    uri = db.StringField(max_length=3000, required=True)
    status = db.StringField(max_length=20, required=True)
    timestamp = db.DateTimeField(default=datetime.datetime.now())


class CrawlAsset(db.Document):
    name = db.StringField(max_length=255)
    description = db.StringField()
    archive = db.EmbeddedDocumentField('ArchiveMetadata')
    filename = db.StringField(max_length=255)
    source = db.StringField(max_length=255)
    tags = db.ListField(db.StringField(), default=list)
    images = db.ListField(db.StringField(), default=list)
    downloads = db.IntField()
    views = db.IntField()
    uri = db.StringField(max_length=3000, required=True)
    timestamp = db.DateTimeField(default=datetime.datetime.now())


class Tag(db.EmbeddedDocument):
    value = db.StringField(max_length=255)


class Asset(db.Document):
    name = db.StringField(max_length=255)
    description = db.StringField(max_length=1000)
    filename = db.StringField(max_length=255)
    archive = db.EmbeddedDocumentField('ArchiveMetadata')
    tags = db.ListField(db.EmbeddedDocumentField(Tag), default=[])
    uri = db.StringField(max_length=3000, required=True)
    banner_uri = db.StringField(max_length=3000, required=False)
    allow_indexing = db.BooleanField(default=False)
    timestamp = db.DateTimeField(default=datetime.datetime.now())


from mongoengine import fields


def update_document(document, data_dict):
    def field_value(field, value):

        if field.__class__ in (fields.ListField, fields.SortedListField):
            return [
                field_value(field.field, item)
                for item in value
            ]
        if field.__class__ in (
                fields.EmbeddedDocumentField,
                fields.GenericEmbeddedDocumentField,
                fields.ReferenceField,
                fields.GenericReferenceField
        ):
            return field.document_type(**value)
        else:
            return value

    [setattr(
        document, key,
        field_value(document._fields[key], value)
    ) for key, value in data_dict.items()]

    return document


@app.route('/api/v1/search', methods=['POST'])
def search():
    if request.headers['Content-Type'] == 'application/x-protobuf':
        payload = request.stream.read()
        search_request = searchpb.SearchRequest()
        search_request.ParseFromString(payload)
        search_result = searcher.search(search_request)
        pb_out = search_result.SerializeToString()
        pb_b64 = base64.b64encode(pb_out)
        return pb_b64.decode('ascii')
    elif 'application/json' in request.headers['Content-Type']:
        search_request = json_format.Parse(request.stream.read(), searchpb.SearchRequest, ignore_unknown_fields=False)
        search_result = searcher.search(search_request)
        json_out = json_format.MessageToDict(search_result)
        return jsonify(json_out)


@app.route('/api/v1/files/<string:fname>')
def return_files_tut(fname: str):
    try:
        return send_file('models/%s' % fname,
                         attachment_filename=fname)
    except Exception as e:
        return str(e)


@app.route('/api/v1/transcribe', methods=['POST'])
def transcribe():
    wavb64 = request.stream.read()

    payload = {
        "config": {
            "encoding": "LINEAR16",
            "sampleRateHertz": 44100,
            "languageCode": "en-US"
        },
        "audio": {
            "content": wavb64.decode('ascii')
        }
    }

    r = requests.post(gspeech_uri, json=payload)

    return jsonify(r.json())


@app.route('/api/v2/reindex')
def reindex():
    import hashlib
    import time
    name = '%s.index-dir' % hashlib.md5(str(time.time()).encode())
    indexer = IndexService()
    indexer.open(name)

    def ii(d):
        log.error(d)
        return DocumentFactory.from_dict(d)

    docs = map(lambda d: ii(d), load_docs())
    indexer.add_documents(docs)
    indexer.commit()
    indexer.close()

    searcher = SearchService()
    searcher.open(name)
    return None


def pagination(res):
    page_nb = int(request.args.get('page', default=1))
    items_per_page = int(request.args.get('limit', default=100))

    offset = (page_nb - 1) * items_per_page

    count = res.objects.count()
    max_pages = res.objects.count() / items_per_page

    resp = {
        'items': res.objects.skip(offset).limit(items_per_page),
        'page': page_nb,
        'itemsPerPage': items_per_page,
        'totalCount': count,
        'totalPages': max_pages
    }

    return resp


@app.route('/api/v2/crawl/jobs/')
def all_crawl_jobs():
    return jsonify(pagination(CrawlJob))


@app.route('/api/v2/crawl/job/', methods=['POST'])
def new_crawl_job():
    content = request.json
    job = CrawlJob(**content)
    job.save()
    return jsonify(job)


@app.route('/api/v2/crawl/job/<string:uuid>', methods=['GET', 'PUT'])
def crawl_job(uuid):
    if request.method == 'GET':
        return jsonify(CrawlJob.objects.get(id=uuid))
    if request.method == 'PUT':
        content = request.json
        new_status = content['status']
        job = CrawlJob.objects.get(id=uuid)
        job.status = new_status
        job.save()
        return jsonify(job)


@app.route('/api/v2/crawl/job/uri', methods=['PUT'])
def crawl_uri_job():
    content = request.json

    res = CrawlJob.objects(uri=content['uri'])
    if len(res) > 0:
        return jsonify(res[0])
    abort(404)


@app.route('/api/v2/crawl/summary', methods=['GET'])
def crawl_summary():
    resp = {
        'jobs': {
            'total': CrawlJob.objects.count(),
            'queued': CrawlJob.objects.filter(status='queued').count(),
            'pending': CrawlJob.objects.filter(status='pending').count(),
            'successful': CrawlJob.objects.filter(status='successful').count(),
            'failure': CrawlJob.objects.filter(status='failure').count()
        },
        'crawlAssets': {
            'total': CrawlAsset.objects.count()
        },
        'assets': {
            'total': Asset.objects.count()
        }
    }

    return jsonify(resp)


@app.route('/api/v2/crawl/assets/')
def all_crawl_assets():
    return jsonify(pagination(CrawlAsset))


@app.route('/api/v2/crawl/asset/<string:uuid>', methods=['GET'])
def crawl_asset(uuid):
    return jsonify(CrawlAsset.objects.get(id=uuid))


@app.route('/api/v2/crawl/asset/', methods=['POST'])
def new_crawl_asset():
    content = request.json
    a = crawl_to_asset2(content) #CrawlAsset(**content)
    ##update_document(a, content)
    a.save()
    crawl_to_asset(content).save()
    return jsonify(a)


@app.route('/api/v2/assets/')
def all_assets():
    return jsonify(pagination(Asset))


@app.route('/api/v2/asset/<string:uuid>', methods=['GET', 'PUT'])
def asset(uuid):
    if request.method == 'GET':
        return jsonify(Asset.objects.get(id=uuid))
    if request.method == 'PUT':
        content = request.json
        try:
            del content['_id']
            del content['timestamp']
        except:
            pass
        Asset.objects(id=uuid).update(**content)
        return jsonify(Asset.objects.get(id=uuid))


@app.route('/api/v2/asset/<string:uuid>/indexing', methods=['PUT'])
def set_asset_indexing(uuid):
    content = request.json
    Asset.objects(id=uuid).update(allow_indexing=content['indexing'])
    return jsonify(Asset.objects.get(id=uuid))


@app.route('/api/v2/asset/<string:uuid>/primary', methods=['PUT'])
def set_asset_primary(uuid):
    content = request.json
    a = Asset.objects.get(id=uuid)
    a.archive.primary_obj = content['obj']
    a.archive.primary_mtl = content['mtl']
    a.save()
    return jsonify(Asset.objects.get(id=uuid))


@app.route('/')
def index():
    return render_template('index.html')


def crawl_to_asset(crawl_dict: dict):
    tags = list(map(lambda t: Tag(value=t), crawl_dict['tags']))
    a = Asset()
    a.name = crawl_dict['name']
    a.description = crawl_dict['description']
    a.filename = crawl_dict['filename']
    a.tags = tags
    archive = ArchiveMetadata(**crawl_dict['archive'])
    a.archive = archive
    a.uri = crawl_dict['uri']
    return a


def crawl_to_asset2(crawl_dict: dict):
    a = CrawlAsset()
    a.name = crawl_dict['name']
    a.views = crawl_dict['views']
    a.downloads = crawl_dict['downloads']
    a.source = crawl_dict['source']
    a.images = crawl_dict['images']
    a.description = crawl_dict['description']
    a.filename = crawl_dict['filename']
    a.tags = crawl_dict['tags']
    archive = ArchiveMetadata(**crawl_dict['archive'])
    a.archive = archive
    a.uri = crawl_dict['uri']
    return a


def load_docs() -> list:
    #log.error(Asset.objects.to_json())
    return json.loads(Asset.objects.filter(allow_indexing=True).to_json())
    d = []

    with open('docs.json') as docs_file:
        docs = json.load(docs_file)
        for doc in docs['models']:
            print(doc)
            d.append(doc)
    return d


if __name__ == '__main__':
    reindex()
    app.run(host='0.0.0.0', port=8001)
