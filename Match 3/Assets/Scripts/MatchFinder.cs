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
                if(currentDot != null) { 
                    if(i > 0 && i < _board.width - 1) { 
                        GameObject leftDot = _board.allDots[i - 1, j];
                        GameObject rightDot = _board.allDots[i + 1, j];
                        if(leftDot != null && rightDot != null) { 
                            if(leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) {
                                // If any of the three dots in the match is a row bomb, mark all pieces in that row as matched
                                if(currentDot.GetComponent<Dot>().bombType == BombType.Row ||
                                    leftDot.GetComponent<Dot>().bombType == BombType.Row || 
                                    rightDot.GetComponent<Dot>().bombType == BombType.Row) 
                                {
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                if(currentDot.GetComponent<Dot>().bombType == BombType.Column) { 
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                if(leftDot.GetComponent<Dot>().bombType == BombType.Column) { 
                                    currentMatches.Union(GetColumnPieces(i - 1));
                                }
                                if(rightDot.GetComponent<Dot>().bombType == BombType.Column) { 
                                    currentMatches.Union(GetColumnPieces(i + 1));
                                }
                                if(!currentMatches.Contains(leftDot)) { 
                                    currentMatches.Add(leftDot);
                                }
                                leftDot.GetComponent<Dot>().isMatched = true;
                                if(!currentMatches.Contains(rightDot)) { 
                                    currentMatches.Add(rightDot);
                                }
                                rightDot.GetComponent<Dot>().isMatched = true;
                                if(!currentMatches.Contains(currentDot)) { 
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<Dot>().isMatched = true;
                            }
                        }
                    }
                    if(j > 0 && j < _board.height - 1) { 
                        GameObject upDot = _board.allDots[i, j + 1];
                        GameObject downDot = _board.allDots[i, j - 1];
                        if(upDot != null && downDot != null) { 
                            if(upDot.tag == currentDot.tag && downDot.tag == currentDot.tag) {
                                // If any of the three dots in the match is a column bomb, mark all pieces in that column as matched
                                if(currentDot.GetComponent<Dot>().bombType == BombType.Column ||
                                    upDot.GetComponent<Dot>().bombType == BombType.Column ||
                                    downDot.GetComponent<Dot>().bombType == BombType.Column) 
                                { 
                                    currentMatches.Union(GetColumnPieces(i));
                                }
                                if(currentDot.GetComponent<Dot>().bombType == BombType.Row) { 
                                    currentMatches.Union(GetRowPieces(j));
                                }
                                if(upDot.GetComponent<Dot>().bombType == BombType.Row) { 
                                    currentMatches.Union(GetRowPieces(j + 1));
                                }
                                if(downDot.GetComponent<Dot>().bombType == BombType.Row) { 
                                    currentMatches.Union(GetRowPieces(j - 1));
                                }
                                if(!currentMatches.Contains(upDot)) { 
                                    currentMatches.Add(upDot);
                                }
                                upDot.GetComponent<Dot>().isMatched = true;
                                if(!currentMatches.Contains(downDot)) { 
                                    currentMatches.Add(downDot);
                                }
                                downDot.GetComponent<Dot>().isMatched = true;
                                if(!currentMatches.Contains(currentDot)) { 
                                    currentMatches.Add(currentDot);
                                }
                                currentDot.GetComponent<Dot>().isMatched = true;
                            }
                        }
                    }
                }
            }
        }
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
