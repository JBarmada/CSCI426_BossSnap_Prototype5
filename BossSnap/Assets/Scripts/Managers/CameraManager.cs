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
        [SerializeField] private bool useSmooth = false;
        [SerializeField] private float transitionSpeed = 5f;

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
        }

        private void Start()
        {
            SnapToRealm(RealmState.ThreeD);
        }

        public void SnapToRealm(RealmState realm)
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

            if (useSmooth)
            {
                isTransitioning = true;
            }
            else
            {
                mainCamera.transform.position = targetPosition;
                mainCamera.transform.rotation = targetRotation;
            }
        }

        private void LateUpdate()
        {
            if (isTransitioning && mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * transitionSpeed);
                mainCamera.transform.rotation = Quaternion.Slerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * transitionSpeed);

                if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.01f)
                {
                    mainCamera.transform.position = targetPosition;
                    mainCamera.transform.rotation = targetRotation;
                    isTransitioning = false;
                }
            }
        }
    }
}
