import matplotlib.pyplot as plt
import csv
import sys

def main():
    print("(python) setting up data display")
    if len(sys.argv) <= 1:
        print("(python) too few arguments")
        return
    filename = sys.argv[1]
    x = []
    y = []

    with open(filename, 'r') as csvfile:
        csvreader = csv.reader(csvfile)
        for line_number, row in enumerate(csvreader, start=0):
            if row:
                x.append(line_number)
                y.append(float(row[1]))

    plt.plot(x, y)
    plt.xlabel('transmissions per node')
    plt.ylabel('proportion of fully covered area')
    plt.ylim((0.0, 1.0))
    plt.title('Proportion of area that is fully covered')
    plt.grid(True)
    plt.show()
    
    return

if __name__ == "__main__":
    main()
    exit()