#!/usr/bin/python3

from multibench import *
from start_servers import *

# Insturction:
# 1. Make a copy of this file and rename to run_multi_bench.py
# 2. Change SERVER_LIST to remote servers with RAC server
# 3. see jsons below to define a workload

SERVER_LIST = ["127.0.0.1"]

if __name__ == "__main__":
    test = {
        "use_server": 1,
        "client_multiplier": [5],

        "typecode": "pnc",
        "total_objects": 100,

        "ops_per_object": [10000],
        "op_ratio": [0.15, 0.15, 0.7],
        "target_throughput": 0
    }


    run_experiment(test, "ops_per_object", "client_multiplier", "test_result_file", SERVER_LIST, True)



    clean_up_local()