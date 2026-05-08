using System.Collections;
using UnityEngine;

public enum BombType {
    Column,
    Row,
    None
}

public class Dot : MonoBehaviour {
    [Header("Board Variales")]
    public int row;
    public int column;
    public int previousRow;
    public int previousColumn;

    public int targetX;
    public int targetY;
    public Color dotColor;

    public bool isMatched = false;

    // === Private variables ===
    private GameObject _otherDot;
    private Board _board;
    private MatchFinder _matchFinder;

    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    private Vector2 _tempPosition; // Temporary variable to store the position of the dot while it is moving

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f; // Minimum distance the swipe must cover to be registered as a valid swipe

    [Header("Powerup Stuff")]
    public BombType bombType; // Enum to specify the type of bomb (column or row)
    private SpriteRenderer _renderer;
    private MaterialPropertyBlock _mpb;

    // Initialize
    private void Start() {
        _board = FindFirstObjectByType<Board>();
        _matchFinder = FindFirstObjectByType<MatchFinder>();
        _renderer = GetComponent<SpriteRenderer>();
        _mpb = new MaterialPropertyBlock();
        ClearBomb();
    }

    private void Update() {
        targetX = column;
        targetY = row;
        if(Mathf.Abs(targetX - transform.position.x) > .1) {
            // If the targetX is more than 0.1 units away from the current x position of the dot,
            // move the dot towards the targetX position
            _tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, _tempPosition, 20f * Time.deltaTime);
            if(_board.allDots[column, row] != this.gameObject) {
                _board.allDots[column, row] = this.gameObject;
            }
            _matchFinder.FindAllMatches(); // Check for matches while the dot is moving towards its target position
        } else {
            // If the targetX is within 0.1 units of the current x position,
            // snap the dot to the targetX position
            _tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = _tempPosition;
        }
        
        if(Mathf.Abs(targetY - transform.position.y) > .1) {
            _tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, _tempPosition, 20f * Time.deltaTime);
            if(_board.allDots[column, row] != this.gameObject) {
                _board.allDots[column, row] = this.gameObject;
            }
            _matchFinder.FindAllMatches(); // Check for matches while the dot is moving towards its target position
        } else {
            _tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = _tempPosition;
        }
    }

    // Testing and debug bomb features
    private void OnMouseOver() {
        if(Input.GetMouseButtonDown(0)) { 
            MakeBomb(BombType.Row);
        }
    }

    private void OnMouseDown() {
        if (_board.currentState == GameState.Move) {
            // Convert the mouse position to world coordinates and store it as the first touch position
            _firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp() {
        if (_board.currentState == GameState.Move) {
            // Convert the mouse position to world coordinates and store it as the final touch position
            _finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private void CalculateAngle() {
        if (Mathf.Abs(_finalTouchPosition.y - _firstTouchPosition.y) > swipeResist ||
            Mathf.Abs(_finalTouchPosition.x - _firstTouchPosition.x) > swipeResist)
        {
            // Calculate the angle of the swipe using the arctangent of the difference in
            // y and x coordinates between the final and first touch positions,
            // and convert it from radians to degrees
            swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y,
                _finalTouchPosition.x - _firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces(); // Call the MovePieces method to move the dots based on the calculated swipe angle
            if(_otherDot != null) {
                _board.currentState = GameState.Wait;
            }
        } else {
            // If the swipe does not meet the minimum distance requirement, reset the swipe angle and return to the Move state
            _board.currentState = GameState.Move;
        }
    }

    private void MovePieces() {
        previousRow = row;
        previousColumn = column;
        if (swipeAngle > -45 && swipeAngle <= 45 && column < _board.width - 1) {
            // Right swipe
            _otherDot = _board.allDots[column + 1, row];
            _otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
            Debug.Log($"Right swipe: column {column}, row {row}");
        } else if(swipeAngle > 45 && swipeAngle <= 135 && row < _board.height - 1) {
            // Up swipe
            _otherDot = _board.allDots[column, row + 1];
            _otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
            Debug.Log($"Up swipe: column {column}, row {row}");
        } else if((swipeAngle < -45 && swipeAngle >= -135) && row > 0) {
            // Down swipe
            _otherDot = _board.allDots[column, row - 1];
            _otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
            Debug.Log($"Down swipe: column {column}, row {row}");
        } else if((swipeAngle > 135 || swipeAngle <= -135) && column > 0) {
            // Left swipe
            _otherDot = _board.allDots[column - 1, row];
            _otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
            Debug.Log($"Left swipe: column {column}, row {row}");
        }
        StartCoroutine(CheckMoveCo()); // Check if not matched, snap back to original pos
    }

    private IEnumerator CheckMoveCo() {
        yield return new WaitForSeconds(.25f);
        if (_otherDot != null) {
            // Check if this dot or the other dot has a match after the move,
            // and if not, swap them back to their original positions
            if (!isMatched && !_otherDot.GetComponent<Dot>().isMatched) { 
                _otherDot.GetComponent<Dot>().row = row;
                _otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(.5f);
                _board.currentState = GameState.Move;
            } else {
                _board.DestroyMatches();
            }
            _otherDot = null;
        }
    }

    // Method to make this dot a bomb of the specified type (column or row), which will affect the way it is destroyed and the matches it creates
    // Set the appropriate properties and visual effects for the bomb based on its type
    private void MakeBomb(BombType type) {
        bombType = type;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_BombBlend", 1f);
        switch(type) {
            case BombType.Column:
                _mpb.SetVector("_Angle", new Vector2(4, 0));
                break;
            case BombType.Row:
                _mpb.SetVector("_Angle", new Vector2(0, 4));
                break;
        }
        _renderer.SetPropertyBlock(_mpb);
    }

    // Helper method to clear the bomb properties
    // and visual effects from this dot, resetting it to a normal state
    private void ClearBomb() { 
        bombType = BombType.None;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_BombBlend", 0f);
        _renderer.SetPropertyBlock(_mpb);
    }

    //private void FindMatches() { 
    //    // Horizontal match check
    //    if(column > 0 && column < _board.width - 1) { 
    //        GameObject leftDot1 = _board.allDots[column - 1, row];
    //        GameObject rightDot1 = _board.allDots[column + 1, row];
    //        // Check if the left and right dots are not null and have the same tag as this dot,
    //        // which indicates a match
    //        if(leftDot1 != null && rightDot1 != null && leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag) { 
    //            leftDot1.GetComponent<Dot>().isMatched = true;
    //            rightDot1.GetComponent<Dot>().isMatched = true;
    //            isMatched = true;
    //        }
    //    }
    //    // Vertical match check
    //    if(row > 0 && row < _board.height - 1) {
    //        GameObject upDot1 = _board.allDots[column, row + 1];
    //        GameObject downDot1 = _board.allDots[column, row - 1];
    //        // Check if the up and down dots are not null and have the same tag as this dot,
    //        // which indicates a match
    //        if (upDot1 != null && downDot1 != null && upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag) {
    //            upDot1.GetComponent<Dot>().isMatched = true;
    //            downDot1.GetComponent<Dot>().isMatched = true;
    //            isMatched = true;
    //        }
    //    }
    //}
}
