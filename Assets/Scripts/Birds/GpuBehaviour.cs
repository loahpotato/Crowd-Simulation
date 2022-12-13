using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

    // Random seed.
    float noiseOffset;

    static readonly int
        birdId = Shader.PropertyToID("_Bird"),
        detectId = Shader.PropertyToID("_Detect"),
        neighborId = Shader.PropertyToID("_Neighbor"),
        timeId = Shader.PropertyToID("_Time"),
        dtId = Shader.PropertyToID("_DeltaTime"),
        numberId = Shader.PropertyToID("_Number"),
        velocityId = Shader.PropertyToID("_VelocityVariation"),
        rotationId = Shader.PropertyToID("_RotationSpeed");


    void Start()
    {
        noiseOffset = UnityEngine.Random.value * 10.0f;
        velocity = GpuFlockManager.Instance.velocity;
        detectDistance = GpuFlockManager.Instance.detectDistance;
        neighborDistance = GpuFlockManager.Instance.neighborDistance;
    }

    void Update()
    {
        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var v = velocity * (1.0f + noise * GpuFlockManager.Instance.velocityVariation);
        float dRange = detectDistance + velocityWeight * v;
        float nDistance = neighborDistance + velocityWeight * v;

        UpdateOnGpu(dRange, nDistance, v);
    }

    void UpdateOnGpu(float detect, float neighbor, float v)
    {
        computeShader.SetFloat(detectId, detect);
        computeShader.SetFloat(neighborId, neighbor);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetFloat(dtId, Time.deltaTime);
        computeShader.SetFloat(velocityId, GpuFlockManager.Instance.velocityVariation);
        computeShader.SetFloat(rotationId, GpuFlockManager.Instance.rotationSpeed);
        computeShader.SetInt(numberId, GpuFlockManager.Instance.initalNumber);

        int m = computeShader.FindKernel("Movement");
        //computeShader.SetBuffer(m, birdId, GpuFlockManager.Instance.birdBuffer);

        var groups = Mathf.CeilToInt(GpuFlockManager.Instance.initalNumber / 128.0f);
        computeShader.Dispatch(m, groups, 1, 1);
        //Graphics.DrawMeshInstancedProcedural(mesh, 0, material);
    }


}
