using System.Collections;
using UnityEditor.Rendering;
using UnityEngine;

public class Dot : MonoBehaviour {
    [Header("Board Variales")]
    public int row;
    public int column;
    public int previousRow;
    public int previousColumn;

    public int targetX;
    public int targetY;

    public bool isMatched = false;

    // === Private variables ===
    private GameObject _otherDot;
    private Board _board;

    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    private Vector2 _tempPosition; // Temporary variable to store the position of the dot while it is moving
    public float swipeAngle = 0;

    // Initialize
    private void Start() {
        _board = FindFirstObjectByType<Board>();
        // Set the targetX and targetY to the current position of the dot,
        // which will be used for movement and matching logic
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        row = targetY;
        column = targetX;
        previousRow = row;
        previousColumn = column;
    }

    private void Update() {
        FindMatches(); // Call the FindMatches method to check for matches and update the isMatched property accordingly
        if (isMatched) { 
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if(spriteRenderer != null) { 
                spriteRenderer.color = new Color(1f, 1f, 1f, .5f); // Set the color of the dot to be semi-transparent to indicate it is matched
            }
        }
        targetX = column;
        targetY = row;
        if(Mathf.Abs(targetX - transform.position.x) > .1) {
            // If the targetX is more than 0.1 units away from the current x position of the dot,
            // move the dot towards the targetX position
            _tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, _tempPosition, 20f * Time.deltaTime);
        }
        else {
            // If the targetX is within 0.1 units of the current x position,
            // snap the dot to the targetX position
            _tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = _tempPosition;
            // Update the reference to this dot in the _board's allDots array to reflect its new position
            _board.allDots[column, row] = this.gameObject;
        }
        
        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            _tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, _tempPosition, 20f * Time.deltaTime);
        }
        else
        {
            _tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = _tempPosition;
            _board.allDots[column, row] = this.gameObject;
        }
    }

    private void OnMouseDown() {
        // Convert the mouse position to world coordinates and store it as the first touch position
        _firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Debug.Log(_firstTouchPosition);
    }

    private void OnMouseUp() {
        // Convert the mouse position to world coordinates and store it as the final touch position
        _finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Debug.Log(_finalTouchPosition);
        CalculateAngle();
    }

    private void CalculateAngle() {
        // Calculate the angle of the swipe using the arctangent of the difference in
        // y and x coordinates between the final and first touch positions,
        // and convert it from radians to degrees
        swipeAngle = Mathf.Atan2(_finalTouchPosition.y - _firstTouchPosition.y, 
            _finalTouchPosition.x - _firstTouchPosition.x) * 180 / Mathf.PI;
        Debug.Log(swipeAngle);
        MovePieces(); // Call the MovePieces method to move the dots based on the calculated swipe angle
    }

    private void MovePieces() { 
        if(swipeAngle > -45 && swipeAngle <= 45 && column < _board.width - 1) {
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
        yield return new WaitForSeconds(.2f);
        if(_otherDot != null) {
            // Check if this dot or the other dot has a match after the move,
            // and if not, swap them back to their original positions
            if (!isMatched && !_otherDot.GetComponent<Dot>().isMatched) { 
                _otherDot.GetComponent<Dot>().row = row;
                _otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
            }
            _otherDot = null;
        }
    }

    private void FindMatches() { 
        // Horizontal match check
        if(column > 0 && column < _board.width - 1) { 
            GameObject leftDot1 = _board.allDots[column - 1, row];
            GameObject rightDot1 = _board.allDots[column + 1, row];
            // Check if the left and right dots are not null and have the same tag as this dot,
            // which indicates a match
            if (leftDot1 != null && rightDot1 != null && leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag) { 
                leftDot1.GetComponent<Dot>().isMatched = true;
                rightDot1.GetComponent<Dot>().isMatched = true;
                isMatched = true;
            }
        }
        // Vertical match check
        if (row > 0 && row < _board.height - 1) {
            GameObject upDot1 = _board.allDots[column, row + 1];
            GameObject downDot1 = _board.allDots[column, row - 1];
            // Check if the up and down dots are not null and have the same tag as this dot,
            // which indicates a match
            if (upDot1 != null && downDot1 != null && upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag) {
                upDot1.GetComponent<Dot>().isMatched = true;
                downDot1.GetComponent<Dot>().isMatched = true;
                isMatched = true;
            }
        }
    }
}
