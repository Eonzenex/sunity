using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    #region Structs
    public struct GlobalMovementStruct
    {
        // Gravity force per delta time
        public float Gravity;

        // Reverse of gravity direction. This ensures consistence when dealing with other aspects of Unity
        public Vector3 ReverseGravityDirection;

        // Whether or not to rotate the character to the current gravity
        public bool RotateCharacterToGravity;

        // Maximum speed when launching or falling
        public float TerminalVelocity;

        public GlobalMovementStruct(Vector3 reverseGravityDirection, float terminalVelocity, float gravity = -9.81f, 
            bool rotateCharacterToGravity = false)
        {
            Gravity = gravity;
            ReverseGravityDirection = reverseGravityDirection;
            RotateCharacterToGravity = rotateCharacterToGravity;
            TerminalVelocity = terminalVelocity;
        }
    }
    public struct BasicMovementStruct
    {
        // How much control when changing directions
        public float GroundFriction;

        // Maximum speed while moving
        public float MaxSpeed;

        // Maximum acceleration while moving
        public float Acceleration;

        // Maximum deceleration while moving
        public float BrakingDeceleration;

        // Friction applied when acceleration == 0 or exceeding max speed
        public float BrakingFriction;

        // Optional friction factor when calculating actual friction
        public float BrakingFrictionFactor;

        public BasicMovementStruct(float groundFriction, float maxSpeed, float acceleration, 
            float brakingDeceleration, float brakingFriction, float brakingFrictionFactor)
        {
            GroundFriction = groundFriction;
            MaxSpeed = maxSpeed;
            Acceleration = acceleration;
            BrakingDeceleration = brakingDeceleration;
            BrakingFriction = brakingFriction;
            BrakingFrictionFactor = brakingFrictionFactor;
        }
    }
    public struct JumpMovementStruct
    {
        public float Force;
        public int MaxNumberOfJumps;

        public JumpMovementStruct(float force, int maxNumberOfJumps)
        {
            Force = force;
            MaxNumberOfJumps = maxNumberOfJumps;
        }
    }
    #endregion
    
    #region Enums
    public enum MovementModeEnum
    {
        Grounded,
        Falling
    }
    public enum GaitEnum
    {
        Walk,
        Run
    }
    public enum StanceEnum
    {
        Crouched,
        Standing
    }
    #endregion

    #region Variables
    [Header("External References")]
    public CharacterController controller;
    public Transform aimLocation;
    public Transform groundCheck;

    [Header("Movement Structs")]
    public GlobalMovementStruct GlobalMovement;
    public BasicMovementStruct BasicMovement;
    public JumpMovementStruct JumpMovement;

    [Header("Movement Enums")]
    public MovementModeEnum MovementMode;

    [Header("Inputs")]
    public Vector3 lastMoveInput;

    [Header("Jump")]
    public int currentJumps;
    public bool pendingJump;

    [Header("Velocity")]
    public Vector3 oldVelocity;
    public Vector3 velocity;
    public Vector3 newVelocity;
    public Vector3 acceleration;
    
    [Header("Ground Check")]
    public float groundCheckRadius;
    public LayerMask groundCollisionMask;
    public bool isGrounded;
    
    #region Readonly
    private float _totalGravity;
    #endregion
    
    #endregion

    // Constructor
    public TestMove()
    {
        GlobalMovement = new GlobalMovementStruct(Vector3.up, 20f, -9.81f, false);
        BasicMovement = new BasicMovementStruct(8f, 50f, 5f, 2.5f, 8f, 1f);
        JumpMovement = new JumpMovementStruct(10f, 2);

        MovementMode = MovementModeEnum.Falling;

        currentJumps = JumpMovement.MaxNumberOfJumps;
        pendingJump = false;
        
        oldVelocity = Vector3.zero;
        velocity = Vector3.zero;
        newVelocity = Vector3.zero;
        acceleration = Vector3.zero;
        
        groundCheckRadius = 0.4f;
        isGrounded = false;
    }
    
    #region Utils
    public bool IsGrounded()
    {
        return MovementMode == MovementModeEnum.Grounded;
    }
    public bool IsFalling()
    {
        return MovementMode == MovementModeEnum.Falling;
    }
    public void UpdateVelocities()
    {
        oldVelocity = velocity;
        velocity = newVelocity;
        newVelocity = Vector3.zero;
    }
    public void UpdateAcceleration()
    {
        acceleration = newVelocity - oldVelocity;
    }
    #endregion

    #region Messages
    private void OnLanded()
    {
        isGrounded = true;
        currentJumps = JumpMovement.MaxNumberOfJumps;
    }
    private void OnFalling()
    {
        isGrounded = false;
    }
    private void OnJumpInput()
    {
        if (currentJumps > 0)
        {
            pendingJump = true;
            Vector3 jumpVec = GlobalMovement.ReverseGravityDirection * JumpMovement.Force;
            newVelocity += jumpVec;
            currentJumps -= 1;
        }
    }
    private void OnMoveInput(Vector3 moveInput)
    {
        lastMoveInput = moveInput;
    }
    #endregion
    
    private void GroundCheck()
    {
        bool newIsGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundCollisionMask);
        if (newIsGrounded != isGrounded)
        {
            if (newIsGrounded)
            {
                BroadcastMessage("OnLanded");
            }
            else {
                BroadcastMessage("OnFalling");
            }
        }
    }
    
    private void CalcGravity()
    {
        if (pendingJump)
        {
            _totalGravity = JumpMovement.Force;
            pendingJump = false;
        }
        else {
            if (isGrounded)
            {
                _totalGravity = -2f;
            }
            _totalGravity += GlobalMovement.Gravity * Time.deltaTime;
        }
        controller.Move(GlobalMovement.ReverseGravityDirection * (_totalGravity * Time.deltaTime));
    }

    private void Movement()
    {
        if (lastMoveInput.magnitude < 0.1f) { return; }
        
        // // x then z due to Unity's different coord layout
        float rotTargetAngle = Mathf.Atan2(lastMoveInput.x, lastMoveInput.z) * Mathf.Rad2Deg + aimLocation.eulerAngles.y;
        Vector3 moveDirection = Quaternion.Euler(0f, rotTargetAngle, 0f) * Vector3.forward;

        Vector3 basicMovement = BasicMovement.Acceleration * Time.deltaTime * moveDirection.normalized;
        controller.Move(basicMovement);
        newVelocity += basicMovement;
    }
    
    void Update()
    {
        GroundCheck();
        CalcGravity();
        Movement();
        
        UpdateVelocities();
        UpdateAcceleration();
    }
}
