using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float boxSize;
    [HideInInspector]
    public int birdNumber;
    [HideInInspector]
    public List<GameObject> allBirds;
    [HideInInspector]
    public Dictionary<Vector3, HashSet<GameObject>> boxes;

    void Start()
    {
        boxSize = detectDistance;
        boxes = new Dictionary<Vector3, HashSet<GameObject>>((int)(area.x/boxSize *area.y/boxSize*area.z/boxSize));
        allBirds = new List<GameObject>();
        for (int i = 0; i < initalNumber; i++)
        {
            Vector3 newPosition = SetNewPosition();
            //plane.transform.position = new Vector3(0, - area.y * 2, 0);

            AddBird(newPosition);
        }
    }

    private void Update()
    {
        //float a = (float)initalNumber * 0.005f + 0.5f;
        //area = new Vector3(a, a, a);
        if (birdNumber != initalNumber)
        {
            if(birdNumber > initalNumber)
            {
                RemoveBird1(birdNumber - initalNumber);
            }
            else
            {
                for (int i = 0; i < (initalNumber-birdNumber); i++)
                {
                    Vector3 newPosition = SetNewPosition();
                    AddBird(newPosition);
                }
            }
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
        newBird.GetComponent<Renderer>().material.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        allBirds.Add(newBird);

        Vector3 boxID = getBoxPosition(position);

        if (!boxes.TryGetValue(boxID, out HashSet<GameObject> bucket))
        {
            bucket = new HashSet<GameObject>();
            boxes[boxID] = bucket;
        }
        bucket.Add(newBird);
        birdNumber++;
        return newBird;
    }

    public void RemoveBird1(int number)
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


    public void RemoveBird(int number)
    {
        Vector3 newPosition = Vector3.zero;
        Vector3 boxID = Vector3.zero;
        while(number > 0) 
        {
            newPosition = SetNewPosition();
            boxID = getBoxPosition(newPosition);
            if (boxes.TryGetValue(boxID, out HashSet<GameObject> bucket))
            {
                int d = bucket.Count - number;
                List<GameObject> removeList = new List<GameObject>();
                GameObject bird;
                if (d > 0)
                {
                    for (int i = 0; i < number; i++)
                    {
                        if (i >= bucket.Count) break; 
                        bird = bucket.ElementAt(i);
                        bucket.Remove(bucket.ElementAt(i));
                        Destroy(bird);
                        birdNumber --;
                        number--;
                    }
                    
                }
                else
                {
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        bird = bucket.ElementAt(i);
                        bucket.Remove(bucket.ElementAt(i));
                        Destroy(bird);
                        birdNumber--;
                        number--;
                    }

                }
            }
        }


    }


    //
    //      Get the position of current box (Uniform Spatial Subdivision)
    //
    static public Vector3 getBoxPosition(Vector3 position)
    {
        Vector3 p = new Vector3(Mathf.Floor(position.x / SpatialManager.Instance.boxSize),
                                    Mathf.Floor(position.y / SpatialManager.Instance.boxSize),
                                    Mathf.Floor(position.z / SpatialManager.Instance.boxSize));
        return p;
    }

}
