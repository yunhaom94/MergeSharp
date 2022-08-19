from .helper import msg_construct
from .helper import req_construct
from type.Action import Action

class PNCounter:

    def __init__(self, s):
        self.server = s

    def get(self, id):
        req = req_construct("pnc", id, "g", [])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def set(self, id):
        req = req_construct("pnc", id, "s", [])
        req = msg_construct(self.server, req)
        
        res = self.server.send(req)
        return res

    def inc(self, id, value):
        req = req_construct("pnc", id, "i", [str(value)])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res


    def dec(self, id, value):
        req = req_construct("pnc", id, "d", [str(value)]) 
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
        elif (opcode == Action.INCREMENT):
            value = text[3]
            print(self.inc(uid, value))
        elif (opcode == Action.DECREMENT):
            value = text[3]
            print(self.dec(uid, value))
        else:
            print("Operation \'{}\' is not valid".format(opcode))
        
        return

