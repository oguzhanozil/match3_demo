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
        // Additional initialization logic here
    }

    public void GameOver()
    {
        isGameOver = true;
        // Handle game over logic here
    }

    public void AddScore(int points)
    {
        if (!isGameOver)
        {
            score += points;
            // Update score display logic here
        }
    }

    public int GetScore()
    {
        return score;
    }
}