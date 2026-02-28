using UnityEngine;
using TMPro;

public class VoiceRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float maxRecordingDuration = 60f;
    [SerializeField] private TextMeshProUGUI recordingStatusText;

    private AudioSource audioSource;
    private bool isRecording = false;
    private float recordingTimer = 0f;
    private AudioClip currentRecording;

    void Start()
    {
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

            if (recordingStatusText != null)
            {
                int seconds = (int)recordingTimer;
                recordingStatusText.text = $"Recording... {seconds}s";
            }

            if (recordingTimer >= maxRecordingDuration)
            {
                StopRecording();
            }
        }
    }

    /// <summary>
    /// Connect this to your UI Button's OnClick event!
    /// </summary>
    public void ToggleRecording()
    {
        if (isRecording) StopRecording();
        else StartRecording();
    }

    public void StartRecording()
    {
        if (isRecording)
        {
            Debug.LogWarning("Already recording!");
            return;
        }

        Microphone.End(null);
        isRecording = true;
        recordingTimer = 0f;
        currentRecording = Microphone.Start(null, false, (int)maxRecordingDuration, sampleRate);

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Recording...";
        }

        Debug.Log("✓ Voice recording started");
    }

    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording!");
            return;
        }

        isRecording = false;
        Microphone.End(null);

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Saved!";
        }

        Debug.Log($"✗ Voice recording stopped. Duration: {recordingTimer:F2}s");

        if (currentRecording != null)
            Debug.Log($"Audio clip saved: {currentRecording.name}");

        OnRecordingSaved();
    }

    void OnRecordingSaved()
    {
        // Add logic here to process the audio (e.g., send to API, play back)
    }

    public AudioClip GetCurrentRecording() => currentRecording;
    public bool IsRecording() => isRecording;
    public float GetRecordingDuration() => recordingTimer;
}