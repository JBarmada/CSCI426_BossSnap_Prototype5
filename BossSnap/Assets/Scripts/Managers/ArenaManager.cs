using UnityEngine;

namespace BossSnap.Managers
{
    public class ArenaManager : MonoBehaviour
    {
        public static ArenaManager Instance { get; private set; }

        [Header("Lane Configuration")]
        [SerializeField] private float[] lanePositions = new float[] { -4f, 0f, 4f };
        [SerializeField] private Vector3 arenaMin = new Vector3(-15f, 0f, -5f);
        [SerializeField] private Vector3 arenaMax = new Vector3(15f, 10f, 15f);

        public float[] LanePositions => lanePositions;
        public Vector3 ArenaMin => arenaMin;
        public Vector3 ArenaMax => arenaMax;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool IsInArenaBounds(Vector3 position)
        {
            return position.x >= arenaMin.x && position.x <= arenaMax.x &&
                   position.y >= arenaMin.y && position.y <= arenaMax.y &&
                   position.z >= arenaMin.z && position.z <= arenaMax.z;
        }
    }
}
