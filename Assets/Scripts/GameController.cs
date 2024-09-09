using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Controls the main gameplay
/// </summary>
public class GameController : MonoBehaviour
{
    [Tooltip("A reference to the tile we want to spawn")]
    public Transform[] tile;

    [Tooltip("A reference to the obstacle we want to spawn")]
    public List<Transform> obstacle;

    [Tooltip("Where the first tile should be placed at")]
    public Vector3 startPoint = new Vector3(0, 0, -5);

    [Tooltip("How many tiles should we create in advance")]
    [Range(1, 15)]
    public int initSpawnNum = 5;

    [Tooltip("How many tiles to spawn initially with no obstacles")]
    public int initNoObstacles = 4;

    [Tooltip("Number of obstacles there are currently")]
    public int obstacleCount;

    [Tooltip("Distance to switch tile")]
    public float switchDistance = 50f;

    [Tooltip("Skybox materials to swtich between when tile changes")]
    public Material[] skyboxes;

    /// Where the next tile should be spawned at.
    private Vector3 nextTileLocation;

    /// How should the next tile be rotated?
    private Quaternion nextTileRotation;

    private Transform currentTile;
    private int currentTileIndex = 0;

    private Transform player;
    private float distanceTravelled = 0f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentTile = tile[currentTileIndex];

        // Set our starting point
        nextTileLocation = startPoint;
        nextTileRotation = Quaternion.identity;

        obstacleCount = 0; // Initialize obstacle counter
        
        for (int i = 0; i < initSpawnNum; ++i)
        {
            SpawnNextTile(i >= initNoObstacles);
        }

        // Set the initial skybox
        RenderSettings.skybox = skyboxes[currentTileIndex];
    }

    private void Update()
    {
        distanceTravelled = Vector3.Distance(startPoint, player.position);

        if (distanceTravelled >= switchDistance)
        {
            SwitchTile();
            startPoint = player.position; // Reset start point for next switch
        }
    }

    /// Will spawn a tile at a certain location and setup the next position
    public void SpawnNextTile(bool spawnObstacles = true)
    {
         var newTile = Instantiate(currentTile, nextTileLocation, nextTileRotation);
         // Figure out where and at what rotation we should spawn
         // the next item
         var nextTile = newTile.Find("Next Spawn Point");
         nextTileLocation = nextTile.position;
         nextTileRotation = nextTile.rotation;

        if (spawnObstacles)
        {
            SpawnObstacle(newTile);
        }
    }

    private void SwitchTile()
    {
        currentTileIndex = (currentTileIndex + 1) % tile.Length;
        currentTile = tile[currentTileIndex];

        // Switch skybox when the tile switches
        RenderSettings.skybox = skyboxes[currentTileIndex];
        DynamicGI.UpdateEnvironment();  // Update the lighting environment to reflect the new skybox
    }

    private void SpawnObstacle(Transform newTile)
    {
        // Now we need to get all of the possible places to spawn the
        // obstacle
        var obstacleSpawnPoints = new List<GameObject>();
        // Go through each of the child game objects in our tile
        foreach (Transform child in newTile)
        {
            // If it has the ObstacleSpawn tag
            if (child.CompareTag("ObstacleSpawn"))
            {
                // We add it as a possibility
                obstacleSpawnPoints.Add(child.gameObject);
            }
        }

        // Make sure there is at least one
        if (obstacleSpawnPoints.Count > 0)
        {
            // Get a random object from the ones we have
            var spawnPoint = obstacleSpawnPoints[Random.Range(0, obstacleSpawnPoints.Count)];

            // Store its position for us to use
            var spawnPos = spawnPoint.transform.position;

            // Create our obstacle
            var newObstacle = Instantiate(obstacle[Random.Range(0, obstacle.Count)], spawnPos, Quaternion.identity);

            // Have it parented to the tile
            newObstacle.SetParent(spawnPoint.transform);

            obstacleCount++;

            if (obstacleCount == initSpawnNum)
            {
                obstacleCount = 0;
            }
        }
    }
}
