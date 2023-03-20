using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        foreach(PlayerController playerController in players)
        {
            if(playerController.pawn == null)
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
}
