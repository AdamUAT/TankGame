using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMover : Mover
{
    //Makes the tank obey physics
    private CharacterController cc;

    public override void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    public override void Move(Vector3 direction, float speed)
    {
        //Moving the player with a Character Controller component is much easier than the Rigidbody, because now we don't have to worry about physics.
        cc.SimpleMove(direction * speed);
    }

    public override void Rotate(float speed)
    {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
    }
}
