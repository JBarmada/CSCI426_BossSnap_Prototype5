using UnityEngine;
using System.Collections;

namespace BossSnap.Boss
{
    public class HorizontalLaserAttack : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject laserGunPrefab;
        [SerializeField] private Transform[] horizontalSpawnPoints;
        
        [Header("Lane Configuration")]
        [SerializeField] private float leftLaneX = -4f;
        [SerializeField] private float midLaneX = 0f;
        [SerializeField] private float rightLaneX = 4f;
        [SerializeField] private float laneMatchTolerance = 0.5f;
        
        [Header("Debug")]
        [SerializeField] private bool showSpawnGizmos = true;
        [SerializeField] private float gizmoArrowLength = 5f;
        
        private float bossHpPercent = 1f;
        private int gunsCompleted;
        private Player.PlayerController player;
        
        public void Execute(float hpPercent)
        {
            bossHpPercent = hpPercent;
            gunsCompleted = 0;
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player.PlayerController>();
                
            StartCoroutine(SpawnHorizontalLasers());
        }
        
        private float GetPlayerLaneX()
        {
            if (player == null)
                return midLaneX;
                
            int laneIndex = player.CurrentLaneIndex;
            
            switch (laneIndex)
            {
                case 0: return leftLaneX;
                case 1: return midLaneX;
                case 2: return rightLaneX;
                default: return midLaneX;
            }
        }
        
        private IEnumerator SpawnHorizontalLasers()
        {
            if (horizontalSpawnPoints == null || horizontalSpawnPoints.Length == 0 || laserGunPrefab == null)
            {
                Debug.LogWarning("HorizontalLaserAttack: Missing spawn points or prefab!");
                yield break;
            }
            
            float playerLaneX = GetPlayerLaneX();
            
            foreach (Transform spawnPoint in horizontalSpawnPoints)
            {
                if (spawnPoint == null)
                    continue;
                
                if (Mathf.Abs(spawnPoint.position.x - playerLaneX) <= laneMatchTolerance)
                {
                    SpawnLaserGun(spawnPoint.position, spawnPoint.rotation);
                }
            }
            
            yield return null;
        }
        
        private void SpawnLaserGun(Vector3 position, Quaternion rotation)
        {
            GameObject gunObj = Instantiate(laserGunPrefab, position, rotation);
            gunObj.SetActive(true);
            
            LaserGun gun = gunObj.GetComponent<LaserGun>();
            
            if (gun != null)
            {
                gun.Initialize(bossHpPercent);
                gun.OnFiringComplete += OnGunComplete;
            }
        }
        
        private void OnGunComplete()
        {
            gunsCompleted++;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showSpawnGizmos || horizontalSpawnPoints == null)
                return;
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player.PlayerController>();
                
            float playerLaneX = GetPlayerLaneX();
            
            foreach (Transform spawnPoint in horizontalSpawnPoints)
            {
                if (spawnPoint == null)
                    continue;
                
                bool isOnPlayerLane = Mathf.Abs(spawnPoint.position.x - playerLaneX) <= laneMatchTolerance;
                
                Gizmos.color = isOnPlayerLane ? Color.yellow : Color.gray;
                Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
                Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * gizmoArrowLength);
            }
        }
    }
}
