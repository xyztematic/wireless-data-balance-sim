import matplotlib.pyplot as plt
import csv
import sys

def main():
    print("(python) setting up data display")
    if len(sys.argv) <= 1:
        print("(python) too few arguments")
        return
    filename = sys.argv[1]
    with open(filename, 'r') as file:
        reader = csv.reader(file)
        header = next(reader)
        
        columns = {name: [] for name in header}
        
        for row in reader:
            for i, value in enumerate(row):
                columns[header[i]].append(float(value))
        
    for column_name, values in columns.items():
        plt.figure()
        plt.plot(range(len(values)), values, marker='o')
        plt.xlabel('Timestep')
        plt.xlim(0, plt.xlim()[1])
        plt.ylim(max(0, plt.ylim()[0]), plt.ylim()[1])
        plt.ylabel(column_name)
        plt.title(f'Plot of {column_name}')
        plt.grid(True)
        plt.show()
    return

if __name__ == "__main__":
    main()
    exit()