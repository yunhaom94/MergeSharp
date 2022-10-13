#!/usr/bin/python3

import csv
import json
from benchmark import *
from startservers import *
import time
import traceback
import datetime 

# 1. prep json
# set & variables
# fix each variable, change others
# 2. start servres
# 3. run bench
# 4. collect data
# 5. stop servers



REDO = 0
BUILD_FLAG = False
run_name = "default"


def run_experiment(wokload_config: dict, prime_variable, secondary_variable, rfilename, server_list, local = False):

    start = datetime.datetime.now()
    print("Currrent time: " + str(start))
    print("Running: " + rfilename)    

    # y-axis
    primaries = wokload_config[prime_variable]

    # more bars
    secondaries = wokload_config[secondary_variable]

    json_dict = wokload_config.copy()

    tp_result = []
    mem_result = []
    latency_results = {}
    

    labels = [prime_variable]
    for s in secondaries:
        labels.append(str(s))

    total = len(primaries) * len(secondaries)
    count = 0

    # running benchmarks
    for p in primaries:

        p_result = {}
        p_result[prime_variable] = p

        pm_result = {}
        pm_result[prime_variable] = p


        for s in secondaries:
            
            redo = REDO
            while True:

                json_dict[prime_variable] = p

                json_dict[secondary_variable] = s

                wlfilename = str(p) + str(s) + ".json"

                if "nodes_pre_server" == primaries:
                    num_server = p
                elif "nodes_pre_server" == secondaries:
                    num_server = s
                else:
                    num_server = json_dict["nodes_pre_server"]

                global BUILD_FLAG

                if local:
                    i = 1
                    addresses = start_server(num_server)
                else:
                    addresses = start_server_remote(
                        num_server, server_list[0:wokload_config["use_server"]], BUILD_FLAG)

                #addresses = ["192.168.41.136:5000", "192.168.41.136:5001"]
                # only build once per run
                if BUILD_FLAG:
                    BUILD_FLAG = False

                json_dict["nodes"] = addresses

                with open(wlfilename, 'w') as json_file:
                    json.dump(json_dict, json_file)
                time.sleep(2)

                try:
                    r = run_benchmark(wlfilename)

                    redo = 0
                except Exception as e:
                    traceback.print_exc()
                    print("Error, redoing left " + str(redo))

                    if (redo <= 0):
                        print("Error, exiting")
                        parse_tpresult(tp_result, labels, rfilename + "_tp.csv")
                        parse_tpresult(mem_result, labels, rfilename + "_mem.csv")
                        parse_latencyresults(latency_results, rfilename + "_lt.txt")
                        exit()

                finally:
                    if local:
                        i = 2
                        stop_server()
                    else:
                        stop_server_remote(server_list[0:wokload_config["use_server"]])
                    
                    os.remove(wlfilename)

                    if (redo > 0):
                        redo -= 1
                        continue
                    



                p_result[str(s)] = r.tp
                pm_result[str(s)] = r.mem
                latency_results[str(p) + str(s)] = r.latency_result
                
                
                json_dict = wokload_config.copy()
                count += 1
                print(str(count) + "/" + str(total) + " done")
                end = datetime.datetime.now()
                print("Elapsed time:" + str(end - start))
                time.sleep(1)
                break

        tp_result.append(p_result)
        mem_result.append(pm_result)
    
    global run_name
    run_dir = 'results/' + run_name + '/'
    os.makedirs(run_dir, exist_ok=True)

    parse_tpresult(tp_result, labels, run_dir + rfilename + "_tp.csv")
    parse_tpresult(mem_result, labels, run_dir + rfilename + "_mem.csv")
    parse_latencyresults(latency_results, run_dir + rfilename + "_lt.txt")


    print("Experiment complete")
    print("=============================================================")


def parse_tpresult(result, labels, rfilename):
    with open(rfilename, 'w') as f:
        writer = csv.DictWriter(f, fieldnames=labels)
        writer.writeheader()
        for elem in result:
            writer.writerow(elem)

def parse_latencyresults(results: dict, rfilename):
    with open(rfilename, 'w') as f:
        for k, v in results.items():
            f.write("EXP:" + k + "\n")
            for l in v:
                f.write(str(l) + "\n")
            



def plot():
    pass


