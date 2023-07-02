using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BackgroundLoop : MonoBehaviour 
{
    public float backgroundSpeed;
    public float cameraSpeedIncrement = 0.015f;
    public Renderer BackgroundRenderer;
    public Text scoreText;
    private int previousScore = 0;

    // Update is called once per frame
    void Update()
    {
        BackgroundRenderer.material.mainTextureOffset += new Vector2(backgroundSpeed * Time.deltaTime, 0f);
        int currentScore = int.Parse(scoreText.text);
        if (currentScore >= previousScore + 100 && backgroundSpeed < 1.1)
        {
            int scoreIncrement = (currentScore - previousScore) / 100;
            previousScore += scoreIncrement * 100;
            backgroundSpeed += scoreIncrement * cameraSpeedIncrement;
        }
    }
}
