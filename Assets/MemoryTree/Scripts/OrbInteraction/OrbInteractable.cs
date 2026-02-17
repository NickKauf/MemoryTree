using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class OrbInteractable : MonoBehaviour
{
    [SerializeField]
    private HandGrabInteractable handGrabInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised += OnGrabEvent;
        }
        else
        {
            Debug.LogError("HandGrabInteractable not assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            Debug.Log($"✓ Orb grabbed! Grab ID: {evt.Identifier}");
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            Debug.Log($"✗ Orb released!");
        }
    }

    void OnDestroy()
    {
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }
}
