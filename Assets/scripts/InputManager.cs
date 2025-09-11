using System.Collections;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private GridManager grid;
    private Cell selected;
    private bool isBusy;

    void Start() => grid = FindObjectOfType<GridManager>();

    void Update()
    {
        if (isBusy) return;
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(world);
            if (hit == null) return;
            var cell = hit.GetComponent<Cell>();
            if (cell == null) return;

            if (selected == null) { selected = cell; Highlight(selected, true); }
            else
            {
                Highlight(selected, false);
                if (selected != cell)
                {
                    StartCoroutine(DoSwap(new Vector2Int(selected.x, selected.y), new Vector2Int(cell.x, cell.y)));
                }
                selected = null;
            }
        }
    }

    IEnumerator DoSwap(Vector2Int a, Vector2Int b)
    {
        isBusy = true;
        yield return StartCoroutine(grid.SwapCells(a, b));
        isBusy = false;
    }

    void Highlight(Cell c, bool on)
    {
        if (c == null) return;
        var sr = c.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = on ? Color.gray : Color.white;
    }
}