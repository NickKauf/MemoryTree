using UnityEngine;
using Oculus.Interaction;

public class PokeButtonMemorySpawner : MonoBehaviour
{
    [Header("Memory Prefabs")]
    public GameObject[] memoryPrefabs; // Your orb variations
    
    [Header("Spawn Settings")]
    public Transform spawnLocation; // Where orbs appear
    public float spawnHeight = 0.3f; // Height above spawn point
    
    [Header("Button Reference")]
    public PokeInteractable pokeButton; // The button's PokeInteractable component
    
    private bool orbSpawned = false; // To prevent multiple spawns from one poke    
    void Start()
    {
        // Listen for button pokes
        if (pokeButton != null)
        {
            pokeButton.WhenPointerEventRaised += HandlePokeEvent;
        }
        else
        {
            Debug.LogError("PokeInteractable not assigned!");
        }
    }
    
    void HandlePokeEvent(PointerEvent evt)
    {
        // Only spawn when button is pressed
        if (evt.Type == PointerEventType.Select)
        {
            SpawnRandomMemory();
        }
    }
    
    void SpawnRandomMemory()
    {
        if (memoryPrefabs.Length == 0)
        {
            Debug.LogWarning("No memory prefabs assigned! And Orb has already spawn");
            return;
        }

        // Prevent spawning if already spawned
        if (orbSpawned)
        {
            Debug.LogWarning("Orb already spawned! Cannot spawn another.");
            return;
        }
        // Pick random prefab
        int randomIndex = Random.Range(0, memoryPrefabs.Length);
        GameObject prefabToSpawn = memoryPrefabs[randomIndex];
        
        // Spawn at designated location
        Vector3 spawnPos = spawnLocation != null ? 
            spawnLocation.position + Vector3.up * spawnHeight : 
            transform.position + Vector3.up * spawnHeight;
        
        GameObject newMemory = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        orbSpawned = true;

        Debug.Log($"Spawned {prefabToSpawn.name}");
    }
    
    void OnDestroy()
    {
        // Clean up event listener
        if (pokeButton != null)
        {
            pokeButton.WhenPointerEventRaised -= HandlePokeEvent;
        }
    }
}