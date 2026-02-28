using UnityEngine;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField]
    private int sampleRate = 44100;

    [SerializeField]
    private float maxRecordingDuration = 60f;

    [SerializeField] private TextMeshProUGUI recordingStatusText;
    private AudioSource audioSource;
    private bool isRecording = false;
    private float recordingTimer = 0f;
    private AudioClip currentRecording;

    void Start()
    {
        // Get or create AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (isRecording)
        {
            recordingTimer += Time.deltaTime;

            // Update timer display
            if (recordingStatusText != null)
            {
                int seconds = (int)recordingTimer;
                recordingStatusText.text = $"Recording... {seconds}s";
            }

            // Auto-stop if max duration reached
            if (recordingTimer >= maxRecordingDuration)
            {
                StopRecording();
            }
        }
    }

    /// <summary>
    /// Set the status text reference (called by OrbInteractable).
    /// </summary>
    public void SetStatusText(TextMeshProUGUI statusText)
    {
        recordingStatusText = statusText;
    }

    /// <summary>
    /// Starts recording voice input.
    /// </summary>
    public void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("Already recording!");
            return;
        }

        // Stop any previous recording
        Microphone.End(null);

        isRecording = true;
        recordingTimer = 0f;

        // Start microphone recording
        currentRecording = Microphone.Start(null, false, (int)maxRecordingDuration, sampleRate);

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Recording...";
        }

        Debug.Log("✓ Voice recording started");
    }

    /// <summary>
    /// Stops recording and saves the audio clip.
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording!");
            return;
        }

        isRecording = false;

        // Stop microphone
        Microphone.End(null);

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Saved!";
        }

        Debug.Log($"✗ Voice recording stopped. Duration: {recordingTimer:F2}s");
        Debug.Log($"Audio clip saved: {currentRecording.name}");

        // Invoke callback or event here if needed
        OnRecordingSaved();
    }

    /// <summary>
    /// Called when recording is saved.
    /// </summary>
    void OnRecordingSaved()
    {
        // Custom logic here
    }

    /// <summary>
    /// Returns the current recording audio clip.
    /// </summary>
    public AudioClip GetCurrentRecording()
    {
        return currentRecording;
    }

    /// <summary>
    /// Returns whether currently recording.
    /// </summary>
    public bool IsRecording()
    {
        return isRecording;
    }

    /// <summary>
    /// Get recording duration in seconds.
    /// </summary>
    public float GetRecordingDuration()
    {
        return recordingTimer;
    }
}