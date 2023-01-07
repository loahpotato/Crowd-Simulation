using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class BirdBehaviour : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float velocityWeight = 0.1f;

    private float velocity;
//    private Vector3 direction;
    private float detectDistance;
    private float neighborDistance;

    // Random seed.
    float noiseOffset;
    bool outBounds = false;

    void Start()
    {
        noiseOffset = UnityEngine.Random.value * 10.0f;
        velocity = FlockManager.Instance.velocity;
        detectDistance = FlockManager.Instance.detectDistance;
        neighborDistance = FlockManager.Instance.neighborDistance;
    }

    // Update is called once per frame
    void Update()
    {
        Bounds b = new Bounds(FlockManager.Instance.center, FlockManager.Instance.area * 2);
        Vector3 currentPosition = transform.position;

        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var v = velocity * (1.0f + noise * FlockManager.Instance.velocityVariation);

        if (!b.Contains(currentPosition))
        {
            outBounds = true;
        }
        else
        {
            outBounds = false;
        }

        if (outBounds)
        {
            Vector3 dir = FlockManager.Instance.center - currentPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                 Quaternion.LookRotation(dir),
                                                 FlockManager.Instance.rotationSpeed * Time.deltaTime);
        }

        else {
            float dRange = detectDistance + velocityWeight * v;
            float nDistance = neighborDistance + velocityWeight * v;

            var direction = Movement(currentPosition, dRange, nDistance) + FlyAround(currentPosition, dRange, 0.2f, nDistance);

            if (direction.sqrMagnitude > 0.00000001f)
            {
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      rotation,
                                                      FlockManager.Instance.rotationSpeed * Time.deltaTime);
            }
            
        }
        transform.position = currentPosition + transform.forward * (v * Time.deltaTime);
    }

    private Vector3 Movement(Vector3 currentPosition, float detectRange, float neighborDistance)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int groupSize = 0;

        //var nearbyBirds = Physics.OverlapSphere(currentPosition, detectRange, FlockManager.Instance.searchLayer); 
        // same way but more convenient implementation than Uniform Spatial Subdivision??

        foreach (var bird in FlockManager.Instance.allBirds)
        {
            if (bird.gameObject == gameObject) continue;

            var towards = bird.transform.position - currentPosition;
            float distance = towards.magnitude;
            if (distance < detectRange && Vector3.Dot(transform.forward, towards) >= 0) // if the neighbor can be detected
            {
                if (distance < neighborDistance)
                {
                    var separateScaler = DistanceScaler(distance, neighborDistance);
                    separation += (currentPosition - bird.transform.position) / distance * separateScaler;
                }

                var alignScaler = DistanceScaler(distance, detectRange);
                alignment += bird.transform.forward * (1 - alignScaler);
                cohesion += bird.transform.position;
                groupSize++;
            }

        }
        cohesion = cohesion / groupSize - currentPosition;
        return separation + alignment + cohesion;
    }

    private float DistanceScaler(float distance, float range)
    {
        var ratio = distance / range;
        return Mathf.Clamp01(1.0f - ratio);
    }


    //
    //      If there is no fixed flying area limit (no bounds), 
    //      this function simulates birds flying around the center.
    //
    private Vector3 FlyAround(Vector3 currentPosition, float detectRange, float toEdgeDistance, float n)
    {
        Vector3 area = FlockManager.Instance.area;
        Vector3 avoid = Vector3.zero;

        float maxX = area.x;
        float maxY = area.y;
        float maxZ = area.z;

        float density = detectRange / n;

        avoid += FlyAroundDirection(currentPosition.x, maxX, detectRange, toEdgeDistance, density, Vector3.right);
        avoid += FlyAroundDirection(currentPosition.y, maxY, detectRange, toEdgeDistance, density, Vector3.up);
        avoid += FlyAroundDirection(currentPosition.z, maxZ, detectRange, toEdgeDistance, density, Vector3.forward);

        return avoid;
    }

    private Vector3 FlyAroundDirection(float currentPos, float max, float detectRange, float toEdgeDistance, float d, Vector3 axis)
    {
        Vector3 dir = Vector3.zero;
        float absPos = Mathf.Abs(currentPos);
        if (absPos + detectRange >= max)
        {
            if (absPos < max)
            {
                float distance = max - absPos;
                if (distance < toEdgeDistance)
                    dir -= axis * DistanceScaler(distance, toEdgeDistance) * currentPos * d;
            }
            else
            {
                dir -= axis * currentPos * absPos * d;
            }
                
        }
        return dir;
    }
}
