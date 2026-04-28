using System.Collections;
using UnityEngine;

// Simple enum to represent the current state of the game
public enum GameState { 
    Wait, // The game is waiting for the player to make a move or for the board to finish updating after a move
    Move // The player is currently making a move, such as swapping two dots or selecting a dot to move
}

public class Board : MonoBehaviour {
    public GameState currentState = GameState.Move;
    
    public int width;
    public int height;
    public int offSet;

    public GameObject tilePrefab;
    public GameObject[] dots; // Array of possible dot prefabs to spawn on the tiles

    private BackgroundTile[,] _allTiles;
    public GameObject[,] allDots;

    private void Start() {
        _allTiles = new BackgroundTile[width, height];
        allDots = new GameObject[width, height];
        Setup();
    }

    private void Setup() { 
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) { 
                Vector2 tempPosition = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform; // Set the parent of the tile to be the Board object
                backgroundTile.name = $"Tile {i} {j}"; // Name the tile for easier identification in the hierarchy
                int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab from the array
                int maxIterations = 0;
                while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) {
                    // If the randomly selected dot creates a match of three or more in a row or column,
                    // select another dot until a non-matching dot is found
                    dotToUse = Random.Range(0, dots.Length);
                    maxIterations++;
                }
                maxIterations = 0;
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity) as GameObject; // Instantiate the selected dot at the tile's position
                dot.GetComponent<Dot>().row = j;
                dot.GetComponent<Dot>().column = i;
                dot.transform.parent = this.transform; // Set the parent of the dot to be the tile for better organization in the hierarchy
                dot.name = $"Dot {i} {j}"; // Name the dot for easier identification in the hierarchy
                allDots[i, j] = dot; // Store the reference to the instantiated dot in the _allDots array
            }
        }
    }



    // Method to check if there are any matches of three or more dots in a row or column, starting from the given column and row
    private bool MatchesAt(int column, int row, GameObject piece) {
        if (column > 1 && row > 1) {
            if (allDots[column - 1, row] != null && allDots[column - 2, row] != null &&
                allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag)
            {
                return true;
            }
            if (allDots[column, row - 1] != null && allDots[column, row - 2] != null &&
                allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag)
            {
                return true;
            }
        } else if(column <= 1 || row <= 1) { 
            if(row > 1) {
                if(allDots[column, row - 1].tag == piece.tag && allDots[column, row - 2].tag == piece.tag) {
                    return true;
                }
            }
            if (column > 1) {
                if (allDots[column - 1, row].tag == piece.tag && allDots[column - 2, row].tag == piece.tag) {
                    return true;
                }
            }
        }

        return false;
    }

    // Method to destroy all the matched dots on the board,
    // which will be called after checking for matches and marking the matched dots accordingly
    public void DestroyMatches() { 
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    // Destroy the matched dots at the current column and row, if they are marked as matched
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    // Method to destroy the matched dots at the given column and row, if they are marked as matched
    private void DestroyMatchesAt(int column, int row) { 
        if (allDots[column, row].GetComponent<Dot>().isMatched) {
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    private IEnumerator DecreaseRowCo() {
        int nullCount = 0;
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] == null) { 
                    nullCount++;
                } else if(nullCount > 0) {
                    // Collapse the dots above the null positions
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(.4f);
        // After collapsing the dots, fill the board with new dots and check for any new matches then destroy them accordingly
        StartCoroutine(FillBoardCo());
    }

    // Coroutine to fill the board with new dots after the matched dots have been destroyed
    // and check for any new matches that may have been created by the new dots
    private IEnumerator FillBoardCo() {
        RefillBoard();
        yield return new WaitForSeconds(.5f);
        // After refilling the board, check for any new matches that may have been created
        // by the new dots and destroy them accordingly
        while (MatchesOnBoard()) { 
            yield return new WaitForSeconds(.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(.5f);
        currentState = GameState.Move;
    }

    // Helper method to refill the board with new dots after the matched dots have been destroyed
    private void RefillBoard() { 
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] == null) { 
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity) as GameObject; // Create a new dot at the null position
                    dot.transform.parent = this.transform;
                    dot.name = $"Dot {i} {j}";
                    allDots[i, j] = dot; // Store the reference to the new dot in the _allDots array
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    // Helper method to check if there are any matches of three in row or column on the board
    // after refilling the board with new dots
    private bool MatchesOnBoard() { 
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    if(allDots[i, j].GetComponent<Dot>().isMatched) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
