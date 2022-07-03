#docker run -d -p 21:21 -p 49152-50000:49152-50000 --name proftp local/proftp

FROM ubuntu:20.04
RUN apt-get update --assume-yes
RUN apt-get install --no-install-recommends --assume-yes proftpd-basic

COPY proftpd.conf /etc/proftpd/proftpd.conf
COPY tls.conf /etc/proftpd/tls.conf

COPY proftpd.crt /etc/ssl/certs/proftpd.crt
COPY proftpd.key /etc/ssl/private/proftpd.key

COPY entrypoint.sh ./entrypoint.sh
RUN chmod 755 ./entrypoint.sh

RUN mkdir /home/proftp
RUN chmod a+rw /home/proftp
RUN useradd --shell /bin/sh -d /home/proftp --password $6$zyqKWVqNaebv2.mJ$ZaOfbSBrks1ASLMrUBxczgO8yikEH3h64iDKJLEh2r8rWSJS7YG1FW08I.Zx7Eq0aSA1i6c8lhwOdwJxMsCta. proftp

EXPOSE 21 49152-50000

ENTRYPOINT ["./entrypoint.sh"]