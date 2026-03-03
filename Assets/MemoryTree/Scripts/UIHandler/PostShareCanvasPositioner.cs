using UnityEngine;

public class PostShareCanvasPositioner : BaseCanvasPositioner
{
    [Tooltip("Drag the Review Canvas GameObject here")]
    [SerializeField] private GameObject reviewCanvas;

    public override void ShowCanvas()
    {
        // Hide the review canvas first
        if (reviewCanvas != null)
        {
            reviewCanvas.SetActive(false);
            AudioManager.Instance.Play("dialogue_three");
        }

        base.ShowCanvas();
        Debug.Log("Post-Share Canvas shown and positioned.");
    }
}