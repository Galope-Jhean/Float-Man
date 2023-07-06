using UnityEngine;
using System.Collections;

public class ShieldPulse : MonoBehaviour
{
    public float minAlpha;
    public float minPulseTime, maxPulseTime;
    public Renderer mainRenderer;

    float cycleDuration;
    float pulseTime, pulseDirection = - 1;

    void Start()
    {
        cycleDuration = Random.Range(minPulseTime, maxPulseTime);
    }

    void Update()
    {
        pulseTime += Time.deltaTime * pulseDirection;

        mainRenderer.material.SetColor("_Color", new Color(1,1,1, Lerp(minAlpha, 1, pulseTime / cycleDuration)));

        if(pulseTime > cycleDuration)
            pulseDirection *= -1;//switch direction
        else if(pulseTime < 0)
        {
            pulseDirection *= -1;//switch direction
            cycleDuration = Random.Range(minPulseTime, maxPulseTime);//this will randomly change the pulse time. comment this out if you want a more consistent pulse
        }
    }

    /// <summary>
    /// Returns the value that is the defined percentage between both values
    /// </summary>
    public static float Lerp(float min, float max, float percentageBetween)
    {
        float differenceSign = (min - max > 0 ? -1 : 1);//if the difference is negative then subtract the value at the end!

        return min + Mathf.Abs(min - max) * percentageBetween * differenceSign;
    }
}
