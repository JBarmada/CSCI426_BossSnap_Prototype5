using UnityEngine;

namespace BossSnap.Boss
{
    public class FallingRock : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float fallSpeed = 15f;
        [SerializeField] private float rotationSpeed = 500f;
        [SerializeField] private float destroyY = -5f;

        [Header("Damage")]
        [SerializeField] private float damageAmount = 20f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFX;

        [Header("Audio")]
        [SerializeField] private AudioClip impactClip;
        [SerializeField] private AudioClip whistleClip;

        [Header("Screen Shake")]
        [SerializeField] private float impactShakeStrength = 0.2f;
        [SerializeField] private float impactShakeDuration = 0.15f;

        private ScreenShake screenShake;
        private bool hasHit = false;

        private void Start()
        {
            screenShake = Camera.main?.GetComponent<ScreenShake>();

            if (whistleClip != null)
            {
                AudioSource.PlayClipAtPoint(whistleClip, transform.position, 0.5f);
            }
        }

        public void Initialize(float speed)
        {
            fallSpeed = speed;
        }

        private void Update()
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            if (transform.position.y < destroyY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit) return;

            if (other.CompareTag("Player"))
            {
                Player.PlayerController player = other.GetComponent<Player.PlayerController>();
                if (player != null && !player.IsInvulnerable)
                {
                    player.TakeDamage(damageAmount);
                    Debug.Log($"Falling stone hit player for {damageAmount} damage!");
                }

                if (impactClip != null)
                    AudioSource.PlayClipAtPoint(impactClip, transform.position);

                if (screenShake != null)
                    screenShake.Shake(impactShakeStrength, impactShakeDuration);

                Break();
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Default") || 
                     other.CompareTag("Ground"))
            {
                Break();
            }
        }

        private void Break()
        {
            if (hasHit) return;
            hasHit = true;

            if (impactVFX != null)
            {
                Instantiate(impactVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
