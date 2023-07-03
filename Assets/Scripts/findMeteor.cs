using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class findMeteor : MonoBehaviour
{

    public Image warning;
    public Image portal;
    private float timer;

    // Update is called once per frame
    void Update()
    {
        GameObject myObject = GameObject.FindWithTag("Meteor");
        if (myObject != null)
        {
            timer += Time.deltaTime;

            if (timer <= 3f)
            {
                warning.enabled = true;
                warning.GetComponent<Animator>().Play("warningBlink");
            }
            else
            {
                warning.enabled = false;
            }
        }
        else
        {
            warning.enabled = false;
            timer = 0f;
        }
    }
}
