using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReviewAndShareHandler : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button tryAgainButton;        
    [SerializeField] private Button shareButton;           
    [SerializeField] private Button recordAnotherButton; 
    [SerializeField] private Button viewWorldTreeButton; 

    [Header("Canvas Positioners")]
    [SerializeField] private BaseCanvasPositioner recordCanvasPositioner;
    [SerializeField] private BaseCanvasPositioner reviewCanvasPositioner;
    [SerializeField] private BaseCanvasPositioner postShareCanvasPositioner;

    [Header("Services")]
    [SerializeField] private VoiceRecorder voiceRecorder;
    [SerializeField] private WorldTreeService worldTreeService;

    [Header("Post-Share Text")]
    [SerializeField] private TextMeshProUGUI postShareStatusText;

    void Start()
    {
        // Setup Button Listeners
        if (tryAgainButton != null) tryAgainButton.onClick.AddListener(OnTryAgainClicked);
        if (shareButton != null) shareButton.onClick.AddListener(OnShareClicked);
        if (viewWorldTreeButton != null) viewWorldTreeButton.onClick.AddListener(OnViewWorldTreeClicked);
        if (recordAnotherButton != null) recordAnotherButton.onClick.AddListener(OnTryAgainClicked);

        // Auto-find services if not assigned
        if (worldTreeService == null) worldTreeService = FindObjectOfType<WorldTreeService>();
        if (voiceRecorder == null) voiceRecorder = FindObjectOfType<VoiceRecorder>();
    }

    private void OnTryAgainClicked()
    {
        Debug.Log("Resetting for new recording...");


        if (reviewCanvasPositioner != null) reviewCanvasPositioner.HideCanvas();
        if (postShareCanvasPositioner != null) postShareCanvasPositioner.HideCanvas();

        if (recordCanvasPositioner != null)
        {
            recordCanvasPositioner.ShowCanvas();
        }
    }

    private void OnShareClicked()
    {
        Debug.Log("Share button clicked!");

        OrbInteractable currentOrb = GameManager.Instance.GetCurrentGrabbedOrb();

        if (currentOrb == null)
        {
            Debug.LogError("No orb is currently grabbed! Cannot share.");
            return;
        }

        if (voiceRecorder == null)
        {
            Debug.LogError("VoiceRecorder not assigned!");
            return;
        }

        // Get Data
        string recordingPath = voiceRecorder.GetLastSavedPath();
        float duration = voiceRecorder.GetRecordingDuration();

        // Attach to Orb
        MemoryData memoryData = new MemoryData(recordingPath, duration);
        currentOrb.SetMemoryData(memoryData);
        Debug.Log($"Orb {currentOrb.OrbId} now holds memory.");

        // Upload
        if (worldTreeService != null)
        {
            worldTreeService.UploadMemory(memoryData, currentOrb.OrbId);
        }

        // Transition UI
        ShowPostSharePanel();
    }

    private void ShowPostSharePanel()
    {
        if (postShareCanvasPositioner != null)
        {
            postShareCanvasPositioner.ShowCanvas();
        }

        if (postShareStatusText != null)
        {
            postShareStatusText.text = "Memory shared to World Tree!";
        }
    }

    private void OnViewWorldTreeClicked()
    {
        Debug.Log("Loading WorldTree scene...");
        SceneManager.LoadScene("WorldTree");
    }

    void OnDestroy()
    {
        if (tryAgainButton != null) tryAgainButton.onClick.RemoveListener(OnTryAgainClicked);
        if (shareButton != null) shareButton.onClick.RemoveListener(OnShareClicked);
        if (viewWorldTreeButton != null) viewWorldTreeButton.onClick.RemoveListener(OnViewWorldTreeClicked);
        if (recordAnotherButton != null) recordAnotherButton.onClick.RemoveListener(OnTryAgainClicked);
    }
}