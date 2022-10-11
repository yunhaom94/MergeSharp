#!/usr/bin/python3
import json
import os
from re import sub
import socket
import subprocess

KVDB_REMOTE_BIN_PATH = "~/ms_kvdb"
KVDB_SERVER_PORT = 8000
TARGET_BUILD_PLATFORM = "ubuntu.20.04-x64"

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
    for ip in servers_list:
        print("Sending KVDB server to " + ip + "...")
        # copy KVDB server binary
        subprocess.run(["scp", "-r", f"../bin/Release/net6.0/{TARGET_BUILD_PLATFORM}/", f"{ip}:{KVDB_REMOTE_BIN_PATH}"])

        # copy cluster_config.node_id.json
        subprocess.run(["scp", cluster_config_dict[ip], f"{ip}:{KVDB_REMOTE_BIN_PATH}"])

        # change access permission
        subprocess.run(["ssh", ip, "chmod", "777", f"{KVDB_REMOTE_BIN_PATH}/{TARGET_BUILD_PLATFORM}/KVDB"])

        # start server
        subprocess.run(["ssh", ip, f"{KVDB_REMOTE_BIN_PATH}/{TARGET_BUILD_PLATFORM}/KVDB", f"{KVDB_REMOTE_BIN_PATH}/{cluster_config_dict[ip]}"])

############################################### Clean up ###############################################

if __name__ == "__main__":
    servers_list = ["206.12.97.242"]

    print("initiating KVDB servers...")

    cluster_config_dict = generate_cluster_configs(servers_list)



    build()

    distribute(servers_list, cluster_config_dict)

