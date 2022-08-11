import sys

num_fields = 2
head_len = 1 + num_fields * 4
typePrefix = "t&"
uidPrefix = "u&"
opPrefix = "o&"
paramPrefix = "p&"

def msg_construct(server, msg: str):

    msg_src = 2

    s = '\f'.encode('utf-8') + \
        msg_src.to_bytes(4, sys.byteorder) + \
        len(msg).to_bytes(4, sys.byteorder) + \
        msg.encode('utf-8')

    return s


def req_construct(tid, uid, op, params):
    req = typePrefix + tid + "\n" + \
          uidPrefix + uid + "\n" + \
          opPrefix + op + "\n"

    for p in params:
        req += paramPrefix + p + "\n" 

    return req


def res_parse(res: bytes):
    
    
    try:
        # maybe do some checks as well?
        lines = res[head_len:].decode("utf-8").split("\n")
    except IndexError:
        print("Parsing failure:")
        print(res)
        print("======================")
        return (False, "")

    if "Succeed" in lines[0]:
        success = True 
    else:
        success = False

    del lines[0]

    return (success, lines)