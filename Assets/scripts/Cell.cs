using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x;
    public int y;
    public Candy candy;

    // obstacle state
    public bool locked = false;
    public int jellyLayers = 0;
    public bool hasTimedBomb = false;
    public int bombTimer = 0;

    private SpriteRenderer obstacleSr;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 0;

        var t = transform.Find("ObstacleOverlay");
        if (t != null) obstacleSr = t.GetComponent<SpriteRenderer>();
        UpdateObstacleVisual();
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.localPosition = new Vector3(x, -y, 0);
        name = $"Cell_{x}_{y}";
    }

    public bool IsEmpty() => candy == null;

    public void ApplyLocked()
    {
        locked = true;
        UpdateObstacleVisual();
    }

    public void ApplyJelly(int layers)
    {
        jellyLayers = Mathf.Max(jellyLayers, layers);
        UpdateObstacleVisual();
    }

    public void ApplyTimedBomb(int timer)
    {
        hasTimedBomb = true;
        bombTimer = timer;
        UpdateObstacleVisual();
    }

    public void RemoveJellyLayer()
    {
        if (jellyLayers > 0) jellyLayers--;
        UpdateObstacleVisual();
    }

    public void Unlock()
    {
        locked = false;
        UpdateObstacleVisual();
    }

    public void DecrementBombTimer()
    {
        if (!hasTimedBomb) return;
        bombTimer--;
        if (bombTimer <= 0)
        {
            // bomb explodes -> clear this cell candy (GridManager will handle)
        }
        UpdateObstacleVisual();
    }

    void UpdateObstacleVisual()
    {
        if (obstacleSr == null) return;
        if (hasTimedBomb)
        {
            obstacleSr.enabled = true;
            obstacleSr.color = Color.black; // placeholder visual
        }
        else if (jellyLayers > 0)
        {
            obstacleSr.enabled = true;
            obstacleSr.color = Color.cyan; // jelly color
        }
        else if (locked)
        {
            obstacleSr.enabled = true;
            obstacleSr.color = Color.gray; // lock color
        }
        else
        {
            obstacleSr.enabled = false;
        }
    }
}