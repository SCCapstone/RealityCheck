import shutil

import lucene
from java.nio.file import Paths
from org.apache.lucene.analysis.standard import StandardAnalyzer
from org.apache.lucene.document import Document, Field, FieldType
from org.apache.lucene.index import DirectoryReader, IndexOptions, IndexWriter, IndexWriterConfig
from org.apache.lucene.store import SimpleFSDirectory

from common import singleton_object, Singleton


@singleton_object
class IndexService(metaclass=Singleton):
    def open(self, dir_path: str) -> None:
        try:
            shutil.rmtree(dir_path)
        except:
            pass

        self.store = SimpleFSDirectory(Paths.get(dir_path))
        self.analyzer = StandardAnalyzer()
        config = IndexWriterConfig(self.analyzer)
        config.setOpenMode(IndexWriterConfig.OpenMode.CREATE)
        self.writer = IndexWriter(self.store, config)

    def add_documents(self, docs):
        for d in docs:
            self.writer.addDocument(d)

    def __len__(self):
        return self.writer.numDocs()

    def commit(self):
        self.writer.commit()

    def close(self):
        self.writer.close()