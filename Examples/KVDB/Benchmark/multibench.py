#!/usr/bin/python3

import csv
import json
from pickle import BUILD, TRUE
from sre_constants import REPEAT
from benchmark import *
from start_servers import *
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



REPEAT = 1
run_name = "default"


def run_experiment(workload_config: dict, prime_variable, secondary_variable, rfilename, server_list, build_flag=False):

    start = datetime.datetime.now()
    print("Currrent time: " + str(start))
    print("Running: " + rfilename)    


    # build and distribute
    if build_flag:
        build()

    cluster_config_dict = generate_cluster_configs(server_list)
    
    

    # y-axis
    primaries = workload_config[prime_variable]

    # more bars
    secondaries = workload_config[secondary_variable]

    json_dict = workload_config.copy()

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
            
            # for repeat
            for redo in range(REPEAT):

                json_dict[prime_variable] = p

                json_dict[secondary_variable] = s

                wlfilename = str(p) + str(s) + ".json"

                print(json_dict)

                # construct addresses which are ip:port in server_list
                addresses = []
                for ip in server_list:
                    addresses.append(ip + ":8000")

                json_dict["nodes"] = addresses

                with open(wlfilename, 'w') as json_file:
                    json.dump(json_dict, json_file)

                distribute(server_list, cluster_config_dict)
                # start servers
                for ip in server_list:
                    start_server(ip, cluster_config_dict)
                
                # Waiting here for a long time becasue we need to wait for the servers to 
                # 1. start
                # 2. join the cluster (servers waits a while before joining the cluster)
                # 3. initialize the keyspace
                # TODO: find a better way to do this, maybe check with the servers before running the benchmark
                time.sleep(10)

                try:
                    r = run_benchmark(wlfilename)

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
                    for ip in server_list:
                        copy_log_files(ip)
                        stop_servers(ip)
                        clean_up(ip)

                    os.remove(wlfilename)

                

                p_result[str(s)] = r.tp
                pm_result[str(s)] = r.mem
                latency_results[str(p) + str(s)] = r.latency_result
                
                
                json_dict = workload_config.copy()
                count += 1
                print(str(count) + "/" + str(total) + " done")
                end = datetime.datetime.now()
                print("Elapsed time:" + str(end - start))
                time.sleep(1)
                

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


