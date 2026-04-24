using UnityEngine;

public class Dot : MonoBehaviour {
    private Vector2 _firstTouchPosition;
    private Vector2 _finalTouchPosition;
    public float swipeAngle = 0;

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
    }
}
