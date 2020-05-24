FROM postgres:latest
ADD initdb/*.sql /docker-entrypoint-initdb.d/
RUN mkdir /home/pg-backups
