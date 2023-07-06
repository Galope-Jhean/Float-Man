using UnityEngine;
using System.Collections;

public class AsteroidBelt : MonoBehaviour
{
    public int asteroidCount;
    public string asteroidSpriteSheetTag;
    public Sprite[] asteroids;
    public AsteroidBeltOrbit asteroidPrefab;

    public Transform startPoint;//this essentially determines the distance/rage of the belt

    public float minSpeed, maxSpeed;
    public Vector2 minSpread, maxSpread;//this is the inital spread between asteroids. Think of this as the belts thickness. A low spread means the asteroids are tightly packed, a wide one means there is a huge distance between each asteroid
    public float minDensity = 2, maxDensity = 2.5f;//this is the space in the orbit between each asteroid. I recommend keeping this above 1 or you will get gaps!

    void Start()
    {
        Transform transform = base.transform;//cache this since we will use it a lot
        int imageIndex = 0;//ensure a wide spread of the images are used
        GameObject current;
        AsteroidBeltOrbit currentOrbit;
        float baseFractionPerAsteroid = 360f / asteroidCount;//helps to determine density

        for(int i = 0; i < asteroidCount; i++)
        {
            current = GameObject.Instantiate(asteroidPrefab.gameObject, startPoint.position, startPoint.rotation) as GameObject;

            currentOrbit = current.GetComponent<AsteroidBeltOrbit>();
            currentOrbit.offset = baseFractionPerAsteroid * Random.Range(minDensity, maxDensity) * i;//set the position within the belt
            currentOrbit.spriteRenderer.sprite = asteroids[imageIndex];//set the srpite
            currentOrbit.pivotObject = transform;
            currentOrbit.direction1 = transform;
            currentOrbit.speed = Random.Range(minSpeed, maxSpeed);

            imageIndex = (imageIndex + 1) % asteroids.Length;

            current.transform.position = startPoint.position + new Vector3(Random.Range(minSpread.x, maxSpread.x), Random.Range(minSpread.y, maxSpread.y), 0);//scatter the asteroid slightly
            currentOrbit.transform.parent = transform.parent;
        }
    }
}
