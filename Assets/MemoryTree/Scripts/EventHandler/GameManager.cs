using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Android; // Add this!
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public UnityEvent onOrbGrabbed = new UnityEvent();
    public UnityEvent onRecordingStop = new UnityEvent();

    private OrbInteractable currentGrabbedOrb;
    private List<MemoryData> sharedMemories = new List<MemoryData>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Explicitly request microphone permission on Quest/Android
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    public void SetCurrentGrabbedOrb(OrbInteractable orb)
    {
        currentGrabbedOrb = orb;
        Debug.Log($"Current grabbed orb set to: {orb.OrbId}");
    }

    public OrbInteractable GetCurrentGrabbedOrb() => currentGrabbedOrb;
    public void ClearCurrentOrb() => currentGrabbedOrb = null;

    public void AddSharedMemory(MemoryData memory)
    {
        sharedMemories.Add(memory);
        Debug.Log($"Memory added. Total memories: {sharedMemories.Count}");
    }

    public List<MemoryData> GetAllSharedMemories() => sharedMemories;
}