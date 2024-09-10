using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class ObstacleGroup
{
    [Tooltip("Obstacles for this tile")]
    public Transform[] obstacles;
}

/// <summary>
/// Controls the main gameplay
/// </summary>
public class GameController : MonoBehaviour
{
    [Tooltip("A reference to the tile we want to spawn")]
    public Transform[] tile;

    [Tooltip("A reference to the obstacle we want to spawn")]
    public ObstacleGroup[] obstacleGroups;

    [Tooltip("Where the first tile should be placed at")]
    public Vector3 startPoint = new Vector3(0, 0, -5);

    [Tooltip("How many tiles should we create in advance")]
    [Range(1, 15)]
    public int initSpawnNum = 5;

    [Tooltip("How many tiles to spawn initially with no obstacles")]
    public int initNoObstacles = 4;

    [Tooltip("Number of obstacles there are currently")]
    public int obstacleCount;

    [Tooltip("Skybox materials to swtich between when tile changes")]
    public Material[] skyboxes;

    /// Where the next tile should be spawned at.
    private Vector3 nextTileLocation;

    /// How should the next tile be rotated?
    private Quaternion nextTileRotation;

    private Transform currentTile;
    private int currentTileIndex = 0;

    private PlayerBehavior score;

    [Header("Distance at which tiles should change")]
    public float summerDistance;
    private bool summer = false;
    public float autumnDistance;
    private bool autumn = false;
    public float winterDistance;
    private bool winter = false;
    
    private void Start()
    {
        score = FindObjectOfType<PlayerBehavior>();
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
        if(score.Score >= summerDistance && !summer)
        {
            SwitchTile(1);
            summer = true;
            Debug.Log("Summer");
        }
        else if (score.Score >= autumnDistance && !autumn)
        {
            SwitchTile(2);
            autumn = true;
            Debug.Log("Autumn");
        }
        else if (score.Score >= winterDistance && !winter)
        {
            SwitchTile(3);
            winter = true;
            Debug.Log("Winter");
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

    private void SwitchTile(int tileIndex)
    {
        switch (tileIndex)
        {
            case 1:
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];     // Switch skybox when the tile 
                DynamicGI.UpdateEnvironment();  // Update the lighting environment to reflect the new skybox
                break;
            case 2:
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];     // Switch skybox when the tile 
                DynamicGI.UpdateEnvironment();  // Update the lighting environment to reflect the new skybox
                break;
            case 3:
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];     // Switch skybox when the tile 
                DynamicGI.UpdateEnvironment();  // Update the lighting environment to reflect the new skybox
                break;
            default:
                currentTileIndex = 0;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];     // Switch skybox when the tile 
                DynamicGI.UpdateEnvironment();  // Update the lighting environment to reflect the new skybox
                break;
        }
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
            var spawnPoint = obstacleSpawnPoints[UnityEngine.Random.Range(0, obstacleSpawnPoints.Count)];

            // Store its position for us to use
            var spawnPos = spawnPoint.transform.position;

            // Create our obstacle
            var newObstacle = Instantiate(obstacleGroups[currentTileIndex].obstacles[UnityEngine.Random.Range(0, obstacleGroups[currentTileIndex].obstacles.Length)], spawnPos, Quaternion.identity);

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
