FROM mcr.microsoft.com/mssql/server:2019-latest

RUN mkdir -p /usr/src/app

WORKDIR /usr/src/app

COPY ./scripts /user/src/app/scripts

RUN chmod +x /usr/src/app/scripts/init-sqlserver.sh

CMD /bin/bash ./scripts/entrypoint.sh