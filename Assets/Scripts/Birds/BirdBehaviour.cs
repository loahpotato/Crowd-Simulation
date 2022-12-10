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
        float dRange = FlockManager.Instance.detectDistance + velocityWeight * velocity;
        float nDistance = FlockManager.Instance.neighborDistance + velocityWeight * velocity;

        direction = Movement(currentPosition, dRange, nDistance) + AvoidEdge(currentPosition, dRange, 0.2f, nDistance);

        if(direction.sqrMagnitude > 0.00000001f)
        {
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  rotation,
                                                  FlockManager.Instance.rotationSpeed * Time.deltaTime);
        }
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    private Vector3 Movement(Vector3 currentPosition, float detectRange, float neighborDistance)
    {
        Vector3 seperation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int groupSize = 0;
        
        foreach (GameObject bird in FlockManager.Instance.allBirds)
        {
            if (bird.gameObject == gameObject) continue;

            float distance = (bird.transform.position - currentPosition).magnitude;
            if (distance < detectRange)
            {
                if (distance < neighborDistance)
                {
                    var separateScaler = DistanceScaler(distance, neighborDistance);
                    seperation += (currentPosition - bird.transform.position) / distance * separateScaler;
                }

                var alignScaler = DistanceScaler(distance, detectRange);
                alignment += bird.transform.forward * (1 - alignScaler);
                cohesion += bird.transform.position;
                groupSize++;
            }

        }
        cohesion = cohesion / groupSize - currentPosition;
        return seperation + alignment + cohesion;
    }

    private float DistanceScaler(float distance, float range)
    {
        var ratio = distance / range;
        return Mathf.Clamp01(1.0f - ratio);
    }

    private Vector3 AvoidEdge(Vector3 currentPosition, float detectRange, float toEdgeDistance, float n)
    {
        Vector3 area = FlockManager.Instance.area;
        Vector3 avoid = Vector3.zero;
        float currentX = Mathf.Abs(currentPosition.x);
        float currentY = Mathf.Abs(currentPosition.y);
        float currentZ = Mathf.Abs(currentPosition.z);

        float maxX = area.x / 2;
        float maxY = area.y / 2;
        float maxZ = area.z / 2;


        if (currentX + detectRange >= maxX)
        {
            if (currentX < maxX)
            {
                float distanceX = maxX - currentX;
                if (distanceX < toEdgeDistance)
                    avoid -= Vector3.right * DistanceScaler(distanceX, toEdgeDistance) * currentPosition.x;
            }
            else
                avoid -= Vector3.right * currentPosition.x * currentX * detectRange / n;
        }

        if (currentY + detectRange >= maxY)
        {
            if (currentY < maxY)
            {
                float distanceY = maxY - currentY;
                if (distanceY < toEdgeDistance)
                    avoid -= Vector3.up * DistanceScaler(distanceY, toEdgeDistance) * currentPosition.y;
            }
            else
                avoid -= Vector3.up * currentPosition.y * currentY * detectRange / n;
        }

        if (currentZ + detectRange >= maxZ)
        {
            if (currentZ < maxZ)
            {
                float distanceZ = maxZ - currentZ;
                if (distanceZ < toEdgeDistance)
                    avoid -= Vector3.forward * DistanceScaler(distanceZ, toEdgeDistance) * currentPosition.z;
            }
            avoid -= Vector3.forward * currentPosition.z * currentZ * detectRange / n;
        }

        return avoid;
    }
}
