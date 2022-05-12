using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static reference to the unique instance of that class (GetComponent<GameManager> or FindObjectOfType<GameManager> can now be replaced by GameManager.instance)
    public static GameManager instance;

    // Constant value representing the delay between each AI move or release simulated actions
    public const float AI_BETWEEN_ACTIONS_DELAY = 0.2f;

    // Materials to be used by pieces to visually differentiate player and AI ones
    public Material AIPieceMaterial;
    public Material playerPieceMaterial;

    // Piece prefab
    public GameObject piecePrefab;

    // Parent of all instantiated pieces
    public Transform piecesContainer;

    // Final UI elements
    public GameObject FinalUI;
    public Text finalUIText;

    // Connect4 game logic
    public Connect4Game game;

    // Used to know whether the game is actually running or not (ended)
    private bool isGameRunning = false;

    // Reference to the piece being used in the current turn (moving/releasing above the board)
    private Piece currentPiece;


    void Start()
    {
        // Initialize the reference to the unique instance
        instance = this;

        // Initialize a new game at start
        resetGame();
    }

    // Initialize a new game
    public void resetGame()
    {
        // Hide UI and destroy all pieces
        FinalUI.SetActive(false);
        foreach (Transform t in piecesContainer)
            Destroy(t.gameObject);

        // Create a new game and listener for end of game event (OnGameFinished funtion will be called when OnFinished event is triggered)
        game = new Connect4Game();
        game.OnFinished.AddListener(OnGameFinished);
        isGameRunning = true;

        // Start the game with a player turn
        NewTurn(true);
    }

    // Start a new turn
    void NewTurn(bool playerTurn)
    {
        // Create a new piece and update its ownership (for logic and visual)
        currentPiece = Instantiate(piecePrefab, piecesContainer).GetComponent<Piece>();
        currentPiece.setOwner(playerTurn? Connect4Game.Owner.PLAYER : Connect4Game.Owner.AI);

        // If it's AI turn, select a random column from the avaialble ones (not very strategic AI) and simulate its actions
        if (!playerTurn)
        {
            List<int> availablesColumns = game.getAvailableColumns();
            int randomColumn = availablesColumns[Random.Range(0, availablesColumns.Count)];
            StartCoroutine(simulateAITurn(randomColumn));
        }
    }

    private void Update()
    {
        // Don't do anything here is game is not running
        if (!isGameRunning)
            return;

        // If the currentPiece exists and the user can interact with it
        if (currentPiece && currentPiece.canMove())
        {
            // Move it with arrow keys
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                currentPiece.MoveLeft();

            if (Input.GetKeyDown(KeyCode.RightArrow))
                currentPiece.MoveRight();

            // and release it if above available column with space bar
            if (Input.GetKeyDown(KeyCode.Space) && game.isAvailableColumn(currentPiece.column))
            {
                // Inform the game logic that a new piece has been added
                game.addPieceInColumn(currentPiece.owner, currentPiece.column);
                currentPiece.Release();
            }
        }

        // TO REMOVE when next turn is triggered by a piece reaching its final place
        // Temporary going to next turn when pressing tab
        if (Input.GetKeyDown(KeyCode.Tab))
            NextTurn();
    }

    // Start the next turn
    public void NextTurn()
    {
        // If game is ended, don't do anything
        if (!isGameRunning)
            return;

        // Start a new turn for the other player than the one that just played
        NewTurn(currentPiece.owner == Connect4Game.Owner.AI);
    }

    // Called when the game just ended
    public void OnGameFinished(Connect4Game.Owner winner)
    {
        // Show the final UI with the updated text (depending on who won)
        // and update the isGameRunning state
        switch(winner)
        {
            case Connect4Game.Owner.NONE:
                finalUIText.text = "That's a draw!";
                break;
            case Connect4Game.Owner.PLAYER:
                finalUIText.text = "You won!";
                break;
            case Connect4Game.Owner.AI:
                finalUIText.text = "You lost!";
                break;

        }
        FinalUI.SetActive(true);
        isGameRunning = false;
    }

    // Called when the Replay button is pressed (on the final UI)
    public void OnReplay()
    {
        // Start a new game
        resetGame();
    }

    // Simulate AI actions according to the selected column
    private IEnumerator simulateAITurn(int column)
    {
        // Every AI_BETWEEN_ACTIONS_DELAY seconds, move by one column towards the selected column
        while (currentPiece.column > column)
        {
            yield return new WaitForSeconds(AI_BETWEEN_ACTIONS_DELAY);
            currentPiece.MoveLeft();
        }
        while (currentPiece.column < column)
        {
            yield return new WaitForSeconds(AI_BETWEEN_ACTIONS_DELAY);
            currentPiece.MoveRight();
        }

        // Once we're above the selected column, wait AI_BETWEEN_ACTIONS_DELAY and release the piece and inform the game logic that a new piece has been added
        yield return new WaitForSeconds(AI_BETWEEN_ACTIONS_DELAY);
        game.addPieceInColumn(currentPiece.owner, currentPiece.column);
        currentPiece.Release();
    }
}
