using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BossSnap.Boss
{
    public class DiagonalLaserAttack : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GameObject laserGunPrefab;
        [SerializeField] private Transform[] cornerSpawnPoints;
        [SerializeField] private float delayBetweenGuns = 0.3f;
        [SerializeField] [Range(1, 4)] private int lasersToSpawn = 4;
        
        [Header("Lane Configuration")]
        [SerializeField] private float leftLaneX = -4f;
        [SerializeField] private float midLaneX = 0f;
        [SerializeField] private float rightLaneX = 4f;
        
        [Header("Debug")]
        [SerializeField] private bool showSpawnGizmos = true;
        [SerializeField] private float gizmoArrowLength = 2f;
        
        private float bossHpPercent = 1f;
        private int gunsCompleted;
        private Player.PlayerController player;
        
        public void Execute(float hpPercent)
        {
            bossHpPercent = hpPercent;
            gunsCompleted = 0;
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player.PlayerController>();
                
            StartCoroutine(SpawnDiagonalLasers());
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
        
        private IEnumerator SpawnDiagonalLasers()
        {
            if (cornerSpawnPoints == null || cornerSpawnPoints.Length < 4 || laserGunPrefab == null)
            {
                Debug.LogWarning("DiagonalLaserAttack: Missing spawn points or prefab!");
                yield break;
            }
            
            float playerLaneX = GetPlayerLaneX();
            
            List<int> spawnOrder = new List<int> { 0, 1, 2, 3 };
            
            for (int i = spawnOrder.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                int temp = spawnOrder[i];
                spawnOrder[i] = spawnOrder[j];
                spawnOrder[j] = temp;
            }
            
            int actualLasersToSpawn = Mathf.Clamp(lasersToSpawn, 1, cornerSpawnPoints.Length);
            
            for (int i = 0; i < actualLasersToSpawn; i++)
            {
                Transform spawnPoint = cornerSpawnPoints[spawnOrder[i]];
                Vector3 spawnPos = spawnPoint.position;
                spawnPos.x = playerLaneX;
                
                SpawnLaserGun(spawnPos, spawnPoint.rotation);
                
                float scaledDelay = Mathf.Lerp(delayBetweenGuns * 0.5f, delayBetweenGuns, bossHpPercent);
                yield return new WaitForSeconds(scaledDelay);
            }
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
            if (!showSpawnGizmos || cornerSpawnPoints == null)
                return;
            
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player.PlayerController>();
                
            float playerLaneX = GetPlayerLaneX();
            
            Gizmos.color = Color.red;
            foreach (Transform spawnPoint in cornerSpawnPoints)
            {
                if (spawnPoint == null)
                    continue;
                
                Vector3 pos = spawnPoint.position;
                pos.x = playerLaneX;
                    
                Gizmos.DrawWireSphere(pos, 0.3f);
                Gizmos.DrawRay(pos, spawnPoint.forward * gizmoArrowLength);
            }
        }
    }
}
