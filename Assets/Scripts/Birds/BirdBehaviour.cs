using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdBehaviour : MonoBehaviour
{
    private float velocity;
    private Vector3 direction;



    // Random seed.
    float noiseOffset;

    void Start()
    {
        noiseOffset = Random.value * 10.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = transform.position;
        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var velocity = FlockManager.Instance.velocity * (1.0f + noise * FlockManager.Instance.velocityVariation);
        
        transform.forward += Separation();
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    private Vector3 Separation()
    {
        Vector3 seperation = Vector3.zero;
        foreach (GameObject bird in FlockManager.Instance.allBirds)
        {
            if (bird.gameObject == gameObject) continue;
            float distance = (bird.transform.position - transform.position).magnitude;
            if (distance < 3f)
            {
                seperation += (transform.position - bird.transform.position) / distance;
                seperation = seperation / distance;
            }

        }

        return seperation;
    }
}
