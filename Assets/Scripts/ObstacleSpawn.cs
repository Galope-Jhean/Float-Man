using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawn : MonoBehaviour
{
    public GameObject Obstacle;
    public GameObject Obstacle1;
    public float maxX;
    public float minX;
    public float minY;
    public float maxY;
    public float TimeBetweenSpawn;
    public bool dimensionHop = false;
    private float SpawnTime;
    public Texture newBackgroundTexture;
    public Renderer backgroundRenderer;

    // Update is called once per frame
    void Update()
    {
        
        if(Time.time > SpawnTime && dimensionHop == false) {
            Spawn();
            SpawnTime = Time.time + TimeBetweenSpawn;
        }
    }

    void Spawn()
    {

        float X = Random.Range(minX, maxX);
        float Y = Random.Range(minY, maxY);
        if(backgroundRenderer.material.mainTexture == newBackgroundTexture)
        {
            Instantiate(Obstacle1, transform.position + new Vector3(X, Y, 0), transform.rotation);
        }
        else
        {
            Instantiate(Obstacle, transform.position + new Vector3(X, Y, 0), transform.rotation);
        }
                         
    }
}
