using UnityEngine;
using System.Collections.Generic;
using BossSnap.Managers;

namespace BossSnap.Boss
{
    public class TortoiseBossController : MonoBehaviour
    {
        [Header("Boss Config & Timer")]
        [SerializeField] private float maxHealth = 60f;
        private float health;
        
        [Header("Timer System")]
        [SerializeField] private bool useHealthAsTimer = true;
        [SerializeField] private bool timerActive = false;

        [Header("Player Detection")]
        [SerializeField] private Player.PlayerController player;

        [Header("Attack 1 - Rolling Stones (3D Realm)")]
        [SerializeField] private GameObject rollingStonePrefab;
        [SerializeField] private Transform[] laneSpawnPoints;

        [Header("Attack 2 - Mouth Rolling Stones (3D Realm)")]
        [SerializeField] private Transform mouthSpawnPoint;
        [SerializeField] private int attack2StonesPerLane = 1;
        [SerializeField] private float attack2StoneSpacing = 0.3f;
        [SerializeField] private float attack2StoneSpeed = 10f;
        [SerializeField] private float attack2SpawnDelay = 1.5f;
        
        [Header("Laser Attack System")]
        [SerializeField] private BossLaserAttackManager laserAttackManager;
        
        [SerializeField] private Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip stompClip;
        [SerializeField] private AudioClip mouthSnapClip;

        [Header("Screen Shake")]
        [SerializeField] private ScreenShake screenShake;
        [SerializeField] private float spawnShakeStrength = 0.5f;
        [SerializeField] private float spawnShakeDuration = 0.2f;

        [Header("Stone Spawn Control")]
        [SerializeField] [Range(1, 5)] private int maxSpawnEventsPerAttack = 2; // 🔥 CONTROL HOW MANY STOMPS ACTUALLY SPAWN
        [SerializeField] private int stonesPerSpawn = 2; // how many lanes per stomp
        [SerializeField] private float forwardSpawnOffset = 2f;
        [SerializeField] private float randomXOffset = 0.5f;
        [SerializeField] private float randomZOffset = 0.5f;
        [SerializeField] private float spawnHeightOffset = 0.5f;

        [SerializeField] private float baseStoneSpeed = 15f;
        [SerializeField] private float baseAcceleration = 0f;

        [Header("Attack Timing")]
        [SerializeField] private float attackInterval = 5f;
        [SerializeField] private float attackRandomVariance = 1.5f;
        [SerializeField] private bool autoAttack = true;
        
        [Header("Debug")]
        [SerializeField] private bool debugForceAttack2Only = false;

        [Header("Difficulty Scaling")]
        [SerializeField] private bool enableDifficultyScaling = true;
        [SerializeField] private float minAttackInterval = 2f;
        [SerializeField] private int maxStonesPerSpawn = 3;
        
        [Header("3D Attack Pattern")]
        [SerializeField] private float patternSwitchTime = 30f;

        private float attackTimer;
        private float currentAttackCooldown;
        private float elapsedTime;
        private int attacksInCurrentCycle;
        private bool useFirstPattern = true;

        private bool isAttacking;
        private int spawnEventsTriggered;
        private bool useAttack2Animation = false;
        private int targetLaneForAttack = -1;

        private static readonly int Attack1Trigger = Animator.StringToHash("Attack1");
        private static readonly int Attack2Trigger = Animator.StringToHash("Attack2");

        public float CurrentHealth => health;
        public float MaxHealth => maxHealth;

        private void Awake()
        {
            if (screenShake == null && Camera.main != null)
                screenShake = Camera.main.GetComponent<ScreenShake>();

            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player.PlayerController>();

            if (laserAttackManager == null)
                laserAttackManager = GetComponent<BossLaserAttackManager>();

            // Check if Attack2 parameter exists
            if (animator != null)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.nameHash == Attack2Trigger)
                    {
                        useAttack2Animation = true;
                        Debug.Log("Attack2 animation found");
                        break;
                    }
                }
                
                if (!useAttack2Animation)
                {
                    Debug.LogWarning("Attack2 animation parameter not found. Will use Attack1 animation for both attacks.");
                }
            }
        }

        private void Start()
        {
            health = maxHealth;
            ResetAttackCooldown();
            
            if (useHealthAsTimer)
            {
                timerActive = true;
                Debug.Log($"Boss timer started! Duration: {maxHealth} seconds");
            }

            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateBossHealth(health, maxHealth);
            }
        }

        private void Update()
        {
            if (useHealthAsTimer && timerActive)
            {
                health -= Time.deltaTime;
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateBossHealth(health, maxHealth);
                }
                
                if (health <= 0f)
                {
                    health = 0f;
                    OnTimerExpired();
                }
            }
            
            elapsedTime += Time.deltaTime;
            
            if (elapsedTime >= patternSwitchTime && useFirstPattern)
            {
                useFirstPattern = false;
                attacksInCurrentCycle = 0;
                Debug.Log("Boss: Switched to pattern 1,1,2");
            }

            if (!autoAttack) return;

            attackTimer += Time.deltaTime;

            if (attackTimer >= currentAttackCooldown)
            {
                TryAttack();
                ResetAttackCooldown();
            }
        }

        private void TryAttack()
        {
            if (isAttacking) return;

            if (debugForceAttack2Only)
            {
                Debug.Log("🔧 DEBUG: Forcing Attack2 (Mouth)");
                StartAttack2();
                return;
            }

            if (player != null && player.CurrentRealm == Player.RealmState.TwoD)
            {
                if (laserAttackManager != null && CameraManager.Instance != null)
                {
                    laserAttackManager.TriggerLaserAttack(player.CurrentRealm);
                }
                else
                {
                    StartAttack2();
                }
            }
            else
            {
                bool useAttack2 = ShouldUseAttack2For3D();
                
                if (useAttack2)
                {
                    Debug.Log("Boss 3D: Triggering Attack2 (Mouth)");
                    StartAttack2();
                }
                else
                {
                    Debug.Log("Boss 3D: Triggering Attack1 (Stomp)");
                    StartAttack1();
                }
                
                attacksInCurrentCycle++;
                
                if (useFirstPattern)
                {
                    if (attacksInCurrentCycle >= 5)
                        attacksInCurrentCycle = 0;
                }
                else
                {
                    if (attacksInCurrentCycle >= 3)
                        attacksInCurrentCycle = 0;
                }
            }
        }
        
        private bool ShouldUseAttack2For3D()
        {
            if (useFirstPattern)
            {
                return attacksInCurrentCycle == 4;
            }
            else
            {
                return attacksInCurrentCycle == 2;
            }
        }

        private void ResetAttackCooldown()
        {
            attackTimer = 0f;
            
            float currentInterval = attackInterval;
            
            // Scale difficulty based on remaining time
            if (enableDifficultyScaling && useHealthAsTimer)
            {
                float timePercent = health / maxHealth;
                
                // As time decreases, interval decreases (more frequent attacks)
                // 100% time -> full interval (5s)
                // 50% time -> 3.5s
                // 0% time -> minInterval (2s)
                currentInterval = Mathf.Lerp(minAttackInterval, attackInterval, timePercent);
            }
            
            currentAttackCooldown = currentInterval + Random.Range(-attackRandomVariance, attackRandomVariance);
            currentAttackCooldown = Mathf.Max(currentAttackCooldown, 1f); // Never less than 1 second
        }

        private int GetCurrentStonesPerSpawn(bool isAttack2)
        {
            if (!enableDifficultyScaling || !useHealthAsTimer)
            {
                return stonesPerSpawn;
            }

            float timePercent = health / maxHealth;
            
            int stoneCount = Mathf.RoundToInt(Mathf.Lerp(maxStonesPerSpawn, stonesPerSpawn, timePercent));
            return Mathf.Clamp(stoneCount, stonesPerSpawn, maxStonesPerSpawn);
        }

        public void StartAttack1()
        {
            if (isAttacking) return;

            isAttacking = true;
            spawnEventsTriggered = 0;

            if (animator != null)
                animator.SetTrigger(Attack1Trigger);
        }

        public void StartAttack2()
        {
            if (isAttacking) return;
            
            Debug.Log("🦕 ATTACK 2 TRIGGERED - Should see mouth snap!");
            isAttacking = true;
            spawnEventsTriggered = 0;

            if (animator != null)
            {
                if (useAttack2Animation)
                {
                    animator.ResetTrigger(Attack1Trigger);
                    animator.SetTrigger(Attack2Trigger);
                }
                else
                {
                    Debug.LogWarning("Attack2 animation not set up! Using Attack1 animation. Please add Attack2 trigger and animation to Animator!");
                    animator.SetTrigger(Attack1Trigger);
                }
            }
        }
        
        public void PlayStompAudio()
        {
            if (audioSource != null && stompClip != null)
                audioSource.PlayOneShot(stompClip);
        }

        public void PlayMouthSnapAudio()
        {
            if (audioSource != null && mouthSnapClip != null)
                audioSource.PlayOneShot(mouthSnapClip);
        }

        public void SpawnStoneFromStomp()
        {
            if (screenShake != null)
                screenShake.Shake(spawnShakeStrength, spawnShakeDuration);

            if (spawnEventsTriggered >= maxSpawnEventsPerAttack)
                return;

            spawnEventsTriggered++;

            int count = GetCurrentStonesPerSpawn(false);
            SpawnRollingStones(count);
        }

        public void SpawnMouthStones()
        {
            if (screenShake != null)
                screenShake.Shake(spawnShakeStrength, spawnShakeDuration);

            SpawnMouthRollingStones();
        }

        private void SpawnRollingStones(int stonesToSpawn)
        {
            if (laneSpawnPoints == null || laneSpawnPoints.Length == 0)
                return;

            List<int> availableLanes = new List<int>();
            for (int i = 0; i < laneSpawnPoints.Length; i++)
                availableLanes.Add(i);

            stonesToSpawn = Mathf.Clamp(stonesToSpawn, 1, laneSpawnPoints.Length);

            for (int i = 0; i < stonesToSpawn; i++)
            {
                if (availableLanes.Count == 0) break;

                int index = Random.Range(0, availableLanes.Count);
                int lane = availableLanes[index];
                availableLanes.RemoveAt(index);

                SpawnStone(lane);
            }
        }

        private void SpawnStone(int laneIndex)
        {
            Vector3 spawnPos = laneSpawnPoints[laneIndex].position;

            spawnPos += transform.forward * forwardSpawnOffset;
            spawnPos += new Vector3(
                Random.Range(-randomXOffset, randomXOffset),
                spawnHeightOffset,
                Random.Range(-randomZOffset, randomZOffset)
            );

            GameObject stone = Instantiate(
                rollingStonePrefab,
                spawnPos,
                Quaternion.identity
            );

            RollingStoneBall ball = stone.GetComponent<RollingStoneBall>();
            if (ball != null)
                ball.SetMovementStats(baseStoneSpeed, baseAcceleration);
        }

        private void SpawnMouthRollingStones()
        {
            if (laneSpawnPoints == null || laneSpawnPoints.Length == 0)
            {
                Debug.LogWarning("No lane spawn points assigned for mouth attack!");
                return;
            }

            if (mouthSpawnPoint == null)
            {
                Debug.LogWarning("No mouth spawn point assigned! Falling back to boss position.");
            }

            Vector3 baseSpawnPos = mouthSpawnPoint != null ? mouthSpawnPoint.position : transform.position + Vector3.up * 2f;

            for (int laneIndex = 0; laneIndex < laneSpawnPoints.Length; laneIndex++)
            {
                for (int stoneNum = 0; stoneNum < attack2StonesPerLane; stoneNum++)
                {
                    Vector3 targetLanePos = laneSpawnPoints[laneIndex].position;
                    
                    Vector3 spawnPos = baseSpawnPos;
                    spawnPos.x = targetLanePos.x;
                    spawnPos.z += stoneNum * attack2StoneSpacing;

                    GameObject stone = Instantiate(
                        rollingStonePrefab,
                        spawnPos,
                        Quaternion.identity
                    );

                    RollingStoneBall ball = stone.GetComponent<RollingStoneBall>();
                    if (ball != null)
                        ball.SetMovementStats(attack2StoneSpeed, baseAcceleration);
                }
            }

            Debug.Log($"🦕 Mouth Attack: Spawned {attack2StonesPerLane} stones on each of {laneSpawnPoints.Length} lanes");
        }

        public void SpawnFallingStoneFromStomp()
        {
            SpawnStoneFromStomp();
        }

        private void SpawnFallingStones(int stonesToSpawn)
        {
        }

        private string GetLaneMarkerName(int laneIndex)
        {
            switch (laneIndex)
            {
                case 0: return "LaneMarkerLeft";
                case 1: return "LaneMarkerCenter";
                case 2: return "LaneMarkerRight";
                default: return $"Unknown Lane {laneIndex}";
            }
        }

        private void SpawnFallingStone(int pointIndex)
        {
        }

        private void SpawnRandomFallingStones(int stonesToSpawn)
        {
        }

        public void OnAttackFinished()
        {
            isAttacking = false;
        }

        public void TakeDamage(float damage)
        {
            if (useHealthAsTimer) return;
            
            health -= damage;
            if (health <= 0f)
                Die();
        }

        private void OnTimerExpired()
        {
            timerActive = false;
            autoAttack = false;
            Debug.Log("Timer expired! Player VICTORY!");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictory();
            }
        }

        private void Die()
        {
            Debug.Log("Tortoise Boss defeated!");
        }

        public void StartTimer()
        {
            timerActive = true;
        }

        public void StopTimer()
        {
            timerActive = false;
        }
    }
}