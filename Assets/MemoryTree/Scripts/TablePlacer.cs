using UnityEngine;
using Meta.XR.MRUtilityKit;

public class PlaceOnTable : MonoBehaviour
{
    public GameObject objectToPlace;
    
    [Header("Position Offsets")]
    public float xOffset = 0f;
    public float heightOffset = 0.1f;
    public float zOffset = 0f;
    
    [Header("Rotation")]
    public Vector3 rotationOffset = Vector3.zero; // Add custom rotation

    void Start()
    {
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

        MRUKAnchor tableAnchor = currentRoom.FindLargestSurface(
            MRUKAnchor.SceneLabels.TABLE
        );

        if (tableAnchor != null)
        {
            Vector3 tableCenter = tableAnchor.transform.position;
            
            Vector3 offset = tableAnchor.transform.right * xOffset +
                           Vector3.up * heightOffset +
                           tableAnchor.transform.forward * zOffset;
            
            objectToPlace.transform.position = tableCenter + offset;
            
            // Apply table rotation + custom offset
            objectToPlace.transform.rotation = tableAnchor.transform.rotation * Quaternion.Euler(rotationOffset);
            
            Debug.Log($"Object placed on table at: {tableCenter + offset}");
        }
        else
        {
            Debug.LogWarning("No table found in scene!");
            FallbackPlacement();
        }
    }

    void FallbackPlacement()
    {
        Transform cam = Camera.main.transform;
        Vector3 pos = cam.position + cam.forward * 1.5f;
        pos.y = 0.75f;
        objectToPlace.transform.position = pos;
        Debug.Log("Fallback: Placed in front of user");
    }
}