using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{


    public Text ScoreText;
    private float score;

    public Text HighScore;

    void Start()
    {
        HighScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            score += 1 * (Time.deltaTime * 3);
            ScoreText.text = ((int)score).ToString();

            if (score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", (int)score);
                HighScore.text = ((int)score).ToString();
            }
        }
    }
}
