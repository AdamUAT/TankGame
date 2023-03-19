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
    [SerializeField]
    private Transform playerSpawnTransform;
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

        SpawnPlayer(); //SpawnPlayer needs to be first thing so other scripts can access it.
    }

    /// <summary>
    /// The function to spawn the player and link the pawn to the controller.
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject newPlayerObj = Instantiate(playerControllerPrefab, Vector3.zero, Quaternion.identity);
        GameObject newPawnObj = Instantiate(tankPawnPrefab, playerSpawnTransform.position, playerSpawnTransform.rotation);

        PlayerController newController = newPlayerObj.GetComponent<PlayerController>();
        TankPawn newPawn = newPawnObj.GetComponent<TankPawn>();

        newController.pawn = newPawn;

        //Add the spawned player to the variable so it can be accessed from anywhere. 
        players.Add(newController);
    }
}
