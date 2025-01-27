import matplotlib.pyplot as plt
import csv
import sys

def main():
	colToPlot = sys.argv[1]
	colors = ["blue", "orange", "green", "red", "purple"]
	plt.figure()

	for i in range(2, len(sys.argv), 2):
		with open(sys.argv[i], 'r') as file:
			reader = csv.reader(file)
			header = next(reader)

			columns = {name: [] for name in header}

			for row in reader:
				for j, value in enumerate(row):
					columns[header[j]].append(float(value))

			for column_name, values in columns.items():
				if (column_name != colToPlot): continue
				plt.plot(range(len(values)), values, marker='o', color=colors[i//2-1], label=sys.argv[i+1])
	
	plt.xlabel('timesteps since start')
	plt.xlim(0, plt.xlim()[1])
	plt.ylim(max(0, plt.ylim()[0]), plt.ylim()[1])
	plt.ylabel(colToPlot)
	plt.title("")
	plt.grid(True)
	plt.legend()
	plt.show()
		

if __name__ == "__main__":
	main()
	exit()
