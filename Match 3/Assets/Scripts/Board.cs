using UnityEngine;

public class Board : MonoBehaviour {
    public int width;
    public int height;
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
    }

    // Method to destroy the matched dots at the given column and row, if they are marked as matched
    private void DestroyMatchesAt(int column, int row) { 
        if (allDots[column, row].GetComponent<Dot>().isMatched) {
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }
}
