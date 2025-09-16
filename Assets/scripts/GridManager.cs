using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;     
    public GameObject candyPrefab;    
    public List<CandyType> candyTypes;
    public float swapSpeed = 8f;
    public LevelData levelData;
    private Cell[,] cells;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (levelData != null)
        {
            width = levelData.width;
            height = levelData.height;
        }
    CreateGrid();
    PlaceObstaclesFromLevel();
    StartCoroutine(FillBoardRoutine());
    }

    void CreateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("GridManager: cellPrefab atanmamış!");
            return;
        }

        cells = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject go = Instantiate(cellPrefab, transform);
                go.transform.localScale = Vector3.one;
                Cell cell = go.GetComponent<Cell>();
                if (cell == null) cell = go.AddComponent<Cell>();
                cell.SetPosition(x, y);
                cells[x, y] = cell;
            }
        }
    }
    void PlaceObstaclesFromLevel()
    {
    if (levelData == null) return;
    foreach (var e in levelData.obstacles)
    {
        if (e.x < 0 || e.x >= width || e.y < 0 || e.y >= height) continue;
        var cell = cells[e.x, e.y];
        if (cell == null) continue;

        switch (e.type)
        {
            case LevelData.ObstacleType.Locked:
                cell.ApplyLocked();
                break;
            case LevelData.ObstacleType.Jelly:
                cell.ApplyJelly(e.jellyLayers);
                break;
            case LevelData.ObstacleType.BombTimed:
                cell.ApplyTimedBomb(e.bombTimer);
                break;
            }
        }
    }
    public void DecrementBombTimers()
{
    List<(int x, int y)> explode = new List<(int, int)>();
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var c = cells[x, y];
            if (c != null && c.hasTimedBomb)
            {
                c.DecrementBombTimer();
                if (c.bombTimer <= 0) explode.Add((x, y));
            }
        }

    foreach (var e in explode)
    {
        AudioManager.Instance?.PlayExplode();
        ClearArea(e.x, e.y, 1);
        StartCoroutine(CollapseRoutine());
    }
}
    IEnumerator FillBoardRoutine()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SpawnRandomCandyAt(x, y);

        int safety = 0;
        while (true)
        {
            var groups = GetMatchGroups();
            if (groups.Count == 0) break;

            ClearMatchGroups(groups);
            yield return StartCoroutine(CollapseRoutine());

            safety++;
            if (safety > 20)
            {
                Debug.LogWarning("FillBoardRoutine: safety break");
                break;
            }
            yield return null;
        }
        yield break;
    }


    void SpawnRandomCandyAt(int x, int y)
    {
        if (candyTypes == null || candyTypes.Count == 0)
        {
            Debug.LogError("GridManager: candyTypes listesi boş. Inspector'dan atayın.", this);
            return;
        }
        CandyType type = candyTypes[Random.Range(0, candyTypes.Count)];
        SpawnCandyAt(x, y, type);
    }

    void SpawnCandyAt(int x, int y, CandyType type)
    {
        if (candyPrefab == null)
        {
            Debug.LogError("GridManager: candyPrefab atanmamış!");
            return;
        }

        GameObject go = Instantiate(candyPrefab, transform);
        go.transform.position = new Vector3(x, -y + height, 0);
        Candy candy = go.GetComponent<Candy>();
        if (candy == null) candy = go.AddComponent<Candy>();
        candy.SetType(type);

        if (cells[x, y] != null)
            cells[x, y].candy = candy;

        StartCoroutine(MoveCandyTo(go, new Vector3(x, -y, 0)));
    }
    public void RemoveCandyInstance(Candy c)
    {
        if (c == null) return;
        StartCoroutine(RemoveCandyCoroutine(c));
    }
    IEnumerator RemoveCandyCoroutine(Candy c)
    {
        if (c == null) yield break;

        if (gameManager != null && c.type != null) gameManager.AddScore(c.type.scoreValue);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] != null && cells[x, y].candy == c) cells[x, y].candy = null;

        float wait = 0.08f;
        if (c != null)
        {
            wait = c.PlayExplodeAndGetDuration();
            if (wait < 0.05f) wait = 0.05f;
            if (wait > 0.6f) wait = 0.6f;
        }

        yield return new WaitForSeconds(wait);
        Destroy(c.gameObject);
    }
    void ClearAll()
    {
        var toClear = new List<Candy>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] != null && cells[x, y].candy != null) toClear.Add(cells[x, y].candy);
        ClearMatches(toClear);
    }
    (List<Candy> horiz, List<Candy> vert) GetLinesAt(int x, int y)
    {
        var horiz = new List<Candy>();
        var vert = new List<Candy>();
        if (cells[x,y] == null || cells[x,y].candy == null) return (horiz, vert);
        var start = cells[x,y].candy;
        horiz.Add(start);
        for (int i = x - 1; i >= 0; i--)
        {
            var c = cells[i, y].candy;
            if (c != null && c.type != null && start.type != null && c.type.id == start.type.id) horiz.Add(c); else break;
        }
        for (int i = x + 1; i < width; i++)
        {
            var c = cells[i, y].candy;
            if (c != null && c.type != null && start.type != null && c.type.id == start.type.id) horiz.Add(c); else break;
        }
        vert.Add(start);
        for (int j = y - 1; j >= 0; j--)
        {
            var c = cells[x, j].candy;
            if (c != null && c.type != null && start.type != null && c.type.id == start.type.id) vert.Add(c); else break;
        }
        for (int j = y + 1; j < height; j++)
        {
            var c = cells[x, j].candy;
            if (c != null && c.type != null && start.type != null && c.type.id == start.type.id) vert.Add(c); else break;
        }
        return (horiz, vert);
    }
    List<List<Candy>> GetMatchGroups()
    {
        var used = new HashSet<Candy>();
        var groups = new List<List<Candy>>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var list = GetMatchesAt(x, y);
                if (list.Count >= 3)
                {
                    bool anyUsed = false;
                    foreach (var c in list) if (used.Contains(c)) { anyUsed = true; break; }
                    if (anyUsed) continue;

                    foreach (var c in list) used.Add(c);
                    groups.Add(new List<Candy>(list));
                }
            }
        }
        return groups;
    }
    void ClearMatchGroups(List<List<Candy>> groups)
    {
        if (groups == null || groups.Count == 0) return;

        foreach (var grp in groups)
        {
            if (grp == null || grp.Count == 0) continue;

            bool wrappedHandled = false;
            foreach (var c in grp)
            {
                if (c == null) continue;

                int cx = -1, cy = -1;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (cells[x, y] != null && cells[x, y].candy == c) { cx = x; cy = y; break; }
                    }
                    if (cx >= 0) break;
                }
                if (cx < 0) continue;

                var (hline, vline) = GetLinesAt(cx, cy);
                if (hline.Count >= 3 && vline.Count >= 3)
                {
                    CreateSpecialCandyAt(cx, cy, SpecialCandyType.Wrapped, c.type);

                    var toRemove = new HashSet<Candy>();
                    foreach (var hc in hline) if (hc != null) toRemove.Add(hc);
                    foreach (var vc in vline) if (vc != null) toRemove.Add(vc);

                    foreach (var rem in toRemove)
                    {
                        if (rem == null) continue;
                        if (cells[cx, cy] != null && cells[cx, cy].candy == rem) continue; 
                        RemoveCandyInstance(rem);
                    }

                    wrappedHandled = true;
                    break; 
                }
            }

            if (wrappedHandled) continue;

            Candy first = grp[0];
            int fx = -1, fy = -1;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (cells[x, y] != null && cells[x, y].candy == first) { fx = x; fy = y; break; }
                }
                if (fx >= 0) break;
            }

            if (fx < 0)
            {
                foreach (var c2 in grp) if (c2 != null) RemoveCandyInstance(c2);
                continue;
            }

            if (grp.Count >= 5)
            {
                CreateSpecialCandyAt(fx, fy, SpecialCandyType.ColorBomb);
                foreach (var c2 in grp)
                {
                    if (c2 == null) continue;
                    if (cells[fx, fy] != null && cells[fx, fy].candy == c2) continue;
                    RemoveCandyInstance(c2);
                }
            }
            else if (grp.Count == 4)
            {
                bool horiz = true;
                int y0 = -999;
                foreach (var c2 in grp)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (cells[x, y] != null && cells[x, y].candy == c2)
                            {
                                if (y0 == -999) y0 = y;
                                else if (y != y0) horiz = false;
                            }
                        }
                    }
                }
                var st = horiz ? SpecialCandyType.Striped_Horizontal : SpecialCandyType.Striped_Vertical;
                CreateSpecialCandyAt(fx, fy, st, first.type);
                foreach (var c2 in grp)
                {
                    if (c2 == null) continue;
                    if (cells[fx, fy] != null && cells[fx, fy].candy == c2) continue;
                    RemoveCandyInstance(c2);
                }
            }
            else
            {
                foreach (var c2 in grp) if (c2 != null) RemoveCandyInstance(c2);
            }
        }
    }
    IEnumerator MoveCandyTo(GameObject go, Vector3 target)
    {
        if (go == null) yield break;
        float t = 0f;
        while ((go.transform.position - target).sqrMagnitude > 0.001f)
        {
            go.transform.position = Vector3.Lerp(go.transform.position, target, Time.deltaTime * swapSpeed);
            t += Time.deltaTime;
            if (t > 2f) { go.transform.position = target; break; }
            yield return null;
        }
        go.transform.position = target;
    }

    public IEnumerator SwapCells(Vector2Int a, Vector2Int b)
{
    if (!AreAdjacent(a, b)) yield break;
    if (cells == null) yield break;

    Cell cellA = cells[a.x, a.y];
    Cell cellB = cells[b.x, b.y];
    if (cellA == null || cellB == null) yield break;
    Candy candyA = cellA.candy;
    Candy candyB = cellB.candy;
    if (candyA == null || candyB == null) yield break;

    Candy origA = candyA;
    Candy origB = candyB;

    cellA.candy = candyB;
    cellB.candy = candyA;
    StartCoroutine(MoveCandyTo(candyA.gameObject, new Vector3(b.x, -b.y, 0)));
    StartCoroutine(MoveCandyTo(candyB.gameObject, new Vector3(a.x, -a.y, 0)));
    yield return new WaitForSeconds(0.16f);
    bool specialSwap = (origA.special != SpecialCandyType.None) || (origB.special != SpecialCandyType.None);
        if (!specialSwap)
        {
            AudioManager.Instance?.PlaySwap();
        }
        else
        {
            AudioManager.Instance?.PlayExplode();
        }
        if (origA.special == SpecialCandyType.ColorBomb && origB.special == SpecialCandyType.ColorBomb)
        {
            if (gameManager != null) gameManager.UseMove();
            Debug.Log("ColorBomb + ColorBomb triggered: clearing all");
            ClearAll();
            yield return StartCoroutine(CollapseRoutine());
            yield break;
        }

    if (origA.special == SpecialCandyType.ColorBomb && origB.type != null)
    {
        if (gameManager != null) gameManager.UseMove();
        Debug.Log("ColorBomb triggered with target type (origA)");
        origA.TriggerSpecial(this, b.x, b.y, origB.type);
        yield return StartCoroutine(CollapseRoutine());
        yield break;
    }
    if (origB.special == SpecialCandyType.ColorBomb && origA.type != null)
    {
        if (gameManager != null) gameManager.UseMove();
        Debug.Log("ColorBomb triggered with target type (origB)");
        origB.TriggerSpecial(this, a.x, a.y, origA.type);
        yield return StartCoroutine(CollapseRoutine());
        yield break;
    }

    if (origA.special != SpecialCandyType.None && origB.special == SpecialCandyType.None)
    {
        if (gameManager != null) gameManager.UseMove();
        Debug.Log($"Special {origA.special} from A triggered by swapping with normal candy");
        origA.TriggerSpecial(this, b.x, b.y, origB.type);
        yield return StartCoroutine(CollapseRoutine());
        yield break;
    }
    if (origB.special != SpecialCandyType.None && origA.special == SpecialCandyType.None)
    {
        if (gameManager != null) gameManager.UseMove();
        Debug.Log($"Special {origB.special} from B triggered by swapping with normal candy");
        origB.TriggerSpecial(this, a.x, a.y, origA.type);
        yield return StartCoroutine(CollapseRoutine());
        yield break;
    }

    var groups = GetMatchGroups();
    if (groups.Count == 0)
    {
        cellA.candy = candyA;
        cellB.candy = candyB;
        StartCoroutine(MoveCandyTo(candyA.gameObject, new Vector3(a.x, -a.y, 0)));
        StartCoroutine(MoveCandyTo(candyB.gameObject, new Vector3(b.x, -b.y, 0)));
        yield return new WaitForSeconds(0.16f);
    }
    else
    {
        if (gameManager != null) gameManager.UseMove();

        ClearMatchGroups(groups);
        yield return StartCoroutine(CollapseRoutine());

        while (true)
        {
            groups = GetMatchGroups();
            if (groups.Count == 0) break;
            ClearMatchGroups(groups);
            yield return StartCoroutine(CollapseRoutine());
            yield return new WaitForSeconds(0.05f);
        }
    }
}


    bool AreAdjacent(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) == 1 && a.y == b.y) || (Mathf.Abs(a.y - b.y) == 1 && a.x == b.x);
    }

    List<Candy> GetMatchesAt(int x, int y)
    {
        List<Candy> result = new List<Candy>();
        if (cells[x, y] == null) return result;
        Candy start = cells[x, y].candy;
        if (start == null || start.type == null) return result;

        List<Candy> horiz = new List<Candy> { start };
        for (int i = x - 1; i >= 0; i--)
        {
            var c = cells[i, y].candy;
            if (c != null && c.type != null && c.type.id == start.type.id) horiz.Add(c); else break;
        }
        for (int i = x + 1; i < width; i++)
        {
            var c = cells[i, y].candy;
            if (c != null && c.type != null && c.type.id == start.type.id) horiz.Add(c); else break;
        }
        if (horiz.Count >= 3) return horiz;

        List<Candy> vert = new List<Candy> { start };
        for (int j = y - 1; j >= 0; j--)
        {
            var c = cells[x, j].candy;
            if (c != null && c.type != null && c.type.id == start.type.id) vert.Add(c); else break;
        }
        for (int j = y + 1; j < height; j++)
        {
            var c = cells[x, j].candy;
            if (c != null && c.type != null && c.type.id == start.type.id) vert.Add(c); else break;
        }
        if (vert.Count >= 3) return vert;

        return new List<Candy>();
    }

    List<Candy> GetAllMatches()
    {
        HashSet<Candy> found = new HashSet<Candy>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var list = GetMatchesAt(x, y);
                foreach (var c in list) if (c != null) found.Add(c);
            }
        return new List<Candy>(found);
    }

    void ClearMatches(List<Candy> matches)
    {
        if (matches == null) return;
        foreach (var c in matches)
        {
            if (c == null) continue;
            RemoveCandyInstance(c);
        }
    }
    public void CreateSpecialCandyAt(int x, int y, SpecialCandyType specialType, CandyType baseType = null)
    {
    if (candyPrefab == null) return;

    if (cells[x, y] != null && cells[x, y].candy != null)
    {
        Candy existing = cells[x, y].candy;
        if (baseType != null) existing.SetType(baseType);
        existing.SetSpecial(specialType);
        return;
    }

    GameObject go = Instantiate(candyPrefab, transform);
    go.transform.localPosition = new Vector3(x, -y, 0); 
    Candy candy = go.GetComponent<Candy>();
    if (candy == null) candy = go.AddComponent<Candy>();
    candy.SetType(baseType);
    candy.SetSpecial(specialType);
    if (cells[x, y] != null) cells[x, y].candy = candy;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            int cx = Mathf.Clamp(width / 2, 0, width - 1);
            int cy = Mathf.Clamp(height / 2, 0, height - 1);
            Debug.Log($"Test: Create Wrapped at {cx},{cy}");
            CreateSpecialCandyAt(cx, cy, SpecialCandyType.Wrapped);
        }
    }
    public void ClearRow(int y)
    {
        var toClear = new List<Candy>();
        for (int x = 0; x < width; x++)
            if (cells[x, y] != null && cells[x, y].candy != null) toClear.Add(cells[x, y].candy);
        ClearMatches(toClear);
    }

    public void ClearColumn(int x)
    {
        var toClear = new List<Candy>();
        for (int y = 0; y < height; y++)
            if (cells[x, y] != null && cells[x, y].candy != null) toClear.Add(cells[x, y].candy);
        ClearMatches(toClear);
    }

    public void ClearArea(int cx, int cy, int radius)
    {
        var toClear = new List<Candy>();
        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int x = cx + dx, y = cy + dy;
                if (x >= 0 && x < width && y >= 0 && y < height)
                    if (cells[x, y] != null && cells[x, y].candy != null) toClear.Add(cells[x, y].candy);
            }
        ClearMatches(toClear);
    }

    public void ClearAllOfType(CandyType type)
    {
        if (type == null) return;
        var toClear = new List<Candy>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (cells[x, y] != null && cells[x, y].candy != null && cells[x, y].candy.type != null && cells[x, y].candy.type.id == type.id)
                    toClear.Add(cells[x, y].candy);
        ClearMatches(toClear);
    }
    void HandleMatchForSpecial(List<Candy> matches)
{
    if (matches == null || matches.Count < 3) return;
    Candy first = matches[0];
    int fx=-1, fy=-1;
    for (int x=0;x<width;x++) for (int y=0;y<height;y++)
        if (cells[x,y]!=null && cells[x,y].candy==first) { fx=x; fy=y; break; }

    if (fx < 0) return;

    if (matches.Count >= 5)
    {
        CreateSpecialCandyAt(fx, fy, SpecialCandyType.ColorBomb);
    }
    else if (matches.Count == 4)
    {
        bool horizontal = true;
        foreach (var c in matches) {
        }
        CreateSpecialCandyAt(fx, fy, SpecialCandyType.Striped_Horizontal, first.type);
    }
}
    IEnumerator CollapseRoutine()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (cells[x, y].candy == null)
                {
                    int k = y - 1; 
                    while (k >= 0 && cells[x, k].candy == null) k--;
                    if (k >= 0 && cells[x, k].candy != null)
                    {
                        Candy moving = cells[x, k].candy;
                        cells[x, y].candy = moving;
                        cells[x, k].candy = null;
                        StartCoroutine(MoveCandyTo(moving.gameObject, new Vector3(x, -y, 0)));
                    }
                    else
                    {
                        SpawnRandomCandyAt(x, y);
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.14f);
    }
}