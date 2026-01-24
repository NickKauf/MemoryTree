// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.AI;

namespace MRUtilityKitSample.NavMesh
{
    [MetaCodeSample("MRUK-NavMesh")]
    public class NavMeshCharacterController : MonoBehaviour
    {
        [SerializeField] private Transform _objectSocket;
        [SerializeField] private GameObject _particleEffect;
        [SerializeField] private Transform _headFocusTarget;
        [SerializeField] private Transform _headBone;

        private float lookX; // Horizontal look value (-1 to 1)
        private float lookY; // Vertical look value (-1 to 1)
        private Animator _animator;
        private NavMeshAgent _agent;
        private GameObject _objectEaten;
        private NavMeshAgentController _agentController;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _agentController = GetComponent<NavMeshAgentController>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (_agent.velocity.magnitude != 0)
            {
                _animator.SetFloat("Moving", _agent.velocity.magnitude);
            }
            else
            {
                _animator.SetFloat("Moving", 0);
            }

            HeadLookAt();
        }

        private void OnTriggerEnter(Collider other)
        {
            _animator.SetTrigger("Eat");
            _objectEaten = other.transform.gameObject;
        }

        public void PickUp() //called by animation event
        {
            if (_objectEaten == null)
            {
                return;
            }
            _objectEaten.transform.parent = _objectSocket;
            _objectEaten.transform.localPosition = Vector3.zero;
        }

        private void EatObject() //called by animation event
        {
            if (_objectEaten != null)
            {
                Destroy(_objectEaten);
                Destroy(_agentController.PositionIndicatorInstance);
                Instantiate(_particleEffect, _objectSocket.position, _objectSocket.rotation);
            }
        }

        private void HeadLookAt()
        {
            if (_headFocusTarget == null)
            {
                if (_agentController.PositionIndicatorInstance == null)
                {
                    return;
                }

                _headFocusTarget = _agentController.PositionIndicatorInstance.transform;
            }

            if (_headFocusTarget == null || _headBone == null)
            {
                return;
            }

            // Get the direction vector from the head bone to the target
            var targetDirection = _headFocusTarget.position - _headBone.position;

            // Convert the world space direction to local space relative to this transform
            var localDirection = transform.InverseTransformDirection(targetDirection).normalized;

            // Calculate lookX and lookY values from the local direction
            // lookX is based on the x component (left/right)
            // lookY is based on the y component (up/down)
            lookX = Mathf.Lerp(lookX, Mathf.Clamp(localDirection.x, -1f, 1f), Time.deltaTime * 20);
            lookY = Mathf.Lerp(lookY, Mathf.Clamp(localDirection.y, -1f, 1f), Time.deltaTime * 20);
            _animator.SetFloat("LookX", lookX);
            _animator.SetFloat("LookY", lookY);
        }
    }
}
