FROM redis:7.2-alpine3.19 AS base
RUN mkdir -p /redis
WORKDIR /redis

FROM base AS certs
RUN apk add --no-cache openssl
RUN openssl req -x509 -newkey rsa:2048 -sha256 -days 3650 -nodes \
  -keyout server.key -out server.crt -subj "/CN=localhost" \
  -addext "subjectAltName=DNS:localhost,IP:127.0.0.1" \
  && openssl req -x509 -newkey rsa:2048 -sha256 -days 3650 -nodes \
  -keyout client.key -out client.crt -subj "/CN=myclient" \
  && cat server.crt >> ca.crt && cat client.crt >> ca.crt

FROM base AS final
COPY --from=certs /redis/* ./
COPY entrypoint.sh /usr/local/bin/
RUN chown redis:redis /redis/* && \
    chmod +x /usr/local/bin/entrypoint.sh
ENV SENTINEL_MASTER_NAME mymaster
ENV SENTINEL_QUORUM 2
ENV SENTINEL_DOWN_AFTER 1000
ENV SENTINEL_FAILOVER 1000
ENTRYPOINT ["entrypoint.sh"]
CMD []
