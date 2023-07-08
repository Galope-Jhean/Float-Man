using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private GameObject Player;
    public AudioSource deathSound;
    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Border") {
            Destroy(this.gameObject);
        }

        else if (collision.tag == "Player")
        {
            deathSound.Play();
            Destroy(Player.gameObject);
        }
    }
}
