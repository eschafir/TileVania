using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Config
    [Header("General Configs")]
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float gravity = -9.8f;

    [Header("Jump Setups")]
    [Tooltip("How many jumps the player can do")]
    [SerializeField] int totalJumps = 2;
    [Tooltip("Force of the jump")]
    [SerializeField] float jumpForce = 2f;
    [Tooltip("Force of the double jump. Relative to the Jump Force.")]
    [SerializeField] float multiJumpForce = .8f;
    [Tooltip("How much the gravity affects when player is falling")]
    [SerializeField] float fallMultiplier = 5f;
    [Tooltip("How much the gravity affects the jump force")]
    [SerializeField] float jumpResistance = 4f;
    [Tooltip("Coyote time is the delay after the player is not grounded but can jumps as normally as if is grounded")]
    [SerializeField] float coyoteDelay = .2f;

    [SerializeField] LayerMask groundLayer;

    // State
    bool isAlive = true;
    bool isGrounded;
    bool canMultiJump = false;
    int currentJumps;
    bool coyoteJump;
    bool isClimbing;

    // Cached components references
    Rigidbody2D rb;
    Collider2D myCollider;
    Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        Run();
        ClimbLadder();
        Jump();
        FlipSprite();
    }

    void FixedUpdate()
    {
        CheckGrounded();
    }

    private void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        float distance = 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, groundLayer);
        if (hit.collider != null)
        {
            isGrounded = true;
            if (!wasGrounded)
            {
                currentJumps = totalJumps;
                canMultiJump = false;
            }
        }
        else
        {
            if (wasGrounded)
                StartCoroutine(CoyoteJumpDelay());
        }
        animator.SetBool("Jump", !isGrounded);
    }

    IEnumerator CoyoteJumpDelay()
    {
        coyoteJump = true;
        yield return new WaitForSeconds(coyoteDelay);
        coyoteJump = false;
    }

    void Run()
    {
        float horizontalValue = Input.GetAxis("Horizontal") * runSpeed;
        animator.SetBool("isRunning", horizontalValue != 0);
        rb.velocity = new Vector2(horizontalValue, rb.velocity.y);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(rb.velocity.x), 1f);
        }
    }

    void Jump()
    {
        if (isClimbing) { return; }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded || coyoteJump)
            {
                canMultiJump = true;
                currentJumps--;

                rb.velocity = Vector2.up * jumpForce;
                animator.SetTrigger("Jump");
            }
            else
            {
                if (canMultiJump && currentJumps > 0)
                {
                    currentJumps--;
                    rb.velocity = Vector2.up * jumpForce * multiJumpForce;
                }
            }
        }
        BetterJumpMod();
        animator.SetBool("Grounded", isGrounded);
    }

    void BetterJumpMod()
    {
        if (rb.velocity.y < Mathf.Epsilon)
        {
            rb.velocity += Vector2.up * gravity * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > Mathf.Epsilon && !Input.GetButtonDown("Jump"))
        {
            rb.velocity += Vector2.up * gravity * (jumpResistance - 1) * Time.deltaTime;
        }
    }

    void ClimbLadder()
    {
        float verticalValue = Input.GetAxis("Vertical") * climbSpeed;
        bool playerIsTouchingALadder = myCollider.IsTouchingLayers(LayerMask.GetMask("Ladders"));

        if (playerIsTouchingALadder)
        {
            bool playerIsMovingUpOrDown = Mathf.Abs(verticalValue) > Mathf.Epsilon;

            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            if (playerIsMovingUpOrDown)
            {
                isClimbing = true;
                rb.velocity = new Vector2(rb.velocity.x, verticalValue);
                rb.gravityScale = 0f;
            }
            animator.SetBool("isClimbing", isClimbing);
        }
        else
        {
            animator.SetBool("isClimbing", false);
            isClimbing = false;
            rb.gravityScale = 1f;
        }
    }
}
