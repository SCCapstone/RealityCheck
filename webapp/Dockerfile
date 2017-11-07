FROM rowles/pylucene:v1

# copy all files to image
WORKDIR /app
COPY . /app

RUN pip install --user -r requirements.txt

ENTRYPOINT ["python", "app.py"]
