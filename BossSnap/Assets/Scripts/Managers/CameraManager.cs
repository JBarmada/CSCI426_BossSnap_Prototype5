using UnityEngine;
using BossSnap.Player;

namespace BossSnap.Managers
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Camera Positions")]
        [SerializeField] private Vector3 threeDPosition = new Vector3(0f, 10f, -15f);
        [SerializeField] private Vector3 threeDRotation = new Vector3(30f, 0f, 0f);
        
        [SerializeField] private Vector3 twoDPosition = new Vector3(-25f, 5f, 5f);
        [SerializeField] private Vector3 twoDRotation = new Vector3(0f, 90f, 0f);

        [Header("Transition Settings")]
        [SerializeField] private bool useSmooth = true;
        [SerializeField] private float transitionSpeed = 5f;

        private ScreenShake screenShake;
        private Camera mainCamera;
        private bool isTransitioning = false;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }
            if (mainCamera != null)
                screenShake = mainCamera.GetComponent<ScreenShake>();
        }

        private void Start()
        {
            SnapToRealm(RealmState.ThreeD, true);
        }

        public void SnapToRealm(RealmState realm, bool instant = false)
        {
            if (mainCamera == null) return;

            if (realm == RealmState.ThreeD)
            {
                targetPosition = threeDPosition;
                targetRotation = Quaternion.Euler(threeDRotation);
            }
            else
            {
                targetPosition = twoDPosition;
                targetRotation = Quaternion.Euler(twoDRotation);
            }

            if (useSmooth && !instant)
            {
                isTransitioning = true;
            }
            else
            {
                isTransitioning = false;
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.rotation = targetRotation;
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;
            Vector3 basePosition = mainCamera.transform.position;
            Quaternion baseRotation = mainCamera.transform.rotation;
            if (isTransitioning)
            {
                basePosition = Vector3.Lerp(
                    mainCamera.transform.position,
                    targetPosition,
                    Time.deltaTime * transitionSpeed
                );

                baseRotation = Quaternion.Slerp(
                    mainCamera.transform.rotation,
                    targetRotation,
                    Time.deltaTime * transitionSpeed
                );

                if (Vector3.Distance(basePosition, targetPosition) < 0.01f)
                {
                    basePosition = targetPosition;
                    baseRotation = targetRotation;
                    isTransitioning = false;
                }
            }
            else
            {
                basePosition = targetPosition;
                baseRotation = targetRotation;
            }

            // 🔥 APPLY SHAKE OFFSET HERE
            Vector3 shakeOffset = Vector3.zero;
            if (screenShake != null)
                shakeOffset = screenShake.CurrentOffset;

            mainCamera.transform.position = basePosition + shakeOffset;
            mainCamera.transform.rotation = baseRotation;
        }
    }
}
