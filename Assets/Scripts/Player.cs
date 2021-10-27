using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float laneSpeed;
    public float jumpLength;
    public float jumpHeight;
    public float slideLength;


    private Animator anim;
    private Rigidbody rb;
    private BoxCollider boxCollider;

    private int currentLane = 1;
    private float jumpStart;
    private float slideStart;

    private bool isJumping = false;
    private bool isSliding = false;
    private bool isSwiping = false;

    private Vector3 verticalTargetPosition;
    private Vector3 boxColliderSize;
    private Vector2  startingTouch;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        boxColliderSize = boxCollider.size;
        anim.Play("runStart");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            ChangeLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {

            Slide();
        }

        if (Input.touchCount == 1)
        {
            if (isSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - startingTouch;
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);
                if (diff.magnitude > 0.01f)
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        if (diff.y < 0)
                        {
                            Slide();
                        }
                        else
                        {
                            Jump();
                        }
                    }
                    else
                    {
                        if (diff.x < 0)
                        {
                            ChangeLane(-1);
                        }
                        else
                        {
                        ChangeLane(1); 
                        }
                    }

                    isSwiping = false;
                }
            }
        }

        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startingTouch = Input.GetTouch(0).position;
            isSwiping = true;
        }
        else if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            isSwiping = false;
        }

        if (isJumping)
        {
            float ratio = (transform.position.z - jumpStart) / jumpLength;
            if (ratio >= 1f)
            {
                isJumping = false;
                anim.SetBool("Jumping", false);
            }
            else
            {
                verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
            }
        }
        else
        {
            verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, 5 * Time.deltaTime);
        }

        if (isSliding)
        {
            float ratio = (transform.position.z - slideStart) / slideLength;
            if (ratio >= 1f) {
                isSliding = false;
                anim.SetBool("Sliding", false);
                boxCollider.size = boxColliderSize;
            }
        }

        Vector3 targetPosition = new Vector3(verticalTargetPosition.x, verticalTargetPosition.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, laneSpeed * Time.deltaTime);
    }

    void ChangeLane(int direction)
    {
        int targetLane = currentLane + direction;
        if (targetLane < 0 || targetLane > 2)
            return;
        currentLane = targetLane;
        verticalTargetPosition = new Vector3((currentLane - 1), 0, 0);
    }

    private void FixedUpdate()
    {
        rb.velocity = Vector3.forward * speed;
    }

    private void Jump()
    {
        if (!isJumping)
        {
            jumpStart = transform.position.z;
            anim.SetFloat("JumpSpeed", speed / jumpLength);
            anim.SetBool("Jumping", true);
            isJumping = true;
        }
    }

    private void Slide()
    {
        if (!isJumping && isSliding)
        {
            slideStart = transform.position.z;
            anim.SetFloat("JumpSpeed", speed / slideLength);
            anim.SetBool("Sliding", true);
            Vector2 newSize = boxCollider.size;
            newSize.y = newSize.y / 2 ;
            boxCollider.size = newSize;
            isSliding = true;
        }
    }
}
