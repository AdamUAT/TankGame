using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    //The singleton of this manager.
    public static NavMeshManager instance;

    //The NavMesh for the entire map.
    public NavMeshSurface globalNavMesh;

    //The NavMeshes that individual tanks use, such as Patrol tanks.
    public List<NavMeshSurface> personalNavMeshes;

    //Assigns the singleton instance.
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GenerateGlobalNavMesh()
    {
        globalNavMesh.BuildNavMesh();
    }
}
