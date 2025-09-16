using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public LevelData levelData; 
    public int movesLeft;
    public int targetScore;
    [HideInInspector] public int score = 0;

    public TextMeshProUGUI movesTMP;
    public TextMeshProUGUI scoreTMP;
    public TextMeshProUGUI targetTMP;

    public GameObject winPanel;
    public GameObject losePanel;

    public int levelSelectSceneIndex = 0;
    public int nextLevelSceneIndex = -1;

    bool gameEnded = false;
    private GridManager grid;

    void Awake()
    {
        grid = FindObjectOfType<GridManager>();
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    void Start()
{
    if (LevelLoader.SelectedLevel != null)
    {
        Debug.Log("GameManager: Found selected level from LevelLoader: " + LevelLoader.SelectedLevel.name);
        StartLevel(LevelLoader.SelectedLevel);
        LevelLoader.SelectedLevel = null;
    }
    else if (levelData != null)
    {
        Debug.Log("GameManager: No selected level -> using inspector level: " + levelData.name);
        StartLevel(levelData);
    }
    UpdateUI();
}
    public void StartLevel(LevelData data)
    {
        levelData = data;
        if (levelData == null)
        {
            Debug.LogWarning("GameManager.StartLevel: levelData null");
            return;
        }

        movesLeft = levelData.moves;
        targetScore = levelData.goalAmount;
        score = 0;
        gameEnded = false;
        Time.timeScale = 1f; 

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        UpdateUI();
        Debug.Log($"Level started: moves={movesLeft}, target={targetScore}");
    }

    public void AddScore(int amount)
    {
        if (gameEnded) return;

        score += amount;
        UpdateUI();
        Debug.Log($"Score added: +{amount} => {score} (target {targetScore})");

        if (targetScore > 0 && score >= targetScore)
        {
            Win();
        }
    }

    public void UseMove()
    {
        if (gameEnded) return;

        movesLeft = Mathf.Max(0, movesLeft - 1);
        UpdateUI();
        Debug.Log($"Move used. Moves left: {movesLeft}");

        if (grid != null) grid.DecrementBombTimers();

        if (movesLeft <= 0)
        {
            if (score >= targetScore) Win();
            else Lose();
        }
    }

    void UpdateUI()
    {
        if (movesTMP) movesTMP.text = $"Moves: {movesLeft}";
        if (scoreTMP) scoreTMP.text = $"Score: {score}";
        if (targetTMP) targetTMP.text = $"Target: {targetScore}";
    }

    void Win()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("Level Won!");
        Time.timeScale = 0f;
        if (winPanel != null) winPanel.SetActive(true);
    }

    void Lose()
    {
        if (gameEnded) return;
        gameEnded = true;
        Debug.Log("Level Lost!");
        Time.timeScale = 0f;
        if (losePanel != null) losePanel.SetActive(true);
    }

    public void RestartLevel()
    {
        LevelLoader.SelectedLevel = levelData;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        if (nextLevelSceneIndex >= 0)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextLevelSceneIndex);
        }
        else
        {
            Debug.LogWarning("NextLevel: nextLevelSceneIndex not set");
        }
    }

    public void OpenLevelSelect()
{
    Debug.Log($"OpenLevelSelect called. inspectorIndex={levelSelectSceneIndex}");

    int buildCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
    Debug.Log($"Build scenes count = {buildCount}");

    for (int i = 0; i < buildCount; i++)
    {
        string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
        string name = System.IO.Path.GetFileNameWithoutExtension(path);
        Debug.Log($"BuildScene[{i}] = {name} (path={path})");
    }

    if (levelSelectSceneIndex >= 0 && levelSelectSceneIndex < buildCount)
    {
        Debug.Log($"Loading build index {levelSelectSceneIndex}");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelSelectSceneIndex);
        return;
    }

    const string fallback = "LevelSelect";
    if (Application.CanStreamedLevelBeLoaded(fallback))
    {
        Debug.Log($"Loading by name: {fallback}");
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(fallback);
        return;
    }

    Debug.LogError("OpenLevelSelect: LevelSelect scene not found in Build Settings. Add it and set levelSelectSceneIndex.");
}

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}