using UnityEngine;

public class RecordingCanvasPositioner : MonoBehaviour
{
    [SerializeField]
    private float canvasDistance = 0.3f;

    [SerializeField]
    private float canvasDropDistance = 0.3f;

    private Camera mainCamera;
    private Canvas canvas;
    private bool isPositioning = false;

    void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();

        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas component not found on this GameObject!");
        }

        // Hide canvas initially
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPositioning && mainCamera != null && canvas != null)
        {
            PositionCanvasInFrontOfUser();
        }
    }

    /// <summary>
    /// Shows the canvas and starts positioning it in front of the user.
    /// </summary>
    public void ShowCanvas()
    {
        gameObject.SetActive(true);
        isPositioning = true;
        Debug.Log("Canvas shown and positioning started");
    }

    /// <summary>
    /// Hides the canvas and stops positioning.
    /// </summary>
    public void HideCanvas()
    {
        gameObject.SetActive(false);
        isPositioning = false;
        Debug.Log("Canvas hidden");
    }

    void PositionCanvasInFrontOfUser()
    {
        if (mainCamera == null)
            return;

        // Position canvas in front of camera and lower
        Vector3 targetPosition = mainCamera.transform.position +
                                 mainCamera.transform.forward * canvasDistance +
                                 Vector3.down * canvasDropDistance;
        transform.position = targetPosition;

        // Make canvas face away from camera (180 degree rotation)
        transform.LookAt(mainCamera.transform.position);
        transform.Rotate(0, 180, 0);
    }
}