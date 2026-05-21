using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MatchFinder : MonoBehaviour {
    private Board _board;
    public List<GameObject> currentMatches = new();

    private void Start() {
        _board = FindFirstObjectByType<Board>();
    }

    public void FindAllMatches() {
        StartCoroutine(FindAllMachesCo());
    }

    public IEnumerator FindAllMatchesCo() { 
        yield return StartCoroutine(FindAllMachesCo());
    }

    private IEnumerator FindAllMachesCo() { 
        yield return new WaitForSeconds(.1f);
        for(int i = 0; i < _board.width; i++) { 
            for(int j = 0; j < _board.height; j++) { 
                GameObject currentDot = _board.allDots[i, j];
                var currentDotComponent = currentDot != null ? currentDot.GetComponent<Dot>() : null;
                
                if (currentDot != null) { 
                    if(i > 0 && i < _board.width - 1) { 
                        GameObject leftDot = _board.allDots[i - 1, j];
                        var leftDotComponent = leftDot != null ? leftDot.GetComponent<Dot>() : null;
                        GameObject rightDot = _board.allDots[i + 1, j];
                        var rightDotComponent = rightDot != null ? rightDot.GetComponent<Dot>() : null;

                        if (leftDot != null && rightDot != null) { 
                            if(leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) {
                                // If any of the three dots in the match is a row bomb, mark all pieces in that row as matched
                                currentMatches.Union(IsRowBomb(leftDotComponent, currentDotComponent, rightDotComponent));
                                // If any of the three dots in the match is a column bomb, mark all pieces in that column as matched
                                currentMatches.Union(IsColumnBomb(leftDotComponent, currentDotComponent, rightDotComponent));
                                GetNearbyPieces(leftDotComponent, currentDotComponent, rightDotComponent);
                            }
                        }
                    }

                    if(j > 0 && j < _board.height - 1) { 
                        GameObject upDot = _board.allDots[i, j + 1];
                        var upDotComponent = upDot != null ? upDot.GetComponent<Dot>() : null;
                        GameObject downDot = _board.allDots[i, j - 1];
                        var downDotComponent = downDot != null ? downDot.GetComponent<Dot>() : null;
                        
                        if (upDot != null && downDot != null) { 
                            if(upDot.tag == currentDot.tag && downDot.tag == currentDot.tag) {
                                // If any of the three dots in the match is a column bomb, mark all pieces in that column as matched
                                currentMatches.Union(IsColumnBomb(upDotComponent, currentDotComponent, downDotComponent));
                                // If any of the three dots in the match is a row bomb, mark all pieces in that row as matched
                                currentMatches.Union(IsRowBomb(upDotComponent, currentDotComponent, downDotComponent));
                                GetNearbyPieces(upDotComponent, currentDotComponent, downDotComponent);
                            }
                        }
                    }
                }
            }
        }
    }

    private void GetNearbyPieces(Dot dot1, Dot dot2, Dot dot3) { 
        AddToListAndMatch(dot1.gameObject);
        AddToListAndMatch(dot2.gameObject);
        AddToListAndMatch(dot3.gameObject);
    }

    private void AddToListAndMatch(GameObject dot) { 
        if(!currentMatches.Contains(dot)) { 
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    // Helper methods to get all pieces in a row if a bomb is present in the match
    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3) { 
        List<GameObject> currentDots = new();
        if(dot1.bombType == BombType.Row) {
            currentDots.Union(GetRowPieces(dot1.row));
        }
        if(dot2.bombType == BombType.Row) {
            currentDots.Union(GetRowPieces(dot2.row));
        }
        if(dot3.bombType == BombType.Row) {
            currentDots.Union(GetRowPieces(dot3.row));
        }
        return currentDots;
    }

    // Helper method to get all pieces in a column if a bomb is present in the match
    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3) { 
        List<GameObject> currentDots = new();
        if(dot1.bombType == BombType.Column) {
            currentDots.Union(GetColumnPieces(dot1.column));
        }
        if(dot2.bombType == BombType.Column) {
            currentDots.Union(GetColumnPieces(dot2.column));
        }
        if(dot3.bombType == BombType.Column) {
            currentDots.Union(GetColumnPieces(dot3.column));
        }
        return currentDots;
    }

    // === Powerup Methods ===
    public void MatchPiecesOfColor(string color) {
        for(int i = 0; i < _board.width; i++) { 
            for(int j = 0; j < _board.height; j++) {
                // check if the piece at this position is the same color as the specified color, and if so, mark it as matched
                if(_board.allDots[i, j] != null) {
                    if(_board.allDots[i, j].tag == color) {
                        // set isMatched to true for the piece at this position to mark it for destruction
                        _board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    }

    private List<GameObject> GetColumnPieces(int column) { 
        List<GameObject> dots = new();
        for(int i = 0; i < _board.height; i++) {
            var dot = _board.allDots[column, i];
            if(dot != null) { 
                dots.Add(dot);
                dot.GetComponent<Dot>().isMatched = true;
            }
        }

        return dots;
    }

    private List<GameObject> GetRowPieces(int row) { 
        List<GameObject> dots = new();
        for(int i = 0; i < _board.width; i++) {
            var dot = _board.allDots[i, row];
            if(dot != null) { 
                dots.Add(dot);
                dot.GetComponent<Dot>().isMatched = true;
            }
        }
        return dots;
    }

    public void CheckBombs() {
        // If the current piece is matched, make it a bomb
        if(_board.currentDot != null) { 
            if(_board.currentDot.isMatched) {
                _board.currentDot.isMatched = false;
                int typeOfBomb = Random.Range(0, 99);
                /*
                if(typeOfBomb < 50) { 
                    // 50% chance to spawn a row bomb
                    _board.currentDot.MakeBomb(BombType.Row);
                } else {
                    // 50% chance to spawn a column bomb
                    _board.currentDot.MakeBomb(BombType.Column);
                }
                */
                
                if((_board.currentDot.swipeAngle > -45 && _board.currentDot.swipeAngle <= 45) || 
                    (_board.currentDot.swipeAngle < -135 && _board.currentDot.swipeAngle >= 135)) 
                { 
                    _board.currentDot.MakeBomb(BombType.Row);
                } else { 
                    _board.currentDot.MakeBomb(BombType.Column);
                }
            } else if(_board.currentDot._otherDot != null) {
                // If the other piece is matched, make it a bomb
                var otherDot = _board.currentDot._otherDot.GetComponent<Dot>();
                if(otherDot != null) {
                    otherDot.isMatched = false;
                    /*
                    int typeOfBomb = Random.Range(0, 99);
                    if(typeOfBomb < 50) { 
                        // 50% chance to spawn a row bomb
                        otherDot.MakeBomb(BombType.Row);
                    } else {
                        // 50% chance to spawn a column bomb
                        otherDot.MakeBomb(BombType.Column);
                    }
                    */
                    if((otherDot.swipeAngle > -45 && otherDot.swipeAngle <= 45) || 
                        (otherDot.swipeAngle < -135 && otherDot.swipeAngle >= 135)) 
                    { 
                        otherDot.MakeBomb(BombType.Row);
                    } else {
                        otherDot.MakeBomb(BombType.Column);
                    }
                }
            }
        }
    }
}
