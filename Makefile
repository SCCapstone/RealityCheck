SRC_DIR=protobuf/
PY_DEST_DIR=backend/webapp/generated/


all: clean python
python: pb_python_dir pb_python_asset

clean:
	@echo "Removing generated sources"
	rm -rf $(PY_DEST_DIR)

pb_python_dir:
	@echo "Creating Python generated sources directory"
	mkdir $(PY_DEST_DIR)
	touch $(PY_DEST_DIR)__init__.py

pb_python_asset: pb_python_dir
	protoc -I=$(SRC_DIR) --python_out=$(PY_DEST_DIR) $(SRC_DIR)asset.proto $(SRC_DIR)search.proto

pb_python_search: pb_python_dir pb_python_asset
	protoc -I=$(SRC_DIR) --python_out=$(PY_DEST_DIR) $(SRC_DIR)search.proto

run: clean python
	docker-compose up --build
