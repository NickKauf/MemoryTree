using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LearnXR.Core.Utilities;

public class WorldTreeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The prefab of the Orb to spawn in the World Tree")]
    [SerializeField] private GameObject orbPrefab;

    [Tooltip("The center point where the tree is located")]
    [SerializeField] private Transform treeCenter;

    [Tooltip("How far away from the center the orbs should spawn (0 = at center, 1+ = in a circle)")]
    [SerializeField] private float spawnRadius = 3.0f; // NOW SERIALIZED

    [Tooltip("Height offset from the tree center")]
    [SerializeField] private float heightOffset = 0f; // NOW SERIALIZED

    [SerializeField] private bool debugVisualization = true;

    void Start()
    {
        StartCoroutine(InitializeAndSpawn());
    }

    private IEnumerator InitializeAndSpawn()
    {
        yield return new WaitForSeconds(1f);

        if (treeCenter == null)
        {
            treeCenter = transform;
            Debug.LogWarning("TreeCenter not assigned, using WorldTreeSpawner's position");
        }

        Debug.Log($"[WorldTreeSpawner] Tree Center: {treeCenter.position}, Spawn Radius: {spawnRadius}, Height Offset: {heightOffset}");
        SpawnMemories();
    }

    private void SpawnMemories()
    {
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

        for (int i = 0; i < memoriesToSpawn.Count; i++)
        {
            float angle = i * Mathf.PI * 2 / memoriesToSpawn.Count;
            float x = Mathf.Cos(angle) * spawnRadius;
            float z = Mathf.Sin(angle) * spawnRadius;

            Vector3 spawnPosition = treeCenter.position + new Vector3(x, heightOffset, z);

            if (debugVisualization)
            {
                Debug.Log($"[Spawn {i}] Position: {spawnPosition}, Distance from tree: {Vector3.Distance(spawnPosition, treeCenter.position):F2}");
            }

            GameObject newOrb = Instantiate(orbPrefab, spawnPosition, Quaternion.identity);
            newOrb.name = $"MemoryOrb_{i}";

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

    private void OnDrawGizmosSelected()
    {
        if (treeCenter == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(treeCenter.position, 0.2f);

        Gizmos.color = Color.green;
        DrawCircle(treeCenter.position, spawnRadius, 20);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(treeCenter.position, treeCenter.position + Vector3.up * heightOffset);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        if (radius <= 0) return;

        float angleStep = Mathf.PI * 2 / segments;
        Vector3 lastPoint = center + new Vector3(radius, heightOffset, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, heightOffset, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}