import datetime
import generated.asset_pb2 as assetpb
import lucene
from org.apache.lucene.document import Document, Field, FieldType
from org.apache.lucene.index import DirectoryReader, IndexOptions, IndexWriter, IndexWriterConfig


class DocumentFactory:
    """
    Lucene Document Factory
    """
    @staticmethod
    def from_dict(doc_dict: dict) -> Document:
        """
        Convert dictionary to Lucene Document
        """
        t_name = FieldType()
        t_name.setStored(True)
        t_name.setTokenized(True)
        t_name.setIndexOptions(IndexOptions.DOCS_AND_FREQS)

        t_file = FieldType()
        t_file.setStored(True)
        t_file.setTokenized(False)
        t_file.setIndexOptions(IndexOptions.DOCS_AND_FREQS)

        t_tag = FieldType()
        t_tag.setStored(True)
        t_tag.setTokenized(False)
        t_tag.setIndexOptions(IndexOptions.DOCS_AND_FREQS)

        doc = Document()

        name = Field("name", doc_dict["name"], t_name)
        fname = Field("filename", doc_dict["filename"], t_file)

        tags = []

        for t in doc_dict["tags"]:
            ft = Field("tags", t, t_tag)
            tags.append(ft)

        doc.add(name)
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