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
        [SerializeField] private float laneTurnAngle = 90f;
        [SerializeField] private float laneTurnSpeed = 360f;
        private bool isTransitioning = false;
        private float targetXPosition;
        private float targetYaw = 0f;

        [Header("2D Jump Movement")]
        [SerializeField] private float jumpForce = 500f;
        [SerializeField] private float jumpDownForce = 300f;
        [SerializeField] private float maxHeight = 8f;
        [SerializeField] private float gravityScale = 2f;
        [Header("Ground Detection")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer = -1;
        private bool isGrounded = false;


        [Header("Audio")]
        [SerializeField] private AudioClip snapSfx;
        [SerializeField] [Range(0f, 1f)] private float snapSfxVolume = 1f;
        [SerializeField] private AudioClip laneSwitchSfx;
        [SerializeField] [Range(0f, 1f)] private float laneSwitchSfxVolume = 1f;

        private Rigidbody rb;
        private AudioSource audioSource;
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
            audioSource = GetComponent<AudioSource>();
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
                targetXPosition = lanePositions[currentLaneIndex];
            }
        }

        private void ConfigureRigidbody()
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionZ;
        }

        private void FixedUpdate()
        {
            if (currentRealm == RealmState.ThreeD)
                Update3DMovement();
            else
                Update2DMovement();

            ApplyGravityScale();
        }

        private void Update3DMovement()
        {
            Quaternion desiredRotation = Quaternion.Euler(0f, targetYaw, 0f);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, desiredRotation, laneTurnSpeed * Time.fixedDeltaTime));

            if (isTransitioning)
            {
                float newX = Mathf.MoveTowards(transform.position.x, targetXPosition, laneTransitionSpeed * Time.fixedDeltaTime);
                rb.MovePosition(new Vector3(newX, transform.position.y, transform.position.z));

                if (Mathf.Abs(transform.position.x - targetXPosition) < 0.01f)
                {
                    isTransitioning = false;
                    targetYaw = 0f;
                }
            }
        }

        private void Update2DMovement()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

            if (transform.position.y > maxHeight)
            {
                rb.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 0f), rb.linearVelocity.z);
            }

            if (transform.position.y <= 0.1f)
            {
                rb.position = new Vector3(transform.position.x, 0f, transform.position.z);
                if (rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                }
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
                    targetYaw = -laneTurnAngle;
                    if (laneSwitchSfx != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(laneSwitchSfx, laneSwitchSfxVolume);
                    }
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
                    targetYaw = laneTurnAngle;

                    if (laneSwitchSfx != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(laneSwitchSfx, laneSwitchSfxVolume);
                    }
                }
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {   
            if (currentRealm != RealmState.TwoD) return;
            
            if (isGrounded && transform.position.y < maxHeight - 1f)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                animController?.TriggerJump();
            }
        }


        private void OnJumpDown(InputAction.CallbackContext context)
        {
            if (currentRealm == RealmState.TwoD)
            {
                rb.AddForce(Vector3.down * jumpDownForce, ForceMode.Impulse);
                animController?.TriggerJump();
            }
        }
        private void OnSnapRealm(InputAction.CallbackContext context)
        {
            SnapRealm();
        }
        public void SnapRealm()
        {

            Debug.Log("Current Realm: " + currentRealm);
            // Stop lane switching
            isTransitioning = false;

            targetYaw = 0f;

            // Toggle realm
            currentRealm = currentRealm == RealmState.ThreeD
                ? RealmState.TwoD
                : RealmState.ThreeD;

            // Reset velocity
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Adjust Rigidbody constraints per realm
            if (currentRealm == RealmState.ThreeD)
            {
                rb.constraints =
                    RigidbodyConstraints.FreezeRotationX |
                    RigidbodyConstraints.FreezeRotationZ |
                    RigidbodyConstraints.FreezePositionZ;
            }
            else
            {
                rb.constraints =
                    RigidbodyConstraints.FreezeRotationX |
                    RigidbodyConstraints.FreezeRotationZ |
                    RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionZ;
            }

            // Play snap sound
            if (snapSfx != null)
            {
                if (audioSource != null)
                    audioSource.PlayOneShot(snapSfx, snapSfxVolume);
            }

            // Notify camera
            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.SnapToRealm(currentRealm);
            }
}
    }
}