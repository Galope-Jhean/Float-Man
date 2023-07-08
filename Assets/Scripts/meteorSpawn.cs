using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class meteorSpawn : MonoBehaviour
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
    public Text score;

    private bool isFirstGameplay = true; // Flag to track first gameplay

    private const string FIRST_GAMEPLAY_KEY = "FirstGameplay"; // Key for PlayerPrefs

    private void Start()
    {
        // Check PlayerPrefs for first gameplay
        if (PlayerPrefs.HasKey(FIRST_GAMEPLAY_KEY))
        {
            isFirstGameplay = false;
        }
        else
        {
            PlayerPrefs.SetInt(FIRST_GAMEPLAY_KEY, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFirstGameplay)
        {
            if (Time.time > 15f && Time.time > SpawnTime && dimensionHop == false && int.Parse(score.text) > GetRandomNumber(30, 51))
            {
                Spawn();
                SpawnTime = Time.time + TimeBetweenSpawn;
            }
        }
        else
        {
            if (Time.time > SpawnTime && dimensionHop == false)
            {
                SpawnTime = Time.time + GetRandomNumber(25, 36);
                Spawn();
            }
        }
    }

    void Spawn()
    {
        float X = Random.Range(minX, maxX);
        float Y = Random.Range(minY, maxY);

        if (backgroundRenderer.material.mainTexture == newBackgroundTexture)
        {
            Instantiate(Obstacle1, transform.position + new Vector3(X, Y, 0), transform.rotation);
        }
        else
        {
            Instantiate(Obstacle, transform.position + new Vector3(X, Y, 0), transform.rotation);
        }
    }

    int GetRandomNumber(int minimum, int maximum)
    {
        return Random.Range(minimum, maximum);
    }

    public void ResetGame()
    {
        isFirstGameplay = false; // Resetting the first gameplay flag
        PlayerPrefs.DeleteKey(FIRST_GAMEPLAY_KEY); // Clearing the first gameplay key from PlayerPrefs
        SpawnTime = Time.time + GetRandomNumber(25, 36); // Randomize the initial spawn time
    }
}
