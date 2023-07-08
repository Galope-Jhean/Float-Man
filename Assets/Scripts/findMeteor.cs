using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class findMeteor : MonoBehaviour
{
    public Image warning;
    public Image portal;
    private float timer;
    public AudioSource warningSound;
    public AudioSource meteorSound;
    // Update is called once per frame
     
    void Update()
    {
        GameObject myObject = GameObject.FindWithTag("Meteor");
        if (myObject != null)
        {
            timer += Time.deltaTime;
            if (timer <= 2f)
            {
                warning.enabled = true;
                warning.GetComponent<Animator>().Play("warningBlink");

                if (!warningSound.isPlaying)
                {
                    warningSound.Play();
                    meteorSound.Play();
                }
            }
            else
            {
                warning.enabled = false;
                warningSound.Stop();
            }
        }
        else
        {
            warning.enabled = false;
            timer = 0f;
            warningSound.Stop();
        }
    }
}
