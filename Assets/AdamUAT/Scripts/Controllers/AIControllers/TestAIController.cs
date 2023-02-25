using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestAIController : AIController
{
    // Start is called before the first frame update
    public override void Start()
    {
        navMesh = NavMeshManager.instance.globalNavMesh;

        Seek(new Vector3(-45, 1, -12));

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
