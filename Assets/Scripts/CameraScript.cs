using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CameraScript : MonoBehaviour
{
    public float initialCameraSpeed = 1f;
    public float cameraSpeedIncrement = 0.5f;
    public Text scoreText;

    private int previousScore = 0;

    void Update()
    {
        transform.position += new Vector3(initialCameraSpeed * Time.deltaTime, 0, 0);

        int currentScore = int.Parse(scoreText.text);
        if (currentScore >= previousScore + 100 && initialCameraSpeed < 8)
        {
            int scoreIncrement = (currentScore - previousScore) / 100;
            previousScore += scoreIncrement * 100;
            initialCameraSpeed += scoreIncrement * cameraSpeedIncrement;
        }
    }
}
