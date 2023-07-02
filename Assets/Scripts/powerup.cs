using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;


public class powerup : MonoBehaviour
{
    private GameObject player;
    public Vector3 collisionScale = new Vector3(0.3653499f, 0.3761048f, 1f);

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            player.transform.localScale = collisionScale;
        }
    }
}
