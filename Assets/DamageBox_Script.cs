using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBox_Script : MonoBehaviour {

    protected Vector2 pushForce_;

    public DamageBox_Script(Vector2 pushForce)
    {
        pushForce_ = pushForce;
    }
    void OnCollisionEnter2D(Collision2D coll)
    {
        
        coll.rigidbody.AddForce(pushForce_);

    }
}
