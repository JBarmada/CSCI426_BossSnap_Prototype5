using UnityEngine;

namespace BossSnap.Boss
{
    public class RollingStoneBall : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float acceleration = 0f;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float destroyZ = -20f;

        [Header("Damage")]
        [SerializeField] private float damageAmount = 1f;

        [Header("Shake")]
        [SerializeField] private float impactShakeStrength = 0.2f;
        [SerializeField] private float impactShakeDuration = 0.15f;

        [Header("Audio")]
        [SerializeField] private AudioSource rollingAudio;
        [SerializeField] private AudioClip breakClip;
        [SerializeField] private AudioClip hitClip;

        private ScreenShake screenShake;

        private void Awake()
        {
            // Automatically find main camera shake
            screenShake = Camera.main.GetComponent<ScreenShake>();
        }

        private void Start()
        {
            if (rollingAudio != null)
                rollingAudio.Play();
        }

        private void Update()
        {
            moveSpeed += acceleration * Time.deltaTime;

            // Move forward
            transform.position += Vector3.back * moveSpeed * Time.deltaTime;

            // Rotate visually
            transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

            if (transform.position.z <= destroyZ)
            {
                Break(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Player.PlayerController player = other.GetComponent<Player.PlayerController>();
                if (player != null && !player.IsInvulnerable)
                {
                    player.TakeDamage(damageAmount);
                    Debug.Log($"Stone hit player for {damageAmount} damage!");
                }

                if (hitClip != null)
                    AudioSource.PlayClipAtPoint(hitClip, transform.position);

                Break(true);
            }
        }

        private void Break(bool hitPlayer)
        {
            if (breakClip != null)
                AudioSource.PlayClipAtPoint(breakClip, transform.position);

            if (screenShake != null)
                screenShake.Shake(impactShakeStrength, impactShakeDuration);

            Destroy(gameObject);
        }

        public void SetMovementStats(float speed, float accel)
        {
            moveSpeed = speed;
            acceleration = accel;
        }
    }
}