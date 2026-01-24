// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

using UnityEngine;


namespace Meta.XR.MRUtilityKitSamples.QRCodeDetection
{
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public class Bounded2DVisualizer : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _lineRenderer;
        [SerializeField]
        RectTransform _canvasRect;
        [SerializeField, Tooltip("(in Canvas-local units)")]
        Vector3 _canvasOffset = new(0f, -15f, 0f);

        MRUKTrackable _trackable;

        Rect _box;

        public void Initialize(MRUKTrackable trackable)
        {
            _trackable = trackable;

            if (trackable.PlaneBoundary2D == null && trackable.PlaneRect == null)
            {
                Debug.LogWarning($"{trackable} is missing a plane component.");
            }
            else
            {
                UpdateBoundingBox();
            }
        }

        void Update()
        {
            if (!_trackable)
            {
                return;
            }

            UnityEngine.Assertions.Assert.IsTrue(_trackable.PlaneRect.HasValue);
            _box = _trackable.PlaneRect.Value;

            UpdateBoundingBox();

            if (!_canvasRect)
            {
                return;
            }

            _canvasRect.localPosition = new Vector3(
                x: _box.center.x + _canvasOffset.x * _canvasRect.localScale.x,
                y: _box.yMin + _canvasOffset.y * _canvasRect.localScale.y,
                z: _canvasOffset.z * _canvasRect.localScale.z
            );
        }

        void UpdateBoundingBox()
        {
            _lineRenderer.positionCount = 4;
            _lineRenderer.SetPosition(0, new Vector3(_box.x, _box.y, 0));
            _lineRenderer.SetPosition(1, new Vector3(_box.x + _box.width, _box.y, 0));
            _lineRenderer.SetPosition(2, new Vector3(_box.x + _box.width, _box.y + _box.height, 0));
            _lineRenderer.SetPosition(3, new Vector3(_box.x, _box.y + _box.height, 0));
        }
    }
}
