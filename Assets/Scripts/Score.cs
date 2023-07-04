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
    public float multiplier = 2.5f;

    private bool multiplierIncreased = false;

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
                score += 1 * (Time.deltaTime * (multiplier * 2));
            }
            else
            {
                score += 1 * (Time.deltaTime * multiplier);
            }

            ScoreText.text = ((int)score).ToString();

            if (int.Parse(ScoreText.text) % 100 == 0 && int.Parse(ScoreText.text) != 0 && !multiplierIncreased)
            {
                multiplierIncreased = true;
                multiplier += 0.5f;
            }

            if (int.Parse(ScoreText.text) % 100 != 0)
            {
                multiplierIncreased = false;
            }

            if (score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", (int)score);
                HighScore.text = ((int)score).ToString();
            }
        }
    }
}
