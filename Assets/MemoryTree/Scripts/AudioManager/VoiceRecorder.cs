using UnityEngine;
using TMPro;
using System;
using System.IO;
using Unity.VisualScripting;

public class VoiceRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private int sampleRate = 44100;
    [SerializeField] private float maxRecordingDuration = 60f;
    [SerializeField] private TextMeshProUGUI recordingStatusText;

    [Header("Storage Settings")]
    [SerializeField] private bool saveLocally = true; // For testing
    [SerializeField] private bool uploadToCloud = false; // Enable when ready

    private AudioSource audioSource;
    private bool isRecording = false;
    private float recordingTimer = 0f;
    private AudioClip currentRecording;
    private string lastSavedFilePath;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Create recordings directory
        if (saveLocally)
        {
            string recordingsDir = Path.Combine(Application.persistentDataPath, "Recordings");
            if (!Directory.Exists(recordingsDir))
            {
                Directory.CreateDirectory(recordingsDir);
                Debug.Log($"Created recordings directory: {recordingsDir}");
            }
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
            Debug.Log($"Audio clip: {currentRecording.name}");

        OnRecordingSaved();
    }

    void OnRecordingSaved()
    {
        // LOCAL STORAGE (Testing)
        if (saveLocally)
        {
            SaveRecordingLocally();
        }

        // CLOUD STORAGE (Production)
        if (uploadToCloud)
        {
            UploadToCloud();
        }
    }

    /// <summary>
    /// Save recording to persistent local storage (Quest or PC)
    /// </summary>
    void SaveRecordingLocally()
    {
        if (currentRecording == null) return;

        string fileName = $"memory_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.wav";
        string filePath = Path.Combine(
            Application.persistentDataPath,
            "Recordings",
            fileName
        );

        try
        {
            byte[] wavData = AudioClipToWav(currentRecording);
            File.WriteAllBytes(filePath, wavData);
            lastSavedFilePath = filePath;

            Debug.Log($"✓ Recording saved locally: {filePath}");
            Debug.Log($"File size: {wavData.Length / 1024f:F2} KB");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save recording: {e.Message}");
        }
    }

    /// <summary>
    /// Convert AudioClip to WAV byte array
    /// </summary>
    byte[] AudioClipToWav(AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int dataSize = samples.Length * 2;
        byte[] wavFile = new byte[44 + dataSize];

        // WAV header
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0, wavFile, 0, 4);
        BitConverter.GetBytes(36 + dataSize).CopyTo(wavFile, 4);
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0, wavFile, 8, 4);
        Array.Copy(System.Text.Encoding.ASCII.GetBytes("fmt "), 0, wavFile, 12, 4);
        BitConverter.GetBytes(16).CopyTo(wavFile, 16); // Subchunk1Size
        BitConverter.GetBytes((ushort)1).CopyTo(wavFile, 20); // AudioFormat (PCM)
        BitConverter.GetBytes((ushort)clip.channels).CopyTo(wavFile, 22);
        BitConverter.GetBytes(clip.frequency).CopyTo(wavFile, 24);
        BitConverter.GetBytes(clip.frequency * clip.channels * 2).CopyTo(wavFile, 28); // ByteRate
        BitConverter.GetBytes((ushort)(clip.channels * 2)).CopyTo(wavFile, 32); // BlockAlign
        BitConverter.GetBytes((ushort)16).CopyTo(wavFile, 34); // BitsPerSample

        Array.Copy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wavFile, 36, 4);
        BitConverter.GetBytes(dataSize).CopyTo(wavFile, 40);

        // Convert samples to bytes
        int sampleIndex = 0;
        for (int i = 44; i < wavFile.Length; i += 2)
        {
            short sample = (short)(samples[sampleIndex] * 32767f);
            BitConverter.GetBytes(sample).CopyTo(wavFile, i);
            sampleIndex++;
        }

        return wavFile;
    }

    /// <summary>
    /// Upload recording to cloud (implement when ready)
    /// </summary>
    void UploadToCloud()
    {
        if (!string.IsNullOrEmpty(lastSavedFilePath))
        {
            Debug.Log($"Ready to upload: {lastSavedFilePath}");
            // Cloud upload logic here
        }
    }

    public AudioClip GetCurrentRecording() => currentRecording;
    public bool IsRecording() => isRecording;
    public float GetRecordingDuration() => recordingTimer;
    public string GetLastSavedPath() => lastSavedFilePath;
}