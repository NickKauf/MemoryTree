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

    [Header("Playback Settings")]
    [SerializeField] private TextMeshProUGUI playbackStatusText;

    [Header("Storage Settings")]
    [SerializeField] private bool saveLocally = true;
    [SerializeField] private bool uploadToCloud = false;

    private AudioSource audioSource;
    private bool isRecording = false;
    private bool isPlaying = false;
    private float recordingTimer = 0f;
    private AudioClip currentRecording;
    private string lastSavedFilePath;
    private int recordingSamplePosition = 0;

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

        // Update playback status text with progress
        if (isPlaying)
        {
            if (audioSource.isPlaying)
            {
                UpdatePlaybackText();
            }
            else
            {
                isPlaying = false;
                if (playbackStatusText != null)
                {
                    playbackStatusText.text = "Playback complete";
                }
                Debug.Log("✓ Playback finished");
            }
        }
    }

    /// <summary>
    /// Update playback progress text with countdown
    /// </summary>
    void UpdatePlaybackText()
    {
        if (playbackStatusText == null || currentRecording == null)
            return;

        float currentTime = audioSource.time;
        float totalTime = currentRecording.length;
        float remainingTime = totalTime - currentTime;

        // Format remaining time
        int remainingMinutes = (int)remainingTime / 60;
        int remainingSeconds = (int)remainingTime % 60;

        playbackStatusText.text = $"Time Left: {remainingMinutes:D2}:{remainingSeconds:D2}";
    }

    /// <summary>
    /// Toggle recording start/stop
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

        // Stop playback if playing
        if (isPlaying)
        {
            StopPlayback();
        }

        Microphone.End(null);
        isRecording = true;
        recordingTimer = 0f;
        recordingSamplePosition = 0;
        currentRecording = Microphone.Start(null, false, (int)maxRecordingDuration, sampleRate);

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "Recording...";
        }

        Debug.Log("Voice recording started");
    }

    public void StopRecording()
    {
        if (!isRecording)
        {
            Debug.LogWarning("Not currently recording!");
            return;
        }

        isRecording = false;

        // Get the actual position where recording ended
        recordingSamplePosition = Microphone.GetPosition(null);
        Microphone.End(null);

        // Trim the audio clip to only the recorded samples
        if (currentRecording != null && recordingSamplePosition > 0)
        {
            currentRecording = TrimAudioClip(currentRecording, recordingSamplePosition);
        }

        if (recordingStatusText != null)
        {
            recordingStatusText.text = "✓ Saved!";
        }

        Debug.Log($"Voice recording stopped. Duration: {recordingTimer:F2}s");

        if (currentRecording != null)
            Debug.Log($"Audio clip: {currentRecording.name} (Length: {currentRecording.length:F2}s)");

        OnRecordingSaved();
    }

    /// <summary>
    /// Trim AudioClip to only include recorded samples
    /// </summary>
    AudioClip TrimAudioClip(AudioClip clip, int sampleCount)
    {
        // Create a new AudioClip with only the recorded samples
        AudioClip trimmedClip = AudioClip.Create(
            clip.name + "_trimmed",
            sampleCount,
            clip.channels,
            clip.frequency,
            false
        );

        // Get only the recorded audio data
        float[] samples = new float[sampleCount * clip.channels];
        clip.GetData(samples, 0);

        // Set the data on the new clip
        trimmedClip.SetData(samples, 0);

        Debug.Log($"Trimmed clip from {clip.length:F2}s to {trimmedClip.length:F2}s");

        return trimmedClip;
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
    /// Save recording to persistent local storage
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

            Debug.Log($"Recording saved locally: {filePath}");
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
        BitConverter.GetBytes(16).CopyTo(wavFile, 16);
        BitConverter.GetBytes((ushort)1).CopyTo(wavFile, 20);
        BitConverter.GetBytes((ushort)clip.channels).CopyTo(wavFile, 22);
        BitConverter.GetBytes(clip.frequency).CopyTo(wavFile, 24);
        BitConverter.GetBytes(clip.frequency * clip.channels * 2).CopyTo(wavFile, 28);
        BitConverter.GetBytes((ushort)(clip.channels * 2)).CopyTo(wavFile, 32);
        BitConverter.GetBytes((ushort)16).CopyTo(wavFile, 34);

        Array.Copy(System.Text.Encoding.ASCII.GetBytes("data"), 0, wavFile, 36, 4);
        BitConverter.GetBytes(dataSize).CopyTo(wavFile, 40);

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
    /// Play the current recording
    /// </summary>
    public void PlayRecording()
    {
        if (currentRecording == null)
        {
            Debug.LogWarning("No recording to play!");
            if (playbackStatusText != null)
            {
                playbackStatusText.text = "No recording available";
            }
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("Already playing!");
            return;
        }

        audioSource.clip = currentRecording;
        audioSource.Play();
        isPlaying = true;

        if (playbackStatusText != null)
        {
            int totalMinutes = (int)currentRecording.length / 60;
            int totalSeconds = (int)currentRecording.length % 60;
            playbackStatusText.text = $"Time Left: {totalMinutes:D2}:{totalSeconds:D2}";
        }

        Debug.Log($"Playing recording ({currentRecording.length:F2}s)");
    }

    /// <summary>
    /// Stop playback
    /// </summary>
    public void StopPlayback()
    {
        if (!isPlaying)
        {
            Debug.LogWarning("Not currently playing!");
            return;
        }

        audioSource.Stop();
        isPlaying = false;

        if (playbackStatusText != null)
        {
            playbackStatusText.text = "Playback stopped";
        }

        Debug.Log("✗ Playback stopped");
    }

    /// <summary>
    /// Toggle playback start/stop
    /// </summary>
    public void TogglePlayback()
    {
        if (isPlaying) StopPlayback();
        else PlayRecording();
    }

    void UploadToCloud()
    {
        if (!string.IsNullOrEmpty(lastSavedFilePath))
        {
            Debug.Log($"Ready to upload: {lastSavedFilePath}");
        }
    }

    public AudioClip GetCurrentRecording() => currentRecording;
    public bool IsRecording() => isRecording;
    public bool IsPlaying() => isPlaying;
    public float GetRecordingDuration() => recordingTimer;
    public string GetLastSavedPath() => lastSavedFilePath;
}