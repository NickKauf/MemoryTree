using UnityEngine;

public class ReviewCanvasPositioner : BaseCanvasPositioner
{
    [Tooltip("Drag the Record Canvas GameObject here so we can hide it")]
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
        // Hide the recording canvas first
        if (recordCanvas != null)
        {
            recordCanvas.SetActive(false);
            AudioManager.Instance.Play("dialogue_two");
        }

        base.ShowCanvas();
        Debug.Log("Review Canvas shown and positioned.");
    }
}