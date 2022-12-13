using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GpuFlockManager : Singleton<GpuFlockManager>
{
    //public Mesh birdMesh;
    //public Material birdMaterial;
    public GameObject birdPrefab;
    public int initalNumber = 100;
    public float neighborDistance = 0.1f;
    public float detectDistance = 0.5f;

    public float rotationSpeed = 10.0f;
    [Range(0.1f, 2.0f)]
    public float velocity = 0.4f;
    [Range(0.0f, 0.5f)]
    public float velocityVariation = 0.1f;
    [Range(0.0f, 1.0f)]
    public float velocityWeight = 0.1f;
    public Vector3 area = new Vector3(1, 1, 1);

    [SerializeField]
    ComputeShader computeShader;

    [HideInInspector]
    public struct Bird
    {
        public Vector3 position;
        public Vector3 forward;
        public Vector3 dir;
    };

    [HideInInspector]
    public Bird[] allBirds;
    List<GameObject> prefabBirds;
    List<Bird> gpuBirds;
    ComputeBuffer birdBuffer;

    // Random seed.
    float noiseOffset;

    static readonly int
        birdId = Shader.PropertyToID("_Bird"),
        detectId = Shader.PropertyToID("_Detect"),
        neighborId = Shader.PropertyToID("_Neighbor"),
        timeId = Shader.PropertyToID("_Time"),
        dtId = Shader.PropertyToID("_DeltaTime"),
        numberId = Shader.PropertyToID("_Number"),
        velocityId = Shader.PropertyToID("_Velocity"),
        rotationId = Shader.PropertyToID("_RotationSpeed");

    void OnEnable()
    {
        birdBuffer = new ComputeBuffer(initalNumber, Marshal.SizeOf(typeof(Bird)));
    }

    void OnDisable()
    {
        birdBuffer.Release();
        birdBuffer = null;
    }

    void Start()
    {
        noiseOffset = UnityEngine.Random.value * 10.0f;
        allBirds = new Bird[initalNumber];
        gpuBirds = new List<Bird>();
        prefabBirds = new List<GameObject>();
        for (int i = 0; i < initalNumber; i++)
        {
            Vector3 newPosition = SetNewPosition();
            //plane.transform.position = new Vector3(0, - area.y * 2, 0);

            AddBird(newPosition);
        }
        birdBuffer.SetData(gpuBirds);
    }

    void Update()
    {
        // Current velocity randomized with noise.
        var noise = Mathf.PerlinNoise(Time.time, noiseOffset);
        var v = velocity * (1.0f + noise * velocityVariation);
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
        computeShader.SetFloat(velocityId, v);
        computeShader.SetFloat(rotationId, rotationSpeed);
        computeShader.SetInt(numberId, initalNumber);

        int m = computeShader.FindKernel("Movement");
        computeShader.SetBuffer(m, birdId, birdBuffer);

        var groups = Mathf.CeilToInt(initalNumber / 128.0f);
        computeShader.Dispatch(m, groups, 1, 1);

        birdBuffer.GetData(allBirds);
        int i = 0;
        foreach (GameObject bird in prefabBirds)
        {
            if (allBirds[i].dir.sqrMagnitude > 0.00000001f) { 
                var rotation = Quaternion.LookRotation(allBirds[i].dir);
                bird.transform.rotation = rotation;
            }
            bird.transform.position = allBirds[i].position;
            bird.transform.forward = allBirds[i].forward;
            i++;
        }

        //var bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        //birdMaterial.SetBuffer(birdId, birdBuffer);
        //Graphics.DrawMeshInstancedProcedural(birdMesh, 0, birdMaterial, bounds, birdBuffer.count);
    }

    private Vector3 SetNewPosition()
    {
        Vector3 initPosition = Vector3.zero;
        float max = Mathf.Min(area.x, area.y);
        max = Mathf.Min(area.z, max);
        return initPosition + UnityEngine.Random.insideUnitSphere * max;
    }

    public void AddBird(Vector3 position)
    {
        //Quaternion rotation = Quaternion.Slerp(transform.rotation, UnityEngine.Random.rotation, 0.3f);
        Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
        GameObject newBird = Instantiate(birdPrefab, position, rotation);

        Bird b = new Bird();
        b.position = position;
        b.forward = (rotation * Vector3.forward).normalized;
        b.dir = rotation.eulerAngles;

        gpuBirds.Add(b);
        prefabBirds.Add(newBird);
    }
}
