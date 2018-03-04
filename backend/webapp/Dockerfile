FROM rowles/pylucene:v1

# Remove apt cache to make the image smaller
RUN rm -rf /var/lib/apt/lists/*


ADD requirements.txt /app/requirements.txt
RUN pip install --user -r /app/requirements.txt


# copy all files to image
WORKDIR /app
COPY . /app

ENTRYPOINT ["python", "app.py"]
