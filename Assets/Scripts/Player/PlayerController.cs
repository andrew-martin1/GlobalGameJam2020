﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    private Rigidbody2D rb;
    private SfxPlayer sfxPlayer;
    private float horiVelocity = 0.0f;  // Horizontal Velocity. Set by player movement.
    public Animator animator;
    GameObject[] pickups;

    public float speed;
    public float jumpHeight;
    public float maxFallSpeed;
    private bool hasntJumped;

    private bool hasntJumpedInAir;
    public bool isActive;
    private bool onWall;
    private int jumps;
    private bool isDead;

    public static Vector2 checkpointPos = new Vector2(0, 0);

    public float fallMultiplier;
    public float lowJumpMultipler;
    private float lowJumpMultiplierOriginal;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>() as Rigidbody2D;
        jumps = 2;
        lowJumpMultiplierOriginal = lowJumpMultipler;
        hasntJumpedInAir = true;
        hasntJumped = true;
        isActive = true;
        sfxPlayer = GetComponent<SfxPlayer>() as SfxPlayer;
        pickups = GameObject.FindGameObjectsWithTag("Pickup");
        this.gameObject.transform.position = checkpointPos;
        isDead = false;
    }

    void Update()
    {
        //print(checkpointPos);

        animator.SetFloat("VertSpeed", rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxisRaw("Horizontal")));
        animator.SetBool("Jumped Once", hasntJumped);
        animator.SetBool("Double Jumped", hasntJumpedInAir);
        animator.SetInteger("Jumps", jumps);
        horiVelocity = Input.GetAxis("Horizontal");

        //Un-jumping code
        if (Input.GetKeyUp(KeyCode.Space) && isActive)
        {
            if (jumps > 0)
            {
                hasntJumpedInAir = true;
            }
        }

        //Jumping code
        if (Input.GetKeyDown(KeyCode.Space) && isActive)
        {
            if (hasntJumped)
            {
                //Grounded / saved jump
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                hasntJumped = false;
                sfxPlayer.PlaySFX("jump");
            }
            else if (hasntJumpedInAir)
            {
                //Double Jump
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
                hasntJumpedInAir = false;
                --jumps;
                sfxPlayer.PlaySFX("jump");
				sfxPlayer.PlaySFX("eyesmash");
                animator.SetInteger("Jumps", jumps);
            }
            else
            {
                if (jumps == 0)
                {
                    //animator.SetBool("Should Die", true);
                    //very slow fall
                    isActive = false;
                    lowJumpMultipler = 0.3f;
                    Explode();
                }
            }
        }
    }



    void FixedUpdate()
    {
        rb.velocity = new Vector2(horiVelocity * speed * (isActive ? 1 : 0), rb.velocity.y);

        //Jumping physics code
        if (rb.velocity.y < 0 || onWall)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1) * Time.deltaTime;
        }

        //ON WALL CODE
        if (onWall)
        {
            rb.AddForce(Vector3.down);
        }


        //Max fall speed limiter
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }

        //Debug for status of jump & double jump booleans
        //print("Jumps: " + jumps + " IsGrounded(): " + IsGrounded() +  " Hasn't Jumped: " + hasntJumped + " Hasn't Jumped In Air: " + hasntJumpedInAir);

    }

    public void AddJump()
    {
        jumps = 2;
    }

    private bool IsGrounded()
    {
        //checks if anything is below the character using a Circle Cast.
        return Physics2D.CircleCast(gameObject.transform.position, 0.18f, Vector2.down, 1f, 9, 0, 0);

        /* Debug line drawing tools:
         * 
         * Debug.DrawLine(gameObject.transform.position,
            new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 0.7f));
            Debug.DrawLine(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y - 1f), new Vector3(gameObject.transform.position.x + 0.18f, gameObject.transform.position.y - 1f));
        */
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        //this code should be moved to the jump function for optomization
        if (IsGrounded())
        {
            hasntJumped = true;
            onWall = false;
            if (pickups.Length > 0)
            {
                foreach (GameObject p in pickups)
                {
                    p.SetActive(true);
                }
            }
        }
        else
        {
            onWall = true;
            hasntJumped = false;
        }
        
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "DeathTrigger")
        {
            if (!isDead)
            {
                isDead = true;
                Explode();
            }
        }
        else if (other.gameObject.tag == "CheckpointTrigger")
        {
            checkpointPos.x = this.gameObject.transform.position.x;
            checkpointPos.y = this.gameObject.transform.position.y;
        }
    }

    Coroutine c;
    void Explode()
    {
        //To validate animator boolean in order to play death animation
        jumps = 0;
        animator.SetBool("Need to Die", true);

        Camera.main.GetComponent<CameraController>().ZoomIn();
        //SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
        c = StartCoroutine(ExplodeCoroutine());
		sfxPlayer.PlaySFX("death-explode");
		
		MusicPlayer music = FindObjectOfType<MusicPlayer>();
		music.DeathPitch();
    }

    public void StopExplode()
    {
        if (c != null)
        {
            StopCoroutine(c);
            //animator.SetBool("Should Die", false);
            animator.SetBool("Need to Die", false);
            //stop very slow fall
            isActive = true;
            lowJumpMultipler = lowJumpMultiplierOriginal;
            jumps = 2;
            Camera.main.GetComponent<CameraController>().ZoomOut();
        }
    }

    IEnumerator ExplodeCoroutine()
    {
        yield return new WaitForSeconds(22 / 60.0f);
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }


}