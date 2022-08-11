# 1-1


    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "pnc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 0,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }


    
    lines = [ "Chart1-1-Counter-base-readheavy-varyclient", "Chart1-1-Counter-base-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.35, 0.35, 0.3]]
    client_multiplier = [2,3,4,5,6,8,10,12,14]
    target_throughput = [1000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 1:
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 1]
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
###########################################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,
    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 0,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }


    
    lines = [ "Chart1-1-Counter-lazy0-readheavy-varyclient" , "Chart1-1-Counter-lazy0-balanced-varyclient",  "Chart1-1-Counter-lazy0-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11,13,15]
    target_throughput = [1000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 1:
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 1]
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1


#########################################

    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 50,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = [ "Chart1-1-Counter-lazy50-readheavy-varyclient", "Chart1-1-Counter-lazy50-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.35, 0.35, 0.3]]
    client_multiplier = [2,3,4,5,6,7]
    target_throughput = [1000, 3000, 5000, 7000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 4: # change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 4] # change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1


##########################

    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 100,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = [ "Chart1-1-Counter-lazy100-readheavy-varyclient","Chart1-1-Counter-lazy100-balanced-varyclient" , "Chart1-1-Counter-lazy100-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]
    client_multiplier = [5,6,7,8,9,10]
    target_throughput = [1000, 3000, 5000, 7000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 4: # change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 4] # change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1

        
##########################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 0,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-3-Graph-rev0lazy-readheavy-varyclient","Chart1-3-Graph-rev0lazy-balanced-varyclient" , "Chart1-3-Graph-rev0lazy-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5], [0.7, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11]
    target_throughput = [1000, 6000, 12000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 3: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 3] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)
        
        
#################################################
if __name__ == "__main__":
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 50,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-3-Graph-rev50lazy-readheavy-varyclient","Chart1-3-Graph-rev50lazy-balanced-varyclient" , "Chart1-3-Graph-rev50lazy-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5], [0.7, 0.3]]
    client_multiplier = [3,4,5,6,7,8]
    target_throughput = [1000, 3000, 6000, 9000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 4: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 4] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)
        
        
    #################################################


        exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 100,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-3-Graph-rev100lazy-readheavy-varyclient","Chart1-3-Graph-rev100lazy-balanced-varyclient" , "Chart1-3-Graph-rev100lazy-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5], [0.7, 0.3]]
    client_multiplier = [2,3,4,5,6,7]
    target_throughput = [1000, 3000, 6000, 9000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 4: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 4] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)
        
            exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 100,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-3-Graph-rev100lazy-readheavy-varyclient", "Chart1-3-Graph-rev100lazy-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.7, 0.3]]
    client_multiplier = [2,3,4,5,6]
    target_throughput = [1000, 2000, 4000, 6000, 8000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 5: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 5] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)
        
#############################
    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [7],

        "typecode": "pnc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": 0,
        "prep_ratio": [1, 0, 0],


        "ops_per_object": 1000,
        "op_ratio": None,
        "target_throughput": 0
    }
    
    

    



    lines = ["Chart2-1-Counter-baseline-readheavy-maxclient",
                "Chart2-1-Counter-baseline-balanced-maxclient",
                "Chart2-1-Counter-baseline-writeheavy-maxclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5] ,[0.35, 0.35, 0.3]]
    
    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder

        run_experiment(exp, "client_multiplier", "total_objects", folder , SERVER_LIST)

#############################


    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [7],

        "typecode": "rc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": None,
        "prep_ratio": [0.5, 0.5, 0],


        "ops_per_object": 1000,
        "op_ratio": None,
        "target_throughput": 0
    }


    numrev = [0, 50, 100, 150, 200, 250, 300]


    lines = ["Chart2-1-Counter-varyrev-lazy-readheavy-maxclient",
                "Chart2-1-Counter-varyrev-lazy-balanced-maxclient",
                "Chart2-1-Counter-varyrev-lazy-writeheavy-maxclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]

    i = 0
    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder

        for j in numrev:
            exp["num_reverse"] = j
            run_experiment(exp, "client_multiplier", "total_objects", folder + "+rev" + str(j), SERVER_LIST)

        i += 1


##########################################

    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [6],

        "typecode": "g",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": 0,
        "prep_ratio": [1, 0],


        "ops_per_object": 1000,
        "op_ratio": None,
        "target_throughput": 0
    }
    
    

    



    lines = ["Chart2-2-graph-baseline-readheavy-maxclient",
                "Chart2-2-graph-baseline-balanced-maxclient",
                "Chart2-2-graph-baseline-writeheavy-maxclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5] ,[0.7, 0.3]]
    
    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder

        run_experiment(exp, "client_multiplier", "total_objects", folder , SERVER_LIST)


#######################################################

    exp1 = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [7],

        "typecode": "pnc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1,
        "num_reverse": 0,
        "prep_ratio": [1, 0, 0],


        "ops_per_object": 2000,
        "op_ratio": [0.25, 0.25, 0.5],
        "target_throughput": 0
    }

    folder = "Chart4-1-Counter-base-balanced-varyclient-6client-ts"
    multibench.run_name = folder
    run_experiment(exp1, "client_multiplier", "total_objects", folder, SERVER_LIST)

####################################
    exp1 = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [7],

        "typecode": "rc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1,
        "num_reverse": None,
        "prep_ratio": [1, 0, 0],


        "ops_per_object": 2000,
        "op_ratio": [0.25, 0.25, 0.5],
        "target_throughput": 0
    }

    numrev = [0, 50, 100]

    for j in numrev:
        exp1["num_reverse"] = j
        folder = "Chart4-1-Counter-" + str(j) + "lazy-balanced-maxclient-ts"
        multibench.run_name = folder
        run_experiment(exp1, "client_multiplier", "total_objects", folder, SERVER_LIST)
###############
    exp1 = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [6],

        "typecode": "g",
        "total_objects": [200],

        "prep_ops_pre_obj": 1,
        "num_reverse": 0,
        "prep_ratio": [1, 0],


        "ops_per_object": 2000,
        "op_ratio": [0.5, 0.5],
        "target_throughput": 0
    }

    folder = "Chart4-2-graph-base-balanced-ts"
    multibench.run_name = folder
    run_experiment(exp1, "client_multiplier", "total_objects", folder, SERVER_LIST)


######################
if __name__ == "__main__":
    exp1 = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [6],

        "typecode": "rg",
        "total_objects": [200],

        "prep_ops_pre_obj": 1,
        "num_reverse": None,
        "prep_ratio": [1, 0],


        "ops_per_object": 2000,
        "op_ratio": [0.5, 0.5],
        "target_throughput": 0
    }

    numrev = [0, 50, 100]

    for j in numrev:
        exp1["num_reverse"] = j
        folder = "Chart4-2-Graph-" + str(j) + "lazy-balanced-maxclient-ts"
        multibench.run_name = folder
        run_experiment(exp1, "client_multiplier", "total_objects", folder, SERVER_LIST)

############################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 0,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }


    
    lines = [ "Chart1-2-Counter-eager0-readheavy-varyclient" , "Chart1-2-Counter-eager0-balanced-varyclient",  "Chart1-2-Counter-eager0-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11,13,15]
    target_throughput = [1000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 1: # TODO: HERE
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 1] #TODO: HERE
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1


###################

    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 50,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = [ "Chart1-2-Counter-eager50-readheavy-varyclient","Chart1-2-Counter-eager50-balanced-varyclient" , "Chart1-2-Counter-eager50-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]
    client_multiplier = [5,6,7,8,9,10]
    target_throughput = [1000, 3000, 5000, 7000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 4: # change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 4] # change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
#########################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 100,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }


    
    lines = [ "Chart1-2-Counter-eager100-readheavy-varyclient" , "Chart1-2-Counter-eager100-balanced-varyclient",  "Chart1-2-Counter-eager100-writeheavy-varyclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11,13,15]
    target_throughput = [1000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 1: # TODO: HERE
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 1] #TODO: HERE
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1

########################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 0,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-4-Graph-rev0eager-readheavy-varyclient","Chart1-4-Graph-rev0eager-balanced-varyclient" , "Chart1-4-Graph-rev0eager-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5], [0.7, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11]
    target_throughput = [1000, 6000, 12000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 3: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 3] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)


#################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rg",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 100,
    "prep_ratio": [1, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }



    lines = ["Chart1-4-Graph-rev100eager-readheavy-varyclient","Chart1-4-Graph-rev100eager-balanced-varyclient" , "Chart1-4-Graph-rev100eager-writeheavy-varyclient"]
    opratio = [[0.3, 0.7], [0.5, 0.5], [0.7, 0.3]]
    client_multiplier = [2,3,4,5,7,9,11]
    target_throughput = [1000, 6000, 12000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 3: # TODO: change here
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 3] # TODO: change here
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1
            sleep(2)


###################
    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [7],

        "typecode": "rc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": None,
        "prep_ratio": [0.5, 0.5, 0],


        "ops_per_object": 1000,
        "op_ratio": None,
        "target_throughput": 0
    }


    numrev = [0, 50, 100, 150, 200, 250, 300]


    lines = ["Chart2-1-Counter-varyrev-eager-readheavy-maxclient",
                "Chart2-1-Counter-varyrev-eager-balanced-maxclient",
                "Chart2-1-Counter-varyrev-eager-writeheavy-maxclient"]
    opratio = [[0.15, 0.15, 0.7], [0.25, 0.25, 0.5], [0.35, 0.35, 0.3]]

    i = 0
    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder

        for j in numrev:
            exp["num_reverse"] = j
            run_experiment(exp, "client_multiplier", "total_objects", folder + "+rev" + str(j), SERVER_LIST)

        i += 1


###################
    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [6],

        "typecode": "rg",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": None,
        "prep_ratio": [1, 0],


        "ops_per_object": 1000,
        "op_ratio": None,
        "target_throughput": 0
    }
    
    


    numrev = [0, 50, 100, 150, 200, 250, 300]



    lines = ["Chart2-2-Graph-varyrev-eager-readheavy-x6client",
                "Chart2-2-Graph-varyrev-eager-balanced-x6client",
                "Chart2-2-Graph-varyrev-eager-writeheavy-x6client"]
    opratio = [[0.3, 0.7], [0.5, 0.5] ,[0.7, 0.3]]

    i = 0
    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder

        for j in numrev:
            exp["num_reverse"] = j
            run_experiment(exp, "client_multiplier", "total_objects", folder + "+rev" + str(j), SERVER_LIST)

        i += 1
##############
    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [10],

        "typecode": "pnc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": 0,
        "prep_ratio": [0.5, 0.5, 0],


        "ops_per_object": 1000,
        "op_ratio": [0.25, 0.25, 0.5],
        "target_throughput": 1000
    }


    folder = "Chart5-1-Counter-base-balanced-1000tp"
    multibench.run_name = folder
    run_experiment(exp, "client_multiplier", "total_objects", folder, SERVER_LIST)


    exp = {
        "nodes_pre_server": 1,
        "use_server": 5,
        "client_multiplier": [10],

        "typecode": "rc",
        "total_objects": [200],

        "prep_ops_pre_obj": 1000,
        "num_reverse": None,
        "prep_ratio": [0.5, 0.5, 0],


        "ops_per_object": 1000,
        "op_ratio": [0.25, 0.25, 0.5],
        "target_throughput": 1000
    }

    numrev = [0, 50, 100]

    for j in numrev:
        exp["num_reverse"] = j
        folder = "Chart5-1-Counter-" + str(j) + "lazy-balanced-1000tp"
        multibench.run_name = folder
        run_experiment(exp, "client_multiplier", "total_objects", folder, SERVER_LIST)



################
    exp = {
    "nodes_pre_server": 1,
    "use_server": 5,
    "client_multiplier": None,

    "typecode": "rc",
    "total_objects": [200],

    "prep_ops_pre_obj": [1000],
    "num_reverse": 50,
    "prep_ratio": [0.5, 0.5, 0],


    "ops_per_object": 1000,
    "op_ratio": None,
    "target_throughput": None
    }


    #"Chart1-2-Counter-eager50-readheavy-varyclient" ,[0.15, 0.15, 0.7],
    lines = [   "Chart1-2-Counter-eager50-writeheavy-varyclient"]
    opratio = [ [0.35, 0.35, 0.3]]
    client_multiplier = [3,4,5,7,9,11,13,15]
    target_throughput = [1000, 10000]

    for k in range(len(lines)):
        exp["op_ratio"] = opratio[k]
        folder = lines[k]
        multibench.run_name = folder
        i = 0
        while i < 10:
            if i < 2: # TODO: HERE
                exp["client_multiplier"] = 5
                exp["target_throughput"] = target_throughput[i]
            else:
                exp["client_multiplier"] = client_multiplier[i - 2] #TODO: HERE
                exp["target_throughput"] = 0 
                
            run_experiment(exp, "prep_ops_pre_obj", "total_objects", folder + "-run" + str(i) , SERVER_LIST)
            i += 1

