#!/usr/bin/python3
from client import *
from startservers import *
import random



def split_ipport(address):

    res = address.split(":")

    return res[0], int(res[1])

def connect(addresses):
    res = []
    for i in range(len(addresses)):
        adddress = addresses[i]
        ip, port = split_ipport(adddress)
        s = Server(ip, port)
        
        if (s.connect() == 0):
            print("Connection to " + adddress + " failed, exiting")
            exit()

        res.append(s)


    print("Connection complete")
    return res


def PNC_Test():
    
    pnc = []

    num_server = 3
    
    addresses = start_server(num_server)
    #addresses = ['127.0.0.1:5000', '127.0.0.1:5001']

    time.sleep(2)
    connections = connect(addresses)
    input()
    for i in range(len(addresses)):
        pnc.append(PNCounter(connections[i]))

    result = 0

    pnc[0].set("test", result)
    time.sleep(1)

    for i in range(100):
        server_id = i % num_server
        num = random.randint(-10, 10)
        if (num < 0):
            res = pnc[server_id].dec("test", abs(num))
        else:
            res = pnc[server_id].inc("test", num)

        if (res == 'F'):
            break

        result += num

    time.sleep(1)
    try:
        for i in range(len(addresses)):
            assert(int(pnc[server_id].get("test")[1][0]) == result)
    finally:
        print("Result server: " + str(pnc[i].get("test")) + " local: " + str(result))
        stop_server()
        
    


if __name__ == "__main__":
    build_server()
    PNC_Test()
