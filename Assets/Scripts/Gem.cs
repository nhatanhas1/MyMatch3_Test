using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector]
    public Vector2Int posIndex;
    [HideInInspector]
    public Board board;

    Vector2 firstTouchPosition;
    Vector2 finalTouchPosition;

    bool mousePressed;
    float swipeAngle = 0;

    Gem otherGem;

    public enum GemType { blue, green, red, yellow, purple }
    public GemType type;

    public bool isMatched;

    private Vector2Int previousPos;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > .01f)
        {
            transform.position = Vector2.Lerp(transform.position, posIndex, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0f);
            board.allGems[posIndex.x, posIndex.y] = this;
        }

        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;

            if (board.currentState == Board.BoardState.move)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    public void SetupGem(Vector2Int pos, Board theBoard)
    {
        posIndex = pos;
        board = theBoard;
    }

    private void OnMouseDown()
    {
        if (board.currentState == Board.BoardState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;
        Debug.Log(swipeAngle);

        if (Vector3.Distance(firstTouchPosition, finalTouchPosition) > .5f)
        {
            MovePieces();
        }
    }

    void MovePieces()
    {
        previousPos = posIndex;

        // just change pos index
        if (swipeAngle > -45 && swipeAngle < 45 && posIndex.x < board.width - 1)
        {
            otherGem = board.allGems[posIndex.x + 1, posIndex.y];
            otherGem.posIndex.x--;
            posIndex.x++;
        }
        else if (swipeAngle > 45 && swipeAngle < 135 && posIndex.y < board.height - 1)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y + 1];
            otherGem.posIndex.y--;
            posIndex.y++;
        }
        else if (swipeAngle > -135 && swipeAngle < -45 && posIndex.y > 0)
        {
            otherGem = board.allGems[posIndex.x, posIndex.y - 1];
            otherGem.posIndex.y++;
            posIndex.y--;
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && posIndex.x > 0)
        {
            otherGem = board.allGems[posIndex.x - 1, posIndex.y];
            otherGem.posIndex.x++;
            posIndex.x--;
        }
        // update board
        board.allGems[posIndex.x, posIndex.y] = this;
        board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;

        StartCoroutine(CheckMoveCo());
    }

    public IEnumerator CheckMoveCo()
    {
        board.currentState = Board.BoardState.wait;

        yield return new WaitForSeconds(.5f);

        board.matchFind.FindAllMatches();

        if (!isMatched && !otherGem.isMatched)
        {
            otherGem.posIndex = posIndex;
            posIndex = previousPos;

            board.allGems[posIndex.x, posIndex.y] = this;
            board.allGems[otherGem.posIndex.x, otherGem.posIndex.y] = otherGem;

            yield return new WaitForSeconds(.5f);

            board.currentState = Board.BoardState.move;
        }
        else
        {
            board.DestroyMatches();
        }
    }


}
