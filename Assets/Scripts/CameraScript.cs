using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class CameraScript : MonoBehaviour
{
    public float initialCameraSpeed = 1f;
    public float cameraSpeedIncrement = 0.5f;
    private float currentSpeed;
    public Text scoreText;
    private GameObject player;
    private int previousScore = 0;
    private bool hasDoubled = false;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        currentSpeed = initialCameraSpeed;
        transform.position += new Vector3(initialCameraSpeed * Time.deltaTime, 0, 0);

        int currentScore = int.Parse(scoreText.text);
        if (currentScore >= previousScore + 100 && initialCameraSpeed < 8)
        {
            int scoreIncrement = (currentScore - previousScore) / 100;
            previousScore += scoreIncrement * 100;
            initialCameraSpeed += scoreIncrement * cameraSpeedIncrement;
        }
        if(player != null)
        {
            if (player.transform.localScale == new Vector3(0.3653499f, 0.3761048f, 1f) && hasDoubled == false)
            {
                initialCameraSpeed *= 1.5f;
                hasDoubled = true;
            }
            else if (hasDoubled == true && player.transform.localScale == new Vector3(0.8498486f, 0.8686022f, 1f))
            {
                initialCameraSpeed /= 1.5f;
                hasDoubled = false;
            }
        }
    }
}
