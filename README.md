# Wireless Network Data Balancing Simulator
## Requirements:
Linux x86 64 bit compatible machine (tested on Ubuntu 22.04 and 24.04) 
### Start with the provided build:
- Clone this repository
- Navigate to /Build
- Execute Buildx86_64.so
### Or build from source:
- Install unityhub and follow the requirements that go along with it
- Use unityhub to install the Unity Editor version 2022.3.48f1
- Clone this repository
- In unityhub, select "Add" -> "Add project from disk"
- Select the parent folder of the cloned repository
- Open the project
- Select Assets/Scenes/SampleScene.unity in the hierarchy and press "Open" in the inspector)
- For building: Select "File" -> "Build and Run" in the Unity Editor
## How to use the simulator:
### Camera movement
- Move the camera view angle with your mouse
- Move up, down, left, right, forward, backward with
  (SPACE), (L_SHIFT), (A), (D), (W), (S), respectively
### Starting a simulation
- Open the internal command line with (TAB)
- Input a simulation command and press (ENTER)
- Stop the simulation by opening the command line (TAB) and enter ```stop``` + (ENTER)
- Before starting a new simulation, open the command line (TAB) and enter ```reset``` + (ENTER)
### Simulation Commands
A simulation command can either be a preset, if you have a simulation identifier, or a full command that specifies all parameters.
#### Using a simulation identifier:
Enter ```p <ID>```, where <ID> is the simulation identifier
#### Using a full command:
Enter ```<inv_behavior> <allow_coding> <range> <dimension> <topology> <source_nodes> <save_file_name>```, where the parameters are the sub-commands specified below:

|Parameter| Sub-Command | Option x | Option y | Option t |
| --- | --- | --- | --- | --- |
|inv_behavior| ```a <x> <y>```| 0 (inventory limit n)| 0 (dynamic inventory off)| |
| | | 2 (inventory limit 2*n/N(v))| 1 (dynamic inventory on)| |
| | | 3 (inventory limit n - 1)| | |
|allow_coding| ```c <x>```| 0 (no coding allowed)| | |
| | | 1 (coding allowed)| | |
|range| ```r <x>```| any positive float (range of all nodes) | | |
|dimension| ```d <x>```| any positive integer (dimension) | | |
|topology| ```<t> <x> <y>``` | any positive integer (#nodes in x-direction)| any positive integer (#nodes in z-direction) | g (square topology) |
| | | | | h (hexagonal topology) |
|source_nodes| ```s <x> <y>``` | any positive integer (x-index of node) | any positive integer (z-index of node) | |
|save_file_name| ```save <x>``` | any alphanumeric string (file name of recorded data)| |


