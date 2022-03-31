# Simple Cellular Automata

A 2D Cellular Automata simulator that supports custom neighborhoods and rules.

This simulator was written and used for the game [Cellular Automata: Conway's Game of Life](https://iamsyko.itch.io/cellular-automata-conways-game-of-life).

## Usage

```csharp
// The rules of the simulation is defined as a CASpec object.
CASpec spec = new CASpec(
	new int[,] // 2d array defining the neighbors that are counted for a cell (cell itself in the middle)
	{
		{ 1, 1, 1 },
		{ 1, 0, 1 },
		{ 1, 1, 1 }
	},
	new int[][] {
		new int[] {0, 0, 0, 1, 0, 0, 0, 0, 0}, // When cell has <index> # of live neighbors: 1 = dead cell becomes alive
		new int[] {0, 0, 1, 1, 0, 0, 0, 0, 0} // When cell has <index> # of live neighbors: 1 = live cell stays alive
	});

// Create the simulator
CASimulator simulator = new CASimulator(20, 20, spec);

// Add a glider
simulator.SetState(5, 5, true);
simulator.SetState(6, 6, true);
simulator.SetState(4, 7, true);
simulator.SetState(5, 7, true);
simulator.SetState(6, 7, true);

// Simulate
simulator.step();
simulator.step();
simulator.step();
simulator.step();

// Read state
bool[,] state = simulator.GetState().Clone();

```