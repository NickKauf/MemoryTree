using System.Collections.Generic;
using UnityEngine;
using LearnXR.Core.Utilities; // Added for SpatialLogger

public class WorldTreeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab of the Orb to spawn in the World Tree")]
    [SerializeField] private GameObject orbPrefab;

    [Tooltip("The center point where the tree is located")]
    [SerializeField] private Transform treeCenter;

    [Tooltip("How far away from the center the orbs should spawn")]
    [SerializeField] private float spawnRadius = 2.0f;

    [Tooltip("Height offset from the tree center")]
    [SerializeField] private float heightOffset = 1.0f;

    void Start()
    {
        if (treeCenter == null) treeCenter = transform;

        SpawnMemories();
    }

    private void SpawnMemories()
    {
        // 1. Check if we have the GameManager and any memories
        if (GameManager.Instance == null)
        {
            SpatialLogger.Instance.LogError("No GameManager found. Are you testing the WorldTree scene directly?");
            return;
        }

        List<MemoryData> memoriesToSpawn = GameManager.Instance.GetAllSharedMemories();

        if (memoriesToSpawn.Count == 0)
        {
            SpatialLogger.Instance.LogError("No shared memories to spawn.");
            return;
        }

        SpatialLogger.Instance.LogInfo($"Spawning {memoriesToSpawn.Count} memory orbs around the World Tree...");

        // 2. Loop through each memory and spawn an orb
        for (int i = 0; i < memoriesToSpawn.Count; i++)
        {
            // Calculate a circular position using basic trigonometry
            // This evenly spaces out however many orbs you have in a 360-degree circle
            float angle = i * Mathf.PI * 2 / memoriesToSpawn.Count;
            float x = Mathf.Cos(angle) * spawnRadius;
            float z = Mathf.Sin(angle) * spawnRadius;

            Vector3 spawnPosition = treeCenter.position + new Vector3(x, heightOffset, z);

            // Instantiate the orb
            GameObject newOrb = Instantiate(orbPrefab, spawnPosition, Quaternion.identity);
            newOrb.name = $"MemoryOrb_{i}";

            // Pass the memory data to the new orb
            OrbPlayer orbPlayer = newOrb.GetComponent<OrbPlayer>();
            if (orbPlayer != null)
            {
                orbPlayer.SetMemoryData(memoriesToSpawn[i]);
                SpatialLogger.Instance.LogInfo($"✓ Successfully spawned and assigned data to {newOrb.name}");
            }
            else
            {
                SpatialLogger.Instance.LogError($"X Orb prefab {newOrb.name} is missing the OrbPlayer script!");
            }
        }
    }
}