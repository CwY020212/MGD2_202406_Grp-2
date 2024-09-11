using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

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
    public AudioSource summerBGM;
    public AudioSource DefaultBGM;
    public AudioSource AutumnBGM;
    public AudioSource WinterBGM;
    [Tooltip("Reference to point collectable")]
    public Transform collectiblePrefab;

    [Tooltip("Reference to rare collectables")]
    public Transform[] rareCollectables;

    [Tooltip("Chance to spawn a rare collectable")]
    [Range(0f, 1f)]
    public float rareCollectableChance = 0.1f;
    public float rareCollectableCD = 10f;
    public float rareCollectableTimer;

    [Tooltip("Reference to powerups")]
    public Transform[] powerups;

    [Tooltip("Cooldown time for powerup spawning")]
    public float powerupCD = 3f;
    public float powerupTimer;

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
        // Apply saved preferences to your background music sources
        summerBGM.volume = AudioPreferences.GetBGMVolume();
        DefaultBGM.volume = AudioPreferences.GetBGMVolume();
        AutumnBGM.volume = AudioPreferences.GetBGMVolume();
        WinterBGM.volume = AudioPreferences.GetBGMVolume();

        score = FindObjectOfType<PlayerBehavior>();
        currentTile = tile[currentTileIndex];

        powerupTimer = 0;
        rareCollectableTimer = 0;

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
        DynamicGI.UpdateEnvironment();


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

        if (powerupTimer > 0f)
        {
            powerupTimer -= Time.deltaTime;     // // Update cooldown timer

        }
        if (rareCollectableTimer > 0)
        {
            rareCollectableTimer -= Time.deltaTime;     // Update cooldown timer
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
        AudioSource currentBGM = null;
        AudioSource nextBGM = null;

        switch (tileIndex)
        {
            case 1:
                currentBGM = GetCurrentBGM();  // Get the current BGM
                nextBGM = summerBGM;
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];
                DynamicGI.UpdateEnvironment();
                break;
            case 2:
                currentBGM = GetCurrentBGM();
                nextBGM = AutumnBGM;
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];
                DynamicGI.UpdateEnvironment();
                break;
            case 3:
                currentBGM = GetCurrentBGM();
                nextBGM = WinterBGM;
                currentTileIndex = tileIndex;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];
                DynamicGI.UpdateEnvironment();
                break;
            default:
                currentBGM = GetCurrentBGM();
                nextBGM = DefaultBGM;
                currentTileIndex = 0;
                currentTile = tile[currentTileIndex];
                RenderSettings.skybox = skyboxes[currentTileIndex];
                DynamicGI.UpdateEnvironment();
                break;
        }

        // Stop all other BGMs
        StopAllBGMsExcept(nextBGM);

        if (currentBGM != nextBGM)
        {
            StartCoroutine(FadeOutIn(currentBGM, nextBGM));
        }
    }
        private AudioSource GetCurrentBGM()
        {
            // Return the currently playing BGM (you may need to adjust this method based on your setup)
            if (summerBGM.isPlaying) return summerBGM;
            if (AutumnBGM.isPlaying) return AutumnBGM;
            if (WinterBGM.isPlaying) return WinterBGM;
            return DefaultBGM;  // Default BGM if no other BGM is playing
        }

        private void StopAllBGMsExcept(AudioSource exceptBGM)
        {
            if (exceptBGM != summerBGM) summerBGM.Stop();
            if (exceptBGM != AutumnBGM) AutumnBGM.Stop();
            if (exceptBGM != WinterBGM) WinterBGM.Stop();
            if (exceptBGM != DefaultBGM) DefaultBGM.Stop();
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
        if (obstacleSpawnPoints.Count > 1)
        {
            // Get a random object from the ones we have
            var spawnPoint = obstacleSpawnPoints[UnityEngine.Random.Range(0, obstacleSpawnPoints.Count)];

            // Remove the obstacle spawn point from the list so it can't be used for the collectible
            obstacleSpawnPoints.Remove(spawnPoint);

            // Store its position for us to use
            var spawnPos = spawnPoint.transform.position;

            // Create our obstacle
            var newObstacle = Instantiate(obstacleGroups[currentTileIndex].obstacles[UnityEngine.Random.Range(0, obstacleGroups[currentTileIndex].obstacles.Length)], spawnPos, Quaternion.identity);

            // Have it parented to the tile
            newObstacle.SetParent(spawnPoint.transform);

            // Spawn a collectable at a different spawn point
            SpawnCollectible(newTile, obstacleSpawnPoints);

            // If there is still one remaining spawn point, try to spawn a powerup
            if (obstacleSpawnPoints.Count == 1 && powerupTimer <= 0)
            {
                SpawnPowerup(obstacleSpawnPoints[0]);
            }

            obstacleCount++;

            if (obstacleCount == initSpawnNum)
            {
                obstacleCount = 0;
            }
        }
    }

    private void SpawnCollectible(Transform newTile, List<GameObject> remainingSpawnPoints)
    {
        // Check if there are remaining spawn points to place the collectible
        if (remainingSpawnPoints.Count > 0)
        {
            // Get a random spawn point for the collectible
            var collectibleSpawnPoint = remainingSpawnPoints[UnityEngine.Random.Range(0, remainingSpawnPoints.Count)];

            // Store the collectible's position
            var collectiblePos = collectibleSpawnPoint.transform.position;

            // Decide whether to spawn a regular or rare collectable
            Transform collectableToSpawn;

            if (rareCollectables.Length > 0 && UnityEngine.Random.value < rareCollectableChance && rareCollectableTimer <= 0)
            {
                // Spawn a rare collectable
                collectableToSpawn = rareCollectables[currentTileIndex];

                // Reset the cooldown timer after spawning a rare collectable
                rareCollectableTimer = rareCollectableCD;
            }
            else
            {
                // Spawn a regular collectable
                collectableToSpawn = collectiblePrefab;
            }

            // Instantiate the collectible at the chosen position
            var newCollectible = Instantiate(collectableToSpawn, collectiblePos, Quaternion.identity);

            // Have it parented to the tile
            newCollectible.SetParent(collectibleSpawnPoint.transform);

            // Remove the collectible spawn point from the list
            remainingSpawnPoints.Remove(collectibleSpawnPoint);
        }
    }

    private void SpawnPowerup(GameObject spawnPoint)
    {
        // Choose a random powerup from the array
        if (powerups.Length > 0 && UnityEngine.Random.value < 0.3f)  // 30% chance to spawn a powerup
        {
            var powerupPrefab = powerups[UnityEngine.Random.Range(0, powerups.Length)];
            var powerupPos = spawnPoint.transform.position;

            // Instantiate the powerup at the last remaining spawn point
            var newPowerup = Instantiate(powerupPrefab, powerupPos, Quaternion.identity);

            // Have it parented to the tile
            newPowerup.SetParent(spawnPoint.transform);

            powerupTimer = powerupCD;
        }
    }

    private IEnumerator FadeOutIn(AudioSource from, AudioSource to, float fadeDuration = 1f)
    {
        // Fade out the current BGM
        float currentTime = 0;
        float startVolume = from.volume;

        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            from.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeDuration);
            yield return null;
        }

        from.Stop();  // Ensure it stops completely
        from.volume = startVolume; // Reset volume for potential future use

        // Start the new BGM
        to.volume = 0; // Start with the volume at 0
        to.Play();

        // Fade in the new BGM
        currentTime = 0;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            to.volume = Mathf.Lerp(0, startVolume, currentTime / fadeDuration);
            yield return null;
        }
    }
}
