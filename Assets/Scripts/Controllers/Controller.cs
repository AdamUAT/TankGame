using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    public Pawn pawn;

    // Start is called before the first frame update
    public virtual void Start() {}
    public virtual void Update() {}
    public virtual void LateUpdate() {}

}
