using TMPro;
using UnityEngine;

public class RecordingCanvasPositioner : MonoBehaviour
{
    [SerializeField] private float canvasDistance = 0.5f;
    [SerializeField] private float canvasDropDistance = 0.2f;
    [SerializeField] private TextMeshProUGUI recordingStatusText;
    [SerializeField] private float xAxisTilt = 10f; // Rotation tilt in degrees

    private Camera mainCamera;
    private Canvas canvas;
    private bool isPositioning = false;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInChildren<Canvas>();

        if (mainCamera == null) Debug.LogError("Main camera not found!");
        if (canvas == null) Debug.LogError("Canvas component not found on this GameObject!");

        // Register the listener while the object is active
        if (GameManager.Instance != null)
        {
            GameManager.Instance.onOrbGrabbed.AddListener(ShowCanvas);
        }

        // Hide canvas initially
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Keep canvas in front of camera if it's active
        if (isPositioning && gameObject.activeSelf)
        {
            PositionCanvasInFrontOfUser();
        }
    }

    public void ShowCanvas()
    {
        gameObject.SetActive(true);
        isPositioning = true;
        PositionCanvasInFrontOfUser();

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Ready to record";
        }

        Debug.Log("Canvas shown and positioning started");
    }

    //public void HideCanvas()
    //{
    //    gameObject.SetActive(false);
    //    isPositioning = false;
    //    Debug.Log("Canvas hidden");
    //}   

    private void PositionCanvasInFrontOfUser()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        // Position canvas in front of camera and lower it
        Vector3 targetPosition = mainCamera.transform.position +
                                 (mainCamera.transform.forward * canvasDistance) +
                                 (Vector3.down * canvasDropDistance);

        transform.position = targetPosition;

        // Rotate the canvas to face the camera
        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        lookDirection.y = 0; // Keeps the canvas vertical

        if (lookDirection != Vector3.zero)
        {
            // Apply look rotation, then tilt 10 degrees on x-axis
            transform.rotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(xAxisTilt, 0, 0);
        }
    }
}