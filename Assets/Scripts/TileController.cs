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

    private static TileController previouseSelected = null;

    private bool isSelected = false;

    private void Awake()
    {
        board = BoardManager.Instance;
        render = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        //Non Selectable
        if (render.sprite == null)
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
            if (previouseSelected == null)
            {
                Select();
            }
            else
            {
                TileController otherTile = previouseSelected;
                //Swap Tile
                SwapTile(otherTile, () =>
                {
                    SwapTile(otherTile);
                });
                
                //run if cant swap
                /*previouseSelected.Deselect();
                Select();*/
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

    //Selecting and Deselecting
    private void Select()
    {
        isSelected = true;
        render.color = selectedColor;
        previouseSelected = this;
    }

    private void Deselect()
    {
        isSelected = false;
        render.color = normalColor;
        previouseSelected = null;
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

    public void SwapTile(TileController otherTile, System.Action onCompleted = null)
    {
        StartCoroutine(board.SwapTilePosition(this, otherTile, onCompleted));
    }
}
