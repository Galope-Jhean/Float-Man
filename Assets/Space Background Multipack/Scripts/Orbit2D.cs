using UnityEngine;
using System.Collections;

public class Orbit2D : MonoBehaviour
{
    public Transform pivotObject;
    public float speed = 1;
    public float distance;

    public Transform direction1;

    void Awake()
    {
        distance = Vector3.Distance(transform.position, pivotObject.position);//how far to maintain the orbit, based off the current distance
    }

    void Update()
    {
        direction1.Rotate(Vector3.forward,  speed * Time.deltaTime, Space.Self);

        transform.position = pivotObject.position;
        transform.position += direction1.right * distance;
    }
}
