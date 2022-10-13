import multiprocessing
from multiprocessing import Manager, managers
import string 
import random 
import math
import time
import json
import timeit
import subprocess
from enum import Enum
from ../Cleint/client.py import *
from draw import *
import numpy as np
from multiprocessing import Process, Pool


KEY_LEN = 5
STR_LEN = 10

class VAR_TYPE(Enum):
    INT = 1
    STRING = 2



def rand_str(n):
    return ''.join(random.choices(string.ascii_uppercase +
                             string.digits, k = n)) 

def split_ipport(address):

    res = address.split(":")

    return res[0], int(res[1])

def mix_lists(lists) -> list:
    '''
    Expand any single nested list
    '''
    res = []

    return [val for tup in zip(*lists) for val in tup]

    for l in lists:
        for i in range(len(l)):
            res.append(l[i])

    return res

def sleep_time(target_tp, num_clients):
    return (1 / target_tp) * num_clients

def reject_outliers(data, m = 2.):
    d = np.abs(data - np.median(data))
    mdev = np.median(d)
    s = d/mdev if mdev else 0.
    return data[s<m]

class Results():
    def __init__(self, num_clients) -> None:
        sharing = multiprocessing.Manager()
        self.tp = []
        self.latency = sharing.list()
        self.latency_result = []
        self.mem = 0

    def hanlde_latency(self):
        flag = False # somehow it is needed
        for l in self.latency:
            for lt in l:
                if lt[1] != 0:
                    self.latency_result.append((lt[0], lt[1] / 1000000, lt[2]))
                    flag = True

            if flag:
                self.latency_result.append("-NW-")
                flag = False

    def get_latency(self):        
        return reject_outliers(reject_outliers(np.array(self.latency_result)))


class ExperimentData():
    def __init__(self, num_objects, keys = []) -> None:
        self.num_objects = num_objects
        self.keys = keys if len(keys) > 0 else self._generate_keys()

    def CRDT(self, server):
        raise NotImplementedError

    def generate_init_req(self):
        raise NotImplementedError
        
    def generate_op_values(self, num_ops, ops_ratio, reverse=0):
        raise NotImplementedError

    def op_execute(self, crdt, req, last_res=""):
        raise NotImplementedError

    def _generate_keys(self) -> list:
        '''
        Return a list of random strings with Key_Len
        '''
        res = []
        for i in range(self.num_objects):
            res.append(rand_str(KEY_LEN))

        return res

    def _generate_values(self, num_ops, val_type):
        res = []
        for i in range(num_ops):
            if val_type == VAR_TYPE.INT:
                res.append(int(random.uniform(1,100)))
            elif val_type == VAR_TYPE.STRING: 
                res.append(rand_str(STR_LEN))

        return res

    def _generate_ops(self, num_ops, ops_ratio, op_types):
        res = []
        if round(sum(ops_ratio)) != 1 or len(ops_ratio) != len(op_types):
            print("Ratio error:" + str(ops_ratio))
            raise ValueError

        slots = [0]
        for r in ops_ratio:
            slots.append(slots[-1] + r)

        for _ in range(num_ops):
            sample = random.uniform(0,1)
            for i in range(len(slots)):
                if sample > slots[i] and sample <= slots[i+1]:
                    res.append(op_types[i])

        return res

class PNCExperimentData(ExperimentData):
    def CRDT(self, server):
        return PNCounter(server)

    def generate_init_req(self):
        res = []
        i = 0
        for key in self.keys:
            res.append(("s", key, i))
            i = i + 1

        return res


    def generate_op_values(self, num_ops, ops_ratio, reverse=0):
        '''
        reverse: num of reverse each key has
        '''
        res = []

        for k in self.keys:
            
            reqs = []
            values = self._generate_values(num_ops, VAR_TYPE.INT)
            ops = self._generate_ops(num_ops, ops_ratio, ["i", "d", "g"])
            assert len(ops) == len(values)

            for i in range(len(values)):
                v = values[i]
                op = ops[i]
                reqs.append((op, k, v))

            res.append(reqs)

        return res

    def op_execute(self, crdt, req, last_res=""):
        op = req[0]
        key = req[1]


        v = req[2]
        if op == "g":
            res = crdt.get(key)
        elif op == "s":
            res = crdt.set(key, v)
        elif op == "i":
            res = crdt.inc(key, v)
        elif op == "d":
            res = crdt.dec(key, v)

        return res

class RCExperimentData(PNCExperimentData):
    def CRDT(self, server):
        return RCounter(server)

    def generate_op_values(self, num_ops, ops_ratio, reverse=0):
        '''
        reverse: num of reverse each key has
        '''
        res = []

        for k in self.keys:
            
            reqs = []
            values = self._generate_values(num_ops, VAR_TYPE.INT)
            ops = self._generate_ops(num_ops, ops_ratio, ["i", "d", "g"])
            assert len(ops) == len(values)

            # reverse interval 
            if reverse > 0:
                r_interval = math.ceil(num_ops / (reverse + 1))


            r_cnt = 0
            for i in range(len(values)):
                v = values[i]
                op = ops[i]
                reqs.append((op, k, v))


                if (reverse > 0 and r_cnt < reverse and (i + 1)  % r_interval == 0):
                    reqs.append(("r", k, ""))
                    r_cnt += 1

            if (reverse > 0 and r_cnt < reverse):
                reqs.append(("r", k, ""))

            res.append(reqs)

        return res

    def op_execute(self, crdt, req, last_res=""):
        op = req[0]
        key = req[1]
        v = req[2]

        if op == "g":
            res = crdt.get(key)
        elif op == "s":
            res = crdt.set(key, v)
        elif op == "i":
            res = crdt.inc(key, v)
        elif op == "d":
            res = crdt.dec(key, v)
        elif op == "r":
            res = crdt.rev(key, last_res)
        else:
            raise ValueError("Incorrect input req: " + str(req))

        return res

class GExperimentData(ExperimentData):
    def CRDT(self, server):
        return Graph(server)

    def generate_init_req(self):
        res = []
        for key in self.keys:
            res.append(("s", key, ""))

        return res


    def generate_op_values(self, num_ops, ops_ratio, reverse=0):
        '''
        reverse: num of reverse each key has
        '''
        res = []

        num_write_ops = num_ops * ops_ratio[0]
        num_cycles_per_key = math.floor(num_write_ops / 5)

        num_read_per_cycle = math.floor(num_ops * ops_ratio[1] / num_cycles_per_key)
        print("Each graph has " + str(num_read_per_cycle) + " cycles")


        for k in self.keys:
            reqs = []
            for _ in range(num_cycles_per_key):
                values = self._generate_values(3, VAR_TYPE.STRING)
                ops = self._generate_ops(k, values[0], values[1], values[2])
                for op in ops:
                    reqs.append(op)
                for _ in range(num_read_per_cycle):
                    reqs.append(("g", k, ""))
            res.append(reqs)

        return res

    def _generate_ops(self, key, v1, v2, v3):
        res = []
        res.append(("av", key, v1))
        res.append(("av", key, v2))
        res.append(("av", key, v3))
        res.append(("ae", key, (v1, v2)))
        res.append(("ae", key, (v2, v3)))

        return res

    

    def op_execute(self, crdt, req, last_res=""):
        op = req[0]
        key = req[1]
        v = req[2]

        if op == "s":
            res = crdt.set(key)
        elif op == "g":
            res = crdt.get(key)
        elif op == "av":
            res = crdt.addvertex(key, v)
        elif op == "rv":
            res = crdt.remvoevertex(key, v)
        elif op == "ae":
            res = crdt.addedge(key, v[0], v[1])
        elif op == "re":
            res = crdt.removeedge(key, v[0], v[1])
        elif op == "r":
            res = crdt.reverse(key, last_res)

        return res

class RGExperimentData(GExperimentData):
    def CRDT(self, server):
        return RGraph(server)



    def generate_op_values(self, num_ops, ops_ratio, reverse=0):
        '''
        reverse: num of reverse each key has
        '''
        res = []

        num_write_ops = num_ops * ops_ratio[0]
        num_cycles_per_key = math.floor(num_write_ops / 5)

        num_read_per_cycle = math.floor(num_ops * ops_ratio[1] / num_cycles_per_key)

        print("Each graph has " + str(num_read_per_cycle) + " cycles")
       

        for k in self.keys:
            reqs = []
            i = 0
            for _ in range(num_cycles_per_key):
                values = self._generate_values(3, VAR_TYPE.STRING)
                ops = self._generate_ops(k, values[0], values[1], values[2])
                for op in ops:
                    reqs.append(op)
                for _ in range( num_read_per_cycle):
                    reqs.append(("g", k, ""))
                if (i < reverse):
                    reqs.append(("r", k, ""))
                i = i + 1 

            res.append(reqs)

        return res


class TestRunner():
    
    def __init__(self, nodes, multiplier, data, SharedManager, measuredops = []) -> None:
        self.nodes = nodes
        self.num_nodes = len(nodes)
        self.num_clients = math.ceil(self.num_nodes * multiplier)
        self.data = data
        self.connections = self._connect()
        self.crdts = [self.data.CRDT(s) for s in self.connections]
        self.timing = False
        self.do_reverse = False
        self.do_measure_reverse = False
        self.num_reverse = 0
        self.results = Results(self.num_clients)
        self.rid = SharedManager.dict()
        self.sleeptime = 0
        for k in self.data.keys:
            self.rid[k] = ""
        

    def _connect(self):
        res = []
        i = 0
        print("Connecting to servers:" + str(self.nodes))
        for _ in range(self.num_clients):
            adddress = self.nodes[i]
            ip, port = split_ipport(adddress)
            s = Server(ip, port)
            
            if (s.connect() == 0):
                print("Connection to " + adddress + " failed, exiting")
                exit()

            res.append(s)

            i += 1
            if i == len(self.nodes):
                i = 0
        print("Connection complete")
        return res

    def init_data(self):
        reqs = self.data.generate_init_req()
        c = 0

        for r in reqs:
            res = self.data.op_execute(self.crdts[0], r)
            if not res[0]:
                raise Exception("Initialization failed because " + str(res))
            c += 1
            if (c == len(self.crdts)):
                c = 0


    
    def split_work(self, list_reqs):
        '''
        list_reqs: expecting [("op", "key", "value"), ("op", "key", "value"), ...]
        '''

        split = math.ceil(len(list_reqs) / len(self.crdts))
        works = []

        for i in range(0, len(list_reqs), split):
            works.append(mix_lists(list_reqs[i:i + split]))

        workers_pool = multiprocessing.Pool(self.num_clients)
        workers_pool.starmap(self.worker, zip(self.crdts, works))
        workers_pool.close()
        workers_pool.join() 

    def worker(self, crdt, list_reqs):
        temp = []
        last_rid = {}
        last_lt = 0

        for req in list_reqs:
            #print("Doing " + str(req))
            if self.sleeptime > 0:
                time.sleep(max(self.sleeptime - last_lt, 0))

            start = time.time_ns() 
            
            if self.do_reverse:
                if req[0] == "r":
                    updateid = last_rid.get(req[1])
                    if (updateid):
                        res = self.data.op_execute(crdt, req, updateid)
                else:
                    try:
                        res = self.data.op_execute(crdt, req)
                        if req[0] != "g" and res[1][0] != "":
                            last_rid[req[1]] = res[1][0] 
                    except Exception:
                        continue
            else:
                res = self.data.op_execute(crdt, req)

            end = time.time_ns() 

            if (res == "F"):
                temp.append((req[0], -1))

            elif (self.timing):
                temp.append((req[0], (end - start), end))

            last_lt = (end - start) / 1000000000
        
        self.results.latency.append(temp)



    def prep_ops(self, total_prep_ops, pre_ops_ratio, reverse=0):
        if reverse > 0: 
            self.do_reverse = True 
        if total_prep_ops == 1:
            print("Information: total_prep_ops == 1, do_measure_reverse set to True")
            self.do_measure_reverse = True
            self.num_reverse = reverse
            self.do_reverse = False
            reverse = 0
            return

        reqs = self.data.generate_op_values(total_prep_ops, pre_ops_ratio, reverse)
        self.split_work(reqs)


    def benchmark(self, ops_per_object, ops_ratio, throughput = 0):
        '''
        throughput = limit # of ops per second per worker, if 0 then unlimited
        '''
        self.do_reverse = False
        if self.do_measure_reverse == True:
            self.do_reverse = True
            print("Information: now worker will see do_reverse == True")
        
        self.timing = True

        if throughput > 0:
            self.sleeptime = sleep_time(throughput, self.num_clients)
        
        
        reqs = self.data.generate_op_values(ops_per_object, ops_ratio, self.num_reverse)
        #reqs = self.data.generate_op_values(ops_per_object, ops_ratio)
        print("num objects: " + str(len(reqs)))
        print("ops per object: " + str(len(reqs[0])))
        start = time.time()
        self.split_work(reqs)
        end = time.time()

        self.results.tp = (ops_per_object * len(self.data.keys)) / (end - start)
        self.results.hanlde_latency()

        time.sleep(2)
        mem = 0
        # TODO: this is not working properly, sometime, dont know why
        for s in self.connections:
            try:
                pref = Performance(s)
                res = pref.get()
                mem += int(res[1][2].split(":")[1])
            except:
                print("Error getting memory: ")
                print(res)
                mem = -1
                break

        self.results.mem = mem / len(self.connections)

    def close_connection(self):
        for c in self.connections:
            c.disconnect()

TYPECODE_MAP = {
    "pnc": PNCExperimentData,
    "rc": RCExperimentData,
    "bftc": RCExperimentData,
    "g": GExperimentData,
    "rg": RGExperimentData
}


def select_exp(typecode:str, total_objects:int) -> ExperimentData:
    return TYPECODE_MAP[typecode](total_objects)

def run_benchmark(workloadfile) -> Results:
    manager = multiprocessing.Manager()
    
    with open(workloadfile) as wl_file:
        workload = json.loads(wl_file.read())

    nodes = workload["nodes"]
    client_multiplier = workload["client_multiplier"]

    typecode = workload["typecode"]
    total_objects = workload["total_objects"]


    prep_ops_pre_obj = workload["prep_ops_pre_obj"]
    num_reverse = workload["num_reverse"]
    prep_ratio = workload["prep_ratio"]
    

    ops_per_object = workload["ops_per_object"]
    op_ratio = workload["op_ratio"]
    target_throughput = workload["target_throughput"]
 
    td = select_exp(typecode, total_objects)

    print("Starting experiment...")
    print("Total " + str(len(nodes)) + " server and " + str(math.ceil(len(nodes) * client_multiplier)) + " client")
    tr = TestRunner(nodes, client_multiplier, td, manager)

    print("Initializing Data with " + str(total_objects) + " objects")
    tr.init_data()

    time.sleep(5)

    print("Preping Ops with " + str(prep_ops_pre_obj) + " prep ops and " + str(num_reverse) + " reverses")
    tr.prep_ops(prep_ops_pre_obj, prep_ratio, num_reverse)

    time.sleep(2)

    print("Total ops:" + str(total_objects * ops_per_object) + " with each obj " + str(ops_per_object) + " ops")
    print("Measuing Throughput")
    tr.benchmark(ops_per_object, op_ratio, target_throughput)

    
    
    #print("Throughput:")
    #print(tr.results.tp)
    #print("Median Latency")
    #print(np.median(tr.results.get_latency()))
    #print("Latency std")
    #print(np.std(tr.results.get_latency()))

    tr.close_connection()

    print("Experiment ends, close connection")


    return tr.results


        
if __name__ == "__main__":
    if len(sys.argv) < 2:
        raise ValueError('wrong arg')

    workloadfile = sys.argv[1]
    run_benchmark(workloadfile)
