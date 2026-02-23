using UnityEngine;

namespace BossSnap.Boss
{
    public class TortoiseBossController : MonoBehaviour
    {
        [Header("Boss Configuration")]
        [SerializeField] private float health = 100f;

        private void Start()
        {
            Debug.Log("Tortoise Boss initialized");
        }

        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Tortoise Boss defeated!");
        }
    }
}
