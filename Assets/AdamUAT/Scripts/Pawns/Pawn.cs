using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Pawn : MonoBehaviour
{
    protected Mover mover;
    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected float turnSpeed;

    // Start is called before the first frame update
    public virtual void Start()
    {
        mover = GetComponent<Mover>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void MoveForward() { }
    public virtual void MoveBackward() { }
    public virtual void MoveTo(Vector3 target) { }
    public virtual void StopMoving() { }
    public virtual bool IsMoving() { return false; }
    public abstract void RotateClockwise();
    public abstract void RotateCounterClockwise();
    public virtual void RotateTowards(Vector3 targetPosition)
    {
        Vector3 vectorToTarget = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
    //Overloaded version that has its own turnSpeed instead of using the pawn's.
    public virtual void RotateTowards(Vector3 targetPosition, float _turnSpeed)
    {
        Vector3 vectorToTarget = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }
    public abstract void Shoot();
}
