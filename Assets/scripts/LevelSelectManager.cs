using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public LevelData[] levels;           // sürükle LevelData asset'lerini
    public GameObject buttonPrefab;      // Button prefab (root'ta Button component olmalı)
    public Transform contentParent;      // Scroll View -> Content veya Panel
    public int gameplaySceneIndex = 0;   // Build Settings içindeki gameplay sahne index'i

    void Start()
    {
        if (levels == null || buttonPrefab == null || contentParent == null)
        {
            Debug.LogWarning("LevelSelectManager: missing references");
            return;
        }

        // Temizle (editteki templateleri kaldır)
        for (int i = contentParent.childCount - 1; i >= 0; i--) Destroy(contentParent.GetChild(i).gameObject);

        for (int i = 0; i < levels.Length; i++)
        {
            var lvl = levels[i];
            var go = Instantiate(buttonPrefab, contentParent, false);
            var btn = go.GetComponent<Button>() ?? go.GetComponentInChildren<Button>();
            if (btn == null) { Debug.LogError("buttonPrefab has no Button component"); Destroy(go); continue; }

            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = string.IsNullOrEmpty(lvl.levelName) ? $"Level {i+1}" : lvl.levelName;

            int idx = i; // closure safety
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => PlayLevelByIndex(idx));

            Debug.Log($"Created button for level #{i} -> {(string.IsNullOrEmpty(lvl.levelName)? lvl.name : lvl.levelName)}");
        }
    }

    public void PlayLevelByIndex(int idx)
    {
        if (idx < 0 || idx >= levels.Length) { Debug.LogWarning("invalid level index " + idx); return; }
        Debug.Log($"PlayLevelByIndex called: {idx} => {levels[idx].name}");
        LevelLoader.SelectedLevel = levels[idx];
        LevelLoader.GameplaySceneIndex = gameplaySceneIndex;
        SceneManager.LoadScene(gameplaySceneIndex);
    }
}