using UnityEngine;
using BossSnap.Managers;
using BossSnap.Player;

namespace BossSnap.Boss
{
    public class BossLaserAttackManager : MonoBehaviour
    {
        [Header("Attack Components")]
        [SerializeField] private DiagonalLaserAttack diagonalAttack;
        [SerializeField] private HorizontalLaserAttack horizontalAttack;
        
        [Header("Pattern Configuration")]
        [SerializeField] private float patternSwitchTime = 30f;
        
        private float elapsedTime;
        private int attacksInCurrentCycle;
        private bool useFirstPattern = true;
        private TortoiseBossController bossController;
        
        private void Awake()
        {
            bossController = GetComponent<TortoiseBossController>();
            
            if (diagonalAttack == null)
                diagonalAttack = gameObject.AddComponent<DiagonalLaserAttack>();
                
            if (horizontalAttack == null)
                horizontalAttack = gameObject.AddComponent<HorizontalLaserAttack>();
        }
        
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            
            if (elapsedTime >= patternSwitchTime && useFirstPattern)
            {
                useFirstPattern = false;
                attacksInCurrentCycle = 0;
                Debug.Log("BossLaserAttackManager: Switched to pattern 1,1,2");
            }
        }
        
        public void TriggerLaserAttack(RealmState currentRealm)
        {
            if (currentRealm != RealmState.TwoD)
                return;
                
            float hpPercent = bossController != null ? (bossController.CurrentHealth / bossController.MaxHealth) : 1f;
            
            bool useAttack2 = ShouldUseAttack2();
            
            if (useAttack2 && horizontalAttack != null)
            {
                Debug.Log("BossLaserAttackManager: Executing Horizontal Laser Attack");
                horizontalAttack.Execute(hpPercent);
            }
            else if (diagonalAttack != null)
            {
                Debug.Log("BossLaserAttackManager: Executing Diagonal Laser Attack");
                diagonalAttack.Execute(hpPercent);
            }
            
            attacksInCurrentCycle++;
            
            if (useFirstPattern)
            {
                if (attacksInCurrentCycle >= 4)
                    attacksInCurrentCycle = 0;
            }
            else
            {
                if (attacksInCurrentCycle >= 3)
                    attacksInCurrentCycle = 0;
            }
        }
        
        private bool ShouldUseAttack2()
        {
            if (useFirstPattern)
            {
                return attacksInCurrentCycle == 3;
            }
            else
            {
                return attacksInCurrentCycle == 2;
            }
        }
    }
}
