using UnityEngine;
using Cinemachine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		[SerializeField] float MoveSpeed = 4.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		[SerializeField] float SprintSpeed = 6.0f;
		[Tooltip("Crouch speed of the character in m/s")]
		[SerializeField] float crouchSpeed = 0.6f;
		[Tooltip("Rotation speed of the character")]
		[SerializeField] float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		[SerializeField] float SpeedChangeRate = 10.0f;
		[Tooltip("height of the camera while in crouch state")]
		[SerializeField] float crouchYScale = 0.5f;

		[Space(10)]
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		[SerializeField] float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		[SerializeField] float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		[SerializeField] bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		[SerializeField] float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		[SerializeField] float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		[SerializeField] LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		[SerializeField] GameObject CinemachineCameraTarget;
		[Tooltip("The the player follow camera set in Cinemachine Brain")]
		[SerializeField] CinemachineVirtualCamera cmVirtualCamera;
		[Tooltip("How far in degrees can you move the camera up")]
		[SerializeField] float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		[SerializeField] float BottomClamp = -90.0f;
		[Tooltip("Additional degress to override the camera.")]
		[SerializeField] float focalLength = 33f;

		// cinemachine
		private float _cinemachineTargetPitch;
		private float startFocalLength;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;
		private float startYScale;
		private float targetSpeed;
		private bool isCrouched = false;

		// timeout deltatime
		private float _fallTimeoutDelta;

	
#if ENABLE_INPUT_SYSTEM
		private PlayerInput _playerInput;
#endif
		private CharacterController _controller;
		private StarterAssetsInputs _input;

		private const float _threshold = 0.01f;

		private bool IsCurrentDeviceMouse
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM
				return _playerInput.currentControlScheme == "KeyboardMouse";
				#else
				return false;
				#endif
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
			_playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
			startYScale = transform.localScale.y;

			// zoom settings for focalLength
			startFocalLength = cmVirtualCamera.m_Lens.FieldOfView;
			Debug.Log(startFocalLength);

			// reset our timeouts on start
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			GroundedCheck();
			JumpAndGravity();
			Move();
			Zoom();
			if (_input.zoom)
			{
				Shoot();
			}
			else
			{
				Action();
			}

			if (_input.crouch && !isCrouched)
			{
				Crouch(true);
			}
			else if (!_input.crouch && isCrouched)
			{
				Crouch(false);
			}
		}

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				//Don't multiply mouse input by Time.deltaTime
				float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
				
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
				_rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			if (_input.sprint && !_input.crouch)
			{
				targetSpeed = SprintSpeed;
			}
			else if (isCrouched)
			{
				targetSpeed = crouchSpeed;
			}
			else
			{
				targetSpeed = MoveSpeed;
			}

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}
		
		private void Zoom()
		{
			cmVirtualCamera.m_Lens.FieldOfView = _input.zoom ? focalLength : startFocalLength;
		}

		private void Shoot()
		{
			if(_input.shoot)
			{
				Debug.Log("Shooting");
			}
		}

		private void Action()
		{
			if (_input.action)
			{
				Debug.Log("Action");
			}
		}

		private void Crouch(bool crouchNow)
		{
			if (crouchNow)
			{
				transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
				isCrouched = true;
				Debug.Log("Crouching");
			}
			else if (!crouchNow)
			{
				transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
				isCrouched = false;
				Debug.Log("Standing");
			}
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
			}
			else
			{
				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}
	}
}