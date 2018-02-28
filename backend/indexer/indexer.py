import logging
import time

import redis
from pymongo import MongoClient
import os
import uuid
from gridfs import GridFS
import zipfile
from google.cloud import storage
import glob
import magic
import libarchive.public
import pathlib
import shutil
import hashlib

mime = magic.Magic(mime=True)


SLAVE_ID = uuid.uuid4().hex

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# create a file handler
handler = logging.FileHandler('slave.log')
handler.setLevel(logging.INFO)

# create a logging format
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)

# add the handlers to the logger
logger.addHandler(handler)

MONGO_DB = 'realitycheck'

'''
MongoDB: realitycheck

Collections:
 - links: sort all links posted to crawl
 - crawled_assets: model assets crawled
 - assets: 

'''

import six

def _get_storage_client():
    return storage.Client\
        .from_service_account_json('RealityCheck-USC-9e170ac8f892.json',
                                   project='RealityCheck-USC')


def upload_file(file_stream, content_type, filename):
    """
    Uploads a file to a given Cloud Storage bucket and returns the public url
    to the new object.
    """
    client = _get_storage_client()
    bucket = client.bucket('realitycheck')
    blob = bucket.blob(filename)

    blob.upload_from_string(
        file_stream, content_type=content_type)

    url = blob.public_url

    if isinstance(url, six.binary_type):
        url = url.decode('utf-8')

    return url


def upload(filename, dest_filename):
    f = open(filename, 'rb')
    content = f.read()
    f.close()
    content_type = mime.from_file(filename)
    upload_file(content, content_type, dest_filename)


def upload_folder(dir_path):
    file_list = []
    for filename in glob.iglob('%s/**/*' % dir_path, recursive=True):
        if os.path.isfile(filename):
            file_list.append(filename)
            upload(filename, filename)


def files_in_dir(dir_path):
    file_list = []
    for filename in glob.iglob('%s/**/*' % dir_path, recursive=True):
        if os.path.isfile(filename):
            file_list.append(filename)
    return file_list


def filter_files(files):
    blacklist = {'MACOSX', 'DS_STORE'}

    def check_blacklist(file):
        for b in blacklist:
            if b in file:
                return False
        return True

    return list(filter(lambda f: check_blacklist(f), files))


def get_extension(filename):
    return pathlib.Path(asset['filename']).suffix


def _extract(filename, output_dir=os.getcwd()):
    """

    extension = get_extension(filename)
    print('ext ', extension)
    print('output ', output_dir)

    if '7z' in extension:
        os.system('7z x %s -o %s' % (filename, output_dir))
    if 'zip' in extension:
        os.system('unzip %s -d %s' % (filename, output_dir))
    if 'tar' in extension:
        os.system('tar -xvzf %s' % filename)
    if 'rar' in extension:
        os.system('unrar x -r %s' % filename)

    :param filename:
    :param output_dir:
    :return:
    """
    f = open(filename, 'rb')
    for eee in libarchive.public.memory_pour(f.read()):
        pass
    f.close()
    return


def extract_archive(dir, filename):
    cwd = os.getcwd()
    os.chdir(dir)
    dpaths, dirs, files = os.walk(os.getcwd()).__next__()
    file_set = set(dirs)
    print('before ', dirs)

    _extract(filename,  os.getcwd())

    for i in range(0, 100):
        ddpaths, ddirs, dfiles = os.walk(os.getcwd()).__next__()
        diff = [x for x in ddirs if x not in file_set]
        if len(diff) > 1:
            break
        time.sleep(0.01)

    assert len(diff) == 1
    os.chdir(cwd)
    return diff[0]


def rename_dir(old_path, new_path):
    shutil.move(old_path, new_path)


def _hash(s):
    h = hashlib.md5(bytes(s.encode()))
    return h.hexdigest()


def zipdir(path, zip_name):
    zipf = zipfile.ZipFile(zip_name, 'w', zipfile.ZIP_DEFLATED)

    for root, dirs, files in os.walk(path):
        for file in files:
            zipf.write(os.path.join(root, file))

    zipf.close()


class Indexer:
    SLEEP_SECS = 1

    def __init__(self):
        return
        self.redis_db = redis.Redis(host='localhost', port=6379, db=0, charset="utf-8", decode_responses=True)
        self.pubsub = self.redis_db.pubsub()
        self.pubsub.subscribe("index")

        self.mongo_client = MongoClient('localhost', 27017)
        self.db = self.mongo_client[MONGO_DB]
        self.fs = GridFS(self.db)

    def download_file(self, filename):
        for grid_out in self.fs.find({"filename": filename},
                                     no_cursor_timeout=True):
            data = grid_out.read()
            zfn = 'dat/%s' % filename
            with open(zfn, 'wb') as f:
                f.write(data)

    def index(self, asset: dict):
        archive_name = asset['filename']
        logger.info('Finding file %s', archive_name)

        dir_name = extract_archive('dat/', asset['filename'])
        new_dir_name = _hash(dir_name)
        rename_dir('dat/%s' % dir_name, 'dat/%s' % new_dir_name)

        zip_name = new_dir_name + '.zip'
        zipdir('dat/%s' % new_dir_name, 'dat/%s' % zip_name)

        print()
        return
        with libarchive.public.file_reader(bytes(mmm, "ascii")) as reader:
            pass
        print('done')
        #mmm = './%s' % asset['filename']

        z = zipfile.ZipFile('dat/%s' % archive_name)
        self.extract(asset, z)
        #z.extractall('dat/')
        #zfiles = filter_files(z.namelist())
        #logger.info(zfiles)

    def extract(self, asset, zf):

        base_dir = zf.namelist()[0]
        # zf.extractall('dat/')
        import shutil
        import pathlib
        import glob
        import magic
        mime = magic.Magic(mime=True)

        name = pathlib.Path(asset['filename']).stem
        # shutil.move('dat/%s' % base_dir, 'dat/%s' % name)
        for filename in glob.iglob('dat/%s/**/*' % name, recursive=True):
            if os.path.isfile(filename):
                print(filename)
                ff = open(filename, 'rb')
                content = ff.read()
                ff.close()
                mime.from_file(filename)
                upload_file(content, mime.from_file(filename), filename)




        print('dat/%s' % asset['filename'])


    def run(self):
        """
        Listen on Redis queue for index
        :return: None
        """
        import os
        import json

        #os.mkdir('dat')

        for msg in self.pubsub.listen():
            logger.info(f'Received message: {{msg}}')

            try:
                data = msg['data']
                logger.info(f'Message data: %s', data)
                d = json.loads(data)
                self.index(d)
            except Exception as ex:
                template = "An exception of type {0} occurred. Arguments:\n{1!r}"
                message = template.format(type(ex).__name__, ex.args)
                logger.info('error %s' % message)
                logger.info('error %s' % ex)


if __name__ == '__main__':
    i = Indexer()
    asset = {'url': 'https://free3d.com/3d-model/bugatti-chiron-2017-model-31847.html', 'tags': ['bugatti', 'bugati', 'car', 'sports', 'chiron', 'vehicles'], 'downloads': '30,892', 'desc': "This is a bugatti chiron 3d model. Modelled in blender.The current file is in .blend format obj also included .The materials are available in the blender file.\nWatch how it was made timelapse here: https://www.youtube.com/watch?v=kmxwvqrwiKY\n\n[Update]\nYou can use this model however you want /modify it /etc\nYou don't have to give credit\nI am glad you all like it :)\nPrice: $0.00\nDate added: Aug 27, 2017", 'images': ['https://free3d.com/imgd/l95485-bugatti-chiron-2017-model-31847.jpg', 'https://free3d.com/imgd/l78666-bugatti-chiron-2017-model-31847.jpg', 'https://free3d.com/imgd/l740-bugatti-chiron-2017-model-31847.jpg', 'https://free3d.com/imgd/l16940-bugatti-chiron-2017-model-31847.jpg', 'https://free3d.com/imgd/l88796-bugatti-chiron-2017-model-31847.jpg'], 'source': 'free3d', 'views': '76,853', 'title': 'Bugatti Chiron 2017 sports car 3d model', 'filename': '147e60fbd91609b09c456924a6e26eae.zip'}
    i.index(asset)
    #i.run()
