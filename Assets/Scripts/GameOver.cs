using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject GameOverPanel;
    public GameObject scoreUI;
    // Update is called once per frame
    void Update()
    {

        
        if (GameObject.FindWithTag("Player") == null)
        {
            GameOverPanel.SetActive(true);
            scoreUI.SetActive(false);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Home()
    {
        SceneManager.LoadScene("Home");
    }
}
