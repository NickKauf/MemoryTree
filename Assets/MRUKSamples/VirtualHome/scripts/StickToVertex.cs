// Copyright (c) Meta Platforms, Inc. and affiliates.

using UnityEngine;

public class StickToVertex : MonoBehaviour
{
    [SerializeField] GameObject _targetGameObject;
    [SerializeField] Transform _parentToScaleCompensate;
    [SerializeField] int _vertexIndex;

    private MeshFilter _meshFilter;
    private Vector3[] _vertices;
    private Vector3 _cachedParentScale = Vector3.zero;
    private Vector3 _cachedInverseScale;
    private Transform _targetTransform;
    private bool _isInitialized = false;

    void Start()
    {
        InitializeCache();
    }

    void InitializeCache()
    {
        if (_targetGameObject == null)
        {
            Debug.LogError($"StickToVertex on {gameObject.name}: Target GameObject is null!");
            enabled = false;
            return;
        }

        _meshFilter = _targetGameObject.GetComponent<MeshFilter>();
        if (_meshFilter == null || _meshFilter.mesh == null)
        {
            Debug.LogError($"StickToVertex on {gameObject.name}: Target GameObject missing MeshFilter or mesh!");
            enabled = false;
            return;
        }

        _vertices = _meshFilter.mesh.vertices;
        if (_vertexIndex < 0 || _vertexIndex >= _vertices.Length)
        {
            Debug.LogError($"StickToVertex on {gameObject.name}: Vertex index {_vertexIndex} out of bounds (mesh has {_vertices.Length} vertices)!");
            enabled = false;
            return;
        }

        _targetTransform = _targetGameObject.transform;

        if (_parentToScaleCompensate != null)
        {
            UpdateInverseScale();
        }

        _isInitialized = true;
    }

    void UpdateInverseScale()
    {
        Vector3 currentScale = _parentToScaleCompensate.localScale;
        if (_cachedParentScale != currentScale)
        {
            _cachedParentScale = currentScale;
            _cachedInverseScale = new Vector3(
                currentScale.x != 0 ? 1f / currentScale.x : 1f,
                currentScale.y != 0 ? 1f / currentScale.y : 1f,
                currentScale.z != 0 ? 1f / currentScale.z : 1f
            );
        }
    }

    void Update()
    {
        if (!_isInitialized)
            return;

        var localVertexPosition = _vertices[_vertexIndex];
        var worldVertexPosition = _targetTransform.TransformPoint(localVertexPosition);
        transform.position = worldVertexPosition;

        if (_parentToScaleCompensate != null)
        {
            UpdateInverseScale();
            transform.localScale = _cachedInverseScale;
        }
    }

}
