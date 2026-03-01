using TMPro;
using UnityEngine;

public class RecordingCanvasPositioner : BaseCanvasPositioner
{
    [SerializeField] private TextMeshProUGUI recordingStatusText;
    [SerializeField] private VoiceRecorder voiceRecorder;

    protected override void Start()
    {
        base.Start();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onOrbGrabbed.AddListener(ShowCanvas);
        }
    }

    public override void ShowCanvas()
    {
        base.ShowCanvas();

        // Reset recording state
        if (voiceRecorder != null)
        {
            voiceRecorder.ResetRecording();
        }

        // Reset text
        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Ready to record";
        }

        Debug.Log("Record Canvas shown and positioned.");
    }
}