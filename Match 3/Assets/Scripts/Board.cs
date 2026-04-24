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
                GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity) as GameObject; // Instantiate the selected dot at the tile's position
                dot.transform.parent = this.transform; // Set the parent of the dot to be the tile for better organization in the hierarchy
                dot.name = $"Dot {i} {j}"; // Name the dot for easier identification in the hierarchy
                allDots[i, j] = dot; // Store the reference to the instantiated dot in the _allDots array
            }
        }
    }
}
