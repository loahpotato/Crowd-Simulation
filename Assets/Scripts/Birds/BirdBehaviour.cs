using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BirdBehaviour : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float velocityWeight = 0.1f;

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
        
        transform.forward += Movement(currentPosition, velocity);
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    private Vector3 Movement(Vector3 currentPosition, float v)
    {
        Vector3 seperation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        float detectRange = FlockManager.Instance.detectDistance + velocityWeight * v;
        foreach (GameObject bird in FlockManager.Instance.allBirds)
        {
            if (bird.gameObject == gameObject) continue;

            float distance = (bird.transform.position - currentPosition).magnitude;
            if (distance < FlockManager.Instance.neighborDistance)
            {
                var sepatateScaler = DistanceScaler(distance, FlockManager.Instance.neighborDistance);
                seperation += (currentPosition - bird.transform.position) / distance * sepatateScaler;
            }

            if(distance < detectRange)
            {
                var alignScaler = DistanceScaler(distance, detectRange);
                alignment += bird.transform.forward * (1 - alignScaler);
                cohesion += bird.transform.position;
            }

        }
        alignment = alignment / FlockManager.Instance.allBirds.Count;
        cohesion = (cohesion - currentPosition).normalized;
        return seperation + alignment + cohesion;
    }

    private float DistanceScaler(float distance, float range)
    {
        var ratio = distance / range;
        return Mathf.Clamp01(1.0f - ratio);
    }
}
