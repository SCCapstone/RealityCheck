from flask import Flask, request
#import generated.search_pb2 as searchpb
from search.indexer import IndexService
from search.searcher import SearchService
import lucene

lucene.initVM()
app = Flask(__name__)
searcher = SearchService()


@app.route("/search", methods=['PUT'])
def search():
    if request.headers['Content-Type'] == 'application/x-protobuf':
        pass#search_request = searchpb.SearchRequest.ParseFromString(request.stream.read())
        #search_result = searcher.search(search_request)
        #return search_result.SerializeToString()
    elif request.headers['Content-Type'] == 'application/json':
        pass


if __name__=='__main__':
    indexer = IndexService()
    indexer.open("test.index-dir")
    indexer.add_documents()
    indexer.commit()
    indexer.close()

    searcher.open("test.index-dir")

    app.run(host='0.0.0.0', port=8001)