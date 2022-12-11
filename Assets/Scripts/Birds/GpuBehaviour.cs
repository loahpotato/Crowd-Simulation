using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GpuBehaviour : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;

    [Range(0.0f, 1.0f)]
    public float velocityWeight = 0.1f;

    private float velocity;
    //private Vector3 direction;
    private float detectDistance;
    private float neighborDistance;

    ComputeBuffer birdBuffer;
    // Random seed.
    float noiseOffset;

    static readonly int
        birdId = Shader.PropertyToID("_Bird"),
        detectId = Shader.PropertyToID("_Detect"),
        neighborId = Shader.PropertyToID("_Neighbor");

    void OnEnable()
    {
        birdBuffer = new ComputeBuffer(FlockManager.Instance.initalNumber, 3 * 4);
    }

    void OnDisable()
    {
        birdBuffer.Release();
        birdBuffer = null;
    }

    void Start()
    {
        noiseOffset = UnityEngine.Random.value * 10.0f;
        velocity = FlockManager.Instance.velocity;
        detectDistance = FlockManager.Instance.detectDistance;
        neighborDistance = FlockManager.Instance.neighborDistance;
    }

    void Update()
    {
        Vector3 currentPosition = transform.position;
        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var v = velocity * (1.0f + noise * FlockManager.Instance.velocityVariation);
        float dRange = detectDistance + velocityWeight * v;
        float nDistance = neighborDistance + velocityWeight * v;

        var direction = UpdateOnGpu(dRange, nDistance);

        if (direction.sqrMagnitude > 0.00000001f)
        {
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  rotation,
                                                  FlockManager.Instance.rotationSpeed * Time.deltaTime);
        }
        transform.position = currentPosition + transform.forward * (velocity * Time.deltaTime);
    }

    Vector3 UpdateOnGpu(float detect, float neighbor)
    {
        Vector3 rotateDir = Vector3.zero;
        Vector3 aroundDir = Vector3.zero;
        computeShader.SetFloat(detectId, detect);
        computeShader.SetFloat(neighborId, neighbor);

        int m = computeShader.FindKernel("Movement");
        computeShader.SetBuffer(m, birdId, birdBuffer);

        var groups = Mathf.CeilToInt(FlockManager.Instance.initalNumber / 128.0f);
        computeShader.Dispatch(m, groups, 1, 1);
        return rotateDir + aroundDir;
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

    private Vector3 FlyAround(Vector3 currentPosition, float detectRange, float toEdgeDistance, float n)
    {
        Vector3 area = FlockManager.Instance.area;
        Vector3 avoid = Vector3.zero;

        float maxX = area.x / 2;
        float maxY = area.y / 2;
        float maxZ = area.z / 2;

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
                dir -= axis * currentPos * absPos * d;
        }
        return dir;
    }
}
