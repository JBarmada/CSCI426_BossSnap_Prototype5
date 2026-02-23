using UnityEngine;

namespace BossSnap.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody rb;
        private PlayerController playerController;

        private static readonly int IsSwitchingHash = Animator.StringToHash("isSwitching");
        private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (animator == null || playerController == null) return;

            bool isSwitching = playerController.CurrentRealm == RealmState.ThreeD && playerController.IsTransitioning;
            bool isJumping = false;

            if (playerController.CurrentRealm == RealmState.TwoD && rb != null)
            {
                bool aboveGround = transform.position.y > 0.05f;
                bool verticalMotion = Mathf.Abs(rb.linearVelocity.y) > 0.05f;
                isJumping = aboveGround || verticalMotion;
            }

            animator.SetBool(IsSwitchingHash, isSwitching);
            animator.SetBool(IsJumpingHash, isJumping);
        }

        public void TriggerJump()
        {
            if (animator != null)
            {
                animator.ResetTrigger(JumpTriggerHash);
                animator.SetTrigger(JumpTriggerHash);
            }
        }
    }
}
