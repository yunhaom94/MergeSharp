import csv
import numpy as np
import matplotlib.pyplot as plt


def write_to_csv(filename, fieldnames, vals):

    csvfile = open(filename, 'w', newline='') 
    writer = csv.DictWriter(csvfile, fieldnames=fieldnames)

    writer.writeheader()
    writer.writerows(vals)

def read_from_csv(filename):
    csvfile = open(filename, 'r', newline='') 
    reader = csv.reader(csvfile)
    headers = []
    x_values = []
    y_values = []
    i = 0
    for row in reader:
        if i == 0:
            headers = row
            y_values = [[] for _ in range((len(headers) - 1))]

        else:
            x_values.append(float(row[0]))
            
            val_i = 0
            for val in row[1:]:
                y_values[val_i].append(float(val))
                val_i += 1

        i += 1

    return headers, x_values, y_values

def draw(name, x_label, y_label, x_values, y_values):
    plt.xlabel(x_label)
    plt.ylabel(y_label)
    plt.title(name)

    markers = ["-x", "-d", "-s", "-o", "-*", "-+"]

    for i in range(len(y_values)):
        if (i > len(markers)):
            print("TOO MUCH DATA")
            return

        plt.plot(x_values, y_values[i], markers[i])


    plt.savefig(name + ".png")

if __name__ == "__main__":
    write_to_csv("x.csv", ["x", "a", "b"], [{"x": 1, "a":100, "b":200}, {"x": 2, "a": 150, "b": 250 }])
    headers, x, y = read_from_csv("x.csv")
    print(headers)
    draw("test", "# of Ops", "Throughput", x, y)