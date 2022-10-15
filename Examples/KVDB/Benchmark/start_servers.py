#!/usr/bin/python3
import datetime
import time
import json
import os
from re import sub
import socket
import subprocess

KVDB_REMOTE_BIN_PATH = "~/ms_kvdb"
KVDB_SERVER_PORT = 8000
TARGET_BUILD_PLATFORM = "ubuntu.22.04-x64"

############################################### Config File Generation ###############################################
# generate cluster_config for each server
def each_server_cfg(node_id: int, servers_list: list):
    cfg = []

    for i in range(len(servers_list)):   
        ip = servers_list[i]
        is_self = False
        if (i == node_id):
            is_self = True

        n = {
            "nodeid": i,
            "address": ip,
            "port": KVDB_SERVER_PORT,
            "isSelf": is_self
        }
        cfg.append(n)

    return json.dumps(cfg)


# generate cluster_config.node_id.json for all servers
# and returns a dict of node_id -> cluster_config.node_id.json
def generate_cluster_configs(servers_list: list) -> dict:
    res = {}
    
    for i in range(len(servers_list)):

        # an ip cannot appear more then once in the servers_list
        assert servers_list.count(servers_list[i]) == 1

        cfg_json = each_server_cfg(i, servers_list)

        filename = "cluster_config." + str(i) + ".json"
        # write cfg_json to file
        with open(filename, "w") as f:
            f.write(cfg_json)

        res[servers_list[i]] = filename

    return res

############################################### Build and Distribute Server Binaries ###############################################

# build
def build():
    # change cwd to one level up
    os.chdir("..")
    
    print("Building KVDB server...")
    subprocess.run(["dotnet", "clean", "--configuration", "Release", "--runtime", TARGET_BUILD_PLATFORM])
    subprocess.run(["dotnet", "build", "--configuration", "Release", "--self-contained", "--runtime", TARGET_BUILD_PLATFORM])

    # change cwd back
    os.chdir("Benchmark")

# distribute server binaries to all servers in servers_list
def distribute(servers_list: list, cluster_config_dict: dict):

    # package the server binaries with tar
    print("Packaging KVDB server...")
    subprocess.run(["tar", "-czvf", "kvdb.tar.gz", "-C", f"../bin/Release/net6.0/{TARGET_BUILD_PLATFORM}/", "."], stdout=subprocess.DEVNULL)

    for ip in servers_list:
        print("Sending KVDB server to " + ip + "...")
        # copy KVDB server binary
        subprocess.run(["scp", "kvdb.tar.gz", f"{ip}:{KVDB_REMOTE_BIN_PATH}"], stdout=subprocess.DEVNULL)

        # copy cluster_config.node_id.json
        subprocess.run(["scp", cluster_config_dict[ip], f"{ip}:{KVDB_REMOTE_BIN_PATH}"], stdout=subprocess.DEVNULL)

        # copy start_server_host.sh
        subprocess.run(["scp", "start_server_host.sh", f"{ip}:{KVDB_REMOTE_BIN_PATH}"], stdout=subprocess.DEVNULL) 

        # make a new directory
        subprocess.run([f"ssh {ip} \"mkdir -p {KVDB_REMOTE_BIN_PATH}/kvdb\""], shell=True)

        # extract the server binaries
        subprocess.run([f"ssh {ip} \"tar -xzvf {KVDB_REMOTE_BIN_PATH}/kvdb.tar.gz -C {KVDB_REMOTE_BIN_PATH}/kvdb\""], shell=True, stdout=subprocess.DEVNULL)

        # change access permission
        subprocess.run([f"ssh {ip} \"chmod +x {KVDB_REMOTE_BIN_PATH}/start_server_host.sh\""], shell=True)
        

# run the KVDB server on the remote server, and redirect the output to a log file, and run it in the background
def start_server(ip, cluster_config_dict):
        print("Starting KVDB server on " + ip + "...")
        # start server

        subprocess.run([f"ssh {ip} \"{KVDB_REMOTE_BIN_PATH}/start_server_host.sh {cluster_config_dict[ip]}\""], shell=True)

############################################### Clean up ###############################################
def stop_servers(ip):
    print("Stopping KVDB server on " + ip + "...")
    subprocess.run([f"ssh {ip} \"killall -9 KVDB\""], shell=True)

def copy_log_files(ip, exp_name = "default"):
    print("Copying log files from " + ip + "...")
    # create a directory for the log files (if not exist)
    subprocess.run(["mkdir", "-p", f"logs_{exp_name}"])
    # copy log files with format *.log
    subprocess.run([f"scp {ip}:{KVDB_REMOTE_BIN_PATH}/*.log logs_{exp_name}/"], shell=True)


def clean_up(ip):
    print("Cleaning up KVDB server on " + ip + "...")
    subprocess.run([f"ssh {ip} \"rm -rf {KVDB_REMOTE_BIN_PATH}/*\""], shell=True)

def clean_up_local():
    print("Cleaning up local files...")
    subprocess.run(["rm", "-rf", "kvdb.tar.gz"], stdout=subprocess.DEVNULL)
    subprocess.run(["rm cluster_config.*.json"], shell=True)
    


