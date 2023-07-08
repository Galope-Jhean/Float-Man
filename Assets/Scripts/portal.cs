using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class portal : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject player;
    public Renderer backgroundRenderer;
    public Texture newBackgroundTexture;
    public Texture ogBackgroundTexture;
    public Texture oneMoreTexture;
    public string obstacleTag = "Obstacle";
    private bool hasCollided = false;
    private GameObject meteor;
    public AudioSource portalSound;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");        
    }

    private void Update()
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            Destroy(gameObject);
        }

        if (collision.CompareTag("Player") && !hasCollided)
        {
            hasCollided = true;
            SwitchDimension();
            portalSound.enabled = true;
            portalSound.Play();
        }
    }

    private void SwitchDimension()
    {
        meteor = GameObject.FindWithTag("Meteor");
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        if (backgroundRenderer.material.mainTexture == ogBackgroundTexture)
        {
            backgroundRenderer.material.mainTexture = newBackgroundTexture;
        }
        else if(backgroundRenderer.material.mainTexture == newBackgroundTexture)
        {
            backgroundRenderer.material.mainTexture = oneMoreTexture;
        }
        else
        {
            backgroundRenderer.material.mainTexture = ogBackgroundTexture;
        }

        Destroy(gameObject);
        Destroy(meteor);
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
    }
}
