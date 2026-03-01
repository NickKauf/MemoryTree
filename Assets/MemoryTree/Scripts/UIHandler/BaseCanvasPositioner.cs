using UnityEngine;

public class BaseCanvasPositioner : MonoBehaviour
{
    [SerializeField] protected float canvasDistance = 0.5f;
    [SerializeField] protected float canvasDropDistance = 0.2f;
    [SerializeField] protected float xAxisTilt = 10f;

    protected Camera mainCamera;
    protected bool isPositioning = false;

    protected virtual void Start()
    {
        mainCamera = Camera.main;
        gameObject.SetActive(false);
    }

    protected virtual void Update()
    {
        if (isPositioning && gameObject.activeSelf)
        {
            PositionCanvasInFrontOfUser();
        }
    }

    public virtual void ShowCanvas()
    {
        gameObject.SetActive(true);
        isPositioning = true;
        PositionCanvasInFrontOfUser();
    }

    public virtual void HideCanvas()
    {
        gameObject.SetActive(false);
        isPositioning = false;
    }

    protected void PositionCanvasInFrontOfUser()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Vector3 targetPosition = mainCamera.transform.position +
                                 (mainCamera.transform.forward * canvasDistance) +
                                 (Vector3.down * canvasDropDistance);

        transform.position = targetPosition;

        Vector3 lookDirection = transform.position - mainCamera.transform.position;
        lookDirection.y = 0;

        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection) * Quaternion.Euler(xAxisTilt, 0, 0);
        }
    }
}