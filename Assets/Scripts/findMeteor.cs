using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class findMeteor : MonoBehaviour
{

    public Image warning;

    // Update is called once per frame
    void Update()
    {
        GameObject myObject = GameObject.Find("Meteor(Clone)");
        if (myObject != null)
        {
            warning.enabled = true;
            warning.GetComponent<Animator>().Play("warningBlink");
        }
        else { warning.enabled = false; }




    }
}
