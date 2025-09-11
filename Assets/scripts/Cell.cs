using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x;
    public int y;
    public Candy candy;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 0; // hücre görseli arkada kalsın
        }
    }

    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.localPosition = new Vector3(x, -y, 0);
        name = $"Cell_{x}_{y}";
    }

    public bool IsEmpty()
    {
        return candy == null;
    }
}