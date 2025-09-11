using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Candy : MonoBehaviour
{
    public CandyType type;
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 1; // candy her zaman hücrenin önünde olsun
        }
    }

    public void SetType(CandyType t)
    {
        type = t;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        sr.sprite = t != null ? t.sprite : null;
        name = t != null ? $"Candy_{t.typeName}" : "Candy_empty";
    }
}