using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;  // Usamos TextMeshPro
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI livesText;    // Referencia al texto de vidas
    public TextMeshProUGUI scoreText;    // Referencia al texto de puntos
    public TextMeshProUGUI highScoreText; // Referencia al texto de high score

    public int lives = 3;
    public int points = 0;
    public int highScore = 0;
    private GameObject currentBall;          // Referencia a la pelota actual
    public int currentLevel;
    public int currentblocks;
    private SaveLoadSystem SLSystem;
    private int maxLvlIterator = 1;
    private bool newGame;
    private AudioSource audioSource;
    private GameObject pauseMenu;
    private List<GameObject> duplicatedBall = new List<GameObject>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentBall = GameObject.FindGameObjectWithTag("Ball");
        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        currentblocks = bricks.Length;
        pauseMenu = GameObject.Find("PauseMenu");
        pauseMenu.SetActive(false);
        SLSystem = GetComponent<SaveLoadSystem>();
        CheckGameState();
    }

    void Update()
    {
        UpdateLives();
        UpdateScore();
        UpdateHighScore();
        // Comprobar si la pelota ha salido por la parte inferior de la pantalla
        if (currentBall != null && currentBall.transform.position.y < -5.0f)
        {
            LoseLife();
        }

        if (currentblocks == 0)
        {
            RemoveAllDuplicatedObjects();

            if (currentLevel == maxLvlIterator)
            {
                currentLevel = 0;
            }
            else
            {
                currentLevel++;
            }
            SaveGame();
            SceneManager.LoadScene("LvlCompleted");
        }

    }

    // M�todo para restar una vida y reiniciar la pelota
    void LoseLife()
    {
        lives--;
        if (lives > 0)
        {
            // Si quedan vidas, volver a generar la pelota
            ResetBall();
        }
        else
        {
            // Si se acaban las vidas, termina el juego
            GameOver();
        }
    }

    // M�todo para reiniciar la pelota
    void ResetBall()
    {
        currentBall.GetComponent<BallController>().gameStarted = false;
    }

    // M�todo para manejar el Game Over
    void GameOver()
    {
        newGame = true;
        SaveGame();
        SceneManager.LoadScene("Game Over"); // Reiniciar la escena para reiniciar el juego
    }

    public void UpdateLives()
    {
        livesText.text = "Vidas: " + lives.ToString();
    }

    // M�todo para actualizar los puntos en el HUD
    public void UpdateScore()
    {
        scoreText.text = "Puntos: " + points.ToString();
    }

    // M�todo para actualizar el high score en el HUD
    public void UpdateHighScore()
    {
        if (highScore < points)
        {
            highScore = points;
        }
        highScoreText.text = "High Score: " + highScore.ToString();
    }

    private void SaveGame()
    {
        PlayerData data = new PlayerData();
        data.lives = this.lives;
        data.points = this.points;
        data.highScore = this.highScore;
        data.currentlvl = this.currentLevel;
        data.newGame = this.newGame;
        SLSystem.SaveData(data);
    }

    private void LoadGame()
    {
        PlayerData data = SLSystem.LoadData();
        if (data != null)
        {
            this.lives = data.lives;
            this.points = data.points;
            this.highScore = data.highScore;
            this.currentLevel = data.currentlvl;
            this.newGame = data.newGame;
        }
    }

    public void LoadScene()
    {
        if (currentLevel == 0)
        {
            SceneManager.LoadScene("Level1");
        }
        else if (currentLevel == 1)
        {
            SceneManager.LoadScene("Level2");
        }
    }

    public void CheckGameState()
    {
        PlayerData data = SLSystem.LoadData();
        if (data != null)
        {
            if (data.newGame)
            {
                StartNewGame();
            }
            else
            {
                LoadGame();
            }
            newGame = false;
        }
    }

    public void StartNewGame()
    {
        LoadGame();
        currentLevel = 0;
        points = 0;
        lives = 3;
    }

    public void PlayBlockSound()
    {
        audioSource.Play();
    }
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;  // Pausa el juego
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;  // Reanuda el juego
    }
    public void PowerUP()
    {
        GameObject duplicatedObject = Instantiate(currentBall);

        // Asigna el mismo padre que el objeto original
        duplicatedObject.transform.SetParent(currentBall.transform.parent);
        duplicatedObject.transform.localScale = currentBall.transform.localScale;
        duplicatedObject.GetComponent<Image>().color = new Color(255, 0, 0);

        duplicatedObject.GetComponent<BallController>().gameStarted = false;

        duplicatedBall.Add(duplicatedObject);
    }

    public void RemoveAllDuplicatedObjects()
    {
        foreach (GameObject obj in duplicatedBall)
        {
            Destroy(obj); // Destruir el objeto duplicado
        }
        duplicatedBall.Clear(); // Limpiar la lista
    }

}