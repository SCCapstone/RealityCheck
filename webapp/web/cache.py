import generated.search_pb2 as searchpb
from common import singleton_object


@singleton_object
class CacheService:

    def open(self):
        pass

    def get_search_results(self, search_request: searchpb.SearchRequest) -> searchpb.SearchResult:
        pass

    def put_search_result(self, search_request: searchpb.SearchRequest,
                          search_result: searchpb.SearchResult) -> None:
        pass


