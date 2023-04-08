using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// The GameManager.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// The singleton instance of the GameManager.
    /// </summary>
    public static GameManager instance;

    public enum GameState { TitleScreen, MainMenu, Options, PlayerCount, MapSettings, HostOrJoin, Host, Join, Lobby, GamePlay, GameOver, Credits, Pause }
    GameState gameState;
    public List<GameObject> canvases;

    // List that holds our player(s)
    public List<PlayerController> players;
    //All the entities that are not players.
    public List<AIController> npcs; //All pawns can be derived from the controllers.
    //All the pickups currently active.
    public List<Pickup> pickups;

    //Prefabs
    [SerializeField]
    private GameObject playerControllerPrefab;
    [SerializeField]
    private GameObject tankPawnPrefab;
    #endregion

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            gameState = GameState.TitleScreen;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// The function to spawn the player and link the pawn to the controller.
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject newPlayerObj = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity);
        GameObject newPawnObj = Instantiate(tankPawnPrefab, FindObjectOfType<MapGenerator>().RandomRoom().playerSpawn.transform.position, Quaternion.identity);

        PlayerController newController = newPlayerObj.GetComponent<PlayerController>();
        TankPawn newPawn = newPawnObj.GetComponent<TankPawn>();

        newController.pawn = newPawn;

        //Add the spawned player to the variable so it can be accessed from anywhere. 
        players.Add(newController);
    }

    /// <summary>
    /// The function to respawn the player after it died. It does not spawn another controller.
    /// </summary>
    public void RespawnPlayer()
    {

        GameObject newPawnObj = Instantiate(tankPawnPrefab, FindObjectOfType<MapGenerator>().RandomRoom().playerSpawn.transform.position, Quaternion.identity);
        TankPawn newPawn = newPawnObj.GetComponent<TankPawn>();

        //Finds which controller is missing a pawn and assign it the new pawn.
        foreach (PlayerController playerController in players)
        {
            if (playerController.pawn == null)
            {
                if (newPawn != null)
                {
                    playerController.pawn = newPawn;
                }
                else
                    Debug.Log("The TankPawn prefab is missing it's TankPawn script!");

                break;
            }
        }

        foreach (AIController enemy in npcs)
        {
            enemy.target = newPawn.gameObject;
        }

    }

    #region GameState functions
    /// <summary>
    /// Activates and Deactivates UI canvases based on which state is changing.
    /// </summary>
    /// <param name="newState"></param>
    public void GameStateChange(GameState newState)
    {
        foreach (GameObject canvas in canvases)
        {
            UI_Object ui = canvas.GetComponent<UI_Object>();
            if (ui != null)
            {
                if (ui.typeUI == gameState)
                {
                    canvas.SetActive(false);
                }
                if (ui.typeUI == newState)
                {
                    canvas.SetActive(true);
                }
            }
        }

        gameState = newState;
    }

    [HideInInspector]
    public int mapRows = 1;
    [HideInInspector]
    public int mapColumns = 1;
    [HideInInspector]
    public bool isRandomSeed = true;
    [HideInInspector]
    public bool isDaySeed = false;
    [HideInInspector]
    public int customSeed = 0;
    [SerializeField]
    private AudioSource soundtrackAudioSource;
    [SerializeField]
    private AudioSource uiAudioSource;
    [SerializeField]
    private AudioClip mainMenu;
    [SerializeField]
    private AudioClip gamePlay;
    [SerializeField]
    private AudioClip uiSoundEffect;
    [SerializeField]
    private AudioMixer audioMixer;

    public enum BackgroundMusicGroups { MainMenu, Gameplay }
    public void ChangeBackgroundMusic(BackgroundMusicGroups group)
    {
        switch (group)
        {
            case BackgroundMusicGroups.MainMenu:
                soundtrackAudioSource.clip = mainMenu;
                soundtrackAudioSource.Play();
                break;
            case BackgroundMusicGroups.Gameplay:
                soundtrackAudioSource.clip = gamePlay;
                soundtrackAudioSource.Play();
                break;
            default:
                Debug.LogWarning("Music enum went out of scope in GameManager.");
                break;
        }
    }

    public void PlayUISoundEffect()
    {
        uiAudioSource.PlayOneShot(uiSoundEffect);
    }

    public void AdjustVolumeMix(string groupVolumeName, float newVolume)
    {
        //Cap the minimum at 0
        if(newVolume <= 0)
        {
            newVolume = -80;
        }
        else
        {
            newVolume = Mathf.Log10(newVolume);
            newVolume = newVolume * 20;
        }

        Debug.Log(newVolume);

        //Set the group's volume.
        audioMixer.SetFloat(groupVolumeName, newVolume);
    }
    #endregion GameState functions
}
