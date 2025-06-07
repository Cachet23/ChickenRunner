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