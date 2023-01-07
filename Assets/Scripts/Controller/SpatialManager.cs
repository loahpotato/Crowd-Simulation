using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SpatialManager : Singleton<SpatialManager>
{
    public GameObject birdPrefab;
    public int initalNumber = 100;
    public float neighborDistance = 0.1f;
    public float detectDistance = 0.5f;
    public int maxQueryNumber = 10;

    [Range(0.1f, 1.0f)]
    public float rotationSpeed = 1.0f;
    [Range(0.1f, 2.0f)]
    public float velocity = 0.4f;
    [Range(0.0f, 0.5f)]
    public float velocityVariation = 0.1f;

    public Vector3 area = new Vector3(2, 2, 2);

    [HideInInspector]
    public Vector3 center = Vector3.zero;
    [HideInInspector]
    public float boxSize = 0.5f;
    [HideInInspector]
    public List<GameObject> allBirds;
    [HideInInspector]
    public Dictionary<Vector3, HashSet<GameObject>> boxes;

    void Start()
    {
        boxes = new Dictionary<Vector3, HashSet<GameObject>>();
        allBirds = new List<GameObject>();
        for (int i = 0; i < initalNumber; i++)
        {
            Vector3 newPosition = SetNewPosition();
            //plane.transform.position = new Vector3(0, - area.y * 2, 0);

            AddBird(newPosition);
        }
    }

    private Vector3 SetNewPosition()
    {
        Vector3 initPosition = Vector3.zero;
        float max = Mathf.Min(area.x, area.y);
        max = Mathf.Min(area.z, max);
        return initPosition + UnityEngine.Random.insideUnitSphere * max;
    }

    public GameObject AddBird(Vector3 position)
    {
        //Quaternion rotation = Quaternion.Slerp(transform.rotation, UnityEngine.Random.rotation, 0.3f);
        Quaternion rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up);
        //var boid = Instantiate(boidPrefab, position, rotation) as GameObject;
        GameObject newBird = Instantiate(birdPrefab, position, rotation);
        allBirds.Add(newBird);

        Vector3 boxID = new Vector3(
                                      Mathf.Floor(position.x / boxSize),
                                      Mathf.Floor(position.y / boxSize),
                                      Mathf.Floor(position.z / boxSize)
                                      );

        /*if (boxes.ContainsKey(boxID))
        {
             boxes[boxID].Add(newBird);
        }
        else
        {
            boxes[boxID] = new List<GameObject> { newBird };
        }*/
        if (!boxes.TryGetValue(boxID, out HashSet<GameObject> birds))
        {
            birds = new HashSet<GameObject>();
            boxes[boxID] = birds;
        }
        birds.Add(newBird);
        return newBird;
    }
}
