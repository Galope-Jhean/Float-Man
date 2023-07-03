using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{


    public Text ScoreText;
    private float score;
    private GameObject player;
    public Text HighScore;

    void Start()
    {
        HighScore.text = PlayerPrefs.GetInt("HighScore", 0).ToString();
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            

            if (player.transform.localScale == new Vector3(0.3653499f, 0.3761048f, 1f))
            {
                score += 1 * (Time.deltaTime * 6);
            }
            else
            {
                score += 1 * (Time.deltaTime * 3); 
            }

            ScoreText.text = ((int)score).ToString();

            if (score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", (int)score);
                HighScore.text = ((int)score).ToString();
            }
        }
    }
}
