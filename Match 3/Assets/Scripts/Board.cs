using UnityEngine;

public class Board : MonoBehaviour {
    public int width;
    public int height;
    public GameObject tilePrefab;
    
    private BackgroundTile[,] _allTiles;

    private void Start() {
        _allTiles = new BackgroundTile[width, height];
        Setup();
    }

    private void Setup() { 
        for(int i = 0; i < width; i++) { 
            for(int j = 0; j < height; j++) { 
                Vector2 tempPosition = new Vector2(i, j);
                GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
                backgroundTile.transform.parent = this.transform; // Set the parent of the tile to be the Board object
                backgroundTile.name = $"Tile {i} {j}"; // Name the tile for easier identification in the hierarchy
            }
        }
    }
}
