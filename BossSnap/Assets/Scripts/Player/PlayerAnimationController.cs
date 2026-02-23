using UnityEngine;

namespace BossSnap.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;
        private PlayerController playerController;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (animator == null || playerController == null) return;

            bool isMoving = playerController.IsTransitioning;
            bool isJumping = playerController.CurrentRealm == RealmState.TwoD;

            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsJumpingHash, isJumping);
        }

        public void TriggerJump()
        {
            if (animator != null)
            {
                animator.SetTrigger(JumpTriggerHash);
            }
        }
    }
}
