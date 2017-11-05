
import lucene
from java.nio.file import Paths
from org.apache.lucene.analysis.standard import StandardAnalyzer
from org.apache.lucene.index import DirectoryReader, IndexOptions, IndexWriter, IndexWriterConfig
from org.apache.lucene.queryparser.classic import QueryParserBase, MultiFieldQueryParser
from org.apache.lucene.search import IndexSearcher
from org.apache.lucene.store import SimpleFSDirectory

from search import Timer, AssetFactory

#import generated.asset_pb2 as assetpb
import generated.search_pb2 as searchpb
from common import singleton_object, Singleton, Paginator


@singleton_object
class SearchService(metaclass=Singleton):

    MAX_RESULTS = 100000
    DEF_PAGE_SIZE = 100

    def open(self, dir_path: str) -> None:
        self.store = SimpleFSDirectory(Paths.get(dir_path))
        self.analyzer = StandardAnalyzer()
        self.searcher = IndexSearcher(DirectoryReader.open(self.store))
        self.parser = MultiFieldQueryParser(["name", "tags"], self.analyzer)
        self.parser.setDefaultOperator(QueryParserBase.OR_OPERATOR)
        self.timer = Timer()

    def search(self, search_request: searchpb.SearchRequest) -> searchpb.SearchResult:
        assert search_request is not None
        assert search_request.query is not None
        assert len(search_request.query) > 0

        query = MultiFieldQueryParser.parse(self.parser, search_request.query)

        self.timer.reset()
        hits = self.searcher.search(query, self.MAX_RESULTS)
        self.timer.stop()

        print("Found %d document(s) that matched query '%s':" % (hits.totalHits, query))

        res = searchpb.SearchResult()
        res.count = hits.totalHits
        res.time = round(self.timer.elapsed_ms())

        page_num = search_request.page_number or 1
        page_size = search_request.result_per_page or self.DEF_PAGE_SIZE

        # bounds for pagination
        bounds = Paginator.calculate(hits.totalHits, page_num, page_size)

        res.max_score = 0.0

        # iterate all hits within bounds
        for i in range(bounds[0] - 1, bounds[1]):
            hit = hits.scoreDocs[i]

            h = res.hits.add()
            h.score = hit.score

            #asset = AssetFactory.from_document(self.searcher.doc(hit.doc))
            h.asset.CopyFrom(AssetFactory.from_document(self.searcher.doc(hit.doc)))

            if res.max_score is 0.0 or h.score > res.max_score:
                res.max_score = h.score

        return res
