// Copyright (c) Meta Platforms, Inc. and affiliates.


using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.XR.MRUtilityKitSamples.StartScene
{
    [MetaCodeSample("MRUKSample-StartScene")]
    public class ReturnToStartScene : MonoBehaviour
    {
        [SerializeField] private GameObject Tooltip;
        private static ReturnToStartScene _instance;
        private const string _startSceneName = "StartScene";
        private bool _showStartButtonTooltip => SceneManager.GetActiveScene().name != _startSceneName;
        private const float _forwardTooltipOffset = -0.05f;
        private const float _upwardTooltipOffset = -0.003f;
        private Transform _leftControllerAnchor;
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += (_, _) =>
                {
                    Tooltip.SetActive(_showStartButtonTooltip);
                    _leftControllerAnchor = FindFirstObjectByType<OVRCameraRig>().leftControllerAnchor;
                };
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }

            _leftControllerAnchor = FindFirstObjectByType<OVRCameraRig>().leftControllerAnchor;
        }


        private void Update()
        {
            if (OVRInput.GetUp(OVRInput.Button.Start) && SceneManager.GetActiveScene().name != _startSceneName)
            {
                SceneManager.LoadScene(0);
            }

            Tooltip.SetActive(_showStartButtonTooltip);

            if (_showStartButtonTooltip && _leftControllerAnchor)
            {

                // place the tooltip on the left controller
                var finalRotation = _leftControllerAnchor.rotation * Quaternion.Euler(45, 0, 0);
                var forwardOffsetPosition = finalRotation * Vector3.forward * _forwardTooltipOffset;
                var upwardOffsetPosition = finalRotation * Vector3.up * _upwardTooltipOffset;
                var finalPosition = _leftControllerAnchor.position +
                                   forwardOffsetPosition + upwardOffsetPosition;
                Tooltip.transform.rotation = finalRotation;
                Tooltip.transform.position = finalPosition;
            }
        }
    }
}
