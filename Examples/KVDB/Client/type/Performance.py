from .helper import msg_construct
from .helper import req_construct
from type.Action import Action


class Performance:
    def __init__(self, s):
        self.server = s

    def get(self):
        req = req_construct("pref", "pf", "g", [])
        req = msg_construct(self.server, req)

        res = self.server.send(req)
        return res

    def operate(self, text):

        uid = text[1]
        opcode = text[2]

        if (opcode == Action.GET):
            print(self.get())
        else:
            print("Operation \'{}\' is not valid".format(opcode))
        
        return