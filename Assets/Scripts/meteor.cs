using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meteor : MonoBehaviour
{

    public float speed = 10;
    private Rigidbody2D rb;
    private Vector2 screenBound;
    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2 (-speed, -5); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
