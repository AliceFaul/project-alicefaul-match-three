using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
}
