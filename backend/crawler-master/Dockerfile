FROM selenium/standalone-chrome

MAINTAINER Andrew Rowles "andrew@rowles.io"
USER root

RUN apt-get update

RUN apt-get -qqy --no-install-recommends install \
  python3.5 \
  wget

RUN wget --no-check-certificate https://bootstrap.pypa.io/get-pip.py
RUN python3.5 get-pip.py

# Remove apt cache to make the image smaller
RUN rm -rf /var/lib/apt/lists/*

ADD requirements.txt /app/requirements.txt
RUN pip install --user -r /app/requirements.txt

WORKDIR /app
COPY . /app

RUN pip install --user -r requirements.txt

ENTRYPOINT ["sh", "start.sh"]