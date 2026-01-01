#!/bin/sh
set -e

cp /redis/redis.conf.template /redis/redis.conf
cp /redis/sentinel.conf.template /redis/sentinel.conf

sed -i -e "s/\$REDIS_HOST/$REDIS_HOST/g" /redis/redis.conf
sed -i -e "s/\$REDIS_PORT/$REDIS_PORT/g" /redis/redis.conf

sed -i -e "s/\$SENTINEL_HOST/$SENTINEL_HOST/g" /redis/sentinel.conf
sed -i -e "s/\$SENTINEL_PORT/$SENTINEL_PORT/g" /redis/sentinel.conf

sed -i -e "s/\$SENTINEL_MASTER_NAME/$SENTINEL_MASTER_NAME/g" /redis/sentinel.conf
sed -i -e "s/\$SENTINEL_MASTER_HOST/$SENTINEL_MASTER_HOST/g" /redis/sentinel.conf
sed -i -e "s/\$SENTINEL_MASTER_PORT/$SENTINEL_MASTER_PORT/g" /redis/sentinel.conf

sed -i -e "s/\$SENTINEL_QUORUM/$SENTINEL_QUORUM/g" /redis/sentinel.conf
sed -i -e "s/\$SENTINEL_DOWN_AFTER/$SENTINEL_DOWN_AFTER/g" /redis/sentinel.conf
sed -i -e "s/\$SENTINEL_FAILOVER/$SENTINEL_FAILOVER/g" /redis/sentinel.conf

redis-server "$@"
