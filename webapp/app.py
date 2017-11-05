from flask import Flask, request, send_file
import generated.search_pb2 as searchpb
from search import DocumentFactory
from search.indexer import IndexService
from search.searcher import SearchService
import lucene
import json

lucene.initVM()
app = Flask(__name__)
searcher = SearchService()

import logging
log = logging.getLogger("my-logger")

@app.route('/api/v1/search', methods=['POST'])
def search():
    if request.headers['Content-Type'] == 'application/x-protobuf':
        payload = request.stream.read()
        search_request = searchpb.SearchRequest()
        search_request.ParseFromString(payload)
        search_result = searcher.search(search_request)
        pb_out = search_result.SerializeToString()
        return pb_out
    elif request.headers['Content-Type'] == 'application/json':
        return None


@app.route('/files/<string:fname>')
def return_files_tut(fname: str):
    try:
        return send_file('models/%s' % fname,
                         attachment_filename=fname)
    except Exception as e:
        return str(e)


def load_docs() -> list:
    d = []
    with open('docs.json') as docs_file:
        docs = json.load(docs_file)
        for doc in docs['models']:
            print(doc)
            d.append(doc)
    return d


if __name__=='__main__':
    indexer = IndexService()
    indexer.open("test.index-dir")
    docs = map(lambda d: DocumentFactory.from_dict(d), load_docs())
    indexer.add_documents(docs)
    indexer.commit()
    indexer.close()

    searcher.open("test.index-dir")

    app.run(host='0.0.0.0', port=8001)