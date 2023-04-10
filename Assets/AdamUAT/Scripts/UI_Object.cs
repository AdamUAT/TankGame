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
    [SerializeField]
    private Slider playerCount;
    [SerializeField]
    private List<RawImage> lives;
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private TextMeshProUGUI score;

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

    public void OptionsToPause()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Pause);
    }
    public void PauseToOptions()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.Options);
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
    public void MainMenuToMapSettings()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MapSettings);
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
        if(playerCount != null)
        {
            GameManager.instance.playerCount = (int)playerCount.value;
        }

        SceneManager.LoadScene("MapTest");
        GameManager.instance.GameStateChange(GameManager.GameState.GamePlay);
        GameManager.instance.ChangeBackgroundMusic(GameManager.BackgroundMusicGroups.Gameplay);
    }

    public void ToggleRandomSeed()
    {
        GameManager.instance.isRandomSeed = !GameManager.instance.isRandomSeed;
    }
    public void ToggleDaySeed()
    {
        GameManager.instance.isDaySeed = !GameManager.instance.isDaySeed;
    }

    public void PlayUISound()
    {
        GameManager.instance.PlayUISoundEffect();
    }

    public void AdjustSoundtrackVolume(Slider slider)
    {
        GameManager.instance.AdjustVolumeMix("SoundtrackVolume", slider.value);
    }
    public void AdjustEffectsVolume(Slider slider)
    {
        GameManager.instance.AdjustVolumeMix("EffectsVolume", slider.value);
    }

    public void PauseToGamePlay()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.GamePlay);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void PauseToMainMenu()
    {
        GameManager.instance.GameStateChange(GameManager.GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
        GameManager.instance.ChangeBackgroundMusic(GameManager.BackgroundMusicGroups.MainMenu);
    }
    
    public void UpdateLifeDisplay(int livesRemaining)
    {
        if(lives.Count > 0)
        {
            //Sets all to inactive.
            foreach(RawImage life in lives)
            {
                life.gameObject.SetActive(false);
            }

            //Sets the lives to active. The order in which the sprites are put into the list matters.
            for(int i = 1; i <= livesRemaining; i++)
            {
                lives[i].gameObject.SetActive(true);
            }
        }
    }

    public void UpdateHealthBar(float percentHealth)
    {
        healthBar.value = percentHealth;
    }

    public void UpdateScore(long newScore)
    {
        score.text = newScore.ToString();
    }
}
