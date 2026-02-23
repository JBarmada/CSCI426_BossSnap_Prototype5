using UnityEngine;
using System.Collections.Generic;

namespace BossSnap.Boss
{
    public class TortoiseBossController : MonoBehaviour
    {
        [Header("Boss Config")]
        [SerializeField] private float health = 100f;

        [Header("Attack 1")]
        [SerializeField] private GameObject rollingStonePrefab;
        [SerializeField] private Transform[] laneSpawnPoints;
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

        private float attackTimer;
        private float currentAttackCooldown;

        private bool isAttacking;
        private bool stompSoundPlayed;
        private int spawnEventsTriggered;

        private static readonly int Attack1Trigger = Animator.StringToHash("Attack1");

        private void Awake()
        {
            if (screenShake == null && Camera.main != null)
                screenShake = Camera.main.GetComponent<ScreenShake>();
        }

        private void Start()
        {
            ResetAttackCooldown();
        }

        private void Update()
        {
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
            StartAttack1();
        }

        private void ResetAttackCooldown()
        {
            attackTimer = 0f;
            currentAttackCooldown =
                attackInterval +
                Random.Range(-attackRandomVariance, attackRandomVariance);
        }

        public void StartAttack1()
        {
            if (isAttacking) return;

            isAttacking = true;
            stompSoundPlayed = false;
            spawnEventsTriggered = 0;

            animator.SetTrigger(Attack1Trigger);
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

            SpawnRollingStones(stonesPerSpawn);
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

        public void OnAttackFinished()
        {
            isAttacking = false;
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
                Die();
        }

        private void Die()
        {
            Debug.Log("Tortoise Boss defeated!");
        }
    }
}