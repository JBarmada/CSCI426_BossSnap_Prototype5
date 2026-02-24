using UnityEngine;
using UnityEngine.UI;

namespace BossSnap.Player
{
    public class PlayerHitFeedback : MonoBehaviour
    {
        [Header("Screen Shake")]
        [SerializeField] private float shakeStrength = 0.3f;
        [SerializeField] private float shakeDuration = 0.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private float hitSoundVolume = 1f;
        [SerializeField] private float impactSoundVolume = 0.8f;

        [Header("Visual Feedback")]
        [SerializeField] private Image damageFlashOverlay;
        [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
        [SerializeField] private float flashDuration = 0.15f;

        private AudioSource audioSource;
        private ScreenShake screenShake;
        private float flashTimer;
        private Color transparentColor;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }

            screenShake = Camera.main?.GetComponent<ScreenShake>();
            if (screenShake == null)
            {
                screenShake = Camera.main?.gameObject.AddComponent<ScreenShake>();
            }

            transparentColor = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            if (damageFlashOverlay != null)
            {
                damageFlashOverlay.color = transparentColor;
            }
        }

        private void Update()
        {
            if (flashTimer > 0f)
            {
                flashTimer -= Time.deltaTime;
                
                if (damageFlashOverlay != null)
                {
                    float alpha = Mathf.Lerp(0f, flashColor.a, flashTimer / flashDuration);
                    damageFlashOverlay.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
                }

                if (flashTimer <= 0f && damageFlashOverlay != null)
                {
                    damageFlashOverlay.color = transparentColor;
                }
            }
        }

        public void PlayHitFeedback()
        {
            PlayScreenShake();
            PlayHitSound();
            PlayImpactSound();
            PlayVisualFlash();
        }

        private void PlayScreenShake()
        {
            if (screenShake != null)
            {
                screenShake.Shake(shakeStrength, shakeDuration);
            }
        }

        private void PlayHitSound()
        {
            if (hitSounds != null && hitSounds.Length > 0 && audioSource != null)
            {
                AudioClip randomHit = hitSounds[Random.Range(0, hitSounds.Length)];
                if (randomHit != null)
                {
                    audioSource.PlayOneShot(randomHit, hitSoundVolume);
                }
            }
        }

        private void PlayImpactSound()
        {
            if (impactSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(impactSound, impactSoundVolume);
            }
        }

        private void PlayVisualFlash()
        {
            flashTimer = flashDuration;
        }

        public void SetDamageFlashOverlay(Image overlay)
        {
            damageFlashOverlay = overlay;
            if (damageFlashOverlay != null)
            {
                damageFlashOverlay.color = transparentColor;
            }
        }
    }
}
