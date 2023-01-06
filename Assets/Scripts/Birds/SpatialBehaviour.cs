using System;
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
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

        boxID = getBoxID(currentPosition);

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

            var direction = Movement(currentPosition, dRange, nDistance) + FlyAround(currentPosition, dRange, 0.2f, nDistance);

            if (direction.sqrMagnitude > 0.00000001f)
            {
                var rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                                                      rotation,
                                                      SpatialManager.Instance.rotationSpeed * Time.deltaTime);
            }

        }
        transform.position = currentPosition + transform.forward * (v * Time.deltaTime);

        Vector3 newBoxID = getBoxID(transform.position);
        if (!newBoxID.Equals(boxID))
        {
            SpatialManager.Instance.boxes[boxID].Remove(gameObject);

            // add boid to its new voxel
            if (SpatialManager.Instance.boxes.ContainsKey(newBoxID))
            {
                SpatialManager.Instance.boxes[newBoxID].Add(gameObject);
            }
            else
            {
                SpatialManager.Instance.boxes[newBoxID] = new List<GameObject> { gameObject };
            }
            boxID = newBoxID;
        }
    }

    private Vector3 Movement(Vector3 currentPosition, float detectRange, float neighborDistance)
    {
        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;
        int groupSize = 0;

        List<GameObject> allBirds = SpatialManager.Instance.boxes[boxID];
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.forward]);
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.back]);
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.right]);
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.left]);
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.up]);
        allBirds.AddRange(SpatialManager.Instance.boxes[boxID + Vector3.down]);
        foreach (GameObject bird in allBirds)
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

    private Vector3 getBoxID(Vector3 position)
    {
        Vector3 boxID = new Vector3(Mathf.Floor(position.x / SpatialManager.Instance.boxSize),
                                    Mathf.Floor(position.y / SpatialManager.Instance.boxSize),
                                    Mathf.Floor(position.z / SpatialManager.Instance.boxSize));
        return boxID;
    }

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
