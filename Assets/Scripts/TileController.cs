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
                previouseSelected.Deselect();
                Select();
            }
        }
    }

    public void ChangeId(int id, int x, int y)
    {
        render.sprite = board.tileTypes[id];
        this.id = id;

        name = "TILE_" + id + " (" + x + ", " + y + ")";
    }

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
}
