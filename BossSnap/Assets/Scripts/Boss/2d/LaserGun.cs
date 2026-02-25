using UnityEngine;
using System;

namespace BossSnap.Boss
{
    public class LaserGun : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float chargeTime = 2f;
        [SerializeField] private float attackDuration = 1f;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem chargeEffect;
        [SerializeField] private GameObject exclamationIndicator;
        [SerializeField] private LaserBeam laserBeam;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip chargeSound;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip dingSound;
        
        private float timer;
        private LaserGunState currentState = LaserGunState.Idle;
        
        public event Action OnFiringComplete;
        
        private enum LaserGunState
        {
            Idle,
            Charging,
            Firing,
            Complete
        }
        
        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            if (exclamationIndicator != null)
                exclamationIndicator.SetActive(false);
                
            if (laserBeam != null)
                laserBeam.gameObject.SetActive(false);
        }
        
        public void Initialize(float hpScaling)
        {
            chargeTime = Mathf.Lerp(2f, 1f, 1f - hpScaling);
            timer = 0f;
            currentState = LaserGunState.Charging;
            
            if (chargeEffect != null)
                chargeEffect.Play();
                
            if (chargeSound != null && audioSource != null)
                audioSource.PlayOneShot(chargeSound);
        }
        
        private void Update()
        {
            if (currentState == LaserGunState.Idle || currentState == LaserGunState.Complete)
                return;
                
            timer += Time.deltaTime;
            
            switch (currentState)
            {
                case LaserGunState.Charging:
                    UpdateCharging();
                    break;
                    
                case LaserGunState.Firing:
                    UpdateFiring();
                    break;
            }
        }
        
        private void UpdateCharging()
        {
            if (timer >= chargeTime - 0.5f && exclamationIndicator != null && !exclamationIndicator.activeSelf)
            {
                exclamationIndicator.SetActive(true);
                
                if (dingSound != null && audioSource != null)
                    audioSource.PlayOneShot(dingSound);
            }
            
            if (timer >= chargeTime)
            {
                StartFiring();
            }
        }
        
        private void StartFiring()
        {
            currentState = LaserGunState.Firing;
            timer = 0f;
            
            if (chargeEffect != null)
                chargeEffect.Stop();
                
            if (exclamationIndicator != null)
                exclamationIndicator.SetActive(false);
                
            if (laserBeam != null)
            {
                laserBeam.gameObject.SetActive(true);
                laserBeam.Fire();
            }
            
            if (fireSound != null && audioSource != null)
                audioSource.PlayOneShot(fireSound);
        }
        
        private void UpdateFiring()
        {
            if (timer >= attackDuration)
            {
                CompleteFiring();
            }
        }
        
        private void CompleteFiring()
        {
            currentState = LaserGunState.Complete;
            
            if (laserBeam != null)
            {
                laserBeam.gameObject.SetActive(false);
            }
            
            OnFiringComplete?.Invoke();
            
            Destroy(gameObject, 0.5f);
        }
    }
}
