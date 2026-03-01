using UnityEngine;
using UnityEngine.UI;

public class ReviewAndShareHandler : MonoBehaviour
{
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button shareButton;

    [Tooltip("Drag the Record Canvas (which has the RecordingCanvasPositioner script) here")]
    [SerializeField] private RecordingCanvasPositioner recordCanvasPositioner;

    [Tooltip("Drag the Share Canvas GameObject here")]
    [SerializeField] private GameObject shareCanvas;

    [SerializeField] private VoiceRecorder voiceRecorder;

    void Start()
    {
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        if (shareButton != null)
            shareButton.onClick.AddListener(OnShareClicked);
    }

    private void OnTryAgainClicked()
    {
        // Reset voice recorder before showing recording canvas
        if (voiceRecorder != null)
        {
            voiceRecorder.ResetRecording();
        }

        // Hide the share canvas
        if (shareCanvas != null)
            shareCanvas.SetActive(false);

        // Show the record canvas
        if (recordCanvasPositioner != null)
        {
            recordCanvasPositioner.ShowCanvas();
        }
        else
        {
            Debug.LogError("Record Canvas Positioner is not assigned!");
        }

        Debug.Log("Try Again - Recording canvas ready for new recording");
    }

    private void OnShareClicked()
    {
        Debug.Log("Share button clicked! Implement sharing logic to World Tree.");
        // TODO: Implement sharing functionality
    }

    void OnDestroy()
    {
        if (tryAgainButton != null)
            tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
        if (shareButton != null)
            shareButton.onClick.RemoveListener(OnShareClicked);
    }
}