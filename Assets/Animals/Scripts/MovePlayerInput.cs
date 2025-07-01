using UnityEngine;

namespace Controller
{
    [RequireComponent(typeof(CreatureMover))]
    public class MovePlayerInput : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField]
        private string m_HorizontalAxis = "Horizontal";
        [SerializeField]
        private string m_VerticalAxis = "Vertical";
        [SerializeField]
        private string m_JumpButton = "Jump";
        [SerializeField]
        private KeyCode m_RunKey = KeyCode.LeftShift;

        [Header("Camera")]
        [SerializeField]
        private PlayerCamera m_Camera;
        [SerializeField]
        private string m_MouseX = "Mouse X";
        [SerializeField]
        private string m_MouseY = "Mouse Y";
        [SerializeField]
        private string m_MouseScroll = "Mouse ScrollWheel";

        private CreatureMover m_Mover;
        private CreatureStats m_Stats;

        private Vector2 m_Axis;
        private bool m_IsRun;
        private bool m_IsJump;

        private Vector3 m_Target;
        private Vector2 m_MouseDelta;
        private float m_Scroll;

        private CreatureStats currentTarget;

        private void Awake()
        {
            m_Mover = GetComponent<CreatureMover>();
            
            // Only get CreatureStats if this is the player
            if (CompareTag("Dice"))
            {
                m_Stats = GetComponent<CreatureStats>();
                if (m_Stats == null)
                {
                    m_Stats = gameObject.AddComponent<CreatureStats>();
                }
            }
        }

        private void Update()
        {
            GatherInput();
            SetInput();

            // Player attack logic (press 'J' to attack nearest creature in range)
            if (m_Stats != null && Input.GetKeyDown(KeyCode.J))
            {
                float attackRange = m_Stats.AttackRange;
                float attackDamage = m_Stats.AttackDamage;
                float attackManaCost = m_Stats.AttackManaCost;
                // Check if enough mana for attack
                if (m_Stats.HasEnoughStamina(0.1f) && m_Stats.HasEnoughMana(attackManaCost))
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
                    CreatureStats closest = null;
                    float minDist = float.MaxValue;
                    foreach (var hit in hits)
                    {
                        if (hit.gameObject == this.gameObject) continue; // skip self
                        var stats = hit.GetComponent<CreatureStats>();
                        if (stats != null)
                        {
                            float dist = Vector3.Distance(transform.position, hit.transform.position);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                closest = stats;
                            }
                        }
                    }

                    // Update target and UI
                    if (currentTarget != closest)
                    {
                        if (currentTarget != null)
                        {
                            currentTarget.SetAsTarget(false);
                        }
                        currentTarget = closest;
                        if (currentTarget != null)
                        {
                            currentTarget.SetAsTarget(true);
                        }
                    }

                    if (closest != null)
                    {
                        closest.ModifyHealth(-attackDamage);
                        m_Stats.ModifyMana(-attackManaCost);
                        Debug.Log($"Attacked {closest.gameObject.name} for {attackDamage} damage. Mana used: {attackManaCost}");
                    }
                }
                else
                {
                    Debug.Log("Not enough mana to attack.");
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up target reference when destroyed
            if (currentTarget != null)
            {
                currentTarget.SetAsTarget(false);
            }
        }

        public void GatherInput()
        {
            // Get raw input
            float horizontal = Input.GetAxis(m_HorizontalAxis);
            float vertical = Input.GetAxis(m_VerticalAxis);
            
            // Allow diagonal movement by combining both inputs
            m_Axis = new Vector2(horizontal, vertical);
            
            // Normalize the input vector if its magnitude is greater than 1
            if (m_Axis.magnitude > 1f)
            {
                m_Axis.Normalize();
            }
            
            // Handle running for the player
            if (m_Stats != null)  // We are the player
            {
                bool wantsToRun = Input.GetKey(m_RunKey);
                bool hasStamina = m_Stats.HasEnoughStamina(0.1f);
                bool isMoving = m_Axis.magnitude > 0;
                
                m_IsRun = wantsToRun && hasStamina && isMoving;
                
                // Only drain stamina if we're actually running and moving
                if (m_IsRun)
                {
                    m_Stats.DrainStaminaForSprint();
                    Debug.Log("Draining stamina for sprint");
                }
            }
            else  // We are not the player
            {
                bool wantsToRun = Input.GetKey(m_RunKey);
                m_IsRun = wantsToRun && m_Axis.magnitude > 0;
            }
            
            m_IsJump = Input.GetButton(m_JumpButton);

            m_Target = (m_Camera == null) ? Vector3.zero : m_Camera.Target;
            m_MouseDelta = new Vector2(Input.GetAxis(m_MouseX), Input.GetAxis(m_MouseY));
            m_Scroll = Input.GetAxis(m_MouseScroll);
        }

        public void BindMover(CreatureMover mover)
        {
            m_Mover = mover;
        }

        public void SetInput()
        {
            if (m_Mover != null)
            {
                m_Mover.SetInput(in m_Axis, in m_Target, in m_IsRun, m_IsJump);
            }

            if (m_Camera != null)
            {
                m_Camera.SetInput(in m_MouseDelta, m_Scroll);
            }
        }
    }
}