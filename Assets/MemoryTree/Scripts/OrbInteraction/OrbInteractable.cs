using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;

public class OrbInteractable : MonoBehaviour
{
    [SerializeField] private HandGrabInteractable handGrabInteractable;
    public bool isOrbGrabbed = false;
    private bool grabEventFired = false; // Prevent duplicate event firing

    private void Awake()
    {
        // Auto-assign if you forget to drag it into the inspector
        if (handGrabInteractable == null)
        {
            handGrabInteractable = GetComponent<HandGrabInteractable>();
        }
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
        // 'Select' indicates a successful grab
        if (evt.Type == PointerEventType.Select)
        {
            // Only fire event if it hasn't fired already
            if (!grabEventFired)
            {
                isOrbGrabbed = true;
                grabEventFired = true; // Mark event as fired

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.onOrbGrabbed.Invoke();
                }

                Debug.Log($"✓ Orb grabbed! Grab ID: {evt.Identifier}");
            }
        }
        // Reset when grab ends
        else if (evt.Type == PointerEventType.Unselect)
        {
            isOrbGrabbed = false;
            grabEventFired = false; // Reset for next grab
            Debug.Log($"✗ Orb released!");
        }
    }
}