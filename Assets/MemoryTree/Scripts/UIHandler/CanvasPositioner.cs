using UnityEngine;

public class CanvasPositioner : BaseCanvasPositioner
{
    [SerializeField] private GameObject recordCanvas;

    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onRecordingStop.AddListener(ShowCanvas);
        }
    }

    public override void ShowCanvas()
    {
        if (recordCanvas != null)
        {
            recordCanvas.SetActive(false);
        }

        base.ShowCanvas();
        Debug.Log("Share Canvas shown and positioned.");
    }
}