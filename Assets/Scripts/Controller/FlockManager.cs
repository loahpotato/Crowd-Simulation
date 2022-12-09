using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : Singleton<FlockManager>
{
    public GameObject birdPrefab;
    public int initalNumber = 10;
    public float neighborDistance = 0.3f;
    public float detectDistance = 4.0f;

    [Range(0.1f, 5.0f)]
    public float velocity = 1.0f;
    [Range(0.0f, 0.5f)]
    public float velocityVariation = 0.2f;

    public GameObject plane;
    [SerializeField] private Vector3 area = new Vector3(5, 5, 5);
    [SerializeField] private Vector3 initPosition = new Vector3(0, 0, 0);
    [SerializeField] private float spawnRadius = 0.2f;

    [HideInInspector]
    public List<GameObject> allBirds;

    void Start()
    {
        allBirds = new List<GameObject>();
        for(int i = 0; i < initalNumber; i++)
        {
            Vector3 newPosition = SetNewPosition();
            while (newPosition.z <= plane.transform.position.z)
            {
                newPosition = SetNewPosition();
            }

            AddBird(newPosition);
        }
    }

    private Vector3 SetNewPosition()
    {
        return initPosition + Random.insideUnitSphere * spawnRadius;
    }

    public GameObject AddBird(Vector3 position)
    {
        //var rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
        //var boid = Instantiate(boidPrefab, position, rotation) as GameObject;
        GameObject newBird = Instantiate(birdPrefab);
        newBird.transform.position = position;
        //boid.GetComponent<BoidBehaviour>().controller = this;

        allBirds.Add(newBird);
        return newBird;
    }
}
