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
    [SerializeField]
    protected GameObject body;

    //Used to determine where the tank is facing and how it will fire.
    [SerializeField]
    protected GameObject turret;

    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected float turnSpeed;

    public void Start()
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
            transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0));
            turret.transform.Rotate(new Vector3(0, -turnSpeed * Time.deltaTime, 0)); //Rotates the turret in the opposite direction so it doesn't change.
        }
        else
        {
            transform.Rotate(new Vector3(0, -turnSpeed * Time.deltaTime, 0));
            turret.transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0)); //Rotates the turret in the opposite direction so it doesn't change.
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
    /// Rotates the tank towards a specific location, instead of a direction.
    /// </summary>
    /// <param name="targetPosition">The location the tank will rotate towards.</param>
    /// <param name="_turnSpeed">How fast the tank will rotate towards the target.</param>
    public virtual void RotateTowards(Vector3 targetPosition, float _turnSpeed)
    {
        Vector3 vectorToTarget = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Rotates the turret to the degree specified.
    /// </summary>
    /// <param name="degree">The new rotation in, euler angles, the turret will have.</param>
    public void TurretRotateTo(float degree)
    {
        turret.transform.eulerAngles = new Vector3(0, degree, 0);
    }

}
