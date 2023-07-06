using UnityEngine;
using System.Collections;

public class AsteroidBeltOrbit : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;//not always needed! Mainly used by asteroids
    public Transform pivotObject;
    public float speed = 1;
    public float distance;
    public float offset;
    float totalRotation;

    public Transform direction1;
    Vector3 startingForward;

    void Start()
    {
        startingForward = direction1.forward;
        distance = Vector3.Distance(transform.position, pivotObject.position);//how far to maintain the orbit, based off the current distance
        totalRotation = offset;
    }

    void Update()
    {
        totalRotation += speed * Time.deltaTime;

        direction1.forward = startingForward;
        direction1.Rotate(Vector3.forward, totalRotation, Space.Self);

        transform.position = pivotObject.position;
        transform.position += direction1.right * distance;
    }
}
