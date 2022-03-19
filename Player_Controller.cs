using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Controller : MonoBehaviour
{
    private Rigidbody2D rb;                                     // A reference to the entity's RigidBody2D
    private Animator anim;                                      // A reference to the entity's Animator
    private Platform platf;                                     // A reference to the entity's Platform Script
    public DashBar dashBar;                                     // A reference to the entity's DashBar Script
    public Slider dashB;                                        // A reference to the DashBar
    public Transform spawnPoint;                                // A reference to the spawn point
    public GameObject dashArrow;
    public GameObject groundSmashParticles;
    private GameObject groundSmashClone;
    public Material arrowMat;
    public Health enemyHealth;

    public int playerMaxHealth;                                 // An integer holding the maximum health of the player
    private int playerCurrentHealth;                            // An integer holding the current health of the player

    private int facingDirection = 1;                            // An int for better use of the players facing direction
    private int previousFacingDirection = 1;                    // To if the players facing direction has changed
    private float facingDirectionWaitTime;                      // The amount of time left before updating the previous facing direction
    private float facingDirectionWaitTimeSet = .15f;            // The amount of time before updating the previous facing direction

    private bool hasReleasedDashButtonBetweenDashs;
    private bool canInstantiateGroundSmash;
    private bool isAboutToDash;
    private bool timeIsSlowedDash;
    private bool m_FacingLeft;                                  // To check if the entity is facing left or not
    public bool isGrounded;                                    // To check if the entity is grounded
    private bool IsDashing;                                     // To check if the entity is dashing
    private bool isCrouching;                                   // To check if the entity is crouching
    private bool isTouchingWall;                                // To check if the entity is touching a wall
    private bool isWallSliding;                                 // To Check if the entity is wall sliding
    private bool canDash;                                       // To determine wether the entity can dash
    private bool isRunning;                                     // To determine if the player is running
    private bool isWalking;                                     // To determine if the player is walking
    //private bool isTouchingLedge = false;                       // To determine if the player is touching a ledge
    //private bool canClimbLedge = false;                         // To determine if the player can climb a ledge
    //private bool ledgeDetected;                                 // To determine if there is a ledge
    private bool canMove = true;                                // To determine if the player can move
    private bool canFlip = true;                                // To determine if the player can flip
    private bool justWallJumped;                                // To determine if the player has just wall jumped
    private bool arrowShouldFadeIn;

    [SerializeField] private float GroundedRadius;              // Radius of the overlap circle to determine if grounded
    [SerializeField] private Transform GroundCheck;             // A position marking where to check if the player is grounded.
    [SerializeField] private LayerMask WhatIsGround;			// A mask determining what is ground to the character
    [SerializeField] private float playerWallCheckDistance;     // Length of the raycast to determine if touching wall
    [SerializeField] private Transform WallCheck;               // A position marking where to check if the player is touching a wall
    //[SerializeField] private Transform LedgeCheck;              // A position marking where to check if the player is touching a ledge

    
    [SerializeField] private Vector2 groundSmashOffset;
    private float dashButtonHoldTime;
    private float dashButtonHoldTimeSet = .1f;
    public float maxDashTimeFreezeTime;
    public float defaultTimeSpeed;
    private float dashTimeFreezeTime;
    public float dashTimeFreezeSpeed;
    private float horizontalMove;                               //
    public float verticalMove;                                  //
    public float wallSlideSpeed;                                // The speed with wich the entity slides down a wall
    public float wallHoldTimeSet;
    [SerializeField] private float run_Speed;                   // This is kind of obvious
    [SerializeField] private float jump_Force;                  //
    [SerializeField] private float dash_Force;                  // The amount of force applied during a dash
    [SerializeField] private float dash_Time;                   // The amount of time for wich the player will dash
    private float dash_Countdown;                               // This one is vital but you don't need to know why
    private int air_Dashs_Left;                                     // The number of times the player has dashed mid-air
    public int air_Dashs;                                       // The number of times the player can dash mid-air
    public float timeBetweenDashs;                              // The amount of time for wich the player cannot dash after dashing
    private float intermidDashVar;                              // This one is also super important for the dash but the reason is kinda dumb
    public float variableJumpHeightMultiplier;                  // 
    private float currentVariableJumpHeight;                    //
    public float airDragMultiplier;                             //
    private float dashBarValue;                                 //
    public float airMoveSpeed;                                  // The speed applied to the entity in air
    public float airMoveSpeedLimit;                             // The speed limit applied to the entity mid-air
    private float postDashTime;
    private float postDashTimeSet = .1f;
    public float jumps;                                         // The total number of jumps the entity has
    private float jumpsLeft;                                    // The number of jumps the entity has left
    private int lastVM;                                         // The last vertical input 
    private int lastHM;                                         // The last horizontal input
    public float groundSmashActiveTimeSet;
    private float groundSmashActiveTime;
    private float arrowFadeInTime;
    public float arrowFadeSpeed;
    public float groundSmashDmg;

    public float wallHopForce;                                  //
    public Vector2 wallHopDirection;                            //
    public Vector2 dashBarOffset;                               //

    //public float ledgeClimbXOffset1 = 0f;
    //public float ledgeClimbYOffset1 = 0f;
    //public float ledgeClimbXOffset2 = 0f;
    //public float ledgeClimbYOffset2 = 0f;
    //private Vector2 ledgePosBot;
    //private Vector2 ledgePos1;
    //private Vector2 ledgePos2;

    public int trapDamage;                                      // The damage dealt to the player by a trap


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        platf = GetComponent<Platform>();
        dashBarValue = timeBetweenDashs;
        dashBar.SetMaxValue(dashBarValue);
        wallHopDirection.Normalize();
        rb.position = spawnPoint.position;
        playerCurrentHealth = playerMaxHealth;
        dashTimeFreezeTime = maxDashTimeFreezeTime;
        dashButtonHoldTime = dashButtonHoldTimeSet;
        hasReleasedDashButtonBetweenDashs = true;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckSurroundings();
        WallSlidingCheck();
        CheckIfCanDash();
        CheckIfCanJump();
        FacingDirectionCheck();
        GroundSmash();
        //CheckLedgeClimb();
    }

    void FixedUpdate()
    {
        ApplyMovement();
        AnimationHandler();
    }

    //private void CheckLedgeClimb()
    //{
    //    if (ledgeDetected && !canClimbLedge)
    //    {
    //        canClimbLedge = true;

    //        if (!m_FacingLeft)
    //        {
    //            ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + playerWallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
    //            ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + playerWallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
    //        }
    //        else
    //        {
    //            ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - playerWallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
    //            ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - playerWallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
    //        }

    //        canMove = false;
    //        canFlip = false;

    //        anim.SetBool("canClimbLedge", canClimbLedge);
    //    }

    //    if (canClimbLedge)
    //    {
    //        transform.position = ledgePos1;
    //    }
    //}

    //public void FinishLedgeClimb()
    //{
    //    canClimbLedge = false;
    //    transform.position = ledgePos2;
    //    canMove = true;
    //    canFlip = true;
    //    ledgeDetected = false;
    //    Debug.Log(ledgePos2);

    //    anim.SetBool("canClimbLedge", canClimbLedge);
    //}

    private void GroundSmash()
    {
        if (IsDashing && lastVM == -1 && lastHM == 0 && rb.velocity.y > -30)
        {
            if (canInstantiateGroundSmash)
            {
                if (groundSmashClone != null)
                {
                    Destroy(groundSmashClone);
                }
                
                canInstantiateGroundSmash = false;
                Vector2 positionToSet = new Vector2(transform.position.x - groundSmashOffset.x, transform.position.y - groundSmashOffset.y);
                groundSmashClone = Instantiate(groundSmashParticles, positionToSet, Quaternion.identity);
                groundSmashActiveTime = groundSmashActiveTimeSet;
                enemyHealth.TakeDamage(groundSmashDmg);
            }
        }

        if (groundSmashActiveTime <= 0)
        {
            Destroy(groundSmashClone);
        }
        else
        {
            groundSmashActiveTime -= Time.deltaTime;
        }
    }

    private void FacingDirectionCheck()
    {
        if (facingDirectionWaitTime <= 0)
        {
            previousFacingDirection = facingDirection;
        }
        else 
        {
            facingDirectionWaitTime -= Time.deltaTime;
        }
    }

    private void CheckIfCanJump()
    {
        if (isGrounded || platf.isOnPlatform || isWallSliding)
        {
            jumpsLeft = jumps;
        }
    }

    private void WallSlidingCheck()
    {
        if (isTouchingWall && !isGrounded && !platf.isOnPlatform && (horizontalMove != 0 || justWallJumped) && !isWallSliding)
        {
            isWallSliding = true;
        }
        else if (isGrounded || platf.isOnPlatform || !isTouchingWall)
        {
            isWallSliding = false;
        }

        if (isWallSliding)
        {
            canFlip = false;
        }
        else 
        {
            canFlip = true;
        }
    }

    private void CheckIfCanDash()
    {
        if (Time.time > intermidDashVar && !isCrouching)
        {
            canDash = true;
        }
        else
        {
            canDash = false;
            dashBarValue += Time.deltaTime;

            if (air_Dashs_Left > 0)
            {
                dashBar.SetValue(dashBarValue);
            }
            else {
                dashBar.SetValue(0f);
            }
        }

        if (arrowShouldFadeIn)
        {
            if (arrowFadeInTime < 1)
            {
                arrowFadeInTime += Time.unscaledDeltaTime * arrowFadeSpeed;
            }
        }
        else if (!arrowShouldFadeIn)
        {
            if (arrowFadeInTime > 0)
            {
                arrowFadeInTime -= Time.unscaledDeltaTime * arrowFadeSpeed;
            }
        }

        arrowMat.SetFloat("_Fade", arrowFadeInTime);
    }

    private void CheckSurroundings() 
    {
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, GroundedRadius, WhatIsGround);
        isTouchingWall = Physics2D.Raycast(WallCheck.position, transform.right, playerWallCheckDistance, WhatIsGround);

        if (rb.velocity.y < -100f)
        {
            Die();
        }

        //isTouchingLedge = Physics2D.Raycast(LedgeCheck.position, transform.right, playerWallCheckDistance, WhatIsGround);

        //if (isTouchingWall && !isTouchingLedge && !ledgeDetected && !isGrounded)
        //{
        //    ledgeDetected = true;
        //    ledgePosBot = WallCheck.position;
        //}
    }
    
    private void ApplyMovement() 
    {
        // Wall Sliding
        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }

        // Entity Aerial Decrementing Movement
        if (!isGrounded && horizontalMove == 0 && !platf.isOnPlatform)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
         
        // Entity Grounded Movement
        if ((isGrounded || platf.isOnPlatform) && !isCrouching && canMove)
        {
            // Walk
            if (horizontalMove <= .5f && horizontalMove >=  -.5f && horizontalMove != 0)
            {
                isWalking = true;
                isRunning = false;
                rb.velocity = new Vector2(.5f * run_Speed * facingDirection, rb.velocity.y);
            }
            // Run
            else if (horizontalMove > .5f || horizontalMove < -.5f)
            {
                isRunning = true;
                isWalking = false;
                rb.velocity = new Vector2(run_Speed * facingDirection, rb.velocity.y);
            }
            // Stop
            else if (horizontalMove == 0f)
            {
                isWalking = false;
                isRunning = false;
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }

            dashBar.SetValue(dashBarValue);
        }

        // Entity Aerial Movement
        if (!isGrounded && !platf.isOnPlatform && horizontalMove != 0 && !isWallSliding)
        {
            if ((rb.velocity.x < airMoveSpeedLimit && rb.velocity.x > -airMoveSpeedLimit) || facingDirection != previousFacingDirection)
            {
                Vector2 forceToAdd = new Vector2(airMoveSpeed * horizontalMove, 0f);
                rb.AddForce(forceToAdd);
            }
            else if (facingDirection == previousFacingDirection)
            {
                rb.velocity = new Vector2(airMoveSpeedLimit * horizontalMove, rb.velocity.y);
            }
        }

        // Dash
        if (Time.time < dash_Countdown)
        {
            IsDashing = true;

            if (lastHM != 0 || lastVM != 0)
            {
                rb.velocity = new Vector2(lastHM * dash_Force, lastVM * dash_Force);
            }
            else
            {
                rb.velocity = new Vector2(facingDirection * dash_Force, 0F);
            }
            postDashTime = Time.time + postDashTimeSet;
        }
        else
        {
            IsDashing = false;
            canInstantiateGroundSmash = false;

            if (postDashTime > Time.time)
            {
                rb.velocity = new Vector2(5f * lastHM, 5f * lastVM);
            }
        }
    }
    
    private void CheckInput()
    {
        dashB.transform.position = new Vector2(rb.position.x - dashBarOffset.x, rb.position.y - dashBarOffset.y);

        if (!IsDashing)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal");
            verticalMove = Input.GetAxisRaw("Vertical");

            // Crouch
            if (verticalMove == -1 && isGrounded || platf.isOnPlatform && verticalMove == -1)
            {
                isCrouching = true;
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            else
            {
                isCrouching = false;
            }

            // If the player can jump he is grounded so set ground speed 
            if (isGrounded || platf.isOnPlatform)
            {
                air_Dashs_Left = air_Dashs;
                currentVariableJumpHeight = variableJumpHeightMultiplier;
                justWallJumped = false;
            }
            // If the jump button is pressed call the Jump Function
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }

            if (Input.GetButtonUp("Jump"))
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * currentVariableJumpHeight);
                currentVariableJumpHeight = 1;
            }
        }

        //Dash Input
        //if (Input.GetButtonDown("Dash"))
        //{
        //    if (canDash)
        //    {
        //        if (air_Dashs_Left > 0)
        //          { 
        //            Dash();
        //            air_Dashs_Left -= 1;
        //        }
        //    }
        //}
        
        // Alternative Dash Input
        if (Input.GetButton("Dash") && hasReleasedDashButtonBetweenDashs)
        { 
            if (horizontalMove > 0)
            {
                lastHM = 1;
            }
            else if (horizontalMove < 0)
            {
                lastHM = -1;
            }
            else if (horizontalMove == 0)
            {
                lastHM = 0;
            }

            if (verticalMove > 0)
            {
                lastVM = 1;
            }
            else if (verticalMove < 0)
            {
                lastVM = -1;
            }
            else if (verticalMove == 0)
            {
                lastVM = 0;
            }

            if (canDash)
            {
                if (air_Dashs_Left > 0)
                {
                    if (dashButtonHoldTime <= 0)
                    {
                        Time.timeScale = dashTimeFreezeSpeed;
                        Time.fixedDeltaTime = Time.timeScale * .02f;
                        timeIsSlowedDash = true;
                        dashButtonHoldTime -= Time.deltaTime;
                        //Arrow Fade in Animation
                        arrowShouldFadeIn = true;
                        if (!isAboutToDash)
                        {
                            isAboutToDash = true;
                        }

                    }
                    else {
                        dashButtonHoldTime -= Time.deltaTime;
                        if (!isAboutToDash)
                        {
                            isAboutToDash = true;
                        }
                    }
                }
            }
        }
        
        if ((Input.GetButtonUp("Dash") || dashTimeFreezeTime <= 0f) && isAboutToDash)
        {
            Dash();
            isAboutToDash = false;
            arrowShouldFadeIn = false;
        }

        if (Input.GetButtonUp("Dash") && !hasReleasedDashButtonBetweenDashs)
        {
            hasReleasedDashButtonBetweenDashs = true;
        }

        if (timeIsSlowedDash)
        {
            dashTimeFreezeTime -= Time.deltaTime;
        }

        // If the input is moving the player right and the player is facing left...
        if (horizontalMove > 0 && m_FacingLeft)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (horizontalMove < 0 && !m_FacingLeft)
        {
            // ... flip the player.
            Flip();
        }
    }

    private void AnimationHandler()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("IsCrouching", isCrouching);
        anim.SetBool("isGrounded", isGrounded || platf.isOnPlatform);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("IsDashing", IsDashing);
        anim.SetBool("IsWallSliding", isWallSliding);
    }

    private void Jump()
    {
        // Regular Jump
        if (isGrounded || platf.isOnPlatform && verticalMove != -1)
        {
            rb.velocity = new Vector2(rb.velocity.x, jump_Force);
            jumpsLeft--;
        }
        // Wall Jump
        else if (isTouchingWall && !isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            isWallSliding = false;
            Vector2 forceToAdd = new Vector2(wallHopForce * wallHopDirection.x * -facingDirection, wallHopForce * wallHopDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            canFlip = true;
            Flip();
            justWallJumped = true;
        }
        else if (!isGrounded && !platf.isOnPlatform && !isWallSliding)
        {
            if (jumpsLeft > 1)
            {   
                rb.velocity = new Vector2(rb.velocity.x, jump_Force * .85f);
                jumpsLeft--;
            }
        }
    }

    private void Dash() 
    {
        timeIsSlowedDash = false;
        canInstantiateGroundSmash = true;
        dashTimeFreezeTime = maxDashTimeFreezeTime;
        air_Dashs_Left -= 1;
        dashButtonHoldTime = dashButtonHoldTimeSet;
        Time.timeScale = defaultTimeSpeed;
        Time.fixedDeltaTime = 0.02f;
        dash_Countdown = Time.time + dash_Time;
        dashBarValue = 0f;
        intermidDashVar = Time.time + timeBetweenDashs;
        hasReleasedDashButtonBetweenDashs = false;
    }

    private void Flip()
    {
        if (canFlip)
        {
            facingDirection *= -1;
            facingDirectionWaitTime = facingDirectionWaitTimeSet;
            // Switch the way the player is labelled as facing.
            m_FacingLeft = !m_FacingLeft;

            // Multiply the player's x local scale by -1.
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(trapDamage);
        }

        if (collision.gameObject.CompareTag("PowerUp"))
        {
            if (collision.gameObject.name == "DashGem" && air_Dashs_Left != air_Dashs)
            {
                collision.gameObject.GetComponent<Renderer>().enabled = false;
                collision.gameObject.GetComponent<Collider2D>().enabled = false;
                air_Dashs_Left += 1;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Take Damage
        playerCurrentHealth -= damage;

        // KnockBack

        // Die
        if (playerCurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        rb.position = spawnPoint.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GroundCheck.position, GroundedRadius);

        Gizmos.DrawLine(WallCheck.position, new Vector3(WallCheck.position.x + playerWallCheckDistance, WallCheck.position.y, WallCheck.position.z));
        //Gizmos.DrawLine(LedgeCheck.position, new Vector3(LedgeCheck.position.x + playerWallCheckDistance, LedgeCheck.position.y, LedgeCheck.position.z));
    }
}