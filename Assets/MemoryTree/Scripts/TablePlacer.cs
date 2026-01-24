using UnityEngine;
using Meta.XR.MRUtilityKit;

public class PlaceOnTable : MonoBehaviour
{
    public GameObject objectToPlace; // Drag your primitive/tree here
    
    [Header("Position Offsets")]
    public float xOffset = 0f; // Left/Right
    public float heightOffset = 0.1f; // Up/Down (Y axis)
    public float zOffset = 0f; // Forward/Back

    void Start()
    {
        // Wait for scene to load, then place object
        if (MRUK.Instance != null && MRUK.Instance.IsInitialized)
        {
            PlaceObjectOnTable();
        }
        else
        {
            MRUK.Instance.RegisterSceneLoadedCallback(PlaceObjectOnTable);
        }
    }

    void PlaceObjectOnTable()
    {
        MRUKRoom currentRoom = MRUK.Instance.GetCurrentRoom();
        
        if (currentRoom == null)
        {
            Debug.LogError("No room found!");
            return;
        }

        // Find the largest table in the room
        MRUKAnchor tableAnchor = currentRoom.FindLargestSurface(
            MRUKAnchor.SceneLabels.TABLE
        );

        if (tableAnchor != null)
        {
            // Get table center position
            Vector3 tableCenter = tableAnchor.transform.position;
            
            // Apply offsets relative to table orientation
            Vector3 offset = tableAnchor.transform.right * xOffset +
                           Vector3.up * heightOffset +
                           tableAnchor.transform.forward * zOffset;
            
            objectToPlace.transform.position = tableCenter + offset;
            objectToPlace.transform.rotation = tableAnchor.transform.rotation;
            
            Debug.Log($"Object placed on table at: {tableCenter + offset}");
        }
        else
        {
            Debug.LogWarning("No table found in scene! Make sure your space has a table labeled.");
            FallbackPlacement();
        }
    }

    void FallbackPlacement()
    {
        // If no table found, place in front of user
        Transform cam = Camera.main.transform;
        Vector3 pos = cam.position + cam.forward * 1.5f;
        pos.y = 0.75f;
        objectToPlace.transform.position = pos;
        Debug.Log("Fallback: Placed in front of user");
    }
}