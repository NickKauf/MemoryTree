using System;

[System.Serializable]
public class MemoryData
{
    public string recordingFilePath;
    public string audioFileName;
    public float recordingDuration;
    public DateTime recordedAt;
    public string userId; // Optional: for tracking who recorded it

    public MemoryData(string filePath, float duration, string userId = "Anonymous")
    {
        recordingFilePath = filePath;
        audioFileName = System.IO.Path.GetFileName(filePath);
        recordingDuration = duration;
        recordedAt = DateTime.Now;
        this.userId = userId;
    }
}