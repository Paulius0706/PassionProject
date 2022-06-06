using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maxSpeed;
    public float stepPower;
    public float acc;
    public float decc;
    public float rotationSpeed;
    public float jumpheight;
    public Transform front;
    public ChunkGenerator chunkGenerator;

    public ItemHolder itemHolder;
    public MoveTrigger moveTrigger;
    public MoveTrigger stepAssistFront;
    public MoveTrigger stepAssistBack;
    public MoveTrigger stepAssistLeft;
    public MoveTrigger stepAssistRight;
    public MoveTrigger stepAssistFrontUP;
    public MoveTrigger stepAssistBackUP;
    public MoveTrigger stepAssistLeftUP;
    public MoveTrigger stepAssistRightUP;

    private Rigidbody rb;
    private bool stepAssistState;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void Update()
    {
        rb.angularVelocity = Vector3.zero;
        Movement();
        StepAssist();
        Jumping();
        Rotation();
        
    }
    private void Jumping()
    {
        Vector3Int roundPosition = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y + 0.4f), Mathf.RoundToInt(transform.position.z));
        if (moveTrigger.contactToObjects && Input.GetKeyDown(KeyCode.Space))
        {

            Vector3 velocity = rb.velocity;
            velocity.y += Mathf.MoveTowards(velocity.y, jumpheight, jumpheight);
            rb.velocity = velocity;
        }
    }
    private void StepAssist()
    {
        Vector3Int roundPosition = new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y - 0.4f), Mathf.RoundToInt(transform.position.z));
        int x = (int)Input.GetAxisRaw("Horizontal");
        int z = (int)Input.GetAxisRaw("Vertical");
        
        if(    (x ==  1 && stepAssistRight.contactToObjects && !stepAssistRightUP.contactToObjects) 
            || (x == -1 && stepAssistLeft .contactToObjects && !stepAssistLeftUP .contactToObjects)
            || (z ==  1 && stepAssistFront.contactToObjects && !stepAssistFrontUP.contactToObjects)
            || (z == -1 && stepAssistBack .contactToObjects && !stepAssistBackUP .contactToObjects))
        {
            rb.AddForce(Vector3.up*(2.5f-rb.velocity.y), ForceMode.VelocityChange);
            stepAssistState = true;
        }
        else
        {
            stepAssistState = false;
        }


    }

    public void Pulse()
    {
        Vector3 velocity = (front.position - transform.position).normalized * stepPower;
        velocity.y = rb.velocity.y;
        rb.velocity = velocity;
    }
    private void Movement()
    {
        // Inputs and movement validation
        Vector3 inputs = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        
        //movement velocity save 
        float fallSpeed = rb.velocity.y;
        Vector3 velocity = rb.velocity;
        velocity.y = 0;

        //movement acceleration and deceleration
        // sword to inactivation of itemHolder
        if ((moveTrigger.contactToObjects || stepAssistState) && !itemHolder.itemActivationMoveStop) velocity = Vector3.MoveTowards(velocity, inputs * maxSpeed, acc * Time.deltaTime);

        //movement deaceleration
        Vector3 deccTarget = velocity;
        deccTarget.x = Mathf.Abs(inputs.x) == 0 ? 0 : deccTarget.x;
        deccTarget.z = Mathf.Abs(inputs.z) == 0 ? 0 : deccTarget.z;
        velocity = Vector3.MoveTowards(velocity, deccTarget, decc * Time.deltaTime);

        //movement & decceleration confirmation
        velocity.y = fallSpeed;
        rb.velocity = velocity;
    }
    private void Movement1()
    {
        // input to velocity
        Vector3 moveTarget = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * maxSpeed;
        if ((moveTrigger.contactToObjects || stepAssistState) && !itemHolder.itemActivationMoveStop)
        {
            rb.AddForce(Vector3.MoveTowards(Vector3.zero, moveTarget - rb.velocity + Vector3.up*rb.velocity.y, acc * Time.deltaTime),ForceMode.VelocityChange);
        }


        // set deceleration
        Vector3 deccTarget = (- rb.velocity + Vector3.up * rb.velocity.y).normalized*maxSpeed;
        deccTarget.x = Input.GetAxisRaw("Horizontal") == 0 ? deccTarget.x : 0;
        deccTarget.z = Input.GetAxisRaw("Vertical"  ) == 0 ? deccTarget.z : 0;
        rb.AddForce(Vector3.MoveTowards(Vector3.zero, deccTarget - rb.velocity + Vector3.up * rb.velocity.y, decc * Time.deltaTime), ForceMode.VelocityChange);


    }
    private void Rotation()
    {
        //rotation
        Vector2 mouseScreenPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 playerScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

        Vector2 tan = (mouseScreenPosition - playerScreenPosition).normalized;
        float tmp = Mathf.Asin(Mathf.Abs(tan.y)) * Mathf.Rad2Deg;
        float mouseAngle = 90 - tmp;                                    // ^>
        mouseAngle = (tan.y < 0 && tan.x > 0) ? 90 + tmp : mouseAngle; // v>
        mouseAngle = (tan.y < 0 && tan.x < 0) ? 270 - tmp : mouseAngle; // v<
        mouseAngle = (tan.y > 0 && tan.x < 0) ? 270 + tmp : mouseAngle; // ^<

        Vector3 tan1 = (front.position - transform.position).normalized;
        tan = new Vector2(tan1.x, tan1.z);
        tmp = Mathf.Asin(Mathf.Abs(tan.y)) * Mathf.Rad2Deg;
        float playerAngle = 90 - tmp;                                     // ^>
        playerAngle = (tan.y < 0 && tan.x > 0) ? 90 + tmp : playerAngle; // v>
        playerAngle = (tan.y < 0 && tan.x < 0) ? 270 - tmp : playerAngle; // v<
        playerAngle = (tan.y > 0 && tan.x < 0) ? 270 + tmp : playerAngle; // ^<


        float angle = Mathf.MoveTowards(playerAngle, mouseAngle, rotationSpeed * Time.deltaTime);
        if (Mathf.Abs(mouseAngle - playerAngle) >= 180)
        {
            playerAngle = playerAngle > 180 ? -360 + playerAngle : playerAngle;
            mouseAngle = mouseAngle > 180 ? -360 + mouseAngle : mouseAngle;
            angle = Mathf.MoveTowards(playerAngle, mouseAngle, rotationSpeed * Time.deltaTime);
        }
        rb.MoveRotation(Quaternion.Euler(0, angle, 0));
    }
}
