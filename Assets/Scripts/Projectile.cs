using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour {

    public Rigidbody rb { get; private set; }

    public float moveSpeed = 2f;
    public float lifeTime = 2f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


}
