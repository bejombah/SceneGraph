using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SceneGraph
{
    public class Player : MonoBehaviour
    {
        public static Player Instance;

        public float minGroundNormalY = .65f;
        public float gravityModifier = 1f;
        protected Vector2 targetVelocity;
        bool _grounded; public bool grounded { get => _grounded; set => _grounded = value; }
        protected Vector2 groundNormal;
        protected Rigidbody2D rb2d; public Rigidbody2D rb => rb2d;
        protected Vector2 _velocity; public Vector2 velocity { get => _velocity; set => _velocity = value; }
        protected ContactFilter2D contactFilter;
        protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
        protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);
        protected const float minMoveDistance = 0.001f;
        protected const float shellRadius = .01f;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public virtual void OnEnable()
        {
            rb2d = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask (Physics2D.GetLayerCollisionMask (gameObject.layer));
            contactFilter.useLayerMask = true;
        }
        void Update()
        {
            targetVelocity = Vector2.zero;
            ComputeVelocity ();        
        }

        void ComputeVelocity()
        {
            // movement
            if(Input.GetAxis("Horizontal") != 0)
            {
                targetVelocity.x = Input.GetAxis("Horizontal") * 10;
            }
            else
            {
                targetVelocity.x = 0;
            }

            // jump
            if(Input.GetButtonDown("Jump") && grounded)
            {
                _velocity.y = 10;
            }
        }

        void FixedUpdate()
        {
            velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
            
            _velocity.x = targetVelocity.x;

            _grounded = false;

            Vector2 deltaPosition = velocity * Time.deltaTime;

            Vector2 moveAlongGround = new Vector2 (groundNormal.y, -groundNormal.x);

            Vector2 move = moveAlongGround * deltaPosition.x;
            
            Movement (move, false);
            
            move = Vector2.up * deltaPosition.y;

            Movement (move, true);
        }

        public void Movement(Vector2 move, bool yMovement)
        {
            float distance = move.magnitude;

            if (distance > minMoveDistance) 
            {
                int count = rb2d.Cast (move, contactFilter, hitBuffer, distance + shellRadius);
                hitBufferList.Clear ();
                for (int i = 0; i < count; i++) {
                    hitBufferList.Add (hitBuffer [i]);
                }

                for (int i = 0; i < hitBufferList.Count; i++) 
                {
                    Vector2 currentNormal = hitBufferList [i].normal;
                    if (currentNormal.y > minGroundNormalY) 
                    {
                        _grounded = true;
                        if (yMovement) 
                        {
                            groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }

                    float projection = Vector2.Dot (velocity, currentNormal);
                    if (projection < 0) 
                    {
                        velocity = velocity - projection * currentNormal;
                    }

                    float modifiedDistance = hitBufferList [i].distance - shellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            
            rb2d.position = rb2d.position + move.normalized * distance;
            
        }
    }
}
