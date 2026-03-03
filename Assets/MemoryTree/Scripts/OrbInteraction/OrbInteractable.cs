using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using System;

public class OrbInteractable : MonoBehaviour
{
    [SerializeField] private HandGrabInteractable handGrabInteractable;
    public bool isOrbGrabbed = false;
    private bool grabEventFired = false;

    // every orb has a unique identifier (for this orb instance)
    private string orbId;
    public string OrbId => orbId;

    // store the memory data when shared
    public MemoryData memoryData { get; private set; }

    private void Awake()
    {
        // generate unique ID for this orb instance
        orbId = Guid.NewGuid().ToString().Substring(0, 8);

        if (handGrabInteractable == null)
        {
            handGrabInteractable = GetComponent<HandGrabInteractable>();
        }

        Debug.Log($"Orb created with ID: {orbId}");
    }

    private void OnEnable()
    {
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised += OnGrabEvent;
        }
    }

    private void OnDisable()
    {
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }

    void OnGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            if (!grabEventFired)
            {
                isOrbGrabbed = true;
                grabEventFired = true;

                // Register this orb as the current grabbed orb
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCurrentGrabbedOrb(this);
                    GameManager.Instance.onOrbGrabbed.Invoke();
                }

                Debug.Log($"✓ Orb grabbed! ID: {orbId}");
            }
        }
        //else if (evt.Type == PointerEventType.Unselect)
        //{
        //    isOrbGrabbed = false;
        //    grabEventFired = false;

        //    // Clear current orb reference when released
        //    if (GameManager.Instance != null)
        //    {
        //        GameManager.Instance.ClearCurrentOrb();
        //    }

        //    Debug.Log($"✗ Orb released! ID: {orbId}");
        //}
    }

    // Attach memory data to this orb before sharing
    public void SetMemoryData(MemoryData data)
    {
        memoryData = data;
        Debug.Log($"Orb {orbId} now has memory: {data.audioFileName}");
    }

    public MemoryData GetMemoryData()
    {
        return memoryData;
    }

    // Destroy this orb (called after sharing when moving to WorldTree scene)
    public void DestroyOrb()
    {
        Debug.Log($"Destroying orb {orbId}");
        Destroy(gameObject);
    }
}