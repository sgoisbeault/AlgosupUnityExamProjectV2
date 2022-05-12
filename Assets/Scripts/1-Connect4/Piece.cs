using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    // Width of a column in the board
    private const float COLUMN_WIDTH = 1f;

    // Owner of that piece
    public Connect4Game.Owner owner;
    // Column in which this piece will be or has been released to
    public int column = 3;

    // Reference to the attached rigidbody
    private Rigidbody rigid;

    // Has this piece been released yet?
    private bool isReleased = false;

    // Has this piece reached its final position in the board (bottom of the board or above another piece)
    private bool hasReachedFinalPlace = false;


    void Awake()
    {
        // Get rigidbody component
        rigid = GetComponent<Rigidbody>();
    }

    // Can the user move this piece with arrow keys?
    public bool canMove()
    {
        // The user can move this piece if and only if he owns it and did not released it yet
        return owner == Connect4Game.Owner.PLAYER && !isReleased;
    }

    // Move the piece above the next column on the right (if exists)
    public void MoveRight()
    {
        // Limit the column to 6 max
        column = Mathf.Min(column+1, 6);

        // New x position of the piece
        float newX = (column - 3) * COLUMN_WIDTH;

        // TODO: update piece position
    }

    // Move the piece above the next column on the left (if exists)
    public void MoveLeft()
    {
        // Limit the column to 0 min
        column = Mathf.Max(column - 1, 0);

        // New x position of the piece
        float newX = (column - 3) * COLUMN_WIDTH;

        // TODO: update piece position
    }

    // Release the piece so it drops in the column
    public void Release()
    {
        // Apply gravity to it and update isReleased state
        rigid.useGravity = true;
        isReleased = true;
    }

    // Tell that piece who owns it and update its visual appearence consequently
    public void setOwner(Connect4Game.Owner owner)
    {
        // Update owner
        this.owner = owner;

        // TODO: Assign the correct material to the piece renderer
    }

    // TODO: Detect When the piece reached its final place and trigger next turn
}
