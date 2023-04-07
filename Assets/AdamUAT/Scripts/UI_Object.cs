using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Object : MonoBehaviour
{
    public GameManager.GameState typeUI;

    [SerializeField]
    private TMP_InputField columns;
    [SerializeField]
    private TMP_InputField rows; 
    [SerializeField]
    private TMP_InputField seed;

    void Start()
    {
        GameManager.instance.canvases.Add(gameObject);
        if(typeUI != GameManager.GameState.TitleScreen && typeUI != GameManager.GameState.GamePlay)
            gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        GameManager.instance.canvases.Remove(gameObject);
    }

    public void TitleScreenToMainMenu()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MainMenu);
    }
    public void MainMenuToOptions()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Options);
    }
    public void OptionsToMainMenu()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MainMenu);
    }
    public void MainMenuToCredits()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Credits);
    }
    public void CreditsToMainMenu()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MainMenu);
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
    public void MainMenuToPlayerCount()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.PlayerCount);
    }
    public void PlayerCountToMapSettings()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MapSettings);
    }
    public void PlayerCountToHostOrJoin()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.HostOrJoin);
    }
    public void HostOrJoinToHost()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Host);
    }
    public void HostOrJoinToJoin()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Join);
    }
    public void HostToLobby()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Lobby);
    }
    public void JoinToLobby()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Lobby);
    }
    public void MapSettingsToGamePlay()
    {
        if(columns != null)
        {
            if(columns.text != "")
            {
                GameManager.instance.mapColumns = int.Parse(columns.text);
            }
            else
            {
                GameManager.instance.mapColumns = 3;
            }
        }
        if (rows != null)
        {
            if (rows.text != "")
            {
                GameManager.instance.mapRows = int.Parse(rows.text);
            }
            else
            {
                GameManager.instance.mapRows = 3;
            }
        }
        if(seed != null)
        {
            if (!GameManager.instance.isRandomSeed && !GameManager.instance.isDaySeed && seed.text != "")
            {
                GameManager.instance.customSeed = int.Parse(seed.text);
            }
        }

        SceneManager.LoadScene("MapTest");
        GameManager.instance.GameStateChange(GameManager.GameState.GamePlay);
    }

    public void ToggleRandomSeed()
    {
        GameManager.instance.isRandomSeed = !GameManager.instance.isRandomSeed;
    }
    public void ToggleDaySeed()
    {
        GameManager.instance.isDaySeed = !GameManager.instance.isDaySeed;
    }
}
