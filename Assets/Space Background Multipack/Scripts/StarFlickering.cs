using UnityEngine;
using System.Collections;

public class StarFlickering : MonoBehaviour
{
    public Renderer renderer;
    public Vector2 scrollSpeed, flickerSpeed;

    Vector2 offset, flickerOffset;

    public bool enableFlickering;

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        flickerOffset += flickerSpeed * Time.deltaTime;

        offset.x = LoopAndClampValue(offset.x);
        offset.y = LoopAndClampValue(offset.y);

        flickerOffset.x = LoopAndClampValue(flickerOffset.x);
        flickerOffset.y = LoopAndClampValue(flickerOffset.y);


        renderer.material.SetTextureOffset("_MainTex", offset);
        renderer.material.SetTextureOffset("_FlickerTex", flickerOffset);
        renderer.material.SetFloat("_isFlickering", enableFlickering ? 1 : 0);
    }

    float LoopAndClampValue(float input)
    {
        input %= 1;

        if(input < 0)
            input += 1;

        return input;
    }
}
