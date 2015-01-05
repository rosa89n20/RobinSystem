using UnityEngine;
using JrDevAssets;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.7
//GAC_TargetTracker.cs
/////////////////////////////////////////////////////////////////////////////////////////

public class GAC_TargetTracker : MonoBehaviour {

	public GAC gacSettings;
	public CharacterController movementController; //Character controller for movement
	public Rigidbody2D movementController2D; //Rigidbody controller for 2D movement
	public Animation animationController; //Animation component reference
	public Animator animatorController; //Animator component reference

	private GameObject thisObject;

	public List<string> storeAnimNames = new List<string>();//The list of all animation names for use in Editor
	public List<string> keepAnimsInSync = new List<string>();//Keep a list of updated animations being used for Animation or Animator Component
	public List<string> dummyStates = new List<string>(); //Keep a list of all the dummy states in Mecanim Animator

	public int gameModeIndex;
	public string[] gameModeNames = {"3D","2D"};//The names for game mode options

	public int directionIndex;
	public string[] directionScales = {"1", "-1"};//The names for directions of gameobject scale

	//Size and position of parameter
	public Vector3 parameterPos;
	public Vector2 parameterSize;

	public bool didHit; //Was this target hit
	public bool playDamage;
	public bool damageMovement;

	public int targetId; //The number set for each target that was added to tracker
	public bool showGizmo; //Shows the gizmo

	//These handle the toggling of features and for sliders and fields
	public bool animToggle;
	public float moveBegin;
	public bool moveToggle;
	public float moveEnd;
	public float moveAmount;
	public float moveTimer;
	public int currentAnimIndex;
	public string damageAnim;

	public float smoothFlipAmount; //The amount to adjust the position when flipping facing direction
	public bool isDraggingSmoothFlip;

	//The amount of vertices to use for each axis hit detection
	public int horizontalSensitivity;
	public int verticalSensitivity;
	public int forwardSensitivity;

	//These lists keep all the attributes for each vertice
	public List<Vector2> parameterVertices2D = new List<Vector2>();
	public List<Vector3> parameterVertices3D = new List<Vector3>();
	public List<float> parameterDistances = new List<float>();
	public List<float> parameterHeights = new List<float>();
	public List<float> parameterAngles = new List<float>();

	//Trigers for handling the facing directions
	public bool detectFacingDirection;
	public bool targetFacingDirection;
	public bool currentFacingDirection;
	public bool facingDirectionRight;
	public bool facingDirectionUp;


	public ControllerType conType;
	
	public enum ControllerType{
		Legacy,
		Mecanim
	}

	//Call when gameObject enabled
	void OnEnable () {
		thisObject = gameObject;

		GAC.gacObjects = FindObjectsOfType<GAC>().ToList();

		foreach(GAC gacSettings in GAC.gacObjects){

			if(!gacSettings.useRange){
				GAC.AddTargetGO(gacSettings.gameObject, thisObject);
			}

			//Check if 2D Mode index selected
			if(gacSettings.gameModeIndex == 0){
				movementController = GetComponent<CharacterController>();
			}else if(gacSettings.gameModeIndex == 1){
				movementController2D = thisObject.GetComponent<Rigidbody2D>();
			}

		}


		animationController = thisObject.GetComponent<Animation>();
		animatorController = thisObject.GetComponent<Animator>();
	}

	//Call when gameObject disabled
	void OnDisable (){

		foreach(GAC gacSettings in GAC.gacObjects){

			if(this.enabled && gacSettings != null){
				GAC.RemoveTargetGO(gacSettings.gameObject, thisObject);
			}

		}
	}

	void Update(){

		foreach(GAC gacSettings in GAC.gacObjects){


			//Check if 3D or 2D mode then use the correct axis' for calculations
			if(gameModeIndex == 0){

				//Create the vertices based on the set attributes from inspector
				parameterVertices3D = CreateParameterVertices3D(horizontalSensitivity, forwardSensitivity, parameterSize.x, parameterSize.y, parameterVertices3D);
				
				for (int i = 0; i < parameterVertices3D.Count; i++){

					//Create the list of values, make sure has right amount to match the number of vertices
					if(parameterDistances.Count < parameterVertices3D.Count){
						parameterDistances.Add(0);
						parameterAngles.Add(0);
						parameterHeights.Add(0);
					}

					//Place the vertices according to the position of the parameter
					parameterVertices3D[i] = new Vector3(parameterVertices3D[i].x + parameterPos.x + thisObject.transform.position.x, thisObject.transform.position.y + parameterPos.y, 
					                                   parameterVertices3D[i].z + parameterPos.z + thisObject.transform.position.z);

					//Get the distance from target to the vertices on the XZ axis
					parameterDistances[i] = Vector2.Distance(new Vector2(parameterVertices3D[i].x, parameterVertices3D[i].z), 
					                                         new Vector2(gacSettings.gameObject.transform.position.x, gacSettings.gameObject.transform.position.z));

					//Get the distance from target to the vertices on the XY axis for height distance
					parameterHeights[i] = Vector2.Distance(parameterVertices3D[i], gacSettings.gameObject.transform.position);

					Debug.DrawLine(parameterVertices3D[i], new Vector3(parameterVertices3D[i].x - 0.01f, thisObject.transform.position.y + parameterPos.y, parameterVertices3D[i].z));
				}

				//Choose to use range tracking
				if(gacSettings.useRange){
					bool inRange = parameterDistances.Any(i => i <= gacSettings.trackerRadius);
					
					if(inRange){
						GAC.AddTargetGO(gacSettings.gameObject, thisObject);
						
					}else{
						GAC.RemoveTargetGO(gacSettings.gameObject, thisObject);
						
					}
				}

			}else if(gameModeIndex == 1){

				//Create the vertices based on the set attributes from inspector
				parameterVertices2D = CreateParameterVertices2D(horizontalSensitivity, verticalSensitivity, parameterSize.x, parameterSize.y, parameterVertices2D);

				//Create the list of values, make sure has right amount to match the number of vertices
				for (int i = 0; i < parameterVertices2D.Count; i++){
					
					if(parameterDistances.Count < parameterVertices2D.Count){
						parameterDistances.Add(0);
						parameterAngles.Add(0);
					}

					//Place the vertices according to the position of the parameter
					parameterVertices2D[i] = new Vector2(parameterVertices2D[i].x + parameterPos.x + thisObject.transform.position.x, parameterVertices2D[i].y + parameterPos.y +
					                                              thisObject.transform.position.y);

					//Get the distance from target to the vertices on the XY axis for height distance
					parameterDistances[i] = Vector2.Distance(parameterVertices2D[i], gacSettings.transform.position);

					
					Debug.DrawLine(parameterVertices2D[i], new Vector2(parameterVertices2D[i].x - 0.01f, parameterVertices2D[i].y));
				}

				//Choose to use range tracking
				if(gacSettings.useRange){
					bool inRange = parameterDistances.Any(i => i <= gacSettings.trackerRadius);

					if(inRange){
						GAC.AddTargetGO(gacSettings.gameObject, thisObject);

					}else{
						GAC.RemoveTargetGO(gacSettings.gameObject, thisObject);
	
					}
				}

				//Retrieve the current facing direction
				currentFacingDirection = facingDirectionRight;
				
				//Check if auto facing direction detection is on
				if(detectFacingDirection){
					facingDirectionRight = GAC.GetFacingDirection(thisObject, facingDirectionRight);
				}
				
				//Get the targets facing direction
				targetFacingDirection = GAC.GetFacingDirection(gacSettings.gameObject, targetFacingDirection);
				
				//If the facing direction changed then flip the position of the parameter to match
				if(currentFacingDirection != facingDirectionRight){
					parameterPos.x = parameterPos.x * -1;
				}
			}

		}


	}

	
	public List<Vector2> CreateParameterVertices2D(int xLoops, int yLoops, float sizeX, float sizeY, List<Vector2> resultVertices){

		//Start a fresh list
		resultVertices = new List<Vector2>(); 

		//Calculate the distance between each vertex of the parameter
		Vector2 vertexDistance = new Vector2(sizeX/xLoops * 2, sizeY/yLoops * 2);

		//Get get amount of positions to be split between each side of the parameter
		Vector2 sidePositions = new Vector2(xLoops/2, yLoops/2);
		
		for (int x = 0; x < xLoops + 1; x++) {
			
			for (int y = 0; y < yLoops + 1; y++) {
				
				Vector2 resultVertex = new Vector2(vertexDistance.x * (x - sidePositions.x), vertexDistance.y * (y - sidePositions.y));
				
				//Only add the edge points to use
				if(x == 0 || x == xLoops || y == 0 || y == yLoops){
					resultVertices.Add(resultVertex);
				}

				//End loop when at loop count (this includes an added loop that represents median (middle) position between each side)
				if (y == yLoops + 1){
					break;
				}
			}
			//End loop when at loop count (this includes an added loop that represents median (middle) position between each side)
			if (x == xLoops + 1){
				break;
			}
		}
		
		return resultVertices;
	}

	public List<Vector3> CreateParameterVertices3D(int xLoops, int yLoops, float sizeX, float sizeY, List<Vector3> resultVertices){
		
		//Start a fresh list
		resultVertices = new List<Vector3>(); 
		
		//Calculate the distance between each vertex of the parameter
		Vector2 vertexDistance = new Vector2(sizeX/xLoops * 2, sizeY/yLoops * 2);
		
		//Get get amount of positions to be split between each side of the parameter
		Vector2 sidePositions = new Vector2(xLoops/2, yLoops/2);
		
		for (int x = 0; x < xLoops + 1; x++) {
			
			for (int y = 0; y < yLoops + 1; y++) {
				
				Vector3 resultVertex = new Vector3(vertexDistance.x * (x - sidePositions.x), 0, vertexDistance.y * (y - sidePositions.y));
				
				//Only add the edge points to use
				if(x == 0 || x == xLoops || y == 0 || y == yLoops){
					resultVertices.Add(resultVertex);
				}
				
				//End loop when at loop count (this includes an added loop that represents median (middle) position between each side)
				if (y == yLoops + 1){
					break;
				}
			}
			//End loop when at loop count (this includes an added loop that represents median (middle) position between each side)
			if (x == xLoops + 1){
				break;
			}
		}
		
		return resultVertices;
	}

	void FixedUpdate(){
		
		foreach(GAC gacSettings in GAC.gacObjects){
			if(GAC.TargetHit(thisObject, gacSettings.gameObject)){
				DamageMovement(gacSettings.gameObject, thisObject);
				damageMovement = true;


			}

			StopDamageMovement(gacSettings.gameObject, thisObject);
		}
	}

	void StopDamageMovement(GameObject theTarget, GameObject thisObject){

		GAC_TargetTracker tracker = thisObject.GetComponent<GAC_TargetTracker>();

		if(conType == GAC_TargetTracker.ControllerType.Mecanim){

			//Check if animation use is toggled on
			if(tracker.animToggle){

				//Make sure move is toggled on
				if(tracker.moveToggle){
						
					//Within what part of animation should the character be able to move
					if (animationController.animation[damageAnim].time >= moveEnd){
							
						if(gameModeIndex == 1){//Check if 2D Mode index selected
							
							//Stop velocity movement
							movementController2D.velocity = Vector2.zero;
							movementController2D.Sleep();
						}
					}
				}

			}

		}else if(conType == GAC_TargetTracker.ControllerType.Mecanim){

			//Check if animation use is toggled on
			if(tracker.animToggle){

				//Extract the specific animation state name from string
				string newDamage = damageAnim.Before(" 'L");

				//Extract the layer number from the string
				int newLayer = System.Convert.ToInt32(damageAnim.Between("'L-", "'"));

				//Make sure the damage animation is available
				if(animatorController.GetCurrentAnimatorStateInfo(newLayer).IsName(newDamage)){

					//Make sure move is toggled on
					if(tracker.moveToggle){

						//Within what part of animation should the character be able to move
						if (animatorController.GetCurrentAnimatorStateInfo(newLayer).normalizedTime >= moveEnd){

							if(gameModeIndex == 1){//Check if 2D Mode index selected

								//Stop velocity movement
								movementController2D.velocity = Vector2.zero;
								movementController2D.Sleep();
							}
						}
					}
				}
			}
		}

		//Check if animation use is not toggled on
		if(!tracker.animToggle){

			if(didHit){

				//Make sure movement is triggered
				if(damageMovement){

					//Check if time has hit 0
					if(moveTimer <= 0){

						//Check if 3D Mode index selected
						if(gameModeIndex == 0){
							movementController.Move(Vector3.zero);
							
						}else if(gameModeIndex == 1){//Check if 2D Mode index selected
							
							//Stop velocity movement
							movementController2D.velocity = Vector2.zero;
							movementController2D.Sleep();
						}

						//Make sure the GAC script is still referenced
						if(gacSettings != null){

							//Add all values to a list
							if(gacSettings.animHit.moveValues.Count < 3){
								gacSettings.animHit.moveValues.Add (gacSettings.animHit.hitKnockBackX);
								gacSettings.animHit.moveValues.Add (gacSettings.animHit.hitKnockBackY);
								gacSettings.animHit.moveValues.Add (gacSettings.animHit.hitKnockBackZ);
							}
							

							//Reset the timer to the max value saved from list and disable movement 
							moveTimer = gacSettings.animHit.moveValues.Max();
							gacSettings.hitCalled = false;
						}

						//Reset
						didHit = false;
						damageMovement = false;
					}else{

						//Decrease the timer
						moveTimer -= Time.deltaTime;

					}
				}
			}

		}
	}
	
	#region Damage Movement
	//Call to move the player when animation is playing
	public void DamageMovement(GameObject theTarget, GameObject thisObject){

		GAC_TargetTracker tracker = thisObject.GetComponent<GAC_TargetTracker>();
		GAC gacSettings = theTarget.GetComponent<GAC>();

		if(conType == GAC_TargetTracker.ControllerType.Legacy){

			if(detectFacingDirection){
				
				//Check if 2D Mode index selected
				if(gameModeIndex == 1){
					
					//Check if facing direction to the right
					if(facingDirectionRight){
						
						//Get the local scale of the object
						Vector3 theScale = thisObject.transform.localScale;
						
						Vector2 thePosition = movementController2D.position;
						
						//Check if the target object is on the left of this object
						if(theTarget.transform.position.x < thisObject.transform.position.x){
							
							//Then flip scale in the opposite direction if so
							theScale.x = 1;
							thisObject.transform.localScale = theScale;
							
							//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
							thePosition.x = thePosition.x + 0.4f;
							movementController2D.position = thePosition;
						}

					}else{

						//Get the local scale of the object
						Vector3 theScale = thisObject.transform.localScale;
						
						Vector2 thePosition = movementController2D.position;
						
						//Check if the target object is on the right of this object
						if(theTarget.transform.position.x > thisObject.transform.position.x){
							
							//Then flip the scale in the opposite direction if so
							theScale.x = -1;
							thisObject.transform.localScale = theScale;
							
							//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
							thePosition.x = thePosition.x - smoothFlipAmount;
							movementController2D.position = thePosition;
						}

					}
				}
			}
			
			if(tracker.animToggle){

				if(playDamage){
					//Play the new animation
					animatorController.Play(damageAnim);
					playDamage = false;
				}

				//Make sure move is toggled on
				if(tracker.moveToggle){

					//Within what part of animation should the character be able to move
					if (animationController.animation[damageAnim].time > moveBegin && animationController.animation[damageAnim].time < moveEnd){
						
						//Check if 3D Mode index selected
						if(gameModeIndex == 0){

							//Make this object faces the target that hit it
							thisObject.transform.LookAt(new Vector3(theTarget.transform.position.x, theTarget.transform.position.y + thisObject.transform.position.y, theTarget.transform.position.z));

							//Move this object based on the knockback values
							movementController.Move(thisObject.transform.right * gacSettings.animHit.hitKnockBackX);
							movementController.Move(thisObject.transform.up * gacSettings.animHit.hitKnockBackY);
							movementController.Move(thisObject.transform.forward * gacSettings.animHit.hitKnockBackZ);
							
						}else if(gameModeIndex == 1){//Check if 2D Mode index selected
							
							//Check what scale direction in use; this modifies the knockback value between negative and positive
							if(directionIndex == 0){
								//Check if facing direction to the right
								if(facingDirectionRight){
									//Move using velocity based on knockback values
									movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, gacSettings.animHit.hitKnockBackY);
									
								}else{
									
									//Move using velocity based on knockback values
									movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, gacSettings.animHit.hitKnockBackY);
									
								}
							}else if(directionIndex == 1){
								
								//Check if facing direction to the right
								if(facingDirectionRight){
									//Move using velocity based on knockback values
									movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, gacSettings.animHit.hitKnockBackY);
									
								}else{
									
									//Move using velocity based on knockback values
									movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, gacSettings.animHit.hitKnockBackY);
									
								}
							}
						}
						
					}

				}
				
			}else{

				//Make sure move is toggled on
				if(tracker.moveToggle){
					
					//Check if 3D Mode index selected
					if(gameModeIndex == 0){

						//Make this object faces the target that hit it
						thisObject.transform.LookAt(new Vector3(theTarget.transform.position.x, theTarget.transform.position.y + thisObject.transform.position.y, theTarget.transform.position.z));

						//Move this object based on the knockback values
						movementController.Move(thisObject.transform.right * gacSettings.animHit.hitKnockBackX);
						movementController.Move(thisObject.transform.up * gacSettings.animHit.hitKnockBackY);
						movementController.Move(thisObject.transform.forward * gacSettings.animHit.hitKnockBackZ);
						
					}else if(gameModeIndex == 1){//Check if 2D Mode index selected
						
						//Check if facing direction to the right
						if(facingDirectionRight){
							
							//Move using velocity based on knockback values
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, gacSettings.animHit.hitKnockBackY);
							
							Vector2 thePosition = thisObject.transform.position;
							
							//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
							thePosition.x = thePosition.x - 1.8f;
							
							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;
							
							//Check if the target object is on the left of this object
							if(theTarget.transform.position.x < thisObject.transform.position.x){
								
								//Then flip scale in the opposite direction if so
								theScale.x = 1;
								thisObject.transform.localScale = theScale;
							}
							
							
						}else{
							
							//Move using velocity based on knockback values
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, 0);
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackY, 0);
							
							Vector2 thePosition = thisObject.transform.position;
							
							//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
							thePosition.x = thePosition.x + 1.8f;
							
							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;
							
							//Check if the target object is on the right of this object
							if(theTarget.transform.position.x > thisObject.transform.position.x){
								
								//Then flip the scale in the opposite direction if so
								theScale.x = -1;
								thisObject.transform.localScale = theScale;
							}
							
						}
					}
				}
				
			}

		}else if(conType == GAC_TargetTracker.ControllerType.Mecanim){

			if(tracker.animToggle){

				if(detectFacingDirection){
					
					//Check if 2D Mode index selected
					if(gameModeIndex == 1){
						
						//Check if facing direction to the right
						if(facingDirectionRight){
							
							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;
							
							//Get the position of the RigidBody
							Vector2 thePosition = movementController2D.position;

							//Check if the target object is on the left of this object
							if(theTarget.transform.position.x < thisObject.transform.position.x){
								
								//Then flip scale in the opposite direction if so
								theScale.x = 1;
								thisObject.transform.localScale = theScale;

								//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
								thePosition.x = thePosition.x + smoothFlipAmount;
								movementController2D.position = thePosition;
							}
							
							
							
						}else{
							
							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;
							
							//Get the position of the RigidBody
							Vector2 thePosition = movementController2D.position;

							//Check if the target object is on the right of this object
							if(theTarget.transform.position.x > thisObject.transform.position.x){
								
								//Then flip the scale in the opposite direction if so
								theScale.x = -1;
								thisObject.transform.localScale = theScale;
								
								//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
								thePosition.x = thePosition.x - smoothFlipAmount;
								movementController2D.position = thePosition;
							}
							
							
							
						}
					}
				}

				//Extract the specific animation state name from string
				string newDamage = damageAnim.Before(" 'L");
				
				//Extract the layer number from the string
				int newLayer = System.Convert.ToInt32(damageAnim.Between("'L-", "'"));

				if(playDamage){
					//Play the new animation
					animatorController.Play(newDamage, newLayer, 0);
					playDamage = false;
				}

				//Make sure move is toggled on
				if(tracker.moveToggle){

					//Make sure the damage animation is available
					if(animatorController.GetCurrentAnimatorStateInfo(newLayer).IsName(newDamage)){

						//Within what part of animation should the character be able to move
						if (animatorController.GetCurrentAnimatorStateInfo(newLayer).normalizedTime > moveBegin && animatorController.GetCurrentAnimatorStateInfo(newLayer).normalizedTime < moveEnd){
							
							//Check if 3D Mode index selected
							if(gameModeIndex == 0){

								//Make this object face the taget that hit it
								thisObject.transform.LookAt(new Vector3(theTarget.transform.position.x, theTarget.transform.position.y + thisObject.transform.position.y, theTarget.transform.position.z));

								//Move this object based on the knockback values
								movementController.Move(thisObject.transform.right * gacSettings.animHit.hitKnockBackX);
								movementController.Move(thisObject.transform.up * gacSettings.animHit.hitKnockBackY);
								movementController.Move(thisObject.transform.forward * gacSettings.animHit.hitKnockBackZ);
								
							}else if(gameModeIndex == 1){//Check if 2D Mode index selected

								//Check what scale direction in use; this modifies the knockback value between negative and positive
								if(directionIndex == 0){
									//Check if facing direction to the right
									if(facingDirectionRight){
										//Move using velocity based on knockback values
										movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, gacSettings.animHit.hitKnockBackY);

									}else{
									
										//Move using velocity based on knockback values
										movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, gacSettings.animHit.hitKnockBackY);

									}
								}else if(directionIndex == 1){

									//Check if facing direction to the right
									if(facingDirectionRight){
										//Move using velocity based on knockback values
										movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, gacSettings.animHit.hitKnockBackY);
										
									}else{
										
										//Move using velocity based on knockback values
										movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, gacSettings.animHit.hitKnockBackY);

									}
								}
							}

						}
					}
				}

			}else{

				//Make sure move is toggled on
				if(tracker.moveToggle){

					//Check if 3D Mode index selected
					if(gameModeIndex == 0){

						//Make this object faces the target that hit it
						thisObject.transform.LookAt(new Vector3(theTarget.transform.position.x, theTarget.transform.position.y + thisObject.transform.position.y, theTarget.transform.position.z));

						//Move this object based on the knockback values
						movementController.Move(thisObject.transform.right * gacSettings.animHit.hitKnockBackX);
						movementController.Move(thisObject.transform.up * gacSettings.animHit.hitKnockBackY);
						movementController.Move(thisObject.transform.forward * gacSettings.animHit.hitKnockBackZ);
						
					}else if(gameModeIndex == 1){//Check if 2D Mode index selected
						
						//Check if facing direction to the right
						if(facingDirectionRight){
							
							//Move using velocity based on knockback values
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX * -1, 0);
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackY * -1, 0);

							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;

							//Get the position of the RigidBody
							Vector2 thePosition = movementController2D.position;

							//Check if the target object is on the left of this object
							if(theTarget.transform.position.x < thisObject.transform.position.x){
								
								//Then flip scale in the opposite direction if so
								theScale.x = 1;
								thisObject.transform.localScale = theScale;

								//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
								thePosition.x = thePosition.x + smoothFlipAmount;
								movementController2D.position = thePosition;
							}
							
							
						}else{
						
							//Move using velocity based on knockback values
							movementController2D.velocity = new Vector2(gacSettings.animHit.hitKnockBackX, 0);
	
							//Get the local scale of the object
							Vector3 theScale = thisObject.transform.localScale;

							//Get the position of the RigidBody
							Vector2 thePosition = movementController2D.position;

							//Check if the target object is on the right of this object
							if(theTarget.transform.position.x > thisObject.transform.position.x){
								
								//Then flip the scale in the opposite direction if so
								theScale.x = -1;
								thisObject.transform.localScale = theScale;

								//Adjust the position to be smooth when flipping to work with the anchor point being bottom left
								thePosition.x = thePosition.x - smoothFlipAmount;
								movementController2D.position = thePosition;
							}
							
						}
					}
				}

			}
		}

	}
	#endregion Damage Movement

}
