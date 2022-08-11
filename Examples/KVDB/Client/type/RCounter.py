from .helper import msg_construct
from .helper import req_construct
from type.Action import Action

'''
Reversible Counter
'''

class RCounter:

    def __init__(self, s):
        self.server = s

    def get(self, id):
        req = req_construct("rc", id, "g", [])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def set(self, id, value):
        req = req_construct("rc", id, "s", [str(value)])
        req = msg_construct(self.server, req)
        
        res = self.server.send(req)
        return res

    def inc(self, id, value, rid = ""):
        req = req_construct("rc", id, "i", [str(value), rid]) 
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res


    def dec(self, id, value, rid = ""):
        req = req_construct("rc", id, "d", [str(value), rid]) 
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def rev(self, id, value):
        req = req_construct("rc", id, "r", [str(value)]) 
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def operate(self, text):

        uid = text[1]
        opcode = text[2]

        if (opcode == Action.GET):
            print(self.get(uid))
        elif (opcode == Action.SET):
            value = text[3]
            print(self.set(uid, value))
        elif (opcode == Action.INCREMENT):
            value = text[3]
            try:
                rid = text[4]
            except:
                rid = ""
            print(self.inc(uid, value, rid))
        elif (opcode == Action.DECREMENT):
            value = text[3]
            try:
                rid = text[4]
            except:
                rid = ""
            print(self.dec(uid, value, rid))
        elif (opcode == Action.REVERSE):
            value = text[3]
            print(self.rev(uid, value))
        else:
            print("Operation \'{}\' is not valid".format(opcode))
        
        return

