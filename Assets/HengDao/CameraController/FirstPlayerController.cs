using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HengDao;

namespace HengDao
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPlayerController : MonoBehaviour
    {

        private bool m_jump;
        private bool m_isJumping;
        [SerializeField]
        private bool m_isWalking;
        private bool m_previouslyGrounded;

        [SerializeField]
        private float m_walkSpeed = 5;
        [SerializeField]
        private float m_runSpeed = 10;
        [SerializeField]
        private float m_jumpSpeed = 10;

        [SerializeField]
        private float m_stickToGroundForce = 10;

        [SerializeField]
        private float m_gravityMultiplier = 2;

        private Vector2 m_input;
        private Vector3 m_moveDir;

        private CharacterController m_characterController;

        private CollisionFlags m_collisionFlags;

        private EMouseLook m_mouseLook;
        private Transform m_camera;

        private Animation mAnimation;

        private bool m_isIdle = true;
        private bool m_isRunning = false;

        // Use this for initialization
        void Start()
        {
            m_jump = false;
            m_characterController = GetComponent<CharacterController>();
            m_camera = transform.GetComponentInChildren<Camera>().transform;

            m_mouseLook = new EMouseLook();
            m_mouseLook.Init(transform, m_camera);
            m_mouseLook.SetCursorLock(false);

            mAnimation = GetComponentInChildren<Animation>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_jump)
            {
                m_jump = EInput.GetJumpStatus();
            }

            if (!m_previouslyGrounded && m_characterController.isGrounded)
            {
                m_moveDir.y = 0;
                m_isJumping = false;
            }

            if (!m_characterController.isGrounded && !m_isJumping && m_previouslyGrounded)
            {
                m_moveDir.y = 0;
            }

            m_previouslyGrounded = m_characterController.isGrounded;

            if (Input.GetMouseButton(0))
            {
                m_mouseLook.LookRotation(transform, m_camera);
            }
        }

        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);

            Vector3 desiredMove = transform.forward * m_input.y + transform.right * m_input.x;

            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_characterController.radius, Vector3.down, out hitInfo,
                m_characterController.height / 2, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_moveDir.x = desiredMove.x * speed;
            m_moveDir.z = desiredMove.z * speed;

            if (m_characterController.isGrounded)
            {
                m_moveDir.y = -m_stickToGroundForce;
                if (m_jump)
                {
                    m_moveDir.y = m_jumpSpeed;
                    m_jump = false;
                    m_isJumping = true;
                }
            }
            else
            {
                m_moveDir += Physics.gravity * m_gravityMultiplier * Time.fixedDeltaTime;
            }

            UpdateAnimation(m_moveDir);
            m_collisionFlags = m_characterController.Move(m_moveDir * Time.fixedDeltaTime);
        }

        private void GetInput(out float speed)
        {
            float horizontal = EInput.GetAxis(InputAxisName.Horizontal);
            float vertical = EInput.GetAxis(InputAxisName.Vertical);

            m_isWalking = !Input.GetKey(KeyCode.LeftShift);
            speed = m_isWalking ? m_walkSpeed : m_runSpeed;
            m_input = new Vector2(horizontal, vertical);

            if (m_input.sqrMagnitude > 1)
            {
                m_input.Normalize();
            }


        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }

        void UpdateAnimation(Vector3 move)
        {
            if(mAnimation == null)
            {
                return;
            }
            // update the animator parameters
            if(Mathf.Abs(move.z) > 0 || Mathf.Abs(move.x) > 0)
            {
                if(m_isIdle)
                {
                    mAnimation.CrossFade("run");
                    m_isRunning = true;
                    m_isIdle = false;
                }
            }
            else
            {
                if(m_isRunning)
                {
                    mAnimation.CrossFade("idle");
                    m_isRunning = false;
                    m_isIdle = true;
                }
            }
        }
    }

}
