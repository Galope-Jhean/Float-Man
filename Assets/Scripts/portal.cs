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
    public string obstacleTag = "Obstacle";
    private bool hasCollided = false;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
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
            Debug.Log("Dimension Switched");
        }
    }

    private void SwitchDimension()
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(obstacleTag);
        if (backgroundRenderer.material.mainTexture == ogBackgroundTexture)
        {
            backgroundRenderer.material.mainTexture = newBackgroundTexture;
        }
        else
        {
            backgroundRenderer.material.mainTexture = ogBackgroundTexture;
        }

        Destroy(gameObject);
        foreach (GameObject obstacle in obstacles)
        {
            Destroy(obstacle);
        }
    }
}
