using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

public class TankMover : MonoBehaviour
{
    //Makes the tank obey physics
    [SerializeField]
    protected CharacterController cc;

    //Used to determine the direction of movement
    public GameObject body;

    //Used to determine where the tank is facing and how it will fire.
    public GameObject turret;

    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected float turnSpeed;

    //PlayerMover controlls cameraController since TankPawn could also belong to enemies.
    //Also, rotation changes how the camera looks so it makes since.
    //Since PlayerMover is accessable from the pawn, CameraController is also accessible from the pawn, but it needs to be in the parent class TankMover.
    [HideInInspector]
    public CameraController cameraController;



    protected virtual void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    /// <summary>
    /// Moves the tank either forward or backward.
    /// </summary>
    /// <param name="speed">Overrides how fast the tank should move. Sign tells direction.</param>
    public void Move(float speed)
    {
        //Moving the player with a Character Controller component is much easier than the Rigidbody, because now we don't have to worry about physics.
        cc.SimpleMove(body.transform.forward * speed);
    }

    /// <summary>
    /// Moves the tank either forward or backward.
    /// </summary>
    /// <param name="forward">Whether or not the tank should move forward or backward.</param>
    public void Move(bool forward = true)
    {
        //Moving the player with a Character Controller component is much easier than the Rigidbody, because now we don't have to worry about physics.
        if (forward)
        {
            cc.SimpleMove(body.transform.forward * moveSpeed);
        }
        else
        {
            cc.SimpleMove(body.transform.forward * -moveSpeed);
        }
    }

    /// <summary>
    /// Rotates just the body of the tank
    /// </summary>
    /// <param name="speed">How fast the tank rotates.</param>
    public void BodyRotate(float speed)
    {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
        turret.transform.Rotate(new Vector3(0, -speed * Time.deltaTime, 0)); //Rotates the turret in the opposite direction so it doesn't change.
    }

    /// <summary>
    /// Rotates just the body of the tank. Uses the speed provided by the TankMover component.
    /// </summary>
    ///<param name="clockwise">Whether the tank moves clockwise or anticlockwise.</param>
    public void BodyRotate(bool clockwise)
    {
        if (clockwise)
        {
            body.transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0));
            //turret.transform.Rotate(new Vector3(0, -turnSpeed * Time.deltaTime, 0)); //Rotates the turret in the opposite direction so it doesn't change.
        }
        else
        {
            body.transform.Rotate(new Vector3(0, -turnSpeed * Time.deltaTime, 0));
            //turret.transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0)); //Rotates the turret in the opposite direction so it doesn't change.
        }
    }

    /// <summary>
    /// Rotates the tank towards a specific location, instead of a direction.
    /// </summary>
    /// <param name="targetPosition">The location the tank will rotate towards.</param>
    public virtual void BodyRotateTowards(Vector3 targetPosition)
    {
        Vector3 vectorToTarget = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Rotates the turret towards a specific location, instead of a direction.
    /// </summary>
    /// <param name="targetPosition">The location the tank will rotate towards.</param>
    /// <param name="_turnSpeed">How fast the tank will rotate towards the target.</param>
    public virtual void TurretRotateTowards(Vector3 targetPosition, float _turnSpeed)
    {
        Vector3 vectorToTarget = targetPosition - turret.transform.position;
        vectorToTarget.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);

        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Rotates the turret to the degree specified.
    /// </summary>
    /// <param name="degree">The new rotation in, euler angles, the turret will have.</param>
    public void TurretInstantRotate(float degree)
    {
        turret.transform.eulerAngles = new Vector3(0, degree, 0);
    }

    /// <summary>
    /// Rotates the turret so it is facing a certain position.
    /// </summary>
    /// <param name="target">The position the turret will be looking at.</param>
    public void TurretRotateTowards(Vector3 target)
    {
        turret.transform.LookAt(target);
        //Prevents the turret from angleing down.
        turret.transform.eulerAngles = new Vector3(0, turret.transform.eulerAngles.y, 0);
    }

    /// <summary>
    /// Rotates the turret so it is facing a certain GameObject.
    /// </summary>
    /// <param name="target">The GameObject the turret will be looking at.</param>
    public void TurretRotateTowards(GameObject target)
    {
        turret.transform.LookAt(target.transform.position);
        //Prevents the turret from angleing down.
        turret.transform.eulerAngles = new Vector3(0, turret.transform.eulerAngles.y, 0);
    }

    /// <summary>
    /// Adds rotation to the turret.
    /// </summary>
    /// <param name="amount">The amount of degrees per second the turret rotates. The sign tells direction.</param>
    public void TurretRotate(float amount)
    {
        turret.transform.Rotate(new Vector3(0, amount * Time.deltaTime, 0));
    }

    /// <summary>
    /// Angles the turret in degrees relative to the body.
    /// </summary>
    /// <param name="eulerAngle">The angle, in degrees, the turret should be rotated towards. 0 is the direction of the body, and it goes clockwise.</param>
    /// <param name="turretRotationSpeed">The speed at which it rotates towards the target rotation.</param>
    public void TurretRotateAngle(float eulerAngle, float turretRotationSpeed)
    {
        turret.transform.rotation = Quaternion.RotateTowards(turret.transform.rotation, Quaternion.Euler(0, eulerAngle + body.transform.eulerAngles.y, 0), turretRotationSpeed * Time.deltaTime);
    }

    #region NavMesh-based movement virtual functions
    /// <summary>
    /// Tells the NavMeshAgent of this pawn to move to a location.
    /// </summary>
    /// <param name="target">The location to move to.</param>
    public virtual void MoveTo(Vector3 target) { }

    /// <summary>
    /// Cancels the current path for the NavMeshAgent.
    /// </summary>
    public virtual void StopMoving() { }

    /// <summary>
    /// Determines if the NavMeshAgent is trying to move.
    /// </summary>
    /// <returns>True if the NavMeshAgent is moving.</returns>
    public virtual bool IsMoving() { return false; }
    #endregion NavMesh-based movement virtual functions
}
