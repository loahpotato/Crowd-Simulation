using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SpatialBehaviour : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float velocityWeight = 0.1f;

    private float velocity;
    //    private Vector3 direction;
    private float detectDistance;
    private float neighborDistance;
    private Vector3 boxID;

    // Random seed.
    float noiseOffset;
    bool outBounds = false;

    private Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
        noiseOffset = UnityEngine.Random.value * 10.0f;
        velocity = SpatialManager.Instance.velocity;
        detectDistance = SpatialManager.Instance.detectDistance;
        neighborDistance = SpatialManager.Instance.neighborDistance;
    }

    // Update is called once per frame
    void Update()
    {
        Bounds b = new Bounds(SpatialManager.Instance.center, SpatialManager.Instance.area * 2);
        Vector3 currentPosition = transform.position;

        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var v = velocity * (1.0f + noise * SpatialManager.Instance.velocityVariation);

        boxID = SpatialManager.getBoxPosition(currentPosition);

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
            Vector3 dir = SpatialManager.Instance.center - currentPosition;
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                 Quaternion.LookRotation(dir),
                                                 SpatialManager.Instance.rotationSpeed * Time.deltaTime);
        }
        else
        {
            float dRange = detectDistance + velocityWeight * v;
            float nDistance = neighborDistance + velocityWeight * v;

            var direction = Movement(currentPosition, nDistance); //+ FlyAround(currentPosition, dRange, 0.5f, nDistance);

            if (direction.sqrMagnitude > 0.00000001f)
            {
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      rotation,
                                                      SpatialManager.Instance.rotationSpeed * Time.deltaTime);
            }

        }
        transform.position = currentPosition + transform.forward * (v * Time.deltaTime);

        Vector3 newBoxID = SpatialManager.getBoxPosition(transform.position);
        if (!newBoxID.Equals(boxID))
        {
            SpatialManager.Instance.boxes[boxID].Remove(gameObject);
            if (!SpatialManager.Instance.boxes.TryGetValue(newBoxID, out HashSet<GameObject> bucket))
            {
                bucket = new HashSet<GameObject>();
                SpatialManager.Instance.boxes[newBoxID] = bucket;
            }
            bucket.Add(gameObject);
            boxID = newBoxID;
        }
    }


    //
    //      The basic movement rule of boids.
    //
    private Vector3 Movement(Vector3 currentPosition, float neighborDistance)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int groupSize = 0;

        HashSet<GameObject> boxBirds = findNearby(boxID);
        Color c = new Color();
        if(boxBirds.Count > 10)
        {

        }
        foreach (GameObject bird in boxBirds)
        {
            if (bird.IsDestroyed() || bird == null) continue;
            if (groupSize > SpatialManager.Instance.maxQueryNumber) break;
            if (bird.gameObject == gameObject) continue;

            var towards = bird.transform.position - currentPosition;
            float distance = towards.magnitude;
            if (Vector3.Dot(transform.forward, towards) >= 0) // if the neighbor can be detected
            {
                if (distance < neighborDistance)
                {
                    var separateScaler = DistanceScaler(distance, neighborDistance);
                    separation += (currentPosition - bird.transform.position) / distance * separateScaler;
                }

                var alignScaler = DistanceScaler(distance, SpatialManager.Instance.boxSize * 0.707f);
                alignment += bird.transform.forward * (1 - alignScaler);
                cohesion += bird.transform.position;
                groupSize++;
                c += bird.GetComponent<Renderer>().material.color;
            }

        }
        if(groupSize != 0)
        {
            rend.material.color = c / (float)groupSize;
        }

        cohesion = cohesion / groupSize - currentPosition;
        return separation + alignment + cohesion;
    }

    private float DistanceScaler(float distance, float range)
    {
        var ratio = distance / range;
        return Mathf.Clamp01(1.0f - ratio);
    }

    private HashSet<GameObject> findNearby(Vector3 boxP)
    {
        List<Vector3> nearbyPs = Enumerable.Range(-1, 3)
                                  .SelectMany(x => Enumerable.Range(-1, 3)
                                  .SelectMany(y => Enumerable.Range(-1, 3)
                                  .Select(z => boxP + new Vector3(x, y, z)))).ToList();

        HashSet<GameObject> nearBy = new HashSet<GameObject>();
        foreach (Vector3 p in nearbyPs){
            if (SpatialManager.Instance.boxes.TryGetValue(p, out HashSet<GameObject> bucket))
            {
                nearBy.UnionWith(bucket);
            }
        }
        return nearBy;

    }

    //
    //      If there is no fixed flying area limit (no bounds), 
    //      this function simulates birds flying around the center.
    //
    private Vector3 FlyAround(Vector3 currentPosition, float detectRange, float toEdgeDistance, float n)
    {
        Vector3 area = SpatialManager.Instance.area;
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
