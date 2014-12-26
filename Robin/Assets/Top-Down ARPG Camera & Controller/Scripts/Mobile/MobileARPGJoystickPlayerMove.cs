using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Axis Mouse Player Move.
/// Created By: Juandre Swart 
/// Email: info@3dforge.co.za
/// 
/// This script controls the player movement. Use the joystick to move.
/// </summary>

// This script requires a character controller to be attached
[RequireComponent (typeof (CharacterController))]
public class MobileARPGJoystickPlayerMove : MonoBehaviour {

	public float moveSpeed = 6.0f; // Character movement speed.
	public int rotationSpeed = 8; // How quick the character rotate to target location.	
	public float gravity = 20.0f; // Gravity for the character.
	public float jumpSpeed = 8.0f; // The Jump speed
	public Transform myRotationObject; // The object we will be rotating when moving
	public MyJoystick myJoystick; // The mobile joystick
	public MobileARPGBaseCameraController mobileCameraController;
	public GUITexture jumpButton;
	
	private Camera myCamera;
	private Transform myTransform;	
	private CharacterController controller;
	private Animator animator; // The animator for the toon. 
	
	private Vector3 moveDirection = Vector3.zero;
	
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
		
		// Reset player rotation to look in the same direction as the camera.
		Quaternion tempRotation = myCamera.transform.rotation;
		tempRotation.x = 0;
		tempRotation.z = 0;		
		myTransform.rotation = tempRotation;
		
		float xMovement = myJoystick.position.x;// The horizontal movement
		float zMovement = myJoystick.position.y;// The vertical movement				
		//Are whe grounded, yes then move.
		if (IsGrounded()) {
	       //Move player the same distance in each direction. Player must move in a circular motion.
			float tempAngle = Mathf.Atan2(zMovement,xMovement);
			xMovement *= Mathf.Abs(Mathf.Cos(tempAngle));
			zMovement *= Mathf.Abs(Mathf.Sin(tempAngle));
			
	       	moveDirection = new Vector3(xMovement, 0, zMovement);
	    	moveDirection = myTransform.TransformDirection(moveDirection);
	    	moveDirection *= moveSpeed;
	       			
			int touchCount = Input.touchCount;
			bool doJump = false;
			for(int i = 0 ; i < touchCount; i++)
			{
				// Make the player jump.
		    	if (jumpButton != null && jumpButton.HitTest(Input.GetTouch(i).position))
			   	{
					doJump = true;
					break;
			   	} 
			}
			
			if(doJump)
			{
				moveDirection.y = jumpSpeed;	
			}
			else
			{
				
			}
			
            
       	}
		
		//Apply gravity
		moveDirection.y -= gravity * Time.deltaTime;
        			
		// Are we moving.
		if(moveDirection.x == 0 && moveDirection.z == 0)
		{
			animator.SetBool("run", false);			
			mobileCameraController.pinchZoom = true;
		}
		else
		{
			mobileCameraController.pinchZoom = false;
			// Make rotation object(The child object that contains animation) rotate to direction we are moving in.
			Vector3 temp = myTransform.position;
			temp.x += xMovement;
			temp.z += zMovement;		
			myRotationObject.localRotation = Quaternion.Slerp(myRotationObject.localRotation, Quaternion.LookRotation(temp-myTransform.position), rotationSpeed * Time.deltaTime);	
			if(animator!=null)
			{
				animator.SetBool("run", true);
			}
		}
			
		controller.Move(moveDirection * Time.deltaTime);					
	}
	
	// Check if the player is grounded.
	bool IsGrounded () {
		bool temp = controller.isGrounded;
		
		return temp;
	}	
	
}
