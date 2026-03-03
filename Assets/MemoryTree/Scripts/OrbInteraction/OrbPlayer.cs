using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using UnityEngine.Networking;
using LearnXR.Core.Utilities; // Added your LearnXR package!

[RequireComponent(typeof(AudioSource))]
public class OrbPlayer : MonoBehaviour
{
    [SerializeField] private HandGrabInteractable handGrabInteractable;

    private AudioSource audioSource;
    private MemoryData memoryData;
    private bool grabEventFired = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (handGrabInteractable == null)
            handGrabInteractable = GetComponent<HandGrabInteractable>();

        audioSource.playOnAwake = false;

        audioSource.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        if (handGrabInteractable != null)
            handGrabInteractable.WhenPointerEventRaised += OnGrabEvent;
    }

    private void OnDisable()
    {
        if (handGrabInteractable != null)
            handGrabInteractable.WhenPointerEventRaised -= OnGrabEvent;
    }

    public void SetMemoryData(MemoryData data)
    {
        memoryData = data;
        SpatialLogger.Instance.LogError($"Received memory: {data.audioFileName}. Loading...");
        StartCoroutine(LoadAudioFile(memoryData.recordingFilePath));
    }

    private IEnumerator LoadAudioFile(string filePath)
    {
        // for Android/Quest local file loading
        string fileUrl = new System.Uri(filePath).AbsoluteUri;

        SpatialLogger.Instance.LogError($"URI: {fileUrl}");

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(fileUrl, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                SpatialLogger.Instance.LogError($"✓ Success! Clip length: {clip.length}s");
            }
            else
            {
                SpatialLogger.Instance.LogError($"X Error loading at {fileUrl}: {www.error}");
            }
        }
    }

    private void OnGrabEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Select && !grabEventFired)
        {
            grabEventFired = true;
            PlayAudio();
        }
        else if (evt.Type == PointerEventType.Unselect)
        {
            grabEventFired = false;
        }
    }

    private void PlayAudio()
    {
        if (audioSource.clip != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            audioSource.Play();
            SpatialLogger.Instance.LogError($"▶ Playing audio: {memoryData.audioFileName}");
        }
        else
        {
            SpatialLogger.Instance.LogError("X Cannot play: Clip is missing.");
        }
    }
}