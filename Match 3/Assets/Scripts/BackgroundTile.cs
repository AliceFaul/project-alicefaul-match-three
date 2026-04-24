using UnityEngine;

public class BackgroundTile : MonoBehaviour {
    public GameObject[] dots; // Array of possible dot prefabs to spawn on this tile

    private void Start() {
        Initialize(); // Call the Initialize method to set up the tile when it starts
    }

    // This method is called to initialize the tile, spawning a random dot on it
    public void Initialize() { 
        int dotToUse = Random.Range(0, dots.Length); // Randomly select a dot prefab from the array
        GameObject dot = Instantiate(dots[dotToUse], transform.position, Quaternion.identity) as GameObject; // Instantiate the selected dot at the tile's position
        dot.transform.parent = this.transform; // Set the parent of the dot to be the tile for better organization in the hierarchy
        dot.name = this.gameObject.name; // Name the dot the same as the tile for easier identification in the hierarchy
    }
}
