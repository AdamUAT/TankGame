using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    public virtual void MoveForward() { }
    public virtual void MoveBackward() { }
    public virtual void MoveTo(Vector3 target) { }
    public virtual void StopMoving() { }
    public virtual bool IsMoving() { return false; }
    public abstract void RotateClockwise();
    public abstract void RotateCounterClockwise();
    public abstract void Shoot();
}
