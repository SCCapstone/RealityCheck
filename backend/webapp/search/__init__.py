import datetime
import generated.asset_pb2 as assetpb
import lucene
from org.apache.lucene.document import Document, Field, FieldType
from org.apache.lucene.index import DirectoryReader, IndexOptions, IndexWriter, IndexWriterConfig
import logging

log = logging.getLogger("my-logger")


def _field_type(stored: bool=True, tokenized: bool=False) -> FieldType:
    ft = FieldType()
    ft.setStored(stored)
    ft.setTokenized(tokenized)
    ft.setIndexOptions(IndexOptions.DOCS_AND_FREQS)
    return ft

class DocumentFactory:

    MAPPING = {
        'name': {
            'type': 'string',
            'tokenized': True,
            'boost': 1.1
        },
        'description': {
            'type': 'string',
            'tokenized': True,
            'boost': 1.0
        },
        'uuid': {
            'type': 'string'
        },
        'bannerUri': {
            'type': 'string'
        },
        'file': {
            'type': 'string'
        },
        'tags': {
            'type': 'list[string]'
        },
        'archive': {
            'type': 'document',
            'fields': {
                'name': {
                    'type': 'string'
                },
                'kilobytes': {
                    'type': 'int'
                },
                'files': {
                    'type': 'list[string]'
                }
            }
        }

    }

    @staticmethod
    def from_dict2(doc_dict: dict) -> Document:
        pass



    """
    Lucene Document Factory
    """
    @staticmethod
    def from_dict(doc_dict: dict) -> Document:
        """
        Convert dictionary to Lucene Document
        """
        t_name = _field_type(tokenized=True)
        t_desc = _field_type(tokenized=True)
        t_tag = _field_type(tokenized=True)
        t_uuid = _field_type()
        t_banner = _field_type()
        t_file = _field_type()
        t_archive_name = _field_type()
        t_archive_kilobytes = _field_type()
        t_archive_file = _field_type()

        doc = Document()

        name = Field("name", doc_dict["name"], t_name)
        uuid = Field("uuid", doc_dict["_id"]["$oid"], t_uuid)
        if 'bannerUri' in doc_dict:
            banner_uri = Field("banner_uri", doc_dict["bannerUri"], t_banner)
            doc.add(banner_uri)
        desc = Field("description", doc_dict["description"], t_desc)
        fname = Field("filename", doc_dict["filename"], t_file)

        tags = []

        for t in doc_dict["tags"]:
            ft = Field("tags", t['value'], t_tag)
            tags.append(ft)

        #for f in doc_dict["archive"]['_id']

        doc.add(name)
        doc.add(uuid)
        doc.add(desc)
        doc.add(fname)
        [doc.add(f) for f in tags]

        return doc


class AssetFactory:
    @staticmethod
    def from_document(doc: Document) -> assetpb.Asset:
        """
        Convert Lucene Document to Protobuf
        """
        asset = assetpb.Asset()

        asset.name = doc.get("name")
        asset.filename = doc.get("filename")
        asset.description = doc.get("description")
        try:
            asset.banner_uri = doc.get("banner_uri")
        except:
            pass
        asset.uuid = doc.get("uuid")

        tags = [t for t in doc.getValues("tags")]

        for t in tags:
            tpb = asset.tags.add()
            tpb.value = t

        return asset


class Timer:
    def __init__(self):
        self._running = False
        self._start = None
        self._end = None

    def reset(self):
        self._running = True
        self._start = datetime.datetime.now()

    def start(self):
        assert self._running is False
        self._running = True
        self._start = datetime.datetime.now()

    def stop(self):
        assert self._running is True
        self._end = datetime.datetime.now()
        self._running = False

    def delta(self) -> datetime:
        if self._running:
            return (datetime.datetime.now() - self._start)
        else:
            return (self._end - self._start)

    def elapsed_ms(self) -> float:
        if self._running:
            diff = datetime.datetime.now() - self._start
        else:
            diff = self._end - self._start
        ms = (diff.days * 86400000) + (diff.seconds * 1000) + (diff.microseconds / 1000)
        return ms