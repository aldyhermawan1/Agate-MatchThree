using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    public int id;

    private BoardManager board;
    private SpriteRenderer render;

    private static readonly Color selectedColor = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color normalColor = Color.white;

    private static readonly float moveDuration = 0.5f;

    private static readonly Vector2[] adjacentDirection = new Vector2[]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private static TileController previousSelected = null;

    private bool isSelected = false;
    
    public bool IsDestroyed
    {
        get;
        private set;
    }

    private void Awake()
    {
        board = BoardManager.Instance;
        render = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        //Non Selectable
        if (render.sprite == null || board.IsAnimating)
        {
            return;
        }

        //Already selected
        if (isSelected)
        {
            Deselect();
        }
        else
        {
            //Nothing selected yet
            if (previousSelected == null)
            {
                Select();
            }
            else
            {
                //Adjacent Check
                if (GetAllAdjacentTiles().Contains(previousSelected))
                {
                    TileController otherTile = previousSelected;
                    previousSelected.Deselect();

                    //Swap Tile
                    SwapTile(otherTile, () =>
                    {
                        if(board.GetAllMatches().Count > 0)
                        {
                            Debug.Log("Match Found");
                        }
                        else
                        {
                            SwapTile(otherTile);
                        }
                    });
                }
                else
                {
                    previousSelected.Deselect();
                    Select();
                }
            }
        }
    }

    //Changing Tiles ID
    public void ChangeId(int id, int x, int y)
    {
        render.sprite = board.tileTypes[id];
        this.id = id;

        name = "TILE_" + id + " (" + x + ", " + y + ")";
    }

    //Selecting Tile
    private void Select()
    {
        isSelected = true;
        render.color = selectedColor;
        previousSelected = this;
    }

    //Deselecting Tile
    private void Deselect()
    {
        isSelected = false;
        render.color = normalColor;
        previousSelected = null;
    }

    //Swapping Animation
    public IEnumerator MoveTilePosition(Vector2 targetPosition, System.Action onCompleted)
    {
        Vector2 startPosition = transform.position;
        float time = 0.0f;

        //Wait for next frame
        yield return new WaitForEndOfFrame();

        while (time < moveDuration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / moveDuration);
            time += Time.deltaTime;

            //Wait for next frame
            yield return new WaitForEndOfFrame();
        }

        transform.position = targetPosition;

        onCompleted?.Invoke();
    }

    //Swapping Tile
    public void SwapTile(TileController otherTile, System.Action onCompleted = null)
    {
        StartCoroutine(board.SwapTilePosition(this, otherTile, onCompleted));
    }

    //Adjacent Checking
    private TileController GetAdjacent(Vector2 castDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir, render.size.x);

        if (hit)
        {
            return hit.collider.GetComponent<TileController>();
        }

        return null;
    }

    public List<TileController> GetAllAdjacentTiles()
    {
        List<TileController> adjacentTiles = new List<TileController>();

        for (int i = 0; i < adjacentDirection.Length; i++)
        {
            adjacentTiles.Add(GetAdjacent(adjacentDirection[i]));
        }

        return adjacentTiles;
    }

    //Checking Match Tiles
    private List<TileController> GetMatch(Vector2 castDir)
    {
        List<TileController> matchingTiles = new List<TileController>();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir, render.size.x);

        while (hit)
        {
            TileController otherTile = hit.collider.GetComponent<TileController>();
            if(otherTile.id != id || otherTile.IsDestroyed)
            {
                break;
            }

            matchingTiles.Add(otherTile);
            hit = Physics2D.Raycast(otherTile.transform.position, castDir, render.size.x);
        }

        return matchingTiles;
    }

    private List<TileController> GetOneLineMatch(Vector2[] paths)
    {
        List<TileController> matchingTiles = new List<TileController>();

        for (int i = 0; i < paths.Length; i++)
        {
            matchingTiles.AddRange(GetMatch(paths[i]));
        }

        //Min 3 Match
        if(matchingTiles.Count >= 2)
        {
            return matchingTiles;
        }

        return null;
    }

    public List<TileController> GetAllMatches()
    {
        if (IsDestroyed)
        {
            return null;
        }

        List<TileController> matchingTiles = new List<TileController>();

        //Horizontal & Vertical
        List<TileController> horizontalMatchingTiles = GetOneLineMatch(new Vector2[2]
        {
            Vector2.up, Vector2.down
        });
        List<TileController> verticalMatchingTiles = GetOneLineMatch(new Vector2[2]
        {
            Vector2.left, Vector2.right
        });

        if(horizontalMatchingTiles != null)
        {
            matchingTiles.AddRange(horizontalMatchingTiles);
        }

        if(verticalMatchingTiles != null)
        {
            matchingTiles.AddRange(verticalMatchingTiles);
        }

        //Add tiles to matched tiles
        if (matchingTiles != null && matchingTiles.Count >= 2)
        {
            matchingTiles.Add(this);
        }

        return matchingTiles;
    }
}
