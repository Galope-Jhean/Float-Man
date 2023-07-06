using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class ParralaxItem : MonoBehaviour
{
    public Vector3 minDirection;
    public Vector3 maxDirection;

    public Vector3 rotationAxis;
    public float rotationSpeed;
    Vector3 direction;

	public float scale = 1;


    void Start()
    {
        direction = new Vector3(Random.Range(minDirection.x, maxDirection.x), Random.Range(minDirection.y, maxDirection.y), Random.Range(minDirection.z, maxDirection.z));
    }

    void Update()
    {
        transform.position += direction * Time.deltaTime * scale;

        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
