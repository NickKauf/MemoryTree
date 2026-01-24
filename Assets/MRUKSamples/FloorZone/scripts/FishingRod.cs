// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.Samples;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

namespace MRUtilityKitSample.FindFloorZone
{
    /// <summary>
    /// Controls the fishing rod mechanics including line tension, fish hooking, and controller input handling.
    /// Manages the interaction between the fishing rod, floater, and hooked fish.
    /// </summary>
    [MetaCodeSample("MRUK-FindFloorZone")]
    public class FishingRod : MonoBehaviour
    {
        private const float DEADZONE = 0.1f; // Deadzone to ignore small movements
        [SerializeField] private AimConstraint _rodBase, _rodMid, _rodEnd;
        [SerializeField] private int _rodLayerIndex;
        public Transform _rodTip, _rodTipUndeformed;
        public Transform _floater, _floaterInWater;
        public Transform _activeFloater;
        public bool _isFloaterInWater;
        public float _pullingOutTimer = .1f;
        public float _floaterInWaterAttractionForce = 10;

        [SerializeField] private Transform _stringRoll;
        [SerializeField] private LineRenderer _lineRenderer;

        [SerializeField] private float _tension = 0.1f;
        public Fish _fishHooked;
        public float _stringGiven = .1f;
        public Slider StaminaSlider;

        public OVRInput.Controller controller = OVRInput.Controller.LTouch;
        [SerializeField] private AnimationCurve _vibrationCurve;
        [SerializeField] private AudioClip _stretchSound;
        [SerializeField] private AudioClip _breakingSound;
        [SerializeField] private AudioClip[] _plickInTheWaterSound;
        [SerializeField] private AudioClip[] _perturbedWaterSound;
        private readonly float _pullingOutTimerStart = .1f;
        private ConfigurableJoint _configurableJoint;
        private float _distance;
        private float _stamina = 1;

        private Rigidbody _floaterInWaterRigidbody;
        private Rigidbody _floaterRigidbody;
        private Transform _floaterInWaterTransform;
        private Transform _floaterTransform;
        private Transform _rodTipTransform;
        private Transform _rodTipUndeformedTransform;

        private Ray _tempRay;
        private ConstraintSource _tempConstraintSource;
        private float _previousAngle;
        private bool _wasInDeadzone = true;

        [Header("Audio")] private AudioSource _audioSource;

        private float Stamina
        {
            get => _stamina;
            set
            {
                _stamina = value;
                if (StaminaSlider != null)
                {
                    StaminaSlider.value = value;
                }
            }
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _configurableJoint = _floater.GetComponent<ConfigurableJoint>();
            _floaterInWaterRigidbody = _floaterInWater.GetComponent<Rigidbody>();
            _floaterRigidbody = _floater.GetComponent<Rigidbody>();

            _floaterInWaterTransform = _floaterInWater.transform;
            _floaterTransform = _floater.transform;
            _rodTipTransform = _rodTip;
            _rodTipUndeformedTransform = _rodTipUndeformed;

            _tempConstraintSource = new ConstraintSource();

            _configurableJoint.anchor = Vector3.up * _stringGiven;
            _activeFloater = _floater;
            _isFloaterInWater = false;
            _pullingOutTimer = _pullingOutTimerStart;
        }

        private void Update()
        {
            _lineRenderer.SetPosition(0, _rodTipTransform.position);
            _lineRenderer.SetPosition(1, _activeFloater.transform.position + _activeFloater.transform.up * 0.05f);

            ControllerRodAdjustment();
            PullingOutTimerHandler();
            _configurableJoint.anchor = Vector3.up * _stringGiven;

            if (_fishHooked)
            {
                FishHookedBehaviour();
            }

            // Switch to float in water
            if (!_isFloaterInWater && _floaterTransform.position.y < 0 && _pullingOutTimer <= 0)
            {
                var rayStart = _floaterTransform.position + Vector3.up * 10f;
                _tempRay = new Ray(rayStart, Vector3.down);

                if (Physics.Raycast(_tempRay, out _, 1000, 1 << 4))
                {
                    AudioSource.PlayClipAtPoint(_plickInTheWaterSound[Random.Range(0, _plickInTheWaterSound.Length)],
                        _activeFloater.position);
                    _isFloaterInWater = true;
                    _activeFloater = _floaterInWater;
                    _floater.gameObject.SetActive(false);
                    _floaterInWater.gameObject.SetActive(true);
                    _floaterInWaterTransform.position = _floaterTransform.position;
                    _floaterInWaterTransform.rotation = _floaterTransform.rotation;

                    _tempConstraintSource.sourceTransform = _floaterInWater;
                    _tempConstraintSource.weight = 1;
                    _rodBase.SetSource(0, _tempConstraintSource);
                    _rodMid.SetSource(0, _tempConstraintSource);
                    _rodEnd.SetSource(0, _tempConstraintSource);
                }
            }

            if (_isFloaterInWater && !_fishHooked)
            {
                _rodBase.weight = _tension * 0.2f;
                _rodMid.weight = _tension * 0.4f;
                _rodEnd.weight = _tension * 0.9f;
                OVRInput.SetControllerVibration(0, 0, controller);
                _distance = GetDistanceBetweenTargetAndUndeformedTip();
                _tension = _distance - _stringGiven;

                var projectedTip = new Vector3(_rodTipUndeformedTransform.position.x, 0,
                    _rodTipUndeformedTransform.position.z);
                if (Vector3.Distance(_rodTipUndeformedTransform.position, projectedTip) > _stringGiven + 0.05f)
                {
                    ChangeFloaterFromWaterToAir();
                }

                FloaterInWaterBehaviour();
            }

            if (!_isFloaterInWater)
            {
                FloaterInAirBehaviour();
            }

            // Debug controls
            if (Input.GetKey(KeyCode.UpArrow) || OVRInput.Get(OVRInput.RawButton.Y))
            {
                StringAdjustment(0.2f);
            }

            if (Input.GetKey(KeyCode.DownArrow) || OVRInput.Get(OVRInput.RawButton.X))
            {
                StringAdjustment(-0.2f);
            }
        }

        private void FishHookedBehaviour()
        {
            if (!_fishHooked)
            {
                return;
            }

            _rodBase.weight = 0.1f + _tension * 0.2f;
            _rodMid.weight = 0.1f + _tension * 0.4f;
            _rodEnd.weight = 0.1f + _tension * 0.9f;
            Vibrations();
            _distance = GetDistanceBetweenTargetAndUndeformedTip();
            _tension = _distance - _stringGiven;
            if (_tension >= 0.3f)
            {
                if (_fishHooked.FishStamina > 0.1f)
                {
                    _fishHooked.FishStamina -= _tension * 0.1f * Time.deltaTime;
                }

                if (_tension > 0.75f)
                {
                    if (Stamina > 0)
                    {
                        if (_audioSource.clip != _stretchSound && _audioSource.isPlaying == false)
                        {
                            _audioSource.clip = _stretchSound;
                            _audioSource.Play();
                        }

                        Stamina -= 0.4f * Time.deltaTime;
                    }
                }
                else
                {
                    if (_audioSource.clip == _stretchSound)
                    {
                        _audioSource.clip = null;
                        _audioSource.Stop();
                    }
                }
            }
            else
            {
                if (Stamina < 1)
                {
                    Stamina += Time.deltaTime;
                }

                if (_fishHooked.FishStamina < 1)
                {
                    _fishHooked.FishStamina += 0.1f * Time.deltaTime;
                }
            }

            if (Stamina < 0)
            {
                StringBroken();
            }

            if (_fishHooked && _fishHooked.FishStamina < 0.2f && _tension > 0.8f)
            {
                FishGetsFished();
            }
        }

        private void FishGetsFished()
        {
            _audioSource.clip = null;
            _audioSource.Stop();
            PlayPerturbedWaterSound();
            _tension = 0;
            _fishHooked.Caught();
            _fishHooked.FishStamina = 1;
            _fishHooked = null;
            ResetRod();
        }

        private void StringBroken()
        {
            _audioSource.clip = null;
            _audioSource.Stop();
            AudioSource.PlayClipAtPoint(_breakingSound, _rodTipTransform.position);
            _fishHooked.FishStamina = 1;
            _fishHooked.SwimRange = _fishHooked.SwimRangeStart;
            _fishHooked.Speed = _fishHooked.StartSpeed * 2;
            _fishHooked = null;
            ResetRod();
        }

        private void ResetRod()
        {
            _floaterInWaterRigidbody.isKinematic = false;
            _floaterInWater.gameObject.layer = _rodLayerIndex;
            _stringGiven = 0.01f;
            Stamina = 1;
            _activeFloater = _floater;
            _floater.gameObject.SetActive(true);
            _floaterInWater.gameObject.SetActive(false);
            _floaterInWaterTransform.parent = _floaterTransform.parent;
            _floaterInWaterTransform.position = _floaterTransform.position;
            _floaterInWaterTransform.rotation = _floaterTransform.rotation;
            _isFloaterInWater = false;
        }

        private void FloaterInAirBehaviour()
        {
            OVRInput.SetControllerVibration(0, 0, controller);
            _rodBase.weight = 0;
            _rodMid.weight = 0;
            _rodEnd.weight = 0;
        }

        private void ChangeFloaterFromWaterToAir()
        {
            _configurableJoint.anchor = Vector3.up * _stringGiven;
            _isFloaterInWater = false;
            _activeFloater = _floater;
            _floater.gameObject.SetActive(true);
            _floaterInWater.gameObject.SetActive(false);
            _floaterTransform.position = _floaterInWaterTransform.position;
            _floaterTransform.rotation = _floaterInWaterTransform.rotation;

            _tempConstraintSource.sourceTransform = _floater;
            _tempConstraintSource.weight = 1;
            _rodBase.SetSource(0, _tempConstraintSource);
            _rodMid.SetSource(0, _tempConstraintSource);
            _rodEnd.SetSource(0, _tempConstraintSource);

#if UNITY_6000_0_OR_NEWER
            _floaterRigidbody.linearVelocity = Vector3.zero;
#else
            _floaterRigidbody.velocity = Vector3.zero;
#endif
            _pullingOutTimer = _pullingOutTimerStart;
        }

        private void PullingOutTimerHandler()
        {
            if (_pullingOutTimer <= 0)
            {
                return;
            }

            _pullingOutTimer -= Time.deltaTime;
        }

        private void FloaterInWaterBehaviour()
        {
            if (!_isFloaterInWater)
            {
                return;
            }

            var tipProjectedOnGround = new Vector3(_rodTipTransform.position.x, 0, _rodTipTransform.position.z);
            var forceDirection = tipProjectedOnGround - _floaterInWaterTransform.position;
            var clampedTension = Mathf.Clamp(_tension, 0, Mathf.Infinity);
            _floaterInWaterRigidbody.AddForce(forceDirection *
                                              (_floaterInWaterAttractionForce * clampedTension * Time.deltaTime));

            var rayStart = _floaterInWaterTransform.position + Vector3.up * 10f;
            _tempRay = new Ray(rayStart, Vector3.down);

            if (Physics.Raycast(_tempRay, out var hit, 1000, 1 << 4))
            {
                var newPosition = new Vector3(
                    _floaterInWaterTransform.position.x,
                    hit.point.y + (Mathf.PerlinNoise(Time.time * 1.5f, 0) - 0.5f) * 0.12f,
                    _floaterInWaterTransform.position.z
                );
                _floaterInWaterRigidbody.position = newPosition;

                _floaterInWaterTransform.rotation = Quaternion.Lerp(_floaterInWaterTransform.rotation,
                    Quaternion.identity, Time.deltaTime * 5);
            }
            else
            {
                ChangeFloaterFromWaterToAir();
            }
        }

        public void PlayPerturbedWaterSound()
        {
            AudioSource.PlayClipAtPoint(_perturbedWaterSound[Random.Range(0, _perturbedWaterSound.Length)],
                _activeFloater.position);
        }

        private void Vibrations()
        {
            OVRInput.SetControllerVibration(_vibrationCurve.Evaluate(_tension), _vibrationCurve.Evaluate(_tension),
                controller);
        }

        public float GetDistanceBetweenTargetAndUndeformedTip()
        {
            return Vector3.Distance(_activeFloater.position, _rodTipUndeformedTransform.position);
        }

        private void StringAdjustment(float stringAdjust)
        {
            _stringGiven += stringAdjust * Time.deltaTime;
            _stringRoll.transform.Rotate(Vector3.right, stringAdjust * 5000 * Time.deltaTime);
        }

        private void ControllerRodAdjustment()
        {
            if (_stringGiven < 0.1f)
            {
                _stringGiven = 0.1f;
            }

            var thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);

            if (thumbstick.magnitude > DEADZONE)
            {
                var currentAngle = Mathf.Atan2(thumbstick.y, thumbstick.x) * Mathf.Rad2Deg;

                // If not the first frame after exiting deadzone, calculate delta
                if (!_wasInDeadzone)
                {
                    var deltaAngle = currentAngle - _previousAngle;

                    // Normalize the delta to handle wraparound (e.g., 350° to 10°)
                    if (deltaAngle > 180f)
                    {
                        deltaAngle -= 360f;
                    }

                    if (deltaAngle < -180f)
                    {
                        deltaAngle += 360f;
                    }

                    _stringGiven += deltaAngle * Time.deltaTime * -.1f;
                    _stringRoll.transform.localRotation = Quaternion.Euler(Vector3.right * -currentAngle);
                }

                _previousAngle = currentAngle;
                _wasInDeadzone = false;
            }
            else
            {
                _wasInDeadzone = true;
            }
        }
    }
}
