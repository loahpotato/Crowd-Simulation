using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class FlockManager : Singleton<FlockManager>
{
    public GameObject birdPrefab;
    public int initalNumber = 100;
    public float neighborDistance = 0.1f;
    public float detectDistance = 0.5f;

    [Range(0.1f, 1.0f)]
    public float rotationSpeed = 1.0f;
    [Range(0.1f, 2.0f)]
    public float velocity = 0.4f;
    [Range(0.0f, 0.5f)]
    public float velocityVariation = 0.1f;

    public Vector3 area = new Vector3(2, 2, 2);

    public LayerMask searchLayer;
    [HideInInspector]
    public List<GameObject> allBirds;
    [HideInInspector]
    public Vector3 center = Vector3.zero;
    [HideInInspector]
    public int birdNumber;

    void Start()
    {
        allBirds = new List<GameObject>();
        for(int i = 0; i < initalNumber; i++)
        {
            Vector3 newPosition = SetNewPosition();
            //plane.transform.position = new Vector3(0, - area.y * 2, 0);

            AddBird(newPosition);
        }
    }

    private void Update()
    {
        if (birdNumber != initalNumber)
        {
            if (birdNumber > initalNumber)
            {
                RemoveBird(birdNumber - initalNumber);
            }
            else
            {
                for (int i = 0; i < (initalNumber - birdNumber); i++)
                {
                    Vector3 newPosition = SetNewPosition();
                    AddBird(newPosition);
                }
            }
        }
    }

    public void RemoveBird(int number)
    {
        while (number > 0)
        {
            GameObject b = allBirds[number];
            allBirds.RemoveAt(number);
            Destroy(b);
            number--;
            birdNumber--;
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
        birdNumber++;
        return newBird;
    }
}
