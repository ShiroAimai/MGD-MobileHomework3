using System.Collections;
using System.Collections.Generic;
using Managers;
using Ui;
using UnityEngine;
using UnityEngine.UI;

public class MenuLayerController : MonoBehaviour
{
    [Header("Menu config")]
    [SerializeField] private Image menuLayerBackground;
    [SerializeField] private GameObject menuBackground;

    [Header("Menu mode config")] 
    [SerializeField] private GameOverController gameOverController;
    [SerializeField] private GameObject pausePanel;
    
    private void Start()
    {
        GameManager.Instance.onGameOver += OnGameOver;
        GameManager.Instance.onGamePaused += OnPause;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.onGameOver -= OnGameOver;
        GameManager.Instance.onGamePaused -= OnPause;
    }

    private void OnPause(bool isPaused)
    {
        OnMenuEvent(isPaused);
        pausePanel.SetActive(isPaused);
    }
    
    private void OnGameOver(int gameScore)
    {
        OnMenuEvent(true);
        gameOverController.GameOver(gameScore);
    }

    private void OnMenuEvent(bool isActive)
    {
        menuLayerBackground.enabled = isActive;
        menuBackground.SetActive(isActive);
    }
}
