using UnityEngine;
using System.Collections;
using System.IO;

public class WorldTreeService : MonoBehaviour
{
    [SerializeField] private string worldTreeApiUrl = "https://your-worldtree-api.com/api/memories";
    [SerializeField] private bool useLocalTesting = true;

    /// <summary>
    /// Upload memory to World Tree
    /// </summary>
    public void UploadMemory(MemoryData memoryData, string orbId)
    {
        if (memoryData == null)
        {
            Debug.LogError("Memory data is null!");
            return;
        }

        StartCoroutine(UploadMemoryRoutine(memoryData, orbId));
    }

    private IEnumerator UploadMemoryRoutine(MemoryData memoryData, string orbId)
    {
        Debug.Log($"Starting upload to World Tree - Orb ID: {orbId}, File: {memoryData.audioFileName}");

        if (useLocalTesting)
        {
            yield return new WaitForSeconds(1f);
            Debug.Log($"[LOCAL TEST] Successfully 'uploaded' memory from orb {orbId}: {memoryData.audioFileName}");

            // Add to GameManager's shared memories collection
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddSharedMemory(memoryData);
            }

            yield break;
        }

        // Check if file exists
        if (!File.Exists(memoryData.recordingFilePath))
        {
            Debug.LogError($"Recording file not found: {memoryData.recordingFilePath}");
            yield break;
        }

        // Read the audio file
        byte[] audioData = File.ReadAllBytes(memoryData.recordingFilePath);

        Debug.Log($"Ready to upload {audioData.Length} bytes to World Tree with orb ID: {orbId}");

        // TODO: Implement actual HTTP upload
        yield return new WaitForSeconds(1f);

        // After successful upload, add to shared memories
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddSharedMemory(memoryData);
        }
    }
}