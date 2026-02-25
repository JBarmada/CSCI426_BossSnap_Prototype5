using UnityEngine;

namespace BossSnap.Boss
{
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        [Header("Beam Settings")]
        [SerializeField] private float maxDistance = 50f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private LayerMask hitLayers = -1;
        
        [Header("Speed Settings")]
        [SerializeField] private float beamExtensionSpeed = 50f;
        [SerializeField] private float initialDelay = 0f;
        [SerializeField] private bool instantBeam = false;
        
        [Header("Audio")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioSource audioSource;
        
        private LineRenderer lineRenderer;
        private float currentLength;
        private bool hasHitPlayer;
        private float delayTimer;
        
        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
                
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.zero);
                lineRenderer.enabled = false;
            }
        }
        
        public void Fire()
        {
            currentLength = instantBeam ? maxDistance : 0f;
            hasHitPlayer = false;
            delayTimer = initialDelay;
            
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
            }
        }
        
        private void Update()
        {
            if (lineRenderer == null || !lineRenderer.enabled)
                return;
            
            if (delayTimer > 0f)
            {
                delayTimer -= Time.deltaTime;
                return;
            }
            
            if (!instantBeam)
            {
                currentLength = Mathf.Min(currentLength + beamExtensionSpeed * Time.deltaTime, maxDistance);
            }
            
            Vector3 startPos = transform.position;
            Vector3 direction = transform.forward;
            
            RaycastHit hit;
            Vector3 endPos;
            
            if (Physics.Raycast(startPos, direction, out hit, currentLength, hitLayers))
            {
                endPos = hit.point;
                
                if (!hasHitPlayer)
                {
                    Player.PlayerController player = hit.collider.GetComponent<Player.PlayerController>();
                    if (player != null && !player.IsDead && !player.IsInvulnerable)
                    {
                        player.TakeDamage(damage);
                        hasHitPlayer = true;
                        
                        if (hitSound != null && audioSource != null)
                            audioSource.PlayOneShot(hitSound);
                    }
                }
            }
            else
            {
                endPos = startPos + direction * currentLength;
            }
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
        
        private void OnDisable()
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }
}
