using UnityEngine;
using System.Collections;

public class LoopingSprite : MonoBehaviour
{
    public Renderer renderer;
    public Vector2 scrollSpeed;
    Vector2 offset;

    void Update()
    {
        offset += scrollSpeed * Time.deltaTime;

        offset.x = offset.x % 1;//The mod means this will loop for positive numbers
        offset.y = offset.y % 1;

        if(offset.x < 0)//loop for negative numbers
            offset.x += 1;

        if(offset.y < 0)
            offset.y += 1;

        renderer.material.SetTextureOffset("_MainTex", offset);;//scroll the texture around
    }
}
