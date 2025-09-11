using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;      // Cell prefab (.prefab) - içinde Cell.cs ve BoxCollider2D olmalı
    public GameObject candyPrefab;     // Candy prefab (.prefab) - içinde Candy.cs ve SpriteRenderer olmalı
    public List<CandyType> candyTypes; // CandyType ScriptableObject'leri
    public float swapSpeed = 8f;

    private Cell[,] cells;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        CreateGrid();
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

    IEnumerator FillBoardRoutine()
    {
        // İlk doldurma
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                SpawnRandomCandyAt(x, y);

        // Eğer başlangıçta eşleşmeler varsa temizle ve doldur (sonsuz döngüye dikkat)
        int safety = 0;
        while (true)
        {
            var matches = GetAllMatches();
            if (matches.Count == 0) break;

            ClearMatches(matches);
            yield return StartCoroutine(CollapseRoutine());

            safety++;
            if (safety > 20) // çok nadir de olsa takılma olursa kes
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
        // Spawn yukarıdan başlatıyoruz (görsel düşme için)
        // DEĞİŞTİRİLDİ: önce -(y + height) kullanılıyordu; yukarıdan spawn => -y + height
        go.transform.position = new Vector3(x, -y + height, 0);
        Candy candy = go.GetComponent<Candy>();
        if (candy == null) candy = go.AddComponent<Candy>();
        candy.SetType(type);

        // cell referansını ata
        if (cells[x, y] != null)
            cells[x, y].candy = candy;

        // Hedef pozisyona hareket ettir
        StartCoroutine(MoveCandyTo(go, new Vector3(x, -y, 0)));
    }

    IEnumerator MoveCandyTo(GameObject go, Vector3 target)
    {
        if (go == null) yield break;
        float t = 0f;
        while ((go.transform.position - target).sqrMagnitude > 0.001f)
        {
            go.transform.position = Vector3.Lerp(go.transform.position, target, Time.deltaTime * swapSpeed);
            t += Time.deltaTime;
            // güvenlik için uzun sürede çık
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

        // swap data
        cellA.candy = candyB;
        cellB.candy = candyA;

        // animate
        StartCoroutine(MoveCandyTo(candyA.gameObject, new Vector3(b.x, -b.y, 0)));
        StartCoroutine(MoveCandyTo(candyB.gameObject, new Vector3(a.x, -a.y, 0)));
        yield return new WaitForSeconds(0.16f);

        var matches = GetAllMatches();
        if (matches.Count == 0)
        {
            // swap back
            cellA.candy = candyA;
            cellB.candy = candyB;
            StartCoroutine(MoveCandyTo(candyA.gameObject, new Vector3(a.x, -a.y, 0)));
            StartCoroutine(MoveCandyTo(candyB.gameObject, new Vector3(b.x, -b.y, 0)));
            yield return new WaitForSeconds(0.16f);
        }
        else
        {
            // temizleme + collapse döngüsü
            while (true)
            {
                matches = GetAllMatches();
                if (matches.Count == 0) break;
                ClearMatches(matches);
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

        // horizontal
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

        // vertical
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
        foreach (var c in matches)
        {
            if (c == null) continue;
            if (gameManager != null && c.type != null) gameManager.AddScore(c.type.scoreValue);
            // cell'tan ayır
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (cells[x, y] != null && cells[x, y].candy == c) cells[x, y].candy = null;

            // yok et (pool varsa onu kullan)
            Destroy(c.gameObject);
        }
    }

    IEnumerator CollapseRoutine()
    {
        // DEĞİŞTİRİLDİ: sütun içinde alttan üste kontrol (y=height-1 -> 0)
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (cells[x, y].candy == null)
                {
                    int k = y - 1; // üstteki (daha küçük y index) öğeyi ara
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
                        // üstten yeni spawn
                        SpawnRandomCandyAt(x, y);
                    }
                }
            }
        }

        // animasyon için bekle
        yield return new WaitForSeconds(0.14f);
    }
}