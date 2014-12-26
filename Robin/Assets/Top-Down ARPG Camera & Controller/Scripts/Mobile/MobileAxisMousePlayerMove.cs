using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Axis Mouse Player Move.
/// Created By: Juandre Swart 
/// Email: info@3dforge.co.za
/// 
/// This script controls the player movement. Use the left joystick to move forward, back, strafe left and strafe right.
/// Use the right joystick to turn player left and right.
/// </summary>

// This script requires a character controller to be attached
[RequireComponent (typeof (CharacterController))]
public class MobileAxisMousePlayerMove : MonoBehaviour {

	public float moveSpeed = 6.0f; // Character movement speed.
	public int rotationSpeed = 100; // How quick the character rotate to target location.
	public float gravity = 20.0f; // Gravity for the character.
	public float jumpSpeed = 8.0f; // The Jump speed
	
	public MyJoystick myJoystickLeft; // The mobile joystick
	public MyJoystick myJoystickRight; // The mobile joystick
	public MobileARPGBaseCameraController mobileCameraController;
	public GUITexture jumpButton;
	
	private Camera myCamera;
	private Transform myTransform;	
	private CharacterController controller;
	private Animator animator; // The animator for the toon. 
	
	private Vector3 moveDirection = Vector3.zero; // The move direction of the player.
	
	void Start()
	{			
		myCamera = Camera.main; // Get main camera as the camera will not always be a child GameObject.
		if(myCamera == null)
		{
			Debug.LogError("No main camera, please add camera or set camera to MainCamera in the tag option.");
		}
		myTransform = transform;		
		// Get the player character controller.
		controller = myTransform.GetComponent<CharacterController>();			

		// Get the player animator in child.
		try
		{
			animator = myTransform.GetComponentInChildren<Animator>();						
		}
		catch(Exception e)
		{
			Debug.LogWarning("No animator attached to character." + e.Message);
		}			
	}	
	
	public void Update()
	{			
		
		float xMovement = myJoystickLeft.position.x;// The horizontal movement
		float zMovement = myJoystickLeft.position.y;// The vertical movement	
		
		// Are whe grounded, yes then move.
		if (IsGrounded()) {
			
			// Move player the same distance in each direction. Player must move in a circular motion.
			float tempAngle = Mathf.Atan2(zMovement,xMovement);
			xMovement *= Mathf.Abs(Mathf.Cos(tempAngle));
			zMovement *= Mathf.Abs(Mathf.Sin(tempAngle));		
			
	       	moveDirection = new Vector3(xMovement, 0, zMovement);
	       	moveDirection = myTransform.TransformDirection(moveDirection);
	       	moveDirection *= moveSpeed;
			
			// Make the player jump.
	       	int touchCount = Input.touchCount;
			for(int i = 0 ; i < touchCount; i++)
			{
				
				// Make the player jump.
		    	if (jumpButton != null && jumpButton.HitTest(Input.GetTouch(i).position))
			   	{
		    		moveDirection.y = jumpSpeed;				
					// TODO Add jump animation.
			   	} 
			}   
       	}
		
		// Apply gravity.
		moveDirection.y -= gravity * Time.deltaTime;        		
		
		//Rotate player based on mouse movement
		myTransform.Rotate(0, myJoystickRight.position.x * rotationSpeed * Time.deltaTime, 0);
		
		// Are we moving.
		// TODO add own animation for movement.
		if(animator != null)
		{
			animateCharacter(xMovement, zMovement);
		}
		
		controller.Move(moveDirection * Time.deltaTime);					
	}
	
	// Check if the player is grounded.
	bool IsGrounded () {
		return controller.isGrounded;
	}
	
	private float prevStrafe = 0f;
	/// <summary>
	/// Animates the character using Animator.
	/// Here you can set your animation variable.
	/// </summary>
	/// <param name='xMovement'>
	/// X axis movement.
	/// </param>
	/// <param name='zMovement'>
	/// Z axis movement.
	/// </param>
	private void animateCharacter(float xMovement, float zMovement)
	{
		animator.SetFloat("speed", zMovement);
		
		xMovement = Mathf.Abs(xMovement) < 0.5 ? 0 : xMovement;
		zMovement = Mathf.Abs(zMovement) < 0.5 ? 0 : zMovement;
		
		if(zMovement > 0 && xMovement !=0)
		{
			prevStrafe = (xMovement<0  ? -0.25f : 0.5f);
			animator.SetFloat("strafe", prevStrafe);			
		}
		else
		{
			
			if(zMovement == 0 && xMovement ==0)
			{
				prevStrafe = 0;
				mobileCameraController.pinchZoom = true;
			}
			else
			{
				mobileCameraController.pinchZoom = false;
			}			
			
			if(prevStrafe == 0 || prevStrafe == 1 || prevStrafe == -1  )
			{
				prevStrafe = 0;
				animator.SetFloat("strafe", xMovement);				
			}
			else
			{
				if(xMovement == 0)
				{
					if(prevStrafe<0)
					{
						prevStrafe += 0.05f;
						prevStrafe = prevStrafe > 0 ? 0 : prevStrafe;
					}
					else if(prevStrafe >0)
					{
						prevStrafe -= 0.05f;
						prevStrafe = prevStrafe < 0 ? 0 : prevStrafe;
					}
					
				}
				else
				{
					if(prevStrafe<0)
					{
						prevStrafe -= 0.05f;
						prevStrafe = prevStrafe<-1 ? -1 : prevStrafe;
					}
					else if(prevStrafe >0)
					{
						prevStrafe += 0.05f;
						prevStrafe = prevStrafe>1 ? 1 : prevStrafe;
					}
				}

				animator.SetFloat("strafe", prevStrafe);
				
			}
		}
	}
}
