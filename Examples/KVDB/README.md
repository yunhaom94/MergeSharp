# KVDB

## How to run the servers:
1. Set-up .Net 6.0
2. Build with `dotnet build` or other configuration
3. For #n of nodes you want, make n copies of `cluster_config_example.json`, where in each of the copy, make n copies of 

```
{
        "nodeid": [node id, from 0 to n], 
        "address": [ip address],
        "port": [port for this node],
        "isSelf": true [only one is true, corresponding to the one the server is taking as input]
}
```

elements.,

4. Use `[Path_to_KVDB_binary] cluster_config.[node_id].json` or the binary to run an instance of sever as a node (binaries can be found in `bin\`)


## How to use the client
1. Use python 3.8.5+
2. Go to `Client\`
3. Run `client.py [server_ip:port]` to connect a node
4. A commandline UI will show up, type in command to interact with the server
5. Commands are the following format
[typecode] [key] [opcode] [param1] [param2]...
6. input `x` to disconnect

Type supported:

PN-Counter: typecode `pnc`

  set `s`
  
  get `g`
  
  increment `i [number_to_add]`
  
  decrement `d [number_to_subtract]`

Example: 

```
$ ./client.py 127.0.0.1:5000
Enter:pnc foo s
(True, ['', ''])
Enter:pnc foo i 5
(True, ['', ''])
Enter:pnc foo d 2
(True, ['', ''])
Enter:pnc foo g
(True, ['3', ''])
```



Run clients connecting to different nodes at the same time to see how replication works!


