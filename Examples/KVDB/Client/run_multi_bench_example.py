#!/usr/bin/python3

from multibench import *

# Insturction:
# 1. Make a copy of this file and rename to run_multi_bench.py
# 2. Change SERVER_LIST to remote servers with RAC server
# 3. see jsons below to define a workload

SERVER_LIST = ["192.168.0.100", "192.168.0.101"]


if __name__ == "__main__":
    test = {
        "nodes_pre_server": 1,
        "use_server": 1,
        "client_multiplier": 30,

        "typecode": "rc",
        "total_objects": 100,

        "prep_ops_pre_obj": 10,
        "num_reverse": [0],
        "prep_ratio": [1, 0, 0],


        "ops_per_object": 1000,
        "op_ratio": [[0.15, 0.15, 0.7]],
        "target_throughput": 0
    }


    run_experiment(test, "num_reverse", "op_ratio", "test_result_file", SERVER_LIST, True)