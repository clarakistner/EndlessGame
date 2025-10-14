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

        [HideInInspector]
        public bool deathState = false;

        [Header("Referências")]
        public Transform groundCheck;

        private Rigidbody2D rb;
        private Animator animator;
        private GameManager gameManager;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        private void FixedUpdate()
        {
            CheckGround();
        }

        void Update()
        {
            // Movimento automático para a direita
            Debug.Log("timeScale: " + Time.timeScale);


            Vector3 direction = transform.right;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, movingSpeed * Time.deltaTime);
            animator.SetInteger("playerState", 1); // Animação de corrida

            // Reseta o double jump ao tocar no chão
            if (isGrounded)
                doubleJump = true;

            // INPUT: pular com espaço ou toque
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0;

            if (jumpPressed && (isGrounded || doubleJump))
            {
                Debug.Log("PULO DETECTADO");

                // Se for o segundo pulo, zera a velocidade vertical
                if (!isGrounded)
                    doubleJump = false;

                rb.velocity = new Vector2(rb.velocity.x, 0);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                animator.SetInteger("playerState", 2); // Animação de pulo
            }

            // Inverte o sprite se precisar
            if (!facingRight && direction.x > 0)
                Flip();
        }

        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 scaler = transform.localScale;
            scaler.x *= -1;
            transform.localScale = scaler;
        }

        private void CheckGround()
        {
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
                gameManager.coinsCounter += 1;
                Destroy(other.gameObject);
            }
        }
    }
}
