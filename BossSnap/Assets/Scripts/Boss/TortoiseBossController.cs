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

        [Header("Attack 2 - Falling Stones (2D Realm)")]
        [SerializeField] private GameObject fallingRockPrefab;
        [SerializeField] private Transform[] verticalSpawnPoints;
        [SerializeField] private int attack2StonesPerSpawn = 3;
        [SerializeField] private float verticalSpawnHeight = 10f;
        
        [SerializeField] private Animator animator;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip stompClip;

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

        [Header("Difficulty Scaling")]
        [SerializeField] private bool enableDifficultyScaling = true;
        [SerializeField] private float minAttackInterval = 2f;
        [SerializeField] private int maxStonesPerSpawn = 3;
        [SerializeField] private int maxAttack2Stones = 5;

        private float attackTimer;
        private float currentAttackCooldown;

        private bool isAttacking;
        private bool stompSoundPlayed;
        private int spawnEventsTriggered;
        private bool useAttack2Animation = false;
        private bool isDoingAttack2 = false;
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

            if (player != null && player.CurrentRealm == Player.RealmState.TwoD)
            {
                StartAttack2();
            }
            else
            {
                StartAttack1();
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
                return isAttack2 ? attack2StonesPerSpawn : stonesPerSpawn;
            }

            float timePercent = health / maxHealth;
            
            // As time decreases, spawn more stones
            // 100% time -> base amount
            // 0% time -> max amount
            if (isAttack2)
            {
                int stoneCount = Mathf.RoundToInt(Mathf.Lerp(maxAttack2Stones, attack2StonesPerSpawn, timePercent));
                return Mathf.Clamp(stoneCount, attack2StonesPerSpawn, maxAttack2Stones);
            }
            else
            {
                int stoneCount = Mathf.RoundToInt(Mathf.Lerp(maxStonesPerSpawn, stonesPerSpawn, timePercent));
                return Mathf.Clamp(stoneCount, stonesPerSpawn, maxStonesPerSpawn);
            }
        }

        public void StartAttack1()
        {
            if (isAttacking) return;

            isAttacking = true;
            isDoingAttack2 = false;
            stompSoundPlayed = false;
            spawnEventsTriggered = 0;

            animator.SetTrigger(Attack1Trigger);
        }

        public void StartAttack2()
        {
            if (isAttacking) return;

            isAttacking = true;
            isDoingAttack2 = true;
            stompSoundPlayed = false;
            spawnEventsTriggered = 0;

            // Capture the player's lane at the START of the attack
            if (player != null)
            {
                targetLaneForAttack = player.CurrentLaneIndex;
                string laneName = GetLaneMarkerName(targetLaneForAttack);
                Debug.Log($"⚔️ ATTACK2 STARTED: Player locked to {laneName} (Index: {targetLaneForAttack})");
            }
            else
            {
                targetLaneForAttack = 1; // Default to center if no player
                Debug.LogWarning("Player reference null at attack start! Defaulting to center lane.");
            }

            if (animator != null)
            {
                if (useAttack2Animation)
                {
                    animator.SetTrigger(Attack2Trigger);
                }
                else
                {
                    // Fallback: use Attack1 animation with Attack2 behavior
                    animator.SetTrigger(Attack1Trigger);
                }
            }
        }

        // 🔥 CALLED BY ANIMATION EVENT (can happen multiple times)
        public void SpawnStoneFromStomp()
        {
            // Always shake (if you want shake per stomp)
            if (screenShake != null)
                screenShake.Shake(spawnShakeStrength, spawnShakeDuration);

            // 🔊 Play sound only once per attack
            if (!stompSoundPlayed)
            {
                stompSoundPlayed = true;

                if (stompClip != null && audioSource != null)
                    audioSource.PlayOneShot(stompClip);
            }

            // 🪨 Only allow controlled number of spawn events
            if (spawnEventsTriggered >= maxSpawnEventsPerAttack)
                return;

            spawnEventsTriggered++;

            // Spawn appropriate stone type based on current attack
            if (isDoingAttack2)
            {
                int count = GetCurrentStonesPerSpawn(true);
                SpawnFallingStones(count);
            }
            else
            {
                int count = GetCurrentStonesPerSpawn(false);
                SpawnRollingStones(count);
            }
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

        public void SpawnFallingStoneFromStomp()
        {
            if (screenShake != null)
                screenShake.Shake(spawnShakeStrength, spawnShakeDuration);

            if (!stompSoundPlayed)
            {
                stompSoundPlayed = true;

                if (stompClip != null && audioSource != null)
                    audioSource.PlayOneShot(stompClip);
            }

            if (spawnEventsTriggered >= maxSpawnEventsPerAttack)
                return;

            spawnEventsTriggered++;

            SpawnFallingStones(attack2StonesPerSpawn);
        }

        private void SpawnFallingStones(int stonesToSpawn)
        {
            if (verticalSpawnPoints == null || verticalSpawnPoints.Length == 0)
            {
                Debug.LogWarning("No vertical spawn points assigned! Falling back to random positions.");
                SpawnRandomFallingStones(stonesToSpawn);
                return;
            }

            // Use the lane that was captured when the attack started
            int playerLaneIndex = targetLaneForAttack;
            
            // Ensure the lane index is valid for our spawn points
            if (playerLaneIndex < 0 || playerLaneIndex >= verticalSpawnPoints.Length)
            {
                Debug.LogWarning($"Target lane index {playerLaneIndex} is out of range! Using center lane.");
                playerLaneIndex = 1; // Default to center
            }

            // Get lane marker name for logging
            string laneMarkerName = GetLaneMarkerName(playerLaneIndex);
            string spawnPointName = verticalSpawnPoints[playerLaneIndex].name;
            float spawnX = verticalSpawnPoints[playerLaneIndex].position.x;

            // Spawn all stones at the player's lane
            for (int i = 0; i < stonesToSpawn; i++)
            {
                SpawnFallingStone(playerLaneIndex);
            }
            
            Debug.Log($"🎯 Boss Attack: Spawning {stonesToSpawn} falling stones at {laneMarkerName} (Index: {playerLaneIndex}, Spawn: {spawnPointName}, X: {spawnX})");
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
            Vector3 spawnPos = verticalSpawnPoints[pointIndex].position;
            spawnPos.y = verticalSpawnHeight;

            GameObject prefabToUse = fallingRockPrefab != null ? fallingRockPrefab : rollingStonePrefab;

            GameObject stone = Instantiate(
                prefabToUse,
                spawnPos,
                Quaternion.identity
            );

            Debug.Log($"  ⚫ Spawned stone at {verticalSpawnPoints[pointIndex].name} (X: {spawnPos.x}, Y: {spawnPos.y})");

            FallingRock fallingRock = stone.GetComponent<FallingRock>();
            if (fallingRock != null)
            {
                fallingRock.Initialize(baseStoneSpeed);
            }
        }

        private void SpawnRandomFallingStones(int stonesToSpawn)
        {
            for (int i = 0; i < stonesToSpawn; i++)
            {
                float randomX = Random.Range(-4f, 4f);
                float randomZ = Random.Range(10f, 20f);
                Vector3 spawnPos = new Vector3(randomX, verticalSpawnHeight, randomZ);

                GameObject prefabToUse = fallingRockPrefab != null ? fallingRockPrefab : rollingStonePrefab;

                GameObject stone = Instantiate(
                    prefabToUse,
                    spawnPos,
                    Quaternion.identity
                );

                FallingRock fallingRock = stone.GetComponent<FallingRock>();
                if (fallingRock != null)
                {
                    fallingRock.Initialize(baseStoneSpeed);
                }
            }
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