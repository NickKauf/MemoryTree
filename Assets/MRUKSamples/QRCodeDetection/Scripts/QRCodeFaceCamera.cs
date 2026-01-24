// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.QRCodeDetection
{
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    [RequireComponent(typeof(Canvas))]
    public class QRCodeFaceCamera : MonoBehaviour
    {
        Canvas _canvas;

        void Start()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.worldCamera = Camera.main;
        }

        void Update()
        {
            if (_canvas && _canvas.worldCamera)
            {
                transform.rotation =
                    Quaternion.LookRotation(transform.position - _canvas.worldCamera.transform.position);
            }
        }
    }
}
