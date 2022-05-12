using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Class representing a connect4 game
public class Connect4Game
{
    // Owner of a piece or position in the grid
    public enum Owner
    {
        NONE,
        PLAYER,
        AI
    }

    // List of possible directions
    private enum Direction
    {
        TOP,
        BOTTOM,
        RIGHT,
        LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT,
        TOP_LEFT
    }
    // Dictionnary used for conversion from direction name to direction value (used for alignment detection)
    private static Dictionary<Direction, int[]> directions = new Dictionary<Direction, int[]> { 
        { Direction.TOP,            new int[2] {  0,  1 } },
        { Direction.BOTTOM,         new int[2] {  0, -1 } },
        { Direction.RIGHT,          new int[2] {  1,  0 } },
        { Direction.LEFT,           new int[2] { -1,  0 } },
        { Direction.TOP_RIGHT,      new int[2] {  1,  1 } },
        { Direction.BOTTOM_RIGHT,   new int[2] {  1, -1 } },
        { Direction.BOTTOM_LEFT,    new int[2] { -1, -1 } },
        { Direction.TOP_LEFT,       new int[2] { -1,  1 } },};


    // Event triggered when the game just ended, either when a player won or the grid is full (draw)
    public UnityEvent<Owner> OnFinished = new UnityEvent<Owner>();

    // The 7x6 grid storing current pieces in the game
    private Owner[,] grid = new Owner[7,6];

    // Constructor
    public Connect4Game()
    {
        // Initialize the grid full of NONE (empty grid)
        grid = new Owner[7, 6];
    }

    // Function answering the question: can we put another piece on this column?
    public bool isAvailableColumn(int column)
    {
        // A column is avaialble if and only if the top position of the column is empty
        return grid[column, 5] == Owner.NONE;
    }

    // Add a piece to the grid in the specified column
    public bool addPieceInColumn(Owner pieceOwner, int column)
    {
        // Don't add if column full
        if (!isAvailableColumn(column))
            return false;

        // Trying to find the lowest avaialble position in that column to add the piece
        for (int i = 0; i<6; i++)
        {
            if (grid[column, i] == Owner.NONE)
            {
                grid[column, i] = pieceOwner;
                break;
            }
        }

        // Once the piece is added, check if a player won or if the grid is full
        Owner winner = checkWin();
        if (winner != Owner.NONE || isGridFull())
            OnFinished?.Invoke(winner);

        return true;
    }

    // Checking if the grid is full (no more NONE position)
    private bool isGridFull()
    {
        // If the top position of the all columns are not empty, the grid is full
        for (int i = 0; i < 7; i++)
            if (isAvailableColumn(i))
                return false;

        return true;
    }

    // Return the list of all not full columns
    public List<int> getAvailableColumns()
    {
        List<int> columns = new List<int>();

        for (int i = 0; i < 7; i++)
            if (isAvailableColumn(i))
                columns.Add(i);

        return columns;
    }

    // Detect if a player is winning in the current state of the game
    public Owner checkWin()
    {
        /*
         * Any alignment of 4 pieces is crossing the middle column or the 3rd (or 4th) line.
         * 0 0 0 X 0 0 0
         * 0 0 0 X 0 0 0
         * X X X X X X X
         * 0 0 0 X 0 0 0
         * 0 0 0 X 0 0 0
         * 0 0 0 X 0 0 0
         * 
         * Checking alignments only from these positions is then enough to cover all cases
         * 
         */

        // For each position in the 3rd line,
        for (int i = 0; i<7; i++)
        {
            // If not owned by a player, no alignment crossing it, go to next one
            Owner owner = grid[i, 3];
            if (owner == Owner.NONE)
                continue;

            // Else, count the number of aligned piece with same owner in each direction (vertical, horizontal, diagonals)
            // which is the sum of counts in each way (TOP and BOTTOM for vertical direction for example) + 1 (the actual position)
            // If 4 or more pieces with same owner found, the owner won

            int[] coord = new int[2] { i, 3 };
            int total = 1 + getAlignedCount(owner, coord, Direction.TOP, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.LEFT, 1);
            total += getAlignedCount(owner, coord, Direction.RIGHT, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.TOP_RIGHT, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM_LEFT, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.TOP_LEFT, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM_RIGHT, 1);

            if (total >= 4)
                return owner;
        }

        // Same thing as above but for middle column (third one)
        for (int i = 0; i < 6; i++)
        {
            // Skip [3,3] as we already checked it in the previous loop
            if (i == 3)
                continue;

            Owner owner = grid[3, i];
            if (owner == Owner.NONE)
                continue;

            int[] coord = new int[2] { 3, i };
            int total = 1 + getAlignedCount(owner, coord, Direction.TOP, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.LEFT, 1);
            total += getAlignedCount(owner, coord, Direction.RIGHT, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.TOP_RIGHT, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM_LEFT, 1);

            if (total >= 4)
                return owner;

            total = 1 + getAlignedCount(owner, coord, Direction.TOP_LEFT, 1);
            total += getAlignedCount(owner, coord, Direction.BOTTOM_RIGHT, 1);

            if (total >= 4)
                return owner;
        }

        // If no return call made before reaching this line (i.e. no alignment found), no winner yet
        return Owner.NONE;
    }

    // Recursively get the count of pieces aligned in the same direction
    private int getAlignedCount(Owner owner, int[] coord, Direction dir, int acc)
    {
        // If 4 pieces has been found aligned, stop here
        if (acc > 4)
            return 0;

        // Updating the new X coordinate following the direction,
        // If out of grid, stop here
        int x = coord[0] + directions[dir][0];
        if (x< 0 || x > 6)
            return 0;

        // Updating the new Y coordinate following the direction,
        // If out of grid, stop here
        int y = coord[1] + directions[dir][1];
        if (y < 0 || y > 5)
            return 0;

        // If owner is different than before, stop here
        if (grid[x, y] != owner)
            return 0;

        // Else count the next aligned pieces going forward in the same direction
        return 1 + getAlignedCount(owner, new int[2] { x, y }, dir, acc + 1);
    }
}
