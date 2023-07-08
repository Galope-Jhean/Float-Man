using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class userPrompt : MonoBehaviour
{

    public Text text;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        time += 1 * (Time.deltaTime);

        if (time > 3) {
            text.text = "";
        }
    }
}
