#! /usr/bin/bash

KVDB_PATH=~/ms_kvdb

CONFIG_FILE=$KVDB_PATH/$1
BIN_PATH=$KVDB_PATH/kvdb/KVDB

# change permissions
chmod 777 $BIN_PATH

echo "Starting kvdb server on $BIN_PATH"

TIME=$(date +"%Y%m%d%H%M%S")
# get the second extension from Config file
EXTENSION=$(echo $CONFIG_FILE | cut -d'.' -f2)
# start kvdb server
nohup $BIN_PATH $CONFIG_FILE 2 > $KVDB_PATH/kvdb_$EXTENSION.log 2>&1 < /dev/null &