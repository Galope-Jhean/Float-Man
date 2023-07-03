using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class oxgn : MonoBehaviour
{
    public GameObject oxgnBar;
    public GameObject magic;
    private Slider slider;
    void Start()
    {
        slider = oxgnBar.GetComponentInChildren<Slider>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            Destroy(this.gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            magic.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}
