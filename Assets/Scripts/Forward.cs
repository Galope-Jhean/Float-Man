using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forward : MonoBehaviour
{
    public float forwardSpeed = 2f;

    void Start()
    {
        // Automatically move the object forward
        MoveObjectForward();
    }

    void Update()
    {
        // Additional game logic and input handling
        MoveObjectForward();
    }

    void MoveObjectForward()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(forwardSpeed, rb.velocity.y);
    }
}
