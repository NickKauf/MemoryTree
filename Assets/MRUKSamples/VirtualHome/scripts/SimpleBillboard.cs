// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.VirtualHome
{
    [MetaCodeSample("MRUKSample-VirtualHome")]
    public class SimpleBillboard : MonoBehaviour
    {
        private Camera _mainCamera;

        void Start()
        {
            _mainCamera = Camera.main;
        }

        void Update()
        {
            var direction = transform.position - _mainCamera.transform.position;
            var rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation;
        }
    }
}
