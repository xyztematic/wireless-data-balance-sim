import matplotlib.pyplot as plt
import csv
import getpass

def main():
    filename = "/home/"+getpass.getuser()+"/.config/unity3d/DefaultCompany/BA/Recordings/simdata_test"
    x = []
    y = []

    with open(filename, 'r') as csvfile:
        csvreader = csv.reader(csvfile)
        for line_number, row in enumerate(csvreader, start=1):
            if row:
                x.append(line_number)
                y.append(float(row[1]))

    plt.plot(x, y)
    plt.xlabel('# transmissions per node')
    plt.ylabel('proportion of fully covered area')
    plt.title('Proportion of area that is fully covered in a 10x10 square grid network')
    plt.grid(True)
    plt.show()

    return

if __name__ == "__main__":
    main()
    exit()