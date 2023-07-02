using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillStatusBar : MonoBehaviour
{
    public GameObject oxgn;
    public Image fillImage;
    private Slider slider;
    private float maxOxygen = 100;
    private float elapsedTime;
    private GameObject player;
    private float multiply = 0.8f;

    // Start is called before the first frame update
    void Awake()
    {
        slider = GetComponent<Slider>();
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if( player != null )
        {

            if(slider.value <= slider.minValue)
            {
                fillImage.enabled = false;
            }

            if(player.transform.localScale == new Vector3(0.3653499f, 0.3761048f, 1f))
            {
                multiply = 1.86f;
            }
            else
            {
                multiply = 0.83f;
            }

            elapsedTime += Time.deltaTime * multiply;
            float fillValue = maxOxygen - elapsedTime;
            slider.value = fillValue;
        }
        else
        {
            oxgn.SetActive(false);
        }

        if(slider.value == 0)
        {
            Destroy( player );
        }
        
    }
}