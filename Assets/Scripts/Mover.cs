using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Mover : MonoBehaviour
{
    public abstract void Start();
    public abstract void Move(Vector3 direction, float speed);
    //The sign of speed tells direction.
    public abstract void Rotate(float speed);
}
