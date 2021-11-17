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
    public float minSpeed = 10f;
    public float maxSpeed = 30f;
    public float invincibleTime;
    public int maxLife = 3;

    // For blinking using other shaders that don't have _BlinkingValue *
    // * Uncomment next line
    // public GameObject model;
    // * Pass "CatMesh" object to "Model" Player's property (from Unity interface) 


    private Animator anim;
    private Rigidbody rb;
    private BoxCollider boxCollider;

    private int currentLane = 1;
    private int currentLife;
    static int blinkingValue;
    private float jumpStart;
    private float slideStart;

    private bool isJumping = false;
    private bool isSliding = false;
    private bool isSwiping = false;
    private bool invincible = false;


    private Vector3 verticalTargetPosition;
    private Vector3 boxColliderSize;
    private Vector2  startingTouch;

    private UIManager  uiManager;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        boxColliderSize = boxCollider.size;
        anim.Play("runStart");
        currentLife = maxLife;
        speed = minSpeed;
        blinkingValue = Shader.PropertyToID("_BlinkingValue");
        uiManager = FindObjectOfType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {

        if (isJumping)
        {
            float ratio = (transform.position.z - jumpStart) / jumpLength;
            if (ratio >= 1f)
            {
                isJumping = false;
                anim.SetBool("Jumping", false);
                anim.Play("runStart");
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

            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                startingTouch = Input.GetTouch(0).position;
                isSwiping = true;
            } else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                isSwiping = false;
            }
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

        if (isSliding)
        {
            float ratio = (transform.position.z - slideStart) / slideLength;
            if (ratio >= 1f) {
                isSliding = false;
                anim.SetBool("Sliding", false);
                boxCollider.size = boxColliderSize;
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
        if (!isJumping && !isSliding)
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

    private void OnTriggerEnter(Collider other)
    {
        if (invincible)
            return;
             
        if (other.CompareTag("Obstacle"))
        {
            currentLife--;
            uiManager.UpdateLives(currentLife);
            anim.SetTrigger("Hit");
            speed = 0;
            if (currentLife <= 0)
            {
                // Game Over
            }
            else
            {
                StartCoroutine(Blinking(invincibleTime));
            }
        }
    }

    IEnumerator Blinking(float time)
    {
        invincible = true;
        float timer = 0;
        float currentBlink = 1f;
        float lastBlink = 0;
        float blinkPeriod = 0.1f;

        // * Comment this line in
        // bool enabled = false;

        yield return new WaitForSeconds(1f);
        speed = minSpeed;
        while (timer < time && invincible)
        {
            // * Uncomment next line
            // LightmapsModeLegacy.SetActive(enabled);

            // * Comment next line out
             Shader.SetGlobalFloat(blinkingValue, currentBlink);
             yield return null;
             timer += Time.deltaTime;
             lastBlink += Time.deltaTime;
             if (blinkPeriod < lastBlink)
             {
                 lastBlink = 0;
                 currentBlink = 1f - currentBlink;
                // * Uncomment next line
                //  enabled = !enabled;
             }
        }
        // * Uncomment next line
        // model.SetActive(true);

        // * Comment next line out
        Shader.SetGlobalFloat(blinkingValue, 0);
        invincible = false;
    }
}
