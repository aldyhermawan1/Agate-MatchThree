using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    //Singleton
    private static BoardManager _instance = null;
    public static BoardManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BoardManager>();

                if (_instance == null)
                {
                    Debug.LogError("Fatal Error: BoardManager not Found!");
                }
            }

            return _instance;
        }
    }

    [Header("Board")]
    public Vector2Int size;
    public Vector2 offsetTile;
    public Vector2 offsetBoard;

    [Header("Tile")]
    public List<Sprite> tileTypes = new List<Sprite>();
    public GameObject tilePrefab;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private TileController[,] tiles;

    public bool IsAnimating
    {
        get
        {
            return IsSwapping;
        }
    }
    
    public bool IsSwapping
    {
        get;
        set;
    }

    private void Start()
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        CreateBoard(tileSize);
    }

    //Spawning Board
    private void CreateBoard(Vector2 tileSize)
    {
        tiles = new TileController[size.x, size.y];

        Vector2 totalSize = (tileSize + offsetTile) * (size - Vector2.one);

        startPosition = (Vector2)transform.position - (totalSize / 2) + offsetBoard;
        endPosition = startPosition + totalSize;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                TileController newTile = Instantiate(tilePrefab, new Vector2(startPosition.x + ((tileSize.x + offsetTile.x) * x), startPosition.y + ((tileSize.y + offsetTile.y) * y)), tilePrefab.transform.rotation, transform).GetComponent<TileController>();
                tiles[x, y] = newTile;

                List<int> possibleId = GetStartingPossibleIdList(x, y);
                int newId = possibleId[Random.Range(0, possibleId.Count)];

                newTile.ChangeId(newId, x, y);
            }
        }
    }

    //Spawning different color
    private List<int> GetStartingPossibleIdList(int x, int y)
    {
        List<int> possibleId = new List<int>();

        for (int i = 0; i < tileTypes.Count; i++)
        {
            possibleId.Add(i);
        }

        if (x > 1 && tiles[x - 1, y].id == tiles[x - 2, y].id)
        {
            possibleId.Remove(tiles[x - 1, y].id);
        }

        if (y > 1 && tiles[x, y - 1].id == tiles[x, y - 2].id)
        {
            possibleId.Remove(tiles[x, y - 1].id);
        }

        return possibleId;
    }

    //Swapping Tiles
    public IEnumerator SwapTilePosition(TileController a, TileController b, System.Action onCompleted)
    {
        IsSwapping = true;

        Vector2Int IndexA = GetTileIndex(a);
        Vector2Int IndexB = GetTileIndex(b);

        tiles[IndexA.x, IndexA.y] = b;
        tiles[IndexB.x, IndexB.y] = a;

        a.ChangeId(a.id, IndexB.x, IndexB.y);
        b.ChangeId(b.id, IndexA.x, IndexA.y);

        bool isRoutineACompleted = false;
        bool isRoutineBCompleted = false;

        StartCoroutine(a.MoveTilePosition(GetIndexPosition(IndexB), () =>
        {
            isRoutineACompleted = true;
        }));
        StartCoroutine(b.MoveTilePosition(GetIndexPosition(IndexA), () =>
        {
            isRoutineBCompleted = true;
        }));

        yield return new WaitUntil(() =>
        {
            return isRoutineACompleted && isRoutineBCompleted;
        });

        onCompleted?.Invoke();

        IsSwapping = false;
    }

    public Vector2Int GetTileIndex(TileController tile)
    {
        for(int x = 0; x < size.x; x++)
        {
            for(int y = 0; y < size.y; y++)
            {
                if (tile == tiles[x, y])
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, 1);
    }

    public Vector2 GetIndexPosition(Vector2Int index)
    {
        Vector2 tileSize = tilePrefab.GetComponent<SpriteRenderer>().size;
        return new Vector2(startPosition.x + ((tileSize.x + offsetTile.x) * index.x), startPosition.y + ((tileSize.y + offsetTile.y) * index.y));
    }

    //Checking Matches
    public List<TileController> GetAllMatches()
    {
        List<TileController> matchingTiles = new List<TileController>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                List<TileController> tileMatched = tiles[x, y].GetAllMatches();

                //Go next if no match
                if (tileMatched == null || tileMatched.Count == 0)
                {
                    continue;
                }

                foreach(TileController item in tileMatched)
                {
                    //only add if not added
                    if (!matchingTiles.Contains(item))
                    {
                        matchingTiles.Add(item);
                    }
                }
            }
        }

        return matchingTiles;
    }
}
