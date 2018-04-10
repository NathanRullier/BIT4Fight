 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerScript : MonoBehaviour {

    //public float 
    public float maxSpeed = 10f;
    public float punchSpeed = 0f;
    public float acceleration = 1;
    public float deceleration = 2;
    public float drag = 1f;
    public float punch1Force = 15f;
    public float time;

    //Animator
    private Animator anim;

    //movement variables
    private bool rightArrowPressed = false;
    private bool leftArrowPressed = false;
    private bool rightArrowReleased = false;
    private bool leftArrowReleased = false;

    //CombatHitboxes variables
    Collider2D damaged;
    public Transform damageBox;
    private float damageBoxRadius;
    public LayerMask damageable;
    //Movement Variable
    private bool facingRight = true;
    private float speed = 0f;
    //State machine
    private const int nbrState=6;
    private enum State {Idle=0,Running=1,Punching1=2,Punching2 = 3, Punching3 = 4, Punching4 = 5};
    private State state;

    // Use this for initialization
    void Start () {

        time = 0;
        state = State.Idle;
        anim = GetComponent<Animator>();

	}
	
	// Update is called once per frame
	void FixedUpdate () {

        damaged = Physics2D.OverlapCircle(damageBox.position, damageBoxRadius, damageable);
        
        //Will be set to 0 each time the charcter punches
        time += Time.deltaTime;

        //Inputs
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);



        //Used to calculate the speed of the Character
        speed = Movement(right,left,speed);

        //Send the absolute Value of the speed
        anim.SetFloat("Speed", Mathf.Abs(speed));
       
        //To flip when going the opposite direction
        if(speed > 0 && !facingRight)
        {
            Flip();
        }
        else if(speed < 0 && facingRight)
        {
            Flip();
        }

        //sending the state to the animator
        anim.SetInteger("State", (int)state);
	}

    //Reverse the character on the y axis
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    //calculate the horizontal speed of the character
    private float Movement(bool right,bool left,float speed )
    {
        //Will Execute when the right arrow key is pressed
        if (right)
        {
            //Will execute when the caracter is Running toward the right or Idle
            //Makes the Character run toward the Right
            if(speed >= 0 && (state == State.Idle || state == State.Running))
            {
                speed += acceleration;
                state = State.Running;
            }
            //Will execute when the caracter is Running toward the left
            //The caracter will punch and loose speed
            else if(speed <= 0)
            {
                punch();
                speed = friction(speed);
            }

            //Will execute when the character is going to fast
            //The charcater is put to maxSpeed
            if (speed > maxSpeed)
            {
                speed = maxSpeed;
            }
        }

        //Will Execute when the left arrow key is pressed
        if (left)
        {
            //Will execute when the caracter is Running toward the left or Idle
            //Makes the Character run toward the left
            if (speed <= 0 && (state == State.Idle || state == State.Running))
            {
                speed -= acceleration;
                state = State.Running;
            }
            //Will execute when the caracter is Running toward the right
            //The caracter will punch and loose speed
            else if (speed >= 0)
            {
                punch();
                speed = friction(speed);
            }

            //Will execute when the character is going to fast
            //The charcater is put to -maxSpeed
            if (speed < -maxSpeed)
            {
                speed = -maxSpeed;
            }
        }
        //Will execute if nothing is pressed
        //The character will lose speed
        if (!left && !right)
        {
            speed = friction(speed);

            //Will execute if the character has no speed
            //The state of the caracter is put to Idle
            if(speed == 0){
                state = State.Idle;
            }
        }
        //Will put the speed as the horizontal velocity of the RigidBody2D

 
        GetComponent<Rigidbody2D>().velocity = getStateVelocity((int)state);
        
        return speed;
    }

    //Makes the character execute an Attack move
    private void punch()
    {
        //Set the variable to execute the right move
        bool canAttack = false;
        keyPressed();

        //looks if enough time as passed since the last attack to attack again
        if(time <= 1)
        {
            canAttack = leftArrowPressed || rightArrowPressed;
        }
        //Will execute when to much time passed since the last attack
        else{
            
            //Will execute if the character is not moving
            //Puts the state to Idle
            if (GetComponent<Rigidbody2D>().velocity == new Vector2(0, 0))
            {
                state = State.Idle;
            }
            //Will execute if the character is moving
            //Puts the state to Running
            else
            {
                state = State.Running;
            }
        }
        
        //Sets the right direction for the attacks
     
        //Executes the right attack in function of the state of the character
        if (state == State.Running)
        {
            if(damaged !=null)
            damaged.GetComponent<Rigidbody2D>().velocity = (new Vector2(1*getDirection(), 0));
            state = State.Punching1;
            time = 0;
        }
        else if (state == State.Punching1 && canAttack)
        {
            if (damaged != null)
                damaged.GetComponent<Rigidbody2D>().velocity = (new Vector2(2*getDirection(), 0));
            state = State.Punching2;
            time = 0;
        }
        else if (state == State.Punching2 && canAttack)
        {
            if (damaged != null)
                damaged.GetComponent<Rigidbody2D>().velocity = (new Vector2(1 * getDirection(), 5));
            state = State.Punching3;
            time = 0;
        }
        else if (state == State.Punching3 && canAttack)
        {
            if (damaged != null)
                damaged.GetComponent<Rigidbody2D>().velocity = (new Vector2(5 * getDirection(), -5));
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 200));
            state = State.Punching4;
            time = 0;
        }


    }

    //Slows the character to a stop
    private float friction(float speed)
    {
        //Will execute if the charater is going faster toward the right than the drag
        //Makes the character slower
        if (speed > drag)
        {
            speed -= drag;
        }
        //Will execute if the charater is going faster toward the left than the drag
        //Makes the character slower
        else if (speed < -drag)
        {
            speed += drag;
        }
        //Will execute if the charater is going slower than the drag
        //Makes the character stop
        else
        {
            speed = 0;
        }
        return speed;
    }

    //TO DO : put it in another class with general key
    //See if either of the arrow key as been pressed this update
    //Had issue with arrowPressed so replaced it
    private void keyPressed()
    {
        //3state only the first returns true
        //1 Key has just been pressed
        //2 Key is still pressed
        //3 Key has been released
        if (!leftArrowPressed && Input.GetKey(KeyCode.LeftArrow) && leftArrowReleased)
        {
            leftArrowPressed = true;
        }
        else if(leftArrowPressed)
        {
            leftArrowPressed = false;
            leftArrowReleased = false;
        }
        else if (!Input.GetKey(KeyCode.LeftArrow))
        {
            leftArrowReleased = true;
        }

        if (!rightArrowPressed && Input.GetKey(KeyCode.RightArrow) && rightArrowReleased)
        {
            rightArrowPressed = true;
        }
        else if (rightArrowPressed)
        {
            rightArrowPressed = false;
            rightArrowReleased = false;
        }
        else if (!Input.GetKey(KeyCode.RightArrow))
        {
            rightArrowReleased = true;
        }
    }

    //Return the direction of the caracter in function of the state
    public Vector2 getStateVelocity(int state)
    { Vector2 vector2 = new Vector2(0,0);
        int direction = getDirection();
        direction = getDirection();

        if (state == 0 || state == 1)
        {
            vector2 = new Vector2(speed, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if (state >= 2 && state <= 4)
        {
            vector2 = new Vector2(punch1Force * direction * (0.5f - time)*5, GetComponent<Rigidbody2D>().velocity.y);
        }
        else if (state == 5)
        {
            vector2 = new Vector2(punch1Force * direction * (1 - time)*5, GetComponent<Rigidbody2D>().velocity.y);
        }
        return vector2;
    }
    
    public int getState()
    {
        return (int)state;
    }
    //Return a multiplicator to have the vectors in the right x direction
    private int getDirection()
    {
        int direction = 0;
        if (facingRight)
        {
            direction = 1;
        }
        else if (!facingRight)
        {
            direction = -1;
        }
        return direction;
    }
}
