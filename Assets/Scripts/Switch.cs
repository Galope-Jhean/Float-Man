using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Switch : MonoBehaviour
{
    public GameObject[] background;
    public Button Prev;
    public Button Nextt;
    int index;
    void Start()
    {
        index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(index == 0)
        {
            background[0].gameObject.SetActive(true);
            Prev.interactable = false;
        }
    }

    public void Next()
    {
        index+=1;
        if (index <= 0)
        {
            index = 0;
            Prev.interactable = false;
        }
        else
        {
            Prev.interactable = true;
        }
        if (index >= 3)
        {
            index = 3;
            Nextt.interactable = false;
        }
        else
        {
            Nextt.interactable = true;
        }
        for (int i = 0; i < background.Length; i++)
        {
            background[i].gameObject.SetActive(false);
            background[index].gameObject.SetActive(true);
        }
    }

    public void Previous()
    {
        index-=1;
        if (index <= 0)
        {
            index = 0;
            Prev.interactable = false;
        }
        else
        {
            Prev.interactable = true;
        }
        if (index >= 3)
        {
            index = 3;
            Nextt.interactable = false;
        }
        else
        {
            Nextt.interactable = true;
        }
        for (int i = 0; i < background.Length; i++)
        {
            background[i].gameObject.SetActive(false);
            background[index].gameObject.SetActive(true);
        }
    }

    public void ResetIndex()
    {
        index = 0;

        for (int i = 0; i < background.Length; i++)
        {
            background[i].SetActive(false);
        }

        background[0].SetActive(true);

        Prev.interactable = false;
        Nextt.interactable = true;
    }
}
