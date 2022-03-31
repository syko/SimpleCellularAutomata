using System;

namespace Syko.SimpleCellularAutomata
{
    /// <summary>
    /// A specification containing the neighborhood definition and ruleset for a simulation.
    /// </summary>
    public struct CASpec
    {
        /// <summary>
        /// A 2d array of flags indicating which cells in the "neighborhood" of a cell to consider during the simulation.
        /// Can be of any size as long as the length of the 2d array in each direction is an odd number.
        /// The 'current cell' being simulated is in the center of the array and the rest of the array elements define whether the
        /// 'neighbors' of the middle cell should be considered or not.
        /// Since this users integers instead of booleans it's possible to make the simulation count neighbors more than once (ie. adding more 'weight' to some than others).
        /// eg. Moore's neighborhood would be defined as { {1, 1, 1}, {1, 0, 1}, {1, 1, 1} }
        /// </summary>
        public int[,] neighborhood;
        /// <summary>
        /// An array of 2 arrays specifying whether a cell should be a live or dead cell after the next step of the simulation.
        /// The first array (index: 0) should indicate if a dead cell should become a live cell in the next step given that it has
        /// the number of live neighbours indicated by the array index of the subarray.
        /// The second array (index: 1) should indicate if a live cell should die in the next step given that it has
        /// the number of live neighbours indicated by the array index of the subarray.
        /// eg. the rules for Conway's Game of Life are { {0, 0, 0, 1, 0, 0, 0, 0, 0}, {0, 0, 1, 1, 0, 0, 0, 0, 0} }
        /// </summary>
        public int[][] rules;

        public CASpec(int[,] neighborhood, int[][] rules)
        {
            this.neighborhood = neighborhood;
            this.rules = rules;
        }
    }

    /// <summary>
    /// Generic class for running CellularAutomata based on rules that can be passed in to the class.
    /// </summary>
    public class CASimulator
    {
        /// <summary>
        /// An array of 2 states (double buffering). One is current (active) and the other is the "next one".
        /// </summary>

        private bool[][,] _states = new bool[][,] { null, null };
        /// <summary>
        /// Specifies which internal state is the active (current) one.
        /// </summary>
        private int activeState = 0;

        public bool[,] State
        {
            get => _states[activeState];
        }
        /// <summary>
        /// The spec that defines the neighborhood definition and rules for the simulation.
        /// </summary>
        protected CASpec spec;

        /// <summary>
        /// Create a new simulation by specifying the width, height and spec
        /// </summary>
        /// <param name="width">The width of the grid</param>
        /// <param name="height">The height of the grid</param>
        /// <param name="spec">The struct specifying the neighorhood and rules for the simulation</param>
        public CASimulator(uint width, uint height, CASpec spec)
        {
            this.spec = spec;
            SetState(new bool[width, height]);
            ValidateSpec(spec);
        }

        /// <summary>
        /// Create a new simulation by specifying the initial state and spec
        /// </summary>
        /// <param name="state">The initial stateof the simulation/param>
        /// <param name="spec">The struct specifying the neighorhood and rules for the simulation</param>
        public CASimulator(bool[,] state, CASpec spec)
        {
            this.spec = spec;
            SetState(state);
            ValidateSpec(spec);
        }

        /// <summary>
        /// Validates the passed in spec
        /// </summary>
        /// <param name="spec">The spec to validate</param>
        protected void ValidateSpec(CASpec spec)
        {
            if (spec.neighborhood.GetLength(0) % 2 != 1 || spec.neighborhood.GetLength(1) % 2 != 1)
            {
                throw new ArgumentException("CASpec.neighborhood length must be an odd number.");
            }
            if (spec.rules.Length != 2)
            {
                throw new ArgumentException("CASpec.rules should have a length of 2.");
            }

            int maxNeighbors = 0;
            foreach (int i in spec.neighborhood)
            {
                maxNeighbors += i;
            }
            if (spec.rules[0].Length < maxNeighbors || spec.rules[1].Length < maxNeighbors)
            {
                throw new ArgumentException("Subarrays of CASpec.rules should have a length of at least the maximum number of live neighbors defined in spec.neighbourhood.");
            }
        }

        /// <summary>
        /// Returns whether the cell at the coordinates is live or not.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <returns>true for a live cell, false for dead</returns>
        public bool GetState(uint x, uint y)
        {
            return _states[activeState][x, y];
        }

        /// <summary>
        /// Returns the full state grid
        /// </summary>
        /// <returns>The full state</returns>
        public bool[,] GetState()
        {
            return _states[activeState];
        }

        /// <summary>
        /// Returns the full state grid
        /// </summary>
        /// <param name="active">A flag indicating if we are looking for the currently active or inactive state</param>
        /// <returns></returns>
        protected bool[,] GetState(bool active)
        {
            return _states[(activeState + (active ? 0 : 1)) % 2];
        }

        /// <summary>
        /// Overwrites the current active state of the simulation
        /// </summary>
        /// <param name="state">The new state</param>
        public void SetState(bool[,] state)
        {
            _states[activeState] = state;
            _states[(activeState + 1) % 2] = new bool[state.GetLength(0), state.GetLength(1)];
        }

        /// <summary>
        /// Sets the value of an individual cell in the active state
        /// </summary>
        /// <param name="x">X coordinate of the cell</param>
        /// <param name="y">Y coordinate of the cell</param>
        /// <param name="state">The value of the cell</param>
        public void SetState(uint x, uint y, bool state)
        {
            _states[activeState][x, y] = state;
        }

        /// <summary>
        /// Resets all cells to dead
        /// </summary>
        public void Clear()
        {
            int stateWidth = State.GetLength(0);
            int stateHeight = State.GetLength(1);
            for (uint x = 0; x < stateWidth; x++)
            {
                for (uint y = 0; y < stateHeight; y++)
                {
                    SetState(x, y, false);
                }
            }

        }

        /// <summary>
        /// Simulates the next step of the automata.
        /// Also flips the states.
        /// </summary>
        public void step()
        {
            bool[,] oldState = GetState(true);
            bool[,] newState = GetState(false);
            int neighborhoodRangeX = (spec.neighborhood.GetLength(0) - 1) / 2;
            int neighborhoodRangeY = (spec.neighborhood.GetLength(1) - 1) / 2;
            int stateWidth = oldState.GetLength(0);
            int stateHeight = oldState.GetLength(1);

            for (uint x = 0; x < stateWidth; x++)
            {
                for (uint y = 0; y < stateHeight; y++)
                {
                    int liveNeighborCount = 0;
                    for (int dx = -neighborhoodRangeX; dx <= neighborhoodRangeX; dx++)
                    {
                        for (int dy = -neighborhoodRangeY; dy <= neighborhoodRangeY; dy++)
                        {
                            // Detect live cells and multiply them by the neighborhood mask.
                            // stateWith and stateHeight are added to the indexes to compensate for C#'s behaviour of -1 % 40 = -1
                            liveNeighborCount += (oldState[(x + dx + stateWidth) % stateWidth, (y + dy + stateHeight) % stateHeight] ? 1 : 0) * spec.neighborhood[dx + neighborhoodRangeX, dy + neighborhoodRangeY];
                        }
                    }
                    newState[x, y] = spec.rules[oldState[x, y] ? 1 : 0][liveNeighborCount] > 0;
                }
            }
            activeState = (activeState + 1) % 2;
        }
    }
}
