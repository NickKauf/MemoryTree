using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;

public class OrbInteractable : MonoBehaviour
{
    [Header("Grab Detection")]
    [SerializeField]
    private HandGrabInteractable handGrabInteractable;

    [Header("UI Canvas")]
    [SerializeField]
    private GameObject recordingCanvas;

    [SerializeField]
    private float canvasDistance = 0.3f;

    [SerializeField]
    private float canvasDropDistance = 0.3f; // How far down from eye level

    private Camera mainCamera;
    private bool isOrbGrabbed = false;

    void Start()
    {
        mainCamera = Camera.main;

        // If canvas isn't assigned, try to find it as a child
        if (recordingCanvas == null)
        {
            Canvas canvasComponent = GetComponentInChildren<Canvas>();
            if (canvasComponent != null)
            {
                recordingCanvas = canvasComponent.gameObject;
            }
        }

        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised += OnGrabEvent;
        }
        else
        {
            Debug.LogError("HandGrabInteractable not assigned!");
        }

        if (recordingCanvas == null)
        {
            Debug.LogWarning("Recording canvas not found as child or assigned!");
        }
        else
        {
            recordingCanvas.SetActive(false);
        }
    }

    void Update()
    {
        // Keep canvas in front of user if orb is grabbed
        if (isOrbGrabbed && recordingCanvas != null && recordingCanvas.activeSelf)
        {
            PositionCanvasInFrontOfUser();
        }
    }

    void OnGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select)
        {
            isOrbGrabbed = true;
            Debug.Log($"✓ Orb grabbed! Grab ID: {evt.Identifier}");
            ShowRecordingCanvas();
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            isOrbGrabbed = false;
            Debug.Log($"✗ Orb released!");
            HideRecordingCanvas();
        }
    }

    void ShowRecordingCanvas()
    {
        if (recordingCanvas != null)
        {
            recordingCanvas.SetActive(true);
            PositionCanvasInFrontOfUser();
            Debug.Log($"Recording canvas shown for {gameObject.name}");
        }
    }

    void HideRecordingCanvas()
    {
        if (recordingCanvas != null)
        {
            recordingCanvas.SetActive(false);
            Debug.Log($"Recording canvas hidden for {gameObject.name}");
        }
    }

    void PositionCanvasInFrontOfUser()
    {
        if (mainCamera == null || recordingCanvas == null)
            return;

        // Position canvas in front of camera and lower
        Vector3 targetPosition = mainCamera.transform.position +
                                 mainCamera.transform.forward * canvasDistance +
                                 Vector3.down * canvasDropDistance;
        recordingCanvas.transform.position = targetPosition;

        // Make canvas face away from camera (180 degree rotation)
        recordingCanvas.transform.LookAt(mainCamera.transform.position);
        recordingCanvas.transform.Rotate(0, 180, 0);
    }

    void OnDestroy()
    {
        if (handGrabInteractable != null)
        {
            handGrabInteractable.WhenPointerEventRaised -= OnGrabEvent;
        }
    }
}       