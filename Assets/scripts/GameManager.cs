using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int score;
    private bool isGameOver;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        score = 0;
        isGameOver = false;
    }

    public void GameOver()
    {
        isGameOver = true;
    }

    public void AddScore(int points)
    {
        if (!isGameOver)
        {
            score += points;
        }
    }

    public int GetScore()
    {
        return score;
    }
}