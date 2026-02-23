using UnityEngine;
using UnityEngine.InputSystem;
using BossSnap.Managers;

namespace BossSnap.Player
{
    public enum RealmState { ThreeD, TwoD }

    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Realm Configuration")]
        [SerializeField] private RealmState currentRealm = RealmState.ThreeD;

        [Header("3D Lane Movement")]
        [SerializeField] private int currentLaneIndex = 1;
        [SerializeField] private float laneTransitionSpeed = 10f;
        private bool isTransitioning = false;
        private float targetXPosition;

        [Header("2D Jump Movement")]
        [SerializeField] private float jumpForce = 500f;
        [SerializeField] private float jumpDownForce = 300f;
        [SerializeField] private float maxHeight = 8f;
        [SerializeField] private float gravityScale = 2f;

        private Rigidbody rb;
        private PlayerAnimationController animController;
        private InputAction moveLeftAction;
        private InputAction moveRightAction;
        private InputAction jumpAction;
        private InputAction jumpDownAction;
        private InputAction snapRealmAction;
        private float[] lanePositions;

        public RealmState CurrentRealm => currentRealm;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animController = GetComponent<PlayerAnimationController>();

            moveLeftAction = new InputAction(binding: "<Keyboard>/a");
            moveLeftAction.AddBinding("<Keyboard>/leftArrow");
            
            moveRightAction = new InputAction(binding: "<Keyboard>/d");
            moveRightAction.AddBinding("<Keyboard>/rightArrow");
            
            jumpAction = new InputAction(binding: "<Keyboard>/w");
            jumpAction.AddBinding("<Keyboard>/upArrow");
            
            jumpDownAction = new InputAction(binding: "<Keyboard>/s");
            jumpDownAction.AddBinding("<Keyboard>/downArrow");
            
            snapRealmAction = new InputAction(binding: "<Keyboard>/space");

            if (ArenaManager.Instance != null)
            {
                lanePositions = ArenaManager.Instance.LanePositions;
                if (lanePositions != null && lanePositions.Length > 0)
                {
                    targetXPosition = lanePositions[currentLaneIndex];
                }
            }
        }

        private void OnEnable()
        {
            moveLeftAction.performed += OnMoveLeft;
            moveRightAction.performed += OnMoveRight;
            jumpAction.performed += OnJump;
            jumpDownAction.performed += OnJumpDown;
            snapRealmAction.performed += OnSnapRealm;

            moveLeftAction.Enable();
            moveRightAction.Enable();
            jumpAction.Enable();
            jumpDownAction.Enable();
            snapRealmAction.Enable();
        }

        private void OnDisable()
        {
            moveLeftAction.performed -= OnMoveLeft;
            moveRightAction.performed -= OnMoveRight;
            jumpAction.performed -= OnJump;
            jumpDownAction.performed -= OnJumpDown;
            snapRealmAction.performed -= OnSnapRealm;

            moveLeftAction.Disable();
            moveRightAction.Disable();
            jumpAction.Disable();
            jumpDownAction.Disable();
            snapRealmAction.Disable();
        }

        private void Start()
        {
            ConfigureRigidbody();
            
            if (ArenaManager.Instance != null)
            {
                lanePositions = ArenaManager.Instance.LanePositions;
                if (lanePositions != null && lanePositions.Length > 0)
                {
                    targetXPosition = lanePositions[currentLaneIndex];
                }
            }
            else
            {
                Debug.LogError("ArenaManager.Instance is null! Cannot get lane positions.");
            }
        }

        private void ConfigureRigidbody()
        {
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;
        }

        private void FixedUpdate()
        {
            if (currentRealm == RealmState.ThreeD)
            {
                Update3DMovement();
            }
            else if (currentRealm == RealmState.TwoD)
            {
                Update2DMovement();
            }

            ApplyGravityScale();
        }

        private void Update3DMovement()
        {
            if (isTransitioning)
            {
                float newX = Mathf.MoveTowards(transform.position.x, targetXPosition, laneTransitionSpeed * Time.fixedDeltaTime);
                rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));

                if (Mathf.Abs(transform.position.x - targetXPosition) < 0.01f)
                {
                    isTransitioning = false;
                }
            }
        }

        private void Update2DMovement()
        {
            if (transform.position.y > maxHeight)
            {
                Vector3 clampedPosition = transform.position;
                clampedPosition.y = maxHeight;
                rb.position = clampedPosition;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(0f, rb.linearVelocity.y), rb.linearVelocity.z);
            }

            if (transform.position.y < 0f)
            {
                Vector3 clampedPosition = transform.position;
                clampedPosition.y = 0f;
                rb.position = clampedPosition;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Max(0f, rb.linearVelocity.y), rb.linearVelocity.z);
            }
        }

        private void ApplyGravityScale()
        {
            rb.AddForce(Physics.gravity * (gravityScale - 1f) * rb.mass);
        }

        private void OnMoveLeft(InputAction.CallbackContext context)
        {
            if (currentRealm == RealmState.ThreeD && !isTransitioning && lanePositions != null)
            {
                if (currentLaneIndex > 0)
                {
                    currentLaneIndex--;
                    targetXPosition = lanePositions[currentLaneIndex];
                    isTransitioning = true;
                    Debug.Log($"Moving left to lane {currentLaneIndex} (X: {targetXPosition})");
                }
                else
                {
                    Debug.Log("Already at leftmost lane");
                }
            }
        }

        private void OnMoveRight(InputAction.CallbackContext context)
        {
            if (currentRealm == RealmState.ThreeD && !isTransitioning && lanePositions != null)
            {
                if (currentLaneIndex < lanePositions.Length - 1)
                {
                    currentLaneIndex++;
                    targetXPosition = lanePositions[currentLaneIndex];
                    isTransitioning = true;
                    Debug.Log($"Moving right to lane {currentLaneIndex} (X: {targetXPosition})");
                }
                else
                {
                    Debug.Log("Already at rightmost lane");
                }
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (currentRealm == RealmState.TwoD)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                if (animController != null)
                {
                    animController.TriggerJump();
                }
            }
        }

        private void OnJumpDown(InputAction.CallbackContext context)
        {
            if (currentRealm == RealmState.TwoD)
            {
                rb.AddForce(Vector3.down * jumpDownForce, ForceMode.Impulse);
                if (animController != null)
                {
                    animController.TriggerJump();
                }
            }
        }

        private void OnSnapRealm(InputAction.CallbackContext context)
        {
            SnapRealm();
        }

        public void SnapRealm()
        {
            currentRealm = currentRealm == RealmState.ThreeD ? RealmState.TwoD : RealmState.ThreeD;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (currentRealm == RealmState.ThreeD)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SnapToRealm(currentRealm);
            }
        }
    }
}
