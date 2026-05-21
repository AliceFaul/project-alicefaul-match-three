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

    //public int comboCounter = 0;
    //public bool isProcessing = false;

    public GameObject tilePrefab;
    public GameObject breakEffect;
    public GameObject[] dots; // Array of possible dot prefabs to spawn on the tiles

    private BackgroundTile[,] _allTiles;
    public Dot currentDot;
    public GameObject[,] allDots;
    private MatchFinder _matchFinder;

    private void Start() {
        _matchFinder = FindFirstObjectByType<MatchFinder>();
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

    //public IEnumerator ProcessMatchesCo() {
    //    if(isProcessing) {
    //        yield break; // Exit the coroutine if it is already processing matches to prevent multiple simultaneous executions
    //    }
    //    isProcessing = true; // Set the processing flag to true to indicate that the coroutine is currently processing matches
    //    comboCounter = 0;
    //    while(true) {
    //        _matchFinder.currentMatches.Clear();
    //        yield return StartCoroutine(_matchFinder.FindAllMatchesCo());
    //        if(_matchFinder.currentMatches.Count == 0) {
    //            comboCounter = 0;
    //            break; // Exit the coroutine if there are no matches found
    //        }
    //        comboCounter++;
    //        Debug.Log($"Combo count: {comboCounter}"); // Log the current combo count for debugging purposes
            
    //        DestroyMatches();
    //        yield return new WaitForSeconds(.1f);
    //        yield return StartCoroutine(DecreaseRowCo());
    //        yield return new WaitForSeconds(.1f);
    //        yield return StartCoroutine(FillBoardCo());
    //    }
    //    comboCounter = 0; // Reset the combo counter after processing all matches
    //    isProcessing = false;
    //    currentState = GameState.Move; // Set the game state back to Move after processing matches and updating the board
    //}

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
        _matchFinder.currentMatches.Clear(); // Clear the current matches list after processing all matches and updating the board
        StartCoroutine(DecreaseRowCo());
    }

    // Method to destroy the matched dots at the given column and row, if they are marked as matched
    private void DestroyMatchesAt(int column, int row) { 
        if(allDots[column, row].GetComponent<Dot>().isMatched) {
            // Check how many elements in the current matches list
            if(_matchFinder.currentMatches.Count == 4 || _matchFinder.currentMatches.Count == 7) {
                _matchFinder.CheckBombs();
            }

            // Instantiate the break effect at the position of the matched dot
            var position = allDots[column, row].transform.position;
            position.z = 0f; // Set the z position of the break effect to be in front of the dots for better visibility
            GameObject effect = Instantiate(
                breakEffect, 
                position, 
                Quaternion.identity) as GameObject;
            //float scale = 1f + comboCounter * .2f;
            //effect.transform.localScale = Vector3.one * scale;

            var color = allDots[column, row].GetComponent<Dot>().dotColor; // Get the color of the matched dot
            var particle = effect?.GetComponent<ParticleSystem>();
            if(particle != null) { 
                var main = particle.main;
                main.startColor = color;
                particle.Play();
            }
            
            Destroy(effect, 1f); // Destroy the break effect after a short delay
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    private IEnumerator DecreaseRowCo() {
        yield return new WaitForSeconds(.2f); // Short delay to allow the break effect to play
        // int nullCount = 0;
        for(int i = 0; i < width; i++) { 
            int nullCount = 0; // Reset the null count for each column
            for (int j = 0; j < height; j++) {
                if(allDots[i, j] == null) { 
                    nullCount++;
                } else if(nullCount > 0) {
                    // Collapse the dots above the null positions
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    // allDots[i, j - nullCount] = allDots[i, j];
                    allDots[i, j] = null;
                }
            }
        }
        yield return new WaitForSeconds(.2f);
        // After collapsing the dots, fill the board with new dots and check for any new matches then destroy them accordingly
        StartCoroutine(FillBoardCo());
    }

    // Coroutine to fill the board with new dots after the matched dots have been destroyed
    // and check for any new matches that may have been created by the new dots
    private IEnumerator FillBoardCo() {
        RefillBoard();
        yield return new WaitForSeconds(.2f);
        // After refilling the board, check for any new matches that may have been created
        // by the new dots and destroy them accordingly
        while (MatchesOnBoard()) {
            yield return new WaitForSeconds(.1f);
            DestroyMatches();
        }
        _matchFinder.currentMatches.Clear(); // Clear the current matches list after processing all matches and updating the board
        currentDot = null; // Reset the current dot
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
