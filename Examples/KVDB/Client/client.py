#!/usr/bin/python3

import socket
import random
import time
import sys
from type.GCounter import PNCounter
from type.RCounter import RCounter
from type.ORSet import ORSet
from type.Graph import Graph
from type.RGraph import RGraph
from type.Performance import Performance
from type.BFTC import BFTC
from type.helper import res_parse
from type.Type import Type
from type.Action import Action


class Server:

    def __init__(self, ip, port):
        self.num_timeout = 0
        self.s = None
        self.ip = ip
        self.port = port

    def connect(self):
        try:
            self.s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.s.connect((self.ip, self.port))
            return 1
        except Exception as e:
            print(e)
            return 0

    def response(self) -> bytes:
        try:
            self.s.settimeout(60)
            msg = self.s.recv(1024)
            self.s.settimeout(None)
        except socket.timeout:
            self.disconnect()
            if self.num_timeout < 5:
                self.connect()
                print("Timeout on receive")
                self.num_timeout += 1
            else:
                raise socket.timeout
            return "F"

        return msg

    def send(self, data: bytes):            
        self.s.send(data)
        res = self.response()
        if res != "F":
            return res_parse(res)
        else:
            return "F"


    def disconnect(self):
        self.s.shutdown(socket.SHUT_RDWR)
        self.s.close()        



def isHelp(args):
    return len(sys.argv) == 2 and (args[1] == '--help' or args[1] == '-h')

def helpMessage():
    string = ("  Go to ../RAC and follow the instruction to boot up replication server  \n\n" +
        "  python3 client.py 127.0.0.1:<port number> \n\n" + 
        "  [For Example] python3 client.py 127.0.0.1:<port number> \n")
    print(string)

if __name__ == "__main__":
    if isHelp(sys.argv):
        helpMessage()

    else:
        if len(sys.argv) < 2:
            helpMessage()
            raise ValueError('wrong arg')
            
    
        # gc <key> <action> [value]
        address = sys.argv[1]
        host = address.split(":")[0]
        port = int(address.split(":")[1])

        s = Server(host, port)   

        if s.connect() == 0:
            print("connection failed")
            exit(1)

        while (True):
            text = input("Enter:").split(" ")

            typecode = text[0]

            if (typecode == Type.DISCONNECT):
                s.disconnect()
                exit(0)

            uid = text[1]
            opcode = text[2]
            typeClass = None

            if (typecode == Type.PNCOUNTER):
                typeClass = PNCounter(s)

            elif (typecode == Type.RCOUNTER):
                typeClass = RCounter(s)
                    
            elif (typecode == Type.ORSET):
                typeClass = ORSet(s)

            elif (typecode == Type.RGRAPH):
                typeClass = RGraph(s)

            elif (typecode == Type.PERFORMANCE):
                typeClass = Performance(s)

            elif (typecode == Type.BFTC):
                typeClass = BFTC(s)

            else:
                print("Type \'{}\' is not valid".format(typecode))
                continue

            typeClass.operate(text)
    
        s.disconnect()


    




        


   
