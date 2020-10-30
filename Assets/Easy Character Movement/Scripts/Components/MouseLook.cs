﻿using ECM.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ECM.Components
{
    /// <summary>
    /// MouseLook.
    /// 
    /// Component used to 'look around' with the mouse.
    /// This rotate the character along its y-axis (yaw) and a child camera along its local x-axis (pitch).
    /// 
    /// This must be attached to the game object with 'CharacterMovement' component.
    /// </summary>

    public class MouseLook : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Tooltip("Should the mouse cursor be locked (eg: hidden)?")]
        [SerializeField]
        private bool _lockCursor = true;

        [Tooltip("Accelerate Mouse Speed")]
        [SerializeField]
        private float _smoothSpeed = 1;

        [Tooltip("How fast the cursor moves in response to mouse lateral (x-axis) movement.")]
        [SerializeField]
        private float _lateralSensitivity = 2.0f;

        [Tooltip("How fast the cursor moves in response to mouse vertical (y-axis) movement.")]
        [SerializeField]
        private float _verticalSensitivity = 2.0f;

        [Tooltip("Should the rotation be smoothed (eg: interpolated)?")]
        [SerializeField]
        private bool _smooth;

        [Tooltip("Approximately the time (in seconds) it will take to reach the target.\n" +
                 "A smaller value will reach the target faster.")]
        [SerializeField]
        public float _smoothTime = 5.0f;

        [Tooltip("Should the rotation around x-axis be clamped?")]
        [SerializeField]
        private bool _clampPitch = true;

        [Tooltip("The minimum pitch angle (in degrees).")]
        [SerializeField]
        private float _minPitchAngle = -90.0f;

        [Tooltip("The maximum pitch angle (in degrees).")]
        [SerializeField]
        private float _maxPitchAngle = 90.0f;

        #endregion

        #region FIELDS

        protected bool _isCursorLocked = true;

        protected Quaternion characterTargetRotation;
        protected Quaternion cameraTargetRotation;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Should the mouse cursor be locked (eg: hidden)?
        /// </summary>

        public bool lockCursor
        {
            get { return _lockCursor; }
            set { _lockCursor = value; }
        }

        /// <summary>
        /// How fast the cursor moves in response to mouse lateral (x-axis) movement.
        /// </summary>

        public float lateralSensitivity
        {
            get { return _lateralSensitivity; }
            set { _lateralSensitivity = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// How fast the cursor moves in response to mouse vertical (y-axis) movement.
        /// </summary>

        public float verticalSensitivity
        {
            get { return _verticalSensitivity; }
            set { _verticalSensitivity = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Should the rotation be smoothed (eg: interpolated)?
        /// </summary>

        public bool smooth
        {
            get { return _smooth; }
            set { _smooth = value; }
        }

        /// <summary>
        /// Approximately the time (in seconds) it will take to reach the target.
        /// A smaller value will reach the target faster.
        /// </summary>

        public float smoothTime
        {
            get { return _smoothTime; }
            set { _smoothTime = Mathf.Max(0.0f, value); }
        }

        /// <summary>
        /// Should the rotation around x-axis be clamped?
        /// </summary>

        public bool clampPitch
        {
            get { return _clampPitch; }
            set { _clampPitch = value; }
        }

        /// <summary>
        /// The minimum pitch angle (in degrees).
        /// </summary>

        public float minPitchAngle
        {
            get { return _minPitchAngle; }
            set { _minPitchAngle = Mathf.Clamp(value, -180.0f, 180.0f); }
        }

        /// <summary>
        /// The maximum pitch angle (in degrees).
        /// </summary>

        public float maxPitchAngle
        {
            get { return _maxPitchAngle; }
            set { _maxPitchAngle = Mathf.Clamp(value, -180.0f, 180.0f); }
        }

        public float _horizontalVelocity = 0;
        public float horizontalVelocity
        {
            get { return _horizontalVelocity; }
            // set { _horizontalVelocity = Mathf.Clamp(value, -1, 1); }
            set { _horizontalVelocity = value; }
        }

        public float _verticalVelocity = 0;
        public float verticalVelocity
        {
            get { return _verticalVelocity; }
            // set { _verticalVelocity = Mathf.Clamp(value, -1, 1); }
            set { _verticalVelocity = value; }
        }

        public float _sensivityVerticalTime = 0;
        public float sensivityVerticalTime
        {
            get { return _sensivityVerticalTime; }
            set { _sensivityVerticalTime = Mathf.Clamp(value, 0, 1); }
        }

        public float _sensivityHorizontalTime = 0;
        public float sensivityHorizontalTime
        {
            get { return _sensivityHorizontalTime; }
            set { _sensivityHorizontalTime = Mathf.Clamp(value, 0, 1); }
        }

        #endregion

        #region METHODS

        public virtual void Init(Transform characterTransform, Transform cameraTransform)
        {
            characterTargetRotation = characterTransform.localRotation;
            cameraTargetRotation = cameraTransform.localRotation;
        }
        
        public void HorizontalVelocity(InputAction.CallbackContext context)
        {
            horizontalVelocity = context.ReadValue<float>();
        }
        
        public void VerticalVelocity(InputAction.CallbackContext context)
        {
            verticalVelocity = context.ReadValue<float>();
        }

        /// <summary>
        /// Perform 'Look' rotation.
        /// This rotate the character along its y-axis (yaw) and a child camera along its local x-axis (pitch).
        /// </summary>
        /// <param name="movement">The character movement component.</param>
        /// <param name="cameraTransform">The camera transform.</param>

        public virtual void LookRotation(CharacterMovement movement, Transform cameraTransform)
        {

            var yaw = horizontalVelocity * lateralSensitivity;
            var pitch = verticalVelocity * verticalSensitivity;

            var yawRotation = Quaternion.Euler(0.0f, yaw, 0.0f);
            var pitchRotation = Quaternion.Euler(-pitch, 0.0f, 0.0f);

            characterTargetRotation *= yawRotation;
            cameraTargetRotation *= pitchRotation;

            if (clampPitch)
                cameraTargetRotation = ClampPitch(cameraTargetRotation);

            if (smooth)
            {
                // On a rotating platform, append platform rotation to target rotation

                if (movement.platformUpdatesRotation && movement.isOnPlatform && movement.platformAngularVelocity != Vector3.zero)
                {
                    characterTargetRotation *=
                        Quaternion.Euler(movement.platformAngularVelocity * Mathf.Rad2Deg * Time.deltaTime);
                }

                movement.rotation = Quaternion.Slerp(movement.rotation, characterTargetRotation,
                    smoothTime * Time.deltaTime);

                cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, cameraTargetRotation,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                movement.rotation *= yawRotation;
                cameraTransform.localRotation *= pitchRotation;

                if (clampPitch)
                    cameraTransform.localRotation = ClampPitch(cameraTransform.localRotation);
            }

            UpdateCursorLock();
        }

        public virtual void SetCursorLock(bool value)
        {
            lockCursor = value;
            if (lockCursor)
                return;
            
            // We force unlock the cursor if the user disable the cursor locking helper

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public virtual void UpdateCursorLock()
        {
            // If the user set "lockCursor" we check & properly lock the cursos

            if (lockCursor)
                InternalLockUpdate();
        }

        public void UnlockCursor()
        {
            _isCursorLocked = !_isCursorLocked;
        }

        protected virtual void InternalLockUpdate()
        {
            if (_isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!_isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        protected virtual void Awake()
        {
            BaseInputManager.Develop.UnlockCursorKey.performed += context => UnlockCursor();

            BaseInputManager.PlayerMovement.ViewHorizontal.performed += context => HorizontalVelocity(context);
            BaseInputManager.PlayerMovement.ViewHorizontal.canceled += context => HorizontalVelocity(context);
            BaseInputManager.PlayerMovement.ViewVertical.performed += context => VerticalVelocity(context);
            BaseInputManager.PlayerMovement.ViewVertical.canceled += context => VerticalVelocity(context);
        }

        protected Quaternion ClampPitch(Quaternion q)
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            var pitch = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            pitch = Mathf.Clamp(pitch, minPitchAngle, maxPitchAngle);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * pitch);

            return q;
        }

        #endregion

        #region MONOBEHAVIOUR

        /// <summary>
        /// Validate editor exposed fields.
        /// 
        /// NOTE: If you override this, it is important to call the parent class' version of method
        /// eg: base.OnValidate, in the derived class method implementation, in order to fully validate the class.  
        /// </summary>

        public virtual void OnValidate()
        {
            lateralSensitivity = _lateralSensitivity;
            verticalSensitivity = _verticalSensitivity;

            smoothTime = _smoothTime;

            minPitchAngle = _minPitchAngle;
            maxPitchAngle = _maxPitchAngle;
        }
        
        #endregion
    }
}
