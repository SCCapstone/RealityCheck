import glob
import hashlib
import json
import logging
import os
import pathlib
import random
import shutil
import sys
import time
import uuid
import zipfile

import libarchive.public
import magic
import redis
import requests
import six
from google.cloud import storage
from kafka import KafkaConsumer
from selenium import webdriver

import consul

mime = magic.Magic(mime=True)

SLAVE_ID = uuid.uuid4().hex

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# create a file handler
handler = logging.FileHandler('/app/slave.log')
handler.setLevel(logging.INFO)

# create a logging format
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)

# add the handlers to the logger
logger.addHandler(handler)

consul_service = consul.Consul(host='consul')


def _get_storage_client():
    index, data = consul_service.kv.get('gbucket')
    f = open("/app/RealityCheck-USC-9e170ac8f892.json", "w+")
    f.write(data)
    f.close()

    return storage.Client \
        .from_service_account_json('/app/RealityCheck-USC-9e170ac8f892.json',
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

    blob.make_public()

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
    return pathlib.Path(filename).suffix


def _extract(filename, output_dir=os.getcwd()):
    f = open(filename, 'rb')
    for e in libarchive.public.memory_pour(f.read()):
        pass
    f.close()
    return


def extract_archive(dir, filename):
    cwd = os.getcwd()
    os.chdir(dir)
    dpaths, dirs, files = os.walk(os.getcwd()).__next__()
    file_set = set(dirs)

    _extract(filename, os.getcwd())

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


def empty_dir(path):
    for root, dirs, files in os.walk(path):
        for f in files:
            os.unlink(os.path.join(root, f))
        for d in dirs:
            shutil.rmtree(os.path.join(root, d))


def asset_uri(path):
    return '%s%s' % ('http://realitycheckservice:8001', path)


def _rand_sleep(min_seconds, max_seconds):
    time.sleep(random.randint(min_seconds, max_seconds))


class SlaveCrawler:
    SLEEP_SECS = 5
    dldir = "/data/dl/%s" % SLAVE_ID

    def __init__(self):
        # redis
        self.redis_db = redis.StrictRedis(host='redis', port=6379, db=0, charset="utf-8")

        self.consumer = KafkaConsumer('dl', bootstrap_servers=['kafka:9092'],
                                      value_deserializer=lambda m: json.loads(m.decode('utf-8')),
                                      group_id='crawler-%s' % SLAVE_ID,
                                      max_poll_records=500)

        # create download folder
        os.mkdir(self.dldir)

    def has_crawled(self, job):
        r = requests.get(asset_uri('/api/v2/crawl/job/%s' % job['_id']['$oid']))
        resp = r.json()
        if resp['status'] != 'queued':
            raise Exception('Already queued %s' % job)
        return False

    def set_crawling(self, job):
        r = requests.put(asset_uri('/api/v2/crawl/job/%s' % job['_id']['$oid']),
                         json={
                             'status': 'pending'
                         })

    def set_crawled_successful(self, job):
        r = requests.put(asset_uri('/api/v2/crawl/job/%s' % job['_id']['$oid']),
                         json={
                             'status': 'successful'
                         })

    def set_crawled_failure(self, job):
        r = requests.put(asset_uri('/api/v2/crawl/job/%s' % job['_id']['$oid']),
                         json={
                             'status': 'failure'
                         })

    def download_model(self, url):
        """
        Scrap metadata from web page and download model.
        Save metadata in MongoDB and file in GridFS within MongoDB

        :param url: url of page
        :return: metadata
        """
        self.driver.get(url)

        title = self.driver.find_element_by_class_name('title-bar').find_element_by_tag_name('h1').text
        logger.info('title: %s' % title)

        # get tags
        elem_tags = self.driver.find_elements_by_class_name('indiv-tag')
        tags = [t.text for t in elem_tags]
        logger.info('tags: %s' % tags)

        # get download button
        dl_btn = self.driver.find_element_by_id("download-prod")

        # description
        desc = self.driver.find_element_by_class_name('desc').text
        logger.info('desc: %s' % desc)

        views = self.driver.find_element_by_class_name('p-stat-left').find_element_by_class_name('num').text
        logger.info('views: %s' % views)

        downloads = self.driver.find_element_by_class_name('p-stat-right').find_element_by_class_name('num').text
        logger.info('downloads: %s' % downloads)

        img_elems = self.driver.find_elements_by_class_name('rsImg')

        img_links = [i.get_attribute('src') for i in img_elems]
        logger.info('images: %s' % img_links)

        # get download folder before starting download!
        paths, dirs, files = os.walk(self.dldir).__next__()
        file_set = set(files)

        dl_btn.click()
        _rand_sleep(4, 14)
        dl_link = self.driver.find_element_by_class_name('file')
        dl_link.click()

        zip_size_kb, zip_name, files = self.save_download(file_set)

        views = int(views.replace(',', ''))
        downloads = int(downloads.replace(',', ''))

        model = {
            'uri': url,
            'name': title,
            'description': desc,
            'tags': tags,
            'views': views,
            'downloads': downloads,
            'filename': zip_name,
            'archive': {
                'files': files,
                'name': zip_name,
                'bytes': zip_size_kb
            },
            'images': img_links,
            'source': 'free3d'
        }

        logger.info('model: %s' % model)

        r = requests.post(asset_uri('/api/v2/crawl/asset/'), json=model)
        logger.info('status code: %s' % r.status_code)
        return model
        # if r.status_code != requests.codes.ok or r.status_code != requests.codes.created:
        #     raise Exception('Failed to add asset!')

    def _download_chrome(self, file_set):
        fname = None

        # incrementally wait for download
        for i in range(300):
            dpaths, ddirs, dfiles = os.walk(self.dldir).__next__()
            diff = [x for x in dfiles if x not in file_set]

            if len(diff) == 1 and 'crdownload' not in diff[0]:
                fname = diff[0]
                break
            time.sleep(0.1)

        if fname is None:
            return None

        logger.info('Downloaded %s' % fname)

        return fname

    def save_download(self, file_set):
        """
        Download file
        :param file_set: Set of files in download folder before starting download
        :return: File name of downloaded file
        """
        fname = self._download_chrome(file_set)
        logger.info('Chrome downloaded %s' % fname)

        # extract the archive file
        dir_name = extract_archive(self.dldir, fname)

        # rename directory as a hash
        new_dir_name = _hash(dir_name)
        rename_dir('%s/%s' % (self.dldir, dir_name), '%s/%s' % (self.dldir, new_dir_name))

        # compress as a zip
        zipdir('%s/%s' % (self.dldir, new_dir_name), '%s/%s.zip' % (self.dldir, new_dir_name))

        # update zip and folder to bucket
        old_cwd = os.getcwd()
        os.chdir(self.dldir)

        zipfilename = '%s.zip' % new_dir_name
        files = files_in_dir('%s' % new_dir_name)

        zip_size_kb = os.path.getsize('%s.zip' % new_dir_name)

        upload('%s.zip' % new_dir_name, '%s.zip' % new_dir_name)
        upload_folder('%s' % new_dir_name)
        os.chdir(old_cwd)

        # empty downloads directory
        empty_dir(self.dldir)

        return zip_size_kb, zipfilename, files

    def crawl(self) -> None:
        """
        Listen on Redis queue for job
        :return: None
        """
        for msg in self.consumer:
            _rand_sleep(2, 10)
            
            if not self.redis_db.exists(msg.value['uri']):
                logger.error('crawling %s' % msg.value)
                self.redis_db.set(msg.value['uri'], SLAVE_ID)
            else:
                logger.error('dupe %s' % msg.value)
                continue

            chromeOptions = webdriver.ChromeOptions()
            prefs = {"download.default_directory": self.dldir,
                     "download.prompt_for_download": False, }
            chromeOptions.add_experimental_option("prefs", prefs)
            chromeOptions.add_argument('--dns-prefetch-disable')
            chromeOptions.add_argument('--disable-gpu')
            self.driver = webdriver.Chrome(chrome_options=chromeOptions)

            try:
                job = msg.value
                logger.info('New job: %s' % job)
                self.has_crawled(job)
                self.set_crawling(job)
                model = self.download_model(job['uri'])
                
                self.set_crawled_successful(job)
                time.sleep(self.SLEEP_SECS)
            except Exception as ex:
                try:
                    if job is not None:
                        self.set_crawled_failure(job)
                except Exception as e:
                    pass
                logger.info('Error on line {}'.format(sys.exc_info()[-1].tb_lineno), type(ex).__name__, ex)

            try:
                self.driver.close()
            except Exception as e:
                logger.error('Error closing chrome ', e)


if __name__ == '__main__':
    time.sleep(10)
    c = SlaveCrawler()
    c.crawl()
