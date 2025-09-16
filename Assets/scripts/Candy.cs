using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Candy : MonoBehaviour
{
    public CandyType type;
    public SpecialCandyType special = SpecialCandyType.None;

    public Sprite overlayStripedH;
    public Sprite overlayStripedV;
    public Sprite overlayWrapped;
    public Sprite overlayBomb; 
    public ParticleSystem swapParticlePrefab;
    public ParticleSystem explodeParticlePrefab;
    private SpriteRenderer sr;
    private SpriteRenderer overlaySr;
    private ParticleSystem colorBombVfx;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        var t = transform.Find("SpecialOverlay");
        if (t != null) overlaySr = t.GetComponent<SpriteRenderer>();

        var v = transform.Find("ColorBombVFX");
        if (v != null) colorBombVfx = v.GetComponent<ParticleSystem>();

        if (sr != null)
        {
            sr.sortingOrder = 1;
        }
        if (overlaySr != null)
        {
            overlaySr.enabled = false;
            overlaySr.sortingOrder = 2;
        }
        if (colorBombVfx != null)
            colorBombVfx.Stop();

        UpdateVisualForSpecial();
    }

    public void SetType(CandyType t)
    {
        type = t;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = t != null ? t.sprite : null;
            sr.color = Color.white;
        }
        UpdateVisualForSpecial();
        name = t != null ? $"Candy_{t.typeName}" : "Candy_empty";
    }

    public void SetSpecial(SpecialCandyType s)
    {
        special = s;
        UpdateVisualForSpecial();
    }

    void UpdateVisualForSpecial()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (overlaySr != null) { overlaySr.enabled = false; overlaySr.sprite = null; }
        if (colorBombVfx != null) colorBombVfx.Stop();

        switch (special)
        {
            case SpecialCandyType.None:
                if (sr != null) sr.color = Color.white;
                break;

            case SpecialCandyType.Striped_Horizontal:
                if (overlaySr != null) { overlaySr.sprite = overlayStripedH; overlaySr.enabled = true; }
                else if (sr != null) sr.color = Color.cyan;
                break;

            case SpecialCandyType.Striped_Vertical:
                if (overlaySr != null) { overlaySr.sprite = overlayStripedV; overlaySr.enabled = true; }
                else if (sr != null) sr.color = Color.green;
                break;

            case SpecialCandyType.Wrapped:
                if (overlaySr != null) { overlaySr.sprite = overlayWrapped; overlaySr.enabled = true; }
                else if (sr != null) sr.color = Color.magenta;
                break;

            case SpecialCandyType.ColorBomb:
                if (overlaySr != null)
                {
                    overlaySr.sprite = overlayBomb;
                    overlaySr.enabled = true;
                    if (sr != null) sr.sprite = null; 
                }
                else if (sr != null) sr.color = Color.yellow;
                if (colorBombVfx != null) colorBombVfx.Play();
                break;
        }
    }
    public void PlaySwapParticle()
    {
        if (swapParticlePrefab == null) return;
        var p = Instantiate(swapParticlePrefab, transform.position, Quaternion.identity, transform.parent);
        var main = p.main;
        if (type != null) main.startColor = new ParticleSystem.MinMaxGradient(type.color);

        var rends = p.GetComponentsInChildren<ParticleSystemRenderer>(true);
        string layerName = sr != null ? sr.sortingLayerName : "Default";
        int baseOrder = sr != null ? sr.sortingOrder : 0;
        foreach (var r in rends)
        {
            r.sortingLayerName = layerName;
            r.sortingOrder = baseOrder + 2; 
        }

        p.Play();
        float life = main.duration + GetMaxStartLifetime(main);
        Destroy(p.gameObject, life + 0.1f);
    }
 public float PlayExplodeAndGetDuration()
    {
        float fallback = 0.12f;
        if (explodeParticlePrefab == null)
        {
            if (sr != null) sr.enabled = false;
            return fallback;
        }

        if (sr != null) sr.enabled = false;

        var p = Instantiate(explodeParticlePrefab, transform.position, Quaternion.identity, transform.parent);
        var main = p.main;
        if (type != null) main.startColor = new ParticleSystem.MinMaxGradient(type.color);

        var rends = p.GetComponentsInChildren<ParticleSystemRenderer>(true);
        string layerName = sr != null ? sr.sortingLayerName : "Default";
        int baseOrder = sr != null ? sr.sortingOrder : 0;
        foreach (var r in rends)
        {
            r.sortingLayerName = layerName;
            r.sortingOrder = baseOrder + 2; 
        }

        p.Play();
        float life = main.duration + GetMaxStartLifetime(main);
        Destroy(p.gameObject, life + 0.1f);
        return life;
    }

    float GetMaxStartLifetime(ParticleSystem.MainModule main)
    {
        try
        {
            var lt = main.startLifetime;
            #if UNITY_2019_1_OR_NEWER
            return lt.constantMax > 0f ? lt.constantMax : lt.constant;
            #else
            return lt.constant;
            #endif
        }
        catch
        {
            return 0.1f;
        }
    }
    public void TriggerSpecial(GridManager grid, int x, int y, CandyType targetType = null)
    {
        if (grid == null) return;

        switch (special)
        {
            case SpecialCandyType.Striped_Horizontal:
                grid.ClearRow(y);
                break;
            case SpecialCandyType.Striped_Vertical:
                grid.ClearColumn(x);
                break;
            case SpecialCandyType.Wrapped:
                grid.ClearArea(x, y, 1);
                break;
            case SpecialCandyType.ColorBomb:
                var tt = targetType != null ? targetType : type;
                if (tt != null) grid.ClearAllOfType(tt);
                break;
            default:
                break;
        }
        grid.RemoveCandyInstance(this);
    }
}