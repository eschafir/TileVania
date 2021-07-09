using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Config
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] float gravity = -9.8f;

    [Header("Jump Setups")]
    [Tooltip("Force of the jump")]
    [SerializeField] float jumpForce = 2f;
    [Tooltip("Force of the double jump. Relative to the Jump Force.")]
    [SerializeField] float doubleJumpForce = .8f;
    [Tooltip("How much the gravity affects when player is falling")]
    [SerializeField] float fallMultiplier = 5f;
    [Tooltip("How much the gravity affects the jump force")]
    [SerializeField] float jumpResistance = 4f;

    [SerializeField] LayerMask groundLayer;

    // State
    bool isAlive = true;
    bool canDoubleJump = false;
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
        if (IsGrounded())
        {
            canDoubleJump = true;

            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = Vector2.up * jumpForce;
                animator.SetTrigger("Jump");
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && canDoubleJump)
            {
                rb.velocity = Vector2.up * jumpForce * doubleJumpForce;
                canDoubleJump = false;
            }
        }

        BetterJumpMod();
        animator.SetBool("Grounded", IsGrounded());
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

    bool IsGrounded()
    {
        float distance = 1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, groundLayer);

        return (hit.collider != null);
    }

    void ClimbLadder()
    {
        float verticalValue = Input.GetAxis("Vertical") * climbSpeed;
        bool playerIsTouchingALadder = myCollider.IsTouchingLayers(LayerMask.GetMask("Ladders"));

        if (IsGrounded())
        {
            isClimbing = false;
        }

        if (playerIsTouchingALadder)
        {
            bool playerIsMovingUpOrDown = Mathf.Abs(verticalValue) > Mathf.Epsilon;

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
