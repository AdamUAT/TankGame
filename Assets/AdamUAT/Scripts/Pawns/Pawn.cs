using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    [HideInInspector]
    public TankMover mover;

    protected virtual void Start()
    {
        mover = GetComponent<TankMover>();
    }
}
