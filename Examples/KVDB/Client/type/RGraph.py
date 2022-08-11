from .helper import msg_construct
from .helper import req_construct
from type.Action import Action

class RGraph:
    def __init__(self, s):
        self.server = s

    def get(self, id):
        req = req_construct("rg", id, "g", [])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def set(self, id):
        req = req_construct("rg", id, "s", [])
        req = msg_construct(self.server, req)
        
        res = self.server.send(req)
        return res

    def addvertex(self, id, value):
        req = req_construct("rg", id, "av", [str(value)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def remvoevertex(self, id, value):
        req = req_construct("rg", id, "rv", [str(value)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def addedge(self, id, value1, value2):
        req = req_construct("rg", id, "ae", [str(value1), str(value2)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def removeedge(self, id, value1, value2):
        req = req_construct("rg", id, "re", [str(value1), str(value2)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def reverse(self, id, value):
        req = req_construct("rg", id, "r", [str(value)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def operate(self, text):

        uid = text[1]
        opcode = text[2]

        if (opcode == Action.GET):
            print(self.get(uid))
        elif (opcode == Action.SET):
            print(self.set(uid))
        elif (opcode == Action.ADDVERTEX):
            value = text[3]
            print(self.addvertex(uid, value))
        elif (opcode == Action.REMOVEVERTEX):
            value = text[3]
            print(self.remvoevertex(uid, value))
        elif (opcode == Action.ADDEDGE):
            value1 = text[3]
            value2 = text[4]
            print(self.addedge(uid, value1, value2))
        elif (opcode == Action.REMOVEEDGE):
            value1 = text[3]
            value2 = text[4]
            print(self.removeedge(uid, value1, value2))
        elif (opcode == Action.REVERSE):
            value = text[3]
            print(self.reverse(uid, value))
        else:
            print("Operation \'{}\' is not valid".format(opcode))
        
        return