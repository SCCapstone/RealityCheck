import logging
import time

from selenium import webdriver
from selenium.webdriver.chrome.options import Options
import os
import json
import requests
from kafka import KafkaProducer


logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# create a file handler
handler = logging.FileHandler('/app/master.log')
handler.setLevel(logging.INFO)

# create a logging format
formatter = logging.Formatter('%(asctime)s - %(name)s - %(levelname)s - %(message)s')
handler.setFormatter(formatter)

# add the handlers to the logger
logger.addHandler(handler)


def asset_uri(path):
    return '%s%s' % ('http://realitycheckservice:8001', path)


class MasterCrawler:
    PAGE_LIMIT = 101
    PAGE_SLEEP_SECS = 20

    def __init__(self, page_url='https://free3d.com/free-3d-models/obj?page=%s'):
        self.page_url = page_url
        self.producer = KafkaProducer(bootstrap_servers='kafka:9092')

    def has_linked(self, href: str) -> bool:
        r = requests.put(asset_uri('/api/v2/crawl/job/uri'),
                         json={
                             'uri': href
                         })

        return r.status_code == requests.codes.ok

    def get_models(self):
        """
        Scrap links to models from results page.
        Add links to Redis pubsub queue for slaves
        :return: List of links
        """
        elems = self.driver.find_elements_by_class_name("model-entry")
        links = []
        misses = 0

        for e in elems:
            l = e.find_element_by_tag_name("a").get_attribute("href")

            queued = False

            if not queued:
                if not self.has_linked(l):

                    # Store links in mongodb
                    r = requests.post(asset_uri('/api/v2/crawl/job/'), json={
                        'uri': l,
                        'status': 'queued',
                        'source': 'free3d'
                    })

                    payload = json.dumps(r.json()).encode('utf-8')
                    self.producer.send('dl', payload)

                    logger.info(l)
                    links.append(l)
                    queued = True

            if not queued:
                misses += 1

        logger.info("misses: %s" % misses)

        return links

    def load_page(self, page_number: int) -> None:
        """
        Load page with page num
        :param page_number:
        :return:
        """
        u = self.page_url % page_number
        logger.info(u)
        self.driver.get(u)

    def crawl_pages(self):
        """
        Crawl all pages
        :return: list of all links to 3D models
        """
        models = []
        models_set = set()

        for p in range(1, self.PAGE_LIMIT):
            self.driver = webdriver.Chrome()
            self.load_page(p)
            models += self.get_models()
            [models_set.add(m) for m in models]
            self.driver.delete_all_cookies()
            self.driver.close()
            time.sleep(self.PAGE_SLEEP_SECS)
            logger.info('Model count: %s, set: %s, page: %s' % (len(models), len(models_set), p))
        return models


if __name__ == '__main__':
    time.sleep(5)
    m = MasterCrawler()
    m.PAGE_LIMIT = 160
    m.crawl_pages()
    m.PAGE_LIMIT = 100
    m.page_url = 'https://free3d.com/3d-models/furniture/%s/obj'
