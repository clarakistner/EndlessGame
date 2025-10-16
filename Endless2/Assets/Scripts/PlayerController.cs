using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimentação")]
        public float movingSpeed = 5f;
        public float jumpForce = 10f;

        private bool doubleJump = true;
        private bool facingRight = true;
        private bool isGrounded;

        [HideInInspector] public bool deathState = false;

        [Header("Referências")]
        public Transform groundCheck;

        [SerializeField] private GameManager gameManager; // arraste aqui no Inspector

        private Rigidbody2D rb;
        private Animator animator;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            // Fallback se não foi arrastado no Inspector:
#if UNITY_2022_2_OR_NEWER
            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>(FindObjectsInactive.Exclude);
#else
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
#endif
            if (gameManager == null)
            {
                Debug.LogError("[PlayerController] GameManager não encontrado. " +
                               "Arraste o GameManager no campo do Inspector OU garanta que exista um GameManager ativo na cena.");
            }
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        void Update()
        {
            Vector3 direction = transform.right;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, movingSpeed * Time.deltaTime);

            if (animator != null)
                animator.SetInteger("playerState", 1);

            if (isGrounded) doubleJump = true;

            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0;
            if (jumpPressed && (isGrounded || doubleJump))
            {
                if (!isGrounded) doubleJump = false;
                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                if (animator != null)
                    animator.SetInteger("playerState", 2);
            }

            if (!facingRight && direction.x > 0) Flip();
        }

        private void Flip()
        {
            facingRight = !facingRight;
            var s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }

        private void CheckGround()
        {
            // Certifique-se de que groundCheck está atribuído
            if (groundCheck == null) return;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, 0.2f);
            isGrounded = colliders.Length > 1;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
                deathState = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Coin"))
            {
                // Protege contra GameManager null
                if (gameManager != null)
                {
                    gameManager.coinsCounter += 1;     // ou gameManager.AddCoins(1);
                }
                else
                {
                    Debug.LogWarning("[PlayerController] Coin coletada, mas GameManager está null. Verifique a referência.");
                }

                Destroy(other.gameObject);
            }
        }
    }
}
