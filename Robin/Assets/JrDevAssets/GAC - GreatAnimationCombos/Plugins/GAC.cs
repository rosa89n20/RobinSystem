using UnityEngine;
using System;
using System.IO;
using JrDevAssets;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.7
//GAC.cs
/////////////////////////////////////////////////////////////////////////////////////////


namespace JrDevAssets{

	public class GAC : MonoBehaviour {

		public GAC gacReference;//Referenece this script
		public static GAC gacStatic;

		public GameObject thisObject;
		public static GameObject gacStaticObject;
		public static List<GAC> gacObjects = new List<GAC>();

		
		public static List<GAC> gacGameObjects = new List<GAC>();
		public static List<GAC_TargetTracker> targetGameObjects = new List<GAC_TargetTracker>();

		public Camera sceneCamera;//Keep a reference to the scene view camera
		public GAC_SetEvent setEvent; //Reference the Set Event script
		public GAC_StarterSetup starterSet; //Keep reference to starter scripts

		public List<GAC_AnimationSetup> animSlots = new List<GAC_AnimationSetup>();//The list of all animations	
		public List<GAC_StarterSetup> starterSlots = new List<GAC_StarterSetup>(); //The animations added to use as starter for combo
		public List<GAC_ComboSetup> comboSlots = new List<GAC_ComboSetup>();//The list of all animations

		public List<GAC_ActivatorSetup> activatorSlots = new List<GAC_ActivatorSetup>();//The list of all animations
		public List<GAC_ActivatorSetup> touchSlotsTrack = new List<GAC_ActivatorSetup>();//The list of all animations

		public List<string> starterNames = new List<string>();//The list of all starter names for reference later
		public List<bool> starterGroupShow = new List<bool>();//Trigger list to show/hide dropdowns for each combo group

		public List<string> storeAnimNames = new List<string>();//The list of all animation names for use in Editor
		public List<string> animationNames = new List<string>();//The list of all animation names to use for index referencing
		public List<string> addedAnims = new List<string>();//The list of all animations that are already added to a slot
		public List<string> starterAnims = new List<string>(); //The animations to use to start a combo
		public List<string> addedStarters = new List<string>(); //The list of all starter animations that are already added to a slot
		public List<string> startersAvailable = new List<string>(); //The starter animations currently available to be used
		public List<string> animationsUsed = new List<string>(); //The starter animations currently available to be used
		public List<string> keepAnimsInSync = new List<string>();//Keep a list of updated animations being used for Animation or Animator Component
		public List<string> dummyStates = new List<string>(); //Keep a list of all the dummy states in Mecanim Animator
		public List<string> activatorsForStarters = new List<string>(); //Keep a list of activators that can call the specific starter

		public int[] globalActivators = new int[101]; //Array of all the activators to use 
		public int globalActivatorIndex; //The index for the activator selected from the dropdown
		public List<int> activators = new List<int>(); //Keep the amount of activators in this list
		public List<string> inputStrings = new List<string>();//Keep a list of the inputs added from activators

		public List<string> touchNamesSet = new List<string>();
		public List<string> activatorNames = new List<string>(); //Used to name sync and sequence activators

		public List<string> syncSlotNames = new List<string>(); //List of all the names of the sync slots inputs
		public List<int> syncSlotReferences = new List<int>(); //List of all the reference indexes for sync activators
		public int syncIndexCounter;//The counter used to set an index for sync activators
		public string syncNameRemoved; //Register the name of the sync slot removed/edited
		

		public List<GameObject> triggeredInputs = new List<GameObject>(); //List of all the input gameobjects called for DFGUI

		public int starterIndex;//Index of current starter animation in the popup
		public int animationIndex;//Index of current activator animation in the popup

		public int inputLoopCount; //Used for looping through the Synchro Activators index
		public int animAmount = 0; //Keeps track of the amount of animations attributes that are setup
		public int starterAmount = 0; //Keeps track of the amount of starter attributes that are setup
		public int comboAmount = 0; //Keeps track of the amount of combo attributes that are setup
		public int activatorAmount = 0; //Keeps track of the amount of activator attributes that are setup
		public int touchAmounts = 0; //Keeps track of the amount of touch activator attributes that are setup
		public int icAmounts = 0; //Keeps track of the amount of InControl activator attributes that are setup
		public int synchroAmounts = 0; //Keeps track of the amount of sync activator attributes that are setup
		public int dfSyncAmounts = 0; //Keeps track of the amount of DF sync activator attributes that are setup
		public int sequenceAmounts = 0; //Keeps track of the amount of sync activator attributes that are setup
		public int dfSequenceAmounts = 0; //Keeps track of the amount of DF sync activator attributes that are setup

		public int trackFrameFreeze; //Tracks the amount of times that animation frame froze during play

		public float directionalRange; //The extra distance between stick parameterAngles to recognize each direction

		//FOR HIT DETECTION
		public float distance; //Distance of target to other objects
		public float angle; //Angle of target negative position to other objects, for 3D
		public float angleTopLeft; //Angle of target top left position; for 2D
		public float angleTopRight; //Angle of target top left position; for 2D
		public float angleBottomLeft; //Angle of target bottom left position; for 2D
		public float angleBottomRight; //Angle of target bottom right position; for 2D
		public bool useRange; //Trigger to use a range for hit detection
		public bool useHeightRange; //Trigger to use a height range for hit detection
		public float trackerRadius; //Range that objects need to be within to be added for hit detection
		public Vector2 angleHeight;//The x,y position of the angle to be used for hit detection; for 2D use

		public List<bool> didHit = new List<bool>(); //Trigger to check if attack hits
		public bool hitCalled;
		public GAC_AnimationSetup animHit;

		public List<GameObject> totalTargets = new List<GameObject >(); //Keeps track of target gameobjects in scene

		//public List<Vector2> parameterEdgeVertices = new List<Vector2>();
		public List<float> parameterDistances = new List<float>();
		public List<float> parameterAngles = new List<float>();
		public Vector2 parameterRange2D;
		public Vector3 parameterRange3D;
		//FOR HIT DETECTION

		public KeyCode retrieveKeyCode;//Reference for the keycode
		public int retrieveMouse;//Reference for the mouse index

		public bool animationsArePlaying; //Trigger to check if any GAC animations playing
		public bool delayTimersCounting; //Trigger to check if any delay timers are counting down
		public bool delayedCombo;

		public int sequenceCounter; //Keeps the attacks that are succesful in a count 
		public string currentAnimation; //The current animation being called by GAC_WT
		public string starterAnimation; //Keep reference of the starter animation for the combo

		//FOR TOUCH INPUT ACTIVATORS
		public List<string> touchSlotNames = new List<string>(); //List of all the names of the touch slots inputs
		public List<int> touchSlotReferences = new List<int>(); //List of all the reference indexes for each Touch Activator
		public int touchIndexCounter; //Keeps track of the amount to increase for reference indexes to use on Touch Activators
		public string touchNameRemoved; //Register the name of the touch slot removed/edited

		public float minSwipeLength; //The length of each swipe
		float swipeMagnitude; //Keeps the result magnitude of touch start and end positions
		Vector2 swipeResult; //Keeps the difference between touch start and end positions
		float swipeRange; //The extra distance between swipe parameterAngles to recognize each swipe type
		Vector2 touchStartPos; //The start position of touch
		Vector2 touchEndPos; //The end position of touch
		static bool gacTouchTweak; //To trigger when to modify animation input times when using touch
		static float gacTapTimeLimit; //Time limit used to recognize when a tap should initiate
		public float doubleGestureRecognizer; //Keeps a reference of current time a touch was registered to check for double taps
		float holdTimeTrigger; //The time goal to trigger a hold gesture
		public float holdTimeElapsed; //The time that has passed while holding down on touch
		public int tapAmount; //The number of taps registered within the tap time limit
		int fingerAmount; //Keeps the current amount of fingers touching screen
		bool inputRelease; //Triggers when a touch input is released
		//FOR TOUCH INPUT ACTIVATORS

		//FOR TOUCH AREA GUI (TAG)
		public bool tagWindowReady; //Trigger to know when the TAG window is able to be used
		public int tagTipIndex = 1; //Index for TAG Window Tips
		public bool simulate; //Switch for when to use Simulation of Resolutions; this is when the VAT window is open
		public bool tagInBuild; //Should you show the touch areas when game is built to device?
		public bool loadTouchArea; //Trigger when to load a Resolution's settings touch areas
		public Vector2 theResolution; //The set resolution selected from drop down menu of VAT
		public int resolutionIndex; //The index of the selection of resolution types
		public int currentIndex; //The current index of the selected resolution type; used to check if the index was changed
		public string[] resolutionNames;//The strings for resolution input options
		public int inGameResolutionIndex; //Keeps a reference to the index to set in game based on Screen resolution
		public List<string> resolutionNamesList = new List<string>(); //Keeps the converted array of resolution names in this list
		public List<int> resolutionScaleFactor = new List<int>(); //Keeps the scale number to multiple each resolution from the in editor to in game

		//These keep a record of the saved touch activator settings for each platform
		public List<GAC_SavedTouchArea> standaloneSavedSlots = new List<GAC_SavedTouchArea>();
		public List<GAC_SavedTouchArea> iosSavedSlots = new List<GAC_SavedTouchArea>();
		public List<GAC_SavedTouchArea> androidSavedSlots = new List<GAC_SavedTouchArea>();
		//FOR TOUCH AREA GUI (TAG)

		public CharacterController movementController; //Character controller for movement
		public Rigidbody2D movementController2D; //Rigidbody controller for 2D movement
		public Animation animationController; //Animation component reference
		public Animator animatorController; //Animator component reference

		public int gameModeIndex; //The index of the selection for Unity Game Mode
		public string[] gameModeNames = {"3D","2D"};//The names for game mode options

		//FOR 2D DEVELOPMENT
		public bool facingDirectionRight;
		public bool detectFacingDirection;
		public int directionIndex;
		public string[] directionScales = {"1", "-1"};
		//FOR 2D DEVELOPMENT

		public bool gizmoShown;
		public bool isDraggingRadius;//Trigger for the radius field

		public InputSource inputSource;
		
		public enum InputSource{
			KEYINPUT,
			MOUSEINPUT,
			BUTTONINPUT,
			SYNCHROINPUT,
			SEQUENCEINPUT,
			TOUCHINPUT,
		}

		public DebugMode debugMode;

		public enum DebugMode{
			Off,
			All,
			AnimationLog,
			HitLog,
			HitRangeLog,
			TargetLog,
			InputLog
		}

		public ControllerType conType;
		
		public enum ControllerType{
			Legacy,
			Mecanim
		}


		public static GAC_Images images;
		public static GUISkin gacSkins;
		public static bool gacPacInitialize;

		//The menu trigger check
		public bool animSetup;
		public bool comboSetup;
		public bool activatorSetup;

		void Awake(){
			gacStaticObject = gameObject;
			thisObject = gameObject;

			//Reference the GAC component
			gacReference = thisObject.GetComponent<GAC>();
			gacStatic = gacStaticObject.GetComponent<GAC>();



			foreach (GAC_AnimationSetup anim in gacReference.animSlots){

				//Loop through and add to list for reference to indexes later
				if (anim.theAnim != null){
					if(! gacReference.animationNames.Contains(anim.theAnim)){
						gacReference.animationNames.Add(anim.theAnim);
					}

				}else{
					#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
					#endif	
					Debug.LogError("GACError - There is an animation clip missing from Animation Index #" + (animSlots.IndexOf(anim) + 1));
				}
			}
			//Check if 2D Mode index selected
			if(gacReference.gameModeIndex == 0){
				movementController = GetComponent<CharacterController>();
			}else if(gacReference.gameModeIndex == 1){
				movementController2D = thisObject.GetComponent<Rigidbody2D>();
			}


			//Start the sequence counting at 0 of course
			gacReference.sequenceCounter = 1;

		}


		void  FixedUpdate (){

			foreach (GAC_AnimationSetup anim in gacReference.animSlots){
				//Make sure move checking is set to be used
				if(anim.moveToggle){

					//Check if 2D Mode index selected
					if(gacReference.gameModeIndex == 1){
	
						if(movementController2D == null){
							
							Debug.LogWarning("There was no RigidBody2D component found on gameObject " + thisObject.name + " to use to move the character");
							
						}else if (movementController2D != null){
							//Then check object to move it
							AttackMovement(anim);
						}
					}
				}
			}
			
		}

		void  Update (){


			foreach (GAC_ActivatorSetup actSet in gacReference.activatorSlots){
				
				if(actSet.useKey || actSet.useMouse || actSet.useButton || actSet.useTouch || actSet.useSync || actSet.useSequence){
					ActivatorControl(actSet, gameObject, gacReference.activatorSlots.IndexOf(actSet));

				}
			}

			foreach (GAC_AnimationSetup anim in gacReference.animSlots){
				
				//Check if the animation is null first
				if (!string.IsNullOrEmpty(anim.theAnim)){

					if(gacReference.conType == GAC.ControllerType.Legacy){

						if(animationController == null){

							//Reference the animation component
							animationController = gameObject.animation;
						
						}else{

							//Set to false if the current animation is not playing
							if (!animationController.IsPlaying(anim.theAnim)){
								if(!anim.delayTiming){
									anim.isPlaying = false;
								}
							}else{
								anim.isPlaying = true;

							}

							if(anim.animTime > gacReference.gameObject.animation[anim.theAnim].length - 0.1){
								if(!anim.delayTiming){
									animationController.Stop(anim.theAnim);
									anim.isPlaying = false;
								}
							}

							//If the animation is playing
							if (anim.isPlaying){
								
								//Reference the animation time
								anim.animTime = gacReference.gameObject.animation[anim.theAnim].time;
								
							}else{
								//Otherwise reset the length
								anim.animTime = 0;
								
							}

							//Check if the delay has been set to use
							if(anim.delayToggle){
								if(anim.animTime > gacReference.gameObject.animation[anim.theAnim].length - 0.1){

									//Make sure the delay animation was triggered
									if(anim.delayMode){
										//Start and set the delay
										anim.delayTiming = true;
										anim.delayCountDown = 0.5f;
									}
								}
							}
							
							//Check if the delay is triggered
							if(anim.delayTiming){
								
								//Count down the time
								anim.delayCountDown -= Time.deltaTime;
								
								//Reset
								if(anim.delayCountDown <= 0){
									anim.delayTiming = false;
									anim.delayMode = false;
								}
							}else{
								anim.delayCountDown = 0.5f;

							}

						}
					}else if(gacReference.conType == GAC.ControllerType.Mecanim){
						
						if(animatorController == null){

							//Reference the animator component
							animatorController = gacReference.GetComponent<Animator>();

						}else{


							//If the animation is playing
							if (anim.isPlaying){


								//Extract only the animation name from the string
								string newAnimName = anim.theAnim.Before(" 'L");
								
								//Extract the layer number from the string
								int animLayer = System.Convert.ToInt32(anim.theAnim.Between("'L-", "'"));
								
								//Reference the animation info for the current state
								AnimatorStateInfo animInfo = animatorController.GetCurrentAnimatorStateInfo(animLayer);

								//Reset the animation if the time goes over 0.99
								if (anim.animTime > 1){
									//if(!anim.delayTiming){
										anim.isPlaying = false;
										animatorController.SetLayerWeight(animLayer, 0);
									//}
								}

								//This is a precaution check to make sure the animation state is not stuck on the same frame
								//if the frame rate drops
								if(anim.prevLength != anim.animTime && anim.animTime > 0){
									anim.prevLength = anim.animTime;
									trackFrameFreeze = 0;
								}else{

									//If this frame is stuck after 2 checks then reset all animation is playing triggers
									if(trackFrameFreeze > 2){
										foreach (GAC_AnimationSetup anims in gacReference.animSlots){

											if(anims.prevLength == anims.animTime && anims.animTime > 0){
												anims.isPlaying = false;
										
											}
										}
									}else{

										//Otherwise keep adding to track the frames frozen
										trackFrameFreeze = trackFrameFreeze + 1;
									}
								}

								//If the animation is playing
								if (animInfo.IsName(newAnimName)){
									//Reference the animation time
									anim.animTime = animInfo.normalizedTime;
								}


							}else{

								//Reset the length
								anim.animTime = 0;

							}

							
							//Set to false if the current animation is not playing
							if (!IsPlaying(thisObject, anim.theAnim)){
								if(!anim.delayTiming){
									anim.isPlaying = false;
								}
							}

							//Check if the delay has been set to use
							if(anim.delayToggle){

								if (anim.animTime > 0.9){

									//Make sure the delay animation was triggered
									if(anim.delayMode){
										//Start and set the delay
										anim.delayTiming = true;
										anim.delayCountDown = 0.5f;
									}
								}
							}

							//Check if the delay is triggered
							if(anim.delayTiming){

								//Count down the time
								anim.delayCountDown -= Time.deltaTime;

								//Reset
								if(anim.delayCountDown <= 0){
									anim.delayTiming = false;
									anim.delayMode = false;
								}
							}else{
								anim.delayCountDown = 0.5f;
							}

						}
					}


					//Check to see if animation has reached the end then set hit trigger check to false
					if(anim.animTime > anim.hitEnd){
						if(anim.layerObjects != null){
							foreach (GameObject theObject in anim.layerObjects){

								//Get reference to the Attack Event script
								GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();

								targetTracker.didHit = false;
								gacReference.hitCalled = false;
							}
						}

					}

					//Make sure hit checking is set to be used
					if(anim.hitToggle){

						//Then check object to affect with hits
						AffectObject(anim);	

					}

					//Make sure move checking is set to be used
					if(anim.moveToggle){

						//Check if 3D Mode index selected
						if(gacReference.gameModeIndex == 0){

							movementController = thisObject.GetComponent<CharacterController>();

							if(movementController == null){

								Debug.LogWarning("There was no CharacterController component found on gameObject " + thisObject.name + " to use to move the character");
								
							}else if (movementController != null){
								//Then check object to move it
								AttackMovement(anim);
							}
						}

					}
				}

			}

			//Reference if any animations are playing with GAC
			gacReference.animationsArePlaying = gacReference.animSlots.Any (i => i.isPlaying == true);

			//Reference if any delay timers are on
			gacReference.delayTimersCounting = gacReference.animSlots.Any(i => i.delayTiming == true);


			//Check if there are no animations playing, then reset
			if(!gacReference.animationsArePlaying){

				if(!gacReference.delayTimersCounting){
					gacReference.sequenceCounter = 1;
					gacReference.currentAnimation = "";
					gacReference.starterAnimation = null;
					gacReference.starterSet.theReferences.Clear();
					trackFrameFreeze = 0;
				
				}
			}

			//Get the gesture info from touches
			GestureRecognizer();


		}
		public bool playCheck;
		#region Gesture Recognizer
		void GestureRecognizer(){

			//Retrive the double tap activator slot
			var upGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Up).ToList();
			var upLeftGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.UpLeft).ToList();
			var upRightGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.UpRight).ToList();
			var leftGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Left).ToList();
			var rightGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Right).ToList();
			var downGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Down).ToList();
			var downLeftGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.DownLeft).ToList();
			var downRightGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.DownRight).ToList();
			var tapGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Tap).ToList();
			var doubleTapGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.DoubleTap).ToList();
			var holdGesture = activatorSlots.Where(i => i.touchedArea == true && i.gestures == GAC_ActivatorSetup.Gestures.Hold).ToList();

			//Make sure there is an double tap activator slot
			if(doubleTapGesture.Count > 0){

				//If the Touch area was touched
				if(doubleTapGesture[0].touchedArea){
				
					//Check if double tap timer was within limit
					if(Time.time - doubleGestureRecognizer < gacTapTimeLimit){	

						//Check if atleast 2 taps
						if(tapAmount == 2){
						
							//Only run if there is atleast 1 finger on screen
							if (fingerAmount > 0) {

								//Check to make sure the set amount of fingers have been use
								if((doubleTapGesture[0].touchIndex + 1) == fingerAmount){

									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									doubleTapGesture[0].timeInput = 0f;
									doubleTapGesture[0].singleInputTriggered = true;
									
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Double Tap!");
									}
									
									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}else{

								//Check to make sure the set amount of fingers have been simulated with specific keys
								if(Input.GetKey(doubleTapGesture[0].modifyKey) || doubleTapGesture[0].touchIndex == 0){
									
									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									doubleTapGesture[0].timeInput = 0f;
									doubleTapGesture[0].singleInputTriggered = true;
									
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Double Tap!");
									}
									
									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}
						}
							
					}
				}

				//Make sure a double tap hasn't been played first
				if(!doubleTapGesture[0].singleInputTriggered){

					//Make sure timer has hit limit
					if(Time.time - doubleGestureRecognizer > gacTapTimeLimit){		
			
						//Make sure there is an double tap activator slot
						if(tapGesture.Count > 0){

							//If the Touch area was touched
							if(tapGesture[0].touchedArea){

								//Make sure the mouse input was released
								if(inputRelease){

									//Make sure swipe length is within ranges
									if (swipeMagnitude < 20){

										//Only run if there is atleast 1 finger on screen
										if (fingerAmount > 0) {
											
											//Check to make sure the set amount of fingers have been use
											if((tapGesture[0].touchIndex + 1) == fingerAmount){

												//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
												tapGesture[0].timeInput = 0f;
												tapGesture[0].singleInputTriggered = true;
												
												//Check if Logging is On
												if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
													Debug.Log("GACLog - Input used is Touch Single Tap!");
												}

												//Reset all
												holdTimeElapsed = 0;
												tapAmount = 0;
												doubleGestureRecognizer = 0;
											}
										}else{

											//Check to make sure the set amount of fingers have been simulated with specific keys
											if(Input.GetKey(tapGesture[0].modifyKey) || tapGesture[0].touchIndex == 0){
												
												//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
												tapGesture[0].timeInput = 0f;
												tapGesture[0].singleInputTriggered = true;
												
												//Check if Logging is On
												if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
													Debug.Log("GACLog - Input used is Touch Single Tap!");
												}
												
												//Reset all
												holdTimeElapsed = 0;
												tapAmount = 0;
												doubleGestureRecognizer = 0;
											}
										}
								
									}
									
								}

							}
						}
					}		
				}
			}else if(tapGesture.Count > 0){
				//If the Touch area was touched
				if(tapGesture[0].touchedArea){
					
					//Make sure the mouse input was released
					if(inputRelease){
						
						//Make sure swipe length is within ranges
						if (swipeMagnitude < 20){
							
							//Only run if there is atleast 1 finger on screen
							if (fingerAmount > 0) {
								
								//Check to make sure the set amount of fingers have been use
								if((tapGesture[0].touchIndex + 1) == fingerAmount){
									
									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									tapGesture[0].timeInput = 0f;
									tapGesture[0].singleInputTriggered = true;
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Single Tap!");
									}
									
									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}else{
								
								//Check to make sure the set amount of fingers have been simulated with specific keys
								if(Input.GetKey(tapGesture[0].modifyKey) || tapGesture[0].touchIndex == 0){
									
									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									tapGesture[0].timeInput = 0f;
									tapGesture[0].singleInputTriggered = true;
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Single Tap!");
									}
									
									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}
							
						}
						
					}
					
				}
				
			}

			//Make sure the mouse input was released
			if(inputRelease){

				//Make sure there is an single tap activator slot
				if(tapGesture.Count > 0){

					//If the Touch area was touched
					if(tapGesture[0].touchedArea){

						//Make sure swipe length is within ranges
						if (swipeMagnitude >= 20 && swipeMagnitude < minSwipeLength){

							//Only run if there is atleast 1 finger on screen
							if (fingerAmount > 0) {
								
								//Check to make sure the set amount of fingers have been use
								if((tapGesture[0].touchIndex + 1) == fingerAmount){
									
									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									tapGesture[0].timeInput = 0f;
									tapGesture[0].singleInputTriggered = true;
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Single Tap!");
									}

									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}else{
								//Check to make sure the set amount of fingers have been simulated with specific keys
								if(Input.GetKey(tapGesture[0].modifyKey) || tapGesture[0].touchIndex == 0){
									
									//Register time pressed and set input to triggered which will be 0 seconds because of the timer already making a latency
									tapGesture[0].timeInput = 0f;
									tapGesture[0].singleInputTriggered = true;
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Touch Double Tap!");
									}

									//Reset all
									holdTimeElapsed = 0;
									tapAmount = 0;
									doubleGestureRecognizer = 0;
								}
							}
						}
					}
				}
			}
		
	
			//Only run if there is atleast 1 finger on screen
			if (Input.touches.Length > 0) {

				//Get the reference to the touch
				Touch theTouch = Input.GetTouch(0);

				if (theTouch.phase == TouchPhase.Began || theTouch.phase == TouchPhase.Stationary) {
					//Make sure there is an hold gesture activator slot
					if(holdGesture.Count > 0){
						
						if(holdGesture[0].gestures == GAC_ActivatorSetup.Gestures.Hold){
							//Increase hold time
							holdTimeElapsed += Time.deltaTime;
						}
					}
				}

				if (theTouch.phase == TouchPhase.Ended) {
					//The end position of the touch
					touchEndPos = new Vector2(theTouch.position.x, theTouch.position.y);

					//The resulting position between end minus beginning of touch
					swipeResult = new Vector3(touchEndPos.x - touchStartPos.x, touchEndPos.y - touchStartPos.y);
					
					inputRelease = true;
					swipeMagnitude = swipeResult.magnitude;
					
					//Make sure swipe length is within range
					if (swipeResult.magnitude < minSwipeLength){
						
						//Make sure there is an hold gesture activator slot
						if(holdGesture.Count > 0){
							
							//Make sure the hold time hasn't been passed
							if(holdTimeElapsed < holdTimeTrigger){
								
								//Make sure there is an double tap gesture activator slot
								if(doubleTapGesture.Count > 0){
									
									if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
										
										//Increase the taps
										tapAmount++;
										
										//Reference the time of the tap
										doubleGestureRecognizer = Time.time;
										
									}
								}else{
									
									//Make sure there is an single tap gesture activator slot
									if(tapGesture.Count > 0){
										
										if(tapGesture[0].gestures == GAC_ActivatorSetup.Gestures.Tap){
											//Increase the taps
											tapAmount++;
											
											//Reference the time of the tap
											doubleGestureRecognizer = Time.time;
										}
									}
									
									//Reset all
									
									holdTimeElapsed = 0;
								}
								
							}else{//if hold time limit has been exceeded, then this is a hold gesture
								
								//Make sure there is a single or double tap gesture activator slot
								if(doubleTapGesture.Count > 0){
									
									if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
										
										//Increase the taps
										tapAmount++;
										
									}
								}
								
								//Make sure the swipe length limit hasn't been exceeded
								if (swipeResult.magnitude < 20){
									
									//Check to make sure the set amount of fingers have been simulated with specific keys
									if(Input.GetKey(holdGesture[0].modifyKey) || holdGesture[0].touchIndex == 0){
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										holdGesture[0].timeInput = 0.083f;
										holdGesture[0].singleInputTriggered = true;
										
										holdTimeElapsed = 0;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Hold!");
										}
										
									}
								}
								
							}
						}else{//if no hold gesture activator slot, then check for single or double tap gestures
							
							//Make sure there is an double tap gesture activator slot
							if(doubleTapGesture.Count > 0){
								
								if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
									
									//Increase the taps
									tapAmount++;
									
									//Reference the time of the tap
									doubleGestureRecognizer = Time.time;
									
								}
							}else{
								
								//Make sure there is an single tap gesture activator slot
								if(tapGesture.Count > 0){
									
									if(tapGesture[0].gestures == GAC_ActivatorSetup.Gestures.Tap){
										//Increase the taps
										tapAmount++;
										
										//Reference the time of the tap
										doubleGestureRecognizer = Time.time;
										
									}
									
									//Reset all
									
									holdTimeElapsed = 0;
								}
							}
						}
						
					}else{
						
						//Normalize the vector position; the result will be between 0-1
						swipeResult.Normalize();
						
						if(swipeResult.y > swipeRange && swipeResult.x > -swipeRange && swipeResult.x < swipeRange) {
							
							if(upGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(upGesture[0].gestures == GAC_ActivatorSetup.Gestures.Up){
									
									//Check to make sure the set amount of fingers have been use
									if((upGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										upGesture[0].timeInput = 0.083f;
										upGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up!");
										}
										
									}
								}
							}
						}else if (swipeResult.y > swipeRange && swipeResult.x < swipeRange) {
							
							if(upLeftGesture.Count > 0){
								//Check what type of gesture was triggered
								if(upLeftGesture[0].gestures == GAC_ActivatorSetup.Gestures.UpLeft){
									
									//Check to make sure the set amount of fingers have been use
									if((upLeftGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										upLeftGesture[0].timeInput = 0.083f;
										upLeftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up-Left!");
										}
									}
									
								}
							}
							
						}else if (swipeResult.y > swipeRange && swipeResult.x > swipeRange) {
							
							if(upRightGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(upRightGesture[0].gestures == GAC_ActivatorSetup.Gestures.UpRight){
									
									//Check to make sure the set amount of fingers have been use
									if((upRightGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										upRightGesture[0].timeInput = 0.083f;
										upRightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up-Right!");
										}
									}
									
								}
							}
							
						}else if (swipeResult.y < swipeRange && swipeResult.x > -swipeRange && swipeResult.x < swipeRange) {
							
							if(downGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(downGesture[0].gestures == GAC_ActivatorSetup.Gestures.Down){
									
									//Check to make sure the set amount of fingers have been use
									if((downGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										downGesture[0].timeInput = 0.083f;
										downGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down!");
										}
									}
									
								}
							}
							
						}else if (swipeResult.y < -swipeRange && swipeResult.x < swipeRange) {
							
							if(downLeftGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(downLeftGesture[0].gestures == GAC_ActivatorSetup.Gestures.DownLeft){
									
									//Check to make sure the set amount of fingers have been use
									if((downLeftGesture[0].touchIndex + 1) == Input.touches.Length){
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										downLeftGesture[0].timeInput = 0.083f;
										downLeftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down-Left!");
										}
									}
									
								}
							}
						}else if (swipeResult.y < -swipeRange  && swipeResult.x > swipeRange) {
							
							if(downRightGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(downRightGesture[0].gestures == GAC_ActivatorSetup.Gestures.DownRight){
									
									//Check to make sure the set amount of fingers have been use
									if((downRightGesture[0].touchIndex + 1) == Input.touches.Length){		
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										downRightGesture[0].timeInput = 0.083f;
										downRightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down-Right!");
										}
									}
									
								}
							}
							
						}else if (swipeResult.x < -swipeRange && swipeResult.y > -swipeRange && swipeResult.y < swipeRange) {
							
							if(leftGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(leftGesture[0].gestures == GAC_ActivatorSetup.Gestures.Left){
									
									//Check to make sure the set amount of fingers have been use
									if((leftGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										leftGesture[0].timeInput = 0.083f;
										leftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Left!");
										}
									}
									
								}
							}
							
						}else if (swipeResult.x > swipeRange && swipeResult.y > -swipeRange && swipeResult.y < swipeRange) {
							
							if(rightGesture.Count > 0){
								
								//Check what type of gesture was triggered
								if(rightGesture[0].gestures == GAC_ActivatorSetup.Gestures.Right){
									
									//Check to make sure the set amount of fingers have been use
									if((rightGesture[0].touchIndex + 1) == Input.touches.Length){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										rightGesture[0].timeInput = 0.083f;
										rightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Right!");
										}
									}
									
								}
								
							}
						}
						
					}
				}
			}else{
				//Holding down mouse button
				if(Input.GetMouseButton(0)){

					//Make sure there is an hold gesture activator slot
					if(holdGesture.Count > 0){
						
						if(holdGesture[0].gestures == GAC_ActivatorSetup.Gestures.Hold){
							//Increase hold time
							holdTimeElapsed += Time.deltaTime;
						}
					}
				}

				if(Input.GetMouseButtonUp(0)){
					
					//The end position of the touch
					touchEndPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
					
					//The resulting position between end minus beginning of touch
					swipeResult = new Vector3(touchEndPos.x - touchStartPos.x, touchEndPos.y - touchStartPos.y);
					
					inputRelease = true;
					swipeMagnitude = swipeResult.magnitude;
					
					//Make sure swipe length is within range
					if (swipeResult.magnitude < minSwipeLength){
		
						//Make sure there is an hold gesture activator slot
						if(holdGesture.Count > 0){
							
							//Make sure the hold time hasn't been passed
							if(holdTimeElapsed < holdTimeTrigger){
								
								//Make sure there is an double tap gesture activator slot
								if(doubleTapGesture.Count > 0){
									
									if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
										
										//Increase the taps
										tapAmount++;
										
										//Reference the time of the tap
										doubleGestureRecognizer = Time.time;
										
									}
								}else{
										
									//Make sure there is an single tap gesture activator slot
									if(tapGesture.Count > 0){
										
										if(tapGesture[0].gestures == GAC_ActivatorSetup.Gestures.Tap){
											//Increase the taps
											tapAmount++;
											
											//Reference the time of the tap
											doubleGestureRecognizer = Time.time;
										}
									}
									
									//Reset all
									
									holdTimeElapsed = 0;
								}
								
							}else{//if hold time limit has been exceeded, then this is a hold gesture
								
								//Make sure there is a single or double tap gesture activator slot
								if(doubleTapGesture.Count > 0){
									
									if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
										
										//Increase the taps
										tapAmount++;
										
									}
								}
								
								//Make sure the swipe length limit hasn't been exceeded
								if (swipeResult.magnitude < 20){
									
									//Check to make sure the set amount of fingers have been simulated with specific keys
									if(Input.GetKey(holdGesture[0].modifyKey) || holdGesture[0].touchIndex == 0){
										
										//Register time pressed and set input to triggered which will be 0 seconds
										holdGesture[0].timeInput = 0f;
										holdGesture[0].singleInputTriggered = true;

										holdTimeElapsed = 0;

										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Hold!");
										}
										
									}
								}
									
							}
						}else{//if no hold gesture activator slot, then check for single or double tap gestures
							
							//Make sure there is an double tap gesture activator slot
							if(doubleTapGesture.Count > 0){
								
								if(doubleTapGesture[0].gestures == GAC_ActivatorSetup.Gestures.DoubleTap){
									
									//Increase the taps
									tapAmount++;

									//Reference the time of the tap
									doubleGestureRecognizer = Time.time;
									
								}
							}else{

								//Make sure there is an single tap gesture activator slot
								if(tapGesture.Count > 0){
									
									if(tapGesture[0].gestures == GAC_ActivatorSetup.Gestures.Tap){
										//Increase the taps
										tapAmount++;
										
										//Reference the time of the tap
										doubleGestureRecognizer = Time.time;

									}
									
									//Reset all
									
									holdTimeElapsed = 0;
								}
							}
						}
							
					}else{
						
						//Normalize the vector position; the result will be between 0-1
						swipeResult.Normalize();

						if(swipeResult.y > swipeRange && swipeResult.x > -swipeRange && swipeResult.x < swipeRange) {

							if(upGesture.Count > 0){

								//Check what type of gesture was triggered
								if(upGesture[0].gestures == GAC_ActivatorSetup.Gestures.Up){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(upGesture[0].modifyKey) || upGesture[0].touchIndex == 0){		
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										upGesture[0].timeInput = 0.083f;
										upGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up!");
										}
										
									}
								}
							}
						}else if (swipeResult.y > swipeRange && swipeResult.x < swipeRange) {
								
							if(upLeftGesture.Count > 0){
								//Check what type of gesture was triggered
								if(upLeftGesture[0].gestures == GAC_ActivatorSetup.Gestures.UpLeft){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(upLeftGesture[0].modifyKey) || upLeftGesture[0].touchIndex == 0){	

										//Register time pressed and set input to triggered which will be 0.083 seconds
										upLeftGesture[0].timeInput = 0.083f;
										upLeftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up-Left!");
										}
									}
									
								}
							}

						}else if (swipeResult.y > swipeRange && swipeResult.x > swipeRange) {

							if(upRightGesture.Count > 0){

								//Check what type of gesture was triggered
								if(upRightGesture[0].gestures == GAC_ActivatorSetup.Gestures.UpRight){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(upRightGesture[0].modifyKey) || upRightGesture[0].touchIndex == 0){	
										
										//Register time pressed and set input to triggered which will be 0.083 seconds
										upRightGesture[0].timeInput = 0.083f;
										upRightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Up-Right!");
										}
									}
									
								}
							}
								
						}else if (swipeResult.y < swipeRange && swipeResult.x > -swipeRange && swipeResult.x < swipeRange) {
								
							if(downGesture.Count > 0){

								//Check what type of gesture was triggered
								if(downGesture[0].gestures == GAC_ActivatorSetup.Gestures.Down){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(downGesture[0].modifyKey) || downGesture[0].touchIndex == 0){		

										//Register time pressed and set input to triggered which will be 0.083 seconds
										downGesture[0].timeInput = 0.083f;
										downGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down!");
										}
									}
									
								}
							}
								
						}else if (swipeResult.y < -swipeRange && swipeResult.x < swipeRange) {

							if(downLeftGesture.Count > 0){

								//Check what type of gesture was triggered
								if(downLeftGesture[0].gestures == GAC_ActivatorSetup.Gestures.DownLeft){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(downLeftGesture[0].modifyKey) || downLeftGesture[0].touchIndex == 0){	

										//Register time pressed and set input to triggered which will be 0.083 seconds
										downLeftGesture[0].timeInput = 0.083f;
										downLeftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down-Left!");
										}
									}
									
								}
							}
						}else if (swipeResult.y < -swipeRange  && swipeResult.x > swipeRange) {
								
							if(downRightGesture.Count > 0){

								//Check what type of gesture was triggered
								if(downRightGesture[0].gestures == GAC_ActivatorSetup.Gestures.DownRight){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(downRightGesture[0].modifyKey)  || downRightGesture[0].touchIndex == 0){		

										//Register time pressed and set input to triggered which will be 0.083 seconds
										downRightGesture[0].timeInput = 0.083f;
										downRightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Down-Right!");
										}
									}
									
								}
							}
								
						}else if (swipeResult.x < -swipeRange && swipeResult.y > -swipeRange && swipeResult.y < swipeRange) {
								
							if(leftGesture.Count > 0){

								//Check what type of gesture was triggered
								if(leftGesture[0].gestures == GAC_ActivatorSetup.Gestures.Left){

									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(leftGesture[0].modifyKey) || leftGesture[0].touchIndex == 0){	

										//Register time pressed and set input to triggered which will be 0.083 seconds
										leftGesture[0].timeInput = 0.083f;
										leftGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Left!");
										}
									}
									
								}
							}
								
						}else if (swipeResult.x > swipeRange && swipeResult.y > -swipeRange && swipeResult.y < swipeRange) {
								
							if(rightGesture.Count > 0){

								//Check what type of gesture was triggered
								if(rightGesture[0].gestures == GAC_ActivatorSetup.Gestures.Right){
									
									//Check to make sure the key input is held to simulate the set amount of fingers to use
									if(Input.GetKey(rightGesture[0].modifyKey) || rightGesture[0].touchIndex == 0){	

										//Register time pressed and set input to triggered which will be 0.083 seconds
										rightGesture[0].timeInput = 0.083f;
										rightGesture[0].singleInputTriggered = true;
										
										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - Input used is Touch Swipe Right!");
										}
									}
									
								}
								
							}
						}

					}
					
				}
			}


		
		}
		#endregion Gesture Recognizer

		#region Single Player PlayAnimation
		//Used to call the animation and link to create a combo
		public static void PlayTheAnimation (string starterAnimName, int activator){

			//Early out if the starter is not found
			if(!gacStatic.animationNames.Contains(starterAnimName)){
				Debug.LogError("GACError - There is no animation with the name " + starterAnimName + " set to be used as a starter animation in GAC! " +
					"Please add the corresponding starter animation or check your spelling!");
				return;
			}

			//Early out if the activator is not found
			if(!gacStatic.activators.Contains(activator)){
				Debug.LogError("GACError - The activator number " + activator + " is not available for use in GAC! Please add the corresponding activator number using" +
					" the 'Animation Setup' menu!");
				return;
			}

			//Keep the time amount to tweak for touch input; helps to balance out the latency with touch to animation response
			float touchTweakTime = 0;

			//Check if a starter animation is registered in GAC
			if(string.IsNullOrEmpty(gacStatic.starterAnimation)){
				if(gacStatic.conType == GAC.ControllerType.Legacy){

					gacStatic.starterAnimation = starterAnimName;
				
				}else if(gacStatic.conType == GAC.ControllerType.Mecanim){

					string newStarterName = starterAnimName.Before("'L");
					gacStatic.starterAnimation = newStarterName;
				}
			}

			//Make sure this is the starter animation being called; this is based on the sequence counter being 1
			if(gacStatic.sequenceCounter == 1){

				//Reference the current animation's slot class
				GAC_AnimationSetup animSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(starterAnimName)];

				if(gacStatic.conType == GAC.ControllerType.Legacy){

					if(animSet.playMode == GAC_AnimationSetup.PlayMode.Normal){

						//Play the animation
						gacStatic.animationController.Play(starterAnimName);
						
					}else if (animSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){						
						
						//Crossfade the animation
						gacStatic.animationController.CrossFade(starterAnimName, animSet.blendTime);
					}

				}else if(gacStatic.conType == GAC.ControllerType.Mecanim){

					//Extract the specific animation state name from string
					string newStarterName = starterAnimName.Before(" 'L");

					//Extract the layer number from the string
					int starterLayer = System.Convert.ToInt32(starterAnimName.Between("'L-", "'"));

					gacStatic.animatorController.SetLayerWeight(starterLayer, 1);

					if(animSet.playMode == GAC_AnimationSetup.PlayMode.Normal){

						//Play the animation
						gacStatic.animatorController.Play(newStarterName, starterLayer, 0);

					}else if (animSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
						//Crossfade the animation
						gacStatic.animatorController.CrossFade(newStarterName, animSet.blendTime, starterLayer);
					}

					//Set the animation slot to playing
					gacStatic.animSlots[gacStatic.animationNames.IndexOf(starterAnimName)].isPlaying = true;

					foreach (GAC_AnimationSetup anim in gacStatic.animSlots){

						//if not the animation played, set slot to false
						if(anim.theAnim != starterAnimName){
							anim.isPlaying = false;
						}
					}
				}
				
				//Add to the sequence
				gacStatic.sequenceCounter = gacStatic.sequenceCounter + 1;
				
				//Register as the current animation playing
				gacStatic.currentAnimation = starterAnimName;

				//Check if a starter for a combo first
				if(gacStatic.starterNames.Contains(starterAnimName)){

					//Reference the starter animation slot
					gacStatic.starterSet = gacStatic.starterSlots[gacStatic.starterNames.IndexOf(starterAnimName)];

					//Get any animation that is next in combo sequence that is a delayed animation
					var delayedAnim = gacStatic.starterSet.starterCombos.Select(i => i.animationReference.Where(n => n.sequence == gacStatic.sequenceCounter)).ToList();
					
					//Make sure there is atleast one animation in combo sequence that is a delayed animation
					if(delayedAnim.Count > 0){
						//Set this animation to be able to trigger delayed animations
						gacStatic.animSlots[gacStatic.animationNames.IndexOf(starterAnimName)].delayMode = true;;
					}
				}
				
				//Check if Logging is On
				if (gacStatic.debugMode == DebugMode.AnimationLog || gacStatic.debugMode == DebugMode.All){
					Debug.Log("GACLog - Animation Starter Success! '" + gacStatic.currentAnimation + "' was used to start a combo.");
				}
			}else if(gacStatic.sequenceCounter == 2){ //Otherwise, when sequence is 2, use this initial section for combo links

				foreach (GAC_ComboSetup comboSet in gacStatic.starterSet.starterCombos){
					
					foreach (GAC_AnimationReference animationReference in comboSet.animationReference){

						//Now compare the activator being and the current sequence the ones registered for the animations in the combo
						if(animationReference.activator == activator && animationReference.sequence == gacStatic.sequenceCounter && !animationReference.delayed){
							
							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(gacStatic.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							//If a touch activator called
							if(gacTouchTweak){
								touchTweakTime = gacTapTimeLimit;
								gacTouchTweak = false;
							}

							//Make sure the animation to link is being called within range
							if (animSet.animTime > animSet.linkBegin && (animSet.animTime - touchTweakTime) < animSet.linkEnd){

								//Reference the next animation's slot class
								GAC_AnimationSetup nextAnimSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)];

								if(gacStatic.conType == GAC.ControllerType.Legacy){

									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){

										//Play the new animation
										gacStatic.animationController.Play(animationReference.animName);
										
									}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){


										//Crossfade the new animation
										gacStatic.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
									}
									
								}else if(gacStatic.conType == GAC.ControllerType.Mecanim){
									
									//Extract the specific animation state name from string
									string newAnimName = animationReference.animName.Before(" 'L");
									
									//Extract the layer number from the string
									int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
									
									//Set the weight of layer to play next to 1
									gacStatic.animatorController.SetLayerWeight(animLayer, 1);

									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacStatic.animatorController.Play(newAnimName, animLayer, 0);
										
									}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){

										//Play the new animation
										gacStatic.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
									}
									
									//Set the animation slot to playing
									nextAnimSet.isPlaying = true;
									
									foreach (GAC_AnimationSetup anim in gacStatic.animSlots){
										
										//if not the animation played, set slot to false
										if(anim.theAnim != animationReference.animName){
											anim.isPlaying = false;
										}
									}
									
								}
								
								//Reference all the combo setups that use this animation being called
								gacStatic.starterSet.theReferences = GetComboLists(gacStatic, gacStatic.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacStatic.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacStatic.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacStatic.sequenceCounter = gacStatic.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacStatic.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacStatic.debugMode == DebugMode.AnimationLog || gacStatic.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacStatic.currentAnimation + "' was linked in combo.");
								}
							}
						}else if(animationReference.activator == activator && animationReference.sequence == gacStatic.sequenceCounter && animationReference.delayed){
							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(gacStatic.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							if(animSet.delayTiming){

								//If a touch activator called
								if(gacTouchTweak){
									touchTweakTime = gacTapTimeLimit;
									gacTouchTweak = false;
								}

								//Make sure the animation to link is being called within range with delayed timing
								if(animSet.delayCountDown > (animSet.delayBegin - touchTweakTime) && animSet.delayCountDown < (animSet.delayEnd - touchTweakTime)){

									//Reference the next animation's slot class
									GAC_AnimationSetup nextAnimSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)];

									if(gacStatic.conType == GAC.ControllerType.Legacy){

										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacStatic.animationController.Play(animationReference.animName);
											
										}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
											
											
											//Crossfade the new animation
											gacStatic.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
										}
										
									}else if(gacStatic.conType == GAC.ControllerType.Mecanim){
										
										//Extract the specific animation state name from string
										string newAnimName = animationReference.animName.Before(" 'L");
										
										//Extract the layer number from the string
										int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
										
										//Set the weight of layer to play next to 1
										gacStatic.animatorController.SetLayerWeight(animLayer, 1);

										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacStatic.animatorController.Play(newAnimName, animLayer, 0);
											
										}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){

											//Play the new animation
											gacStatic.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
										}
										
										//Set the animation slot to playing
										nextAnimSet.isPlaying = true;
										
										foreach (GAC_AnimationSetup anim in gacStatic.animSlots){
											
											//if not the animation played, set slot to false
											if(anim.theAnim != animationReference.animName){
												anim.isPlaying = false;
											}
										}
										
									}
								}

								//Reference all the combo setups that use this animation being called
								gacStatic.starterSet.theReferences = GetComboLists(gacStatic, gacStatic.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacStatic.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacStatic.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacStatic.sequenceCounter = gacStatic.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacStatic.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacStatic.debugMode == DebugMode.AnimationLog || gacStatic.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacStatic.currentAnimation + "' was linked in combo.");
								}
							}

						}
					}
				}

			}else{//Otherwise, use this one that continues any combo links for sequences after 2

				foreach (GAC_ComboSetup comboSet in gacStatic.starterSet.theReferences){
					
					foreach (GAC_AnimationReference animationReference in comboSet.animationReference){

						//Now compare the activator being and the current sequence the ones registered for the animations in the combo
						if(animationReference.activator == activator && animationReference.sequence == gacStatic.sequenceCounter && !animationReference.delayed){

							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(gacStatic.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							//If a touch activator called
							if(gacTouchTweak){
								touchTweakTime = gacTapTimeLimit;
								gacTouchTweak = false;
							}

							//Make sure the animation to link is being called within range
							if (animSet.animTime > animSet.linkBegin && (animSet.animTime - touchTweakTime) < animSet.linkEnd){

								//Reference the next animation's slot class
								GAC_AnimationSetup nextAnimSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)];

								if(gacStatic.conType == GAC.ControllerType.Legacy){

									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacStatic.animationController.Play(animationReference.animName);
										
									}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
										
										
										//Crossfade the new animation
										gacStatic.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
									}
									
								}else if(gacStatic.conType == GAC.ControllerType.Mecanim){

									//Extract the specific animation state name from string
									string newAnimName = animationReference.animName.Before(" 'L");
									
									//Extract the layer number from the string
									int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));

									//Set the weight of layer to play next to 1
									gacStatic.animatorController.SetLayerWeight(animLayer, 1);
	
									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacStatic.animatorController.Play(newAnimName, animLayer, 0);
										
									}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){

										//Play the new animation
										gacStatic.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
									}

									//Set the animation slot to playing
									nextAnimSet.isPlaying = true;
									
									foreach (GAC_AnimationSetup anim in gacStatic.animSlots){

										//if not the animation played, set slot to false
										if(anim.theAnim != animationReference.animName){
											anim.isPlaying = false;
										}
									}
									
								}

								//Reference all the combo setups that use this animation being called
								gacStatic.starterSet.theReferences = GetComboLists(gacStatic, gacStatic.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacStatic.starterSet.theReferences.Where(i => i.animationReference[gacStatic.sequenceCounter - 2].delayed == true).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacStatic.sequenceCounter = gacStatic.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacStatic.currentAnimation = animationReference.animName;

								//Check if Logging is On
								if (gacStatic.debugMode == DebugMode.AnimationLog || gacStatic.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacStatic.currentAnimation + "' was linked in combo.");
								}
							}
						}else if(animationReference.activator == activator && animationReference.sequence == gacStatic.sequenceCounter && animationReference.delayed){

							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(gacStatic.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							if(animSet.delayTiming){

								//If a touch activator called
								if(gacTouchTweak){
									touchTweakTime = gacTapTimeLimit;
									gacTouchTweak = false;
								}

								//Make sure the animation to link is being called within range with delayed timing
								if(animSet.delayCountDown > (animSet.delayBegin - touchTweakTime) && animSet.delayCountDown < (animSet.delayEnd - touchTweakTime)){

									//Reference the next animation's slot class
									GAC_AnimationSetup nextAnimSet = gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)];

									if(gacStatic.conType == GAC.ControllerType.Legacy){
			
										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacStatic.animationController.Play(animationReference.animName);
											
										}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
											
											
											//Crossfade the new animation
											gacStatic.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
										}
										
									}else if(gacStatic.conType == GAC.ControllerType.Mecanim){
										
										//Extract the specific animation state name from string
										string newAnimName = animationReference.animName.Before(" 'L");
										
										//Extract the layer number from the string
										int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
										
										//Set the weight of layer to play next to 1
										gacStatic.animatorController.SetLayerWeight(animLayer, 1);

										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacStatic.animatorController.Play(newAnimName, animLayer, 0);
											
										}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){

											//Play the new animation
											gacStatic.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
										}
										
										//Set the animation slot to playing
										nextAnimSet.isPlaying = true;
										
										foreach (GAC_AnimationSetup anim in gacStatic.animSlots){
											
											//if not the animation played, set slot to false
											if(anim.theAnim != animationReference.animName){
												anim.isPlaying = false;
											}
										}
										
									}
								}
							
								//Reference all the combo setups that use this animation being called
								gacStatic.starterSet.theReferences = GetComboLists(gacStatic, gacStatic.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacStatic.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacStatic.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacStatic.animSlots[gacStatic.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacStatic.sequenceCounter = gacStatic.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacStatic.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacStatic.debugMode == DebugMode.AnimationLog || gacStatic.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacStatic.currentAnimation + "' was linked in combo.");
								}
							}


							
						}
					}
				}
			}

		}
		#endregion Single Player PlayAnimation

		#region Default PlayAnimation
		//Used to call the animation and link to create a combo. Specify a target gameobject
		public static void PlayTheAnimation (GameObject target, string starterAnimName, int activator){

			//Keep the time amount to tweak for touch input; helps to balance out the latency with touch to animation response
			float touchTweakTime = 0;
			GAC gacReference = target.GetComponent<GAC>();

			//Early out if the GAC gameobject is not found
			if(gacReference == null){
				Debug.LogError("GACError - The target " + target.name + " does not have the GAC component to use to perform combos! Please add GAC to continue!");
				return;
			}

			//Early out if the starter is not found
			if(!gacStatic.animationNames.Contains(starterAnimName)){
				Debug.LogError("GACError - There is no animation with the name " + starterAnimName + " set to be used as a starter animation in GAC! " +
				               "Please add the corresponding starter animation or check your spelling!");
				return;
			}
			
			//Early out if the activator is not found
			if(!gacStatic.activators.Contains(activator)){
				Debug.LogError("GACError - The activator number " + activator + " is not available for use in GAC! Please add the corresponding activator number using" +
				               " the 'Animation Setup' menu!");
				return;
			}

			//Check if a starter animation is registered in GAC
			if(string.IsNullOrEmpty(gacReference.starterAnimation)){
				if(gacReference.conType == GAC.ControllerType.Legacy){
					
					gacReference.starterAnimation = starterAnimName;
					
				}else if(gacReference.conType == GAC.ControllerType.Mecanim){
					
					string newStarterName = starterAnimName.Before("'L");
					gacReference.starterAnimation = newStarterName;
				}
			}
			
			//Make sure this is the starter animation being called; this is based on the sequence counter being 1
			if(gacReference.sequenceCounter == 1){
				//Reference the current animation's slot class
				GAC_AnimationSetup animSet = gacReference.animSlots[gacReference.animationNames.IndexOf(starterAnimName)];
				
				if(gacReference.conType == GAC.ControllerType.Legacy){
					
					if(animSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
						
						//Play the animation
						gacReference.animationController.Play(starterAnimName);
						
					}else if (animSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){						
						
						//Crossfade the animation
						gacReference.animationController.CrossFade(starterAnimName, animSet.blendTime);
					}
					
				}else if(gacReference.conType == GAC.ControllerType.Mecanim){
					
					//Extract the specific animation state name from string
					string newStarterName = starterAnimName.Before(" 'L");
					
					//Extract the layer number from the string
					int starterLayer = System.Convert.ToInt32(starterAnimName.Between("'L-", "'"));
					
					gacReference.animatorController.SetLayerWeight(starterLayer, 1);
					
					if(animSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
						
						//Play the animation
						gacReference.animatorController.Play(newStarterName, starterLayer, 0);
						
					}else if (animSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
						//Crossfade the animation
						gacReference.animatorController.CrossFade(newStarterName, animSet.blendTime, starterLayer);
					}
					
					//Set the animation slot to playing
					gacReference.animSlots[gacReference.animationNames.IndexOf(starterAnimName)].isPlaying = true;
					
					foreach (GAC_AnimationSetup anim in gacReference.animSlots){
						
						//if not the animation played, set slot to false
						if(anim.theAnim != starterAnimName){
							anim.isPlaying = false;
						}
					}
				}
				
				//Add to the sequence
				gacReference.sequenceCounter = gacReference.sequenceCounter + 1;
				
				//Register as the current animation playing
				gacReference.currentAnimation = starterAnimName;
				
				//Check if a starter for a combo first
				if(gacReference.starterNames.Contains(starterAnimName)){
					gacReference.starterNames.IndexOf(starterAnimName);

					if(gacReference.starterNames.IndexOf(starterAnimName) > -1 && gacReference.starterNames.IndexOf(starterAnimName) < gacReference.starterSlots.Count){

						//Reference the starter animation slot
						gacReference.starterSet = gacReference.starterSlots[gacReference.starterNames.IndexOf(starterAnimName)];

						//Get any animation that is next in combo sequence that is a delayed animation
						var delayedAnim = gacReference.starterSet.starterCombos.Select(i => i.animationReference.Where(n => n.sequence == gacReference.sequenceCounter)).ToList();

						//Make sure there is atleast one animation in combo sequence that is a delayed animation
						if(delayedAnim.Count > 0){
							//Set this animation to be able to trigger delayed animations
							gacReference.animSlots[gacReference.animationNames.IndexOf(starterAnimName)].delayMode = true;;
						}
					}
				}
				
				//Check if Logging is On
				if (gacReference.debugMode == DebugMode.AnimationLog || gacReference.debugMode == DebugMode.All){
					Debug.Log("GACLog - Animation Starter Success! '" + gacReference.currentAnimation + "' was used to start a combo.");
				}

			}else if(gacReference.sequenceCounter == 2){ //Otherwise, when sequence is 2, use this initial section for combo links
				
				foreach (GAC_ComboSetup comboSet in gacReference.starterSet.starterCombos){
					
					foreach (GAC_AnimationReference animationReference in comboSet.animationReference){

						//Now compare the activator being and the current sequence the ones registered for the animations in the combo
						if(animationReference.activator == activator && animationReference.sequence == gacReference.sequenceCounter && !animationReference.delayed){

							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacReference.animSlots[gacReference.animationNames.IndexOf(gacReference.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							//If a touch activator called
							if(gacTouchTweak){
								touchTweakTime = gacTapTimeLimit;
								gacTouchTweak = false;
							}

							//Make sure the animation to link is being called within range
							if (animSet.animTime > animSet.linkBegin && (animSet.animTime - touchTweakTime) < animSet.linkEnd){
								
								//Reference the next animation's slot class
								GAC_AnimationSetup nextAnimSet = gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)];
								
								if(gacReference.conType == GAC.ControllerType.Legacy){
									
									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacReference.animationController.Play(animationReference.animName);
										
									}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
										
										
										//Crossfade the new animation
										gacReference.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
									}
									
								}else if(gacReference.conType == GAC.ControllerType.Mecanim){
									
									//Extract the specific animation state name from string
									string newAnimName = animationReference.animName.Before(" 'L");
									
									//Extract the layer number from the string
									int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
									
									//Set the weight of layer to play next to 1
									gacReference.animatorController.SetLayerWeight(animLayer, 1);
									
									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacReference.animatorController.Play(newAnimName, animLayer, 0);

									}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
										
										//Play the new animation
										gacReference.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
									}
									
									//Set the animation slot to playing
									nextAnimSet.isPlaying = true;
									
									foreach (GAC_AnimationSetup anim in gacReference.animSlots){
										
										//if not the animation played, set slot to false
										if(anim.theAnim != animationReference.animName){
											anim.isPlaying = false;
										}
									}
									
								}
								
								//Reference all the combo setups that use this animation being called
								gacReference.starterSet.theReferences = GetComboLists(gacReference, gacReference.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacReference.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacReference.sequenceCounter)).ToList();

								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)].delayMode = true;
								}

								//Add to the sequence
								gacReference.sequenceCounter = gacReference.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacReference.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.AnimationLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacReference.currentAnimation + "' was linked in combo.");
								}

							}
						}else if(animationReference.activator == activator && animationReference.sequence == gacReference.sequenceCounter && animationReference.delayed){
							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacReference.animSlots[gacReference.animationNames.IndexOf(gacReference.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}


							if(animSet.delayTiming){

								//If a touch activator called
								if(gacTouchTweak){
									touchTweakTime = gacTapTimeLimit;
									gacTouchTweak = false;
								}

								//if(animSet.delayCountDown > (animSet.delayTime - touchTweakTime) && animSet.delayCountDown < (0.5 - touchTweakTime)){
								if(animSet.delayCountDown > (animSet.delayBegin - touchTweakTime) && animSet.delayCountDown < (animSet.delayEnd - touchTweakTime)){
									//Reference the next animation's slot class
									GAC_AnimationSetup nextAnimSet = gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)];
									
									if(gacReference.conType == GAC.ControllerType.Legacy){
										
										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacReference.animationController.Play(animationReference.animName);
											
										}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
											
											
											//Crossfade the new animation
											gacReference.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
										}
										
									}else if(gacReference.conType == GAC.ControllerType.Mecanim){
										
										//Extract the specific animation state name from string
										string newAnimName = animationReference.animName.Before(" 'L");
										
										//Extract the layer number from the string
										int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
										
										//Set the weight of layer to play next to 1
										gacReference.animatorController.SetLayerWeight(animLayer, 1);
										
										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacReference.animatorController.Play(newAnimName, animLayer, 0);
											
										}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){

											//Play the new animation
											gacReference.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
										}
										
										//Set the animation slot to playing
										nextAnimSet.isPlaying = true;
										
										foreach (GAC_AnimationSetup anim in gacReference.animSlots){
											
											//if not the animation played, set slot to false
											if(anim.theAnim != animationReference.animName){
												anim.isPlaying = false;
											}
										}
										
									}
								}

								//Reference all the combo setups that use this animation being called
								gacReference.starterSet.theReferences = GetComboLists(gacReference, gacReference.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacReference.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacReference.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacReference.sequenceCounter = gacReference.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacReference.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.AnimationLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacReference.currentAnimation + "' was linked in combo.");
								}
							}

						}
					}
				}
				
			}else{//Otherwise, use this one that continues any combo links for sequences after 2
				
				foreach (GAC_ComboSetup comboSet in gacReference.starterSet.theReferences){
					
					foreach (GAC_AnimationReference animationReference in comboSet.animationReference){

						//Now compare the activator being and the current sequence the ones registered for the animations in the combo
						if(animationReference.activator == activator && animationReference.sequence == gacReference.sequenceCounter && !animationReference.delayed){
							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacReference.animSlots[gacReference.animationNames.IndexOf(gacReference.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}

							//If a touch activator called
							if(gacTouchTweak){
								touchTweakTime = gacTapTimeLimit;
								gacTouchTweak = false;
							}
							
							//Make sure the animation to link is being called within range
							if (animSet.animTime > animSet.linkBegin && (animSet.animTime - touchTweakTime) < animSet.linkEnd){
								
								//Reference the next animation's slot class
								GAC_AnimationSetup nextAnimSet = gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)];
								
								if(gacReference.conType == GAC.ControllerType.Legacy){
									
									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacReference.animationController.Play(animationReference.animName);
										
									}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
										
										
										//Crossfade the new animation
										gacReference.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
									}
									
								}else if(gacReference.conType == GAC.ControllerType.Mecanim){
									
									//Extract the specific animation state name from string
									string newAnimName = animationReference.animName.Before(" 'L");
									
									//Extract the layer number from the string
									int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
									
									//Set the weight of layer to play next to 1
									gacReference.animatorController.SetLayerWeight(animLayer, 1);
									
									if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
										
										//Play the new animation
										gacReference.animatorController.Play(newAnimName, animLayer, 0);
										
									}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
										
										//Play the new animation
										gacReference.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
									}
									
									//Set the animation slot to playing
									nextAnimSet.isPlaying = true;
									
									foreach (GAC_AnimationSetup anim in gacReference.animSlots){
										
										//if not the animation played, set slot to false
										if(anim.theAnim != animationReference.animName){
											anim.isPlaying = false;
										}
									}
									
								}
								
								//Reference all the combo setups that use this animation being called
								gacReference.starterSet.theReferences = GetComboLists(gacReference, gacReference.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacReference.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacReference.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacReference.sequenceCounter = gacReference.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacReference.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.AnimationLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacReference.currentAnimation + "' was linked in combo.");
								}
							}

						}else if(animationReference.activator == activator && animationReference.sequence == gacReference.sequenceCounter && animationReference.delayed){
							
							//Reference the current animation's slot class
							GAC_AnimationSetup animSet = gacReference.animSlots[gacReference.animationNames.IndexOf(gacReference.currentAnimation)];
							
							//Don't use if not have any objects to check hits against
							if(animSet.layerObjects != null){
								
								///Reset Trigger
								foreach (GameObject theObject in animSet.layerObjects){
									
									//Get reference to the Attack Event script
									GAC_TargetTracker targetTracker = theObject.GetComponent<GAC_TargetTracker>();
									
									targetTracker.didHit = false;
								}
							}
								
							if(animSet.delayTiming){
								
								if(animSet.delayCountDown > (animSet.delayBegin - touchTweakTime) && animSet.delayCountDown < (animSet.delayEnd - touchTweakTime)){
							
									//Reference the next animation's slot class
									GAC_AnimationSetup nextAnimSet = gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)];
									
									if(gacReference.conType == GAC.ControllerType.Legacy){
										
										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacReference.animationController.Play(animationReference.animName);
											
										}else if (nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
											
											
											//Crossfade the new animation
											gacReference.animationController.CrossFade(animationReference.animName, nextAnimSet.blendTime);
										}
										
									}else if(gacReference.conType == GAC.ControllerType.Mecanim){
										
										//Extract the specific animation state name from string
										string newAnimName = animationReference.animName.Before(" 'L");
										
										//Extract the layer number from the string
										int animLayer = System.Convert.ToInt32(animationReference.animName.Between("'L-", "'"));
										
										//Set the weight of layer to play next to 1
										gacReference.animatorController.SetLayerWeight(animLayer, 1);
										
										if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
											
											//Play the new animation
											gacReference.animatorController.Play(newAnimName, animLayer, 0);
											
										}else if(nextAnimSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
											
											//Play the new animation
											gacReference.animatorController.CrossFade(newAnimName, nextAnimSet.blendTime, animLayer);
										}
										
										//Set the animation slot to playing
										nextAnimSet.isPlaying = true;
										
										foreach (GAC_AnimationSetup anim in gacReference.animSlots){
											
											//if not the animation played, set slot to false
											if(anim.theAnim != animationReference.animName){
												anim.isPlaying = false;
											}
										}
										
									}
								}
								
								//Reference all the combo setups that use this animation being called
								gacReference.starterSet.theReferences = GetComboLists(gacReference, gacReference.starterSet, animationReference.animName, activator);

								//Get any animation that is next in combo sequence that is a delayed animation
								var delayedAnim = gacReference.starterSet.theReferences.Select(i => i.animationReference.Where(n => n.sequence == gacReference.sequenceCounter)).ToList();
								
								//Make sure there is atleast one animation in combo sequence that is a delayed animation
								if(delayedAnim.Count > 0){
									//Set this animation to be able to trigger delayed animations
									gacReference.animSlots[gacReference.animationNames.IndexOf(animationReference.animName)].delayMode = true;;
								}

								//Add to the sequence
								gacReference.sequenceCounter = gacReference.sequenceCounter + 1;
								
								//Register as the current animation playing
								gacReference.currentAnimation = animationReference.animName;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.AnimationLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Animation Link Success! '" + gacReference.currentAnimation + "' was linked in combo.");
								}
							}

						}
					}
				}
			}

		}
		#endregion Default PlayAnimation

		//Use to retrieve the combo setups that contain the animation string
		static List<GAC_ComboSetup> GetComboLists(GAC gac, GAC_StarterSetup startSet, string animString, int activator){

			List<GAC_ComboSetup> newReferences = new List<GAC_ComboSetup>();

			GAC_StarterSetup starter = startSet;

			foreach (GAC_ComboSetup comboSet in starter.starterCombos){

				foreach (GAC_AnimationReference animRef in comboSet.animationReference){

					//Compare all the animation names, the activators and the sequence to find all the matches and add them to list
					if(animRef.animName == animString && animRef.sequence == gac.sequenceCounter){
						newReferences.Add (comboSet);
					}
				}
			}

			return newReferences;
		}

		
		#region Target Hit
		public static bool TargetHit(GameObject target, GameObject theObject){
			
			GAC gacReference = theObject.GetComponent<GAC>();
			
			if(gacReference.hitCalled){
				if(target.GetComponent<GAC_TargetTracker>().didHit){
					
					return target.GetComponent<GAC_TargetTracker>().didHit;
					
				}else{
					
					return false;
				}
			}else{
				return false;
			}
			
		}
		#endregion Target Hit
		
		public static bool GetFacingDirection(GameObject theTarget, bool facingDirection){
			
			GAC gacReference = theTarget.GetComponent<GAC>();
			GAC_TargetTracker tracker = theTarget.GetComponent<GAC_TargetTracker>();
			
			if(gacReference != null){
				
				if(gacReference.gameModeIndex == 1){//Check if 2D Mode index selected
					
					// Multiply the player's x local scale by -1.
					Vector3 theScale = theTarget.transform.localScale;
					
					//Check the Scale of the 2D sprite to determine the facing direction
					if(theScale.x == 1){
						
						if(gacReference.directionIndex == 0){//Set to 1
							facingDirection = true;
						}else{
							facingDirection = false;
						}
					}else if(theScale.x == -1){
						
						if(gacReference.directionIndex == 1){//Set to -1
							facingDirection = true;
						}else{
							facingDirection = false;
						}
					}
					
				}
			}else if(tracker != null){
				
				if(tracker.gameModeIndex == 1){//Check if 2D Mode index selected
					
					// Multiply the player's x local scale by -1.
					Vector3 theScale = theTarget.transform.localScale;
					
					//Check the Scale of the 2D sprite to determine the facing direction
					if(theScale.x == 1){
						
						facingDirection = false;
					}else if(theScale.x == -1){
						
						facingDirection = true;
					}
					
				}
			}
			
			return facingDirection;
		}

		static void SetRightFacingDirection(GameObject theTarget, bool direction){
			GAC gacReference = theTarget.GetComponent<GAC>();
			GAC_TargetTracker tracker = theTarget.GetComponent<GAC_TargetTracker>();
			
			if(gacReference != null){
				
				if(gacReference.gameModeIndex == 1){//Check if 2D Mode index selected
					gacReference.facingDirectionRight = direction;
					
				}else{
					Debug.LogError("GACError - This gameobject needs to be set to 2D mode to use facing direction.");
				}
			}else if(tracker != null){
				if(tracker.gameModeIndex == 1){//Check if 2D Mode index selected
					tracker.facingDirectionRight = direction;
					
				}else{
					Debug.LogError("GACError - This gameobject needs to be set to 2D mode to use facing direction.");
				}
			}else{
				Debug.LogError("GACError - There is no GAC or GAC_TargetTracker script on the gameObject " + theTarget.name + ". Please add either one to continue.");
			}
		}

		static public void AddTarget(GameObject target){

			if(!gacStatic.totalTargets.Contains(target)){
				gacStatic.totalTargets.Add (target);

				//Check if Logging is On
				if (gacStatic.debugMode == DebugMode.TargetLog || gacStatic.debugMode == DebugMode.All){
					Debug.Log("GACLog - The Target " + target.name + " is within radius or enabled and is added to Tracker!");
				}
			}

		}

		static public void AddTargetGO(GameObject theObject, GameObject target){

			GAC gacReference = theObject.GetComponent<GAC>();

			if(!gacReference.totalTargets.Contains(target)){
				gacReference.totalTargets.Add (target);

				target.GetComponent<GAC_TargetTracker>().targetId = gacReference.totalTargets.Count;

				//Check if Logging is On
				if (gacReference.debugMode == DebugMode.TargetLog || gacReference.debugMode == DebugMode.All){
					Debug.Log("GACLog - The Target " + target.name + " is within radius or enabled and is added to Tracker!");
				}
			}
			
		}

		static public void RemoveTarget(GameObject target){

			if(gacStatic.totalTargets.Contains(target)){

				target.GetComponent<GAC_TargetTracker>().targetId = 0;
				gacStatic.totalTargets.Remove (target);

				//Check if Logging is On
				if (gacStatic.debugMode == DebugMode.TargetLog || gacStatic.debugMode == DebugMode.All){
					Debug.Log("GACLog - The Target " + target.name + " is out of radius or disabled and is removed from Tracker!");
				}
			}
		}

		static public void RemoveTargetGO(GameObject theObject, GameObject target){

			GAC gacReference = theObject.GetComponent<GAC>();

			if(gacReference.totalTargets.Contains(target)){

				target.GetComponent<GAC_TargetTracker>().targetId = 0;
				gacReference.totalTargets.Remove (target);

				//Check if Logging is On
				if (gacReference.debugMode == DebugMode.TargetLog || gacReference.debugMode == DebugMode.All){
					Debug.Log("GACLog - The Target " + target.name + " is out of radius or disabled and is removed from Tracker!");
				}
			}
			
		}

		static public bool IsPlaying(GameObject target, string animName){

			bool playing = false;

			foreach (GAC_AnimationSetup anim in target.GetComponent<GAC>().gacReference.animSlots){
			
				if (anim.theAnim == animName){

					if(anim.isPlaying){
						playing = true;
					}else{
						playing = false;
					}
				}
			}

			return playing;

		}

		static public bool ArePlaying(GameObject target){

			if(!target.GetComponent<GAC>().gacReference.animationsArePlaying){
				return false;
			}else{

				return true;
			}

		}

		static public string AnimationPlaying(GameObject target){
			
			string animation = "None";
			
			foreach (GAC_AnimationSetup anim in target.GetComponent<GAC>().gacReference.animSlots){
				
				if(anim.isPlaying){
					animation = anim.theAnim;
				}
			}
			
			return animation;
		}


		//Contains code from http://answers.unity3d.com/questions/179310/how-to-find-all-objects-in-specific-layer.html
		private List<GameObject> FindGameObjectsWithLayer (LayerMask layer){
		    
		    List<GameObject> goList = new List<GameObject>();

		    LayerMask objectMask;
		    
		    for (int i= 0; i < totalTargets.Count; i++) {
		    	objectMask = 1 << totalTargets[i].layer;

			    if ((layer.value & objectMask.value) > 0) {

					//Do not add THIS game object to its own list
					if(totalTargets[i] != gameObject){
			    		goList.Add(totalTargets[i]);
					}
			    }

		    }
		    
		    if (goList.Count == 0) {
		    	return null;
		    }
		    
			return goList;
	    }

		#region Affect Object
		//How to affect objects around when an animation is playing
	    private void  AffectObject (GAC_AnimationSetup anim){
	    	
			//Get all the layer objects
			anim.layerObjects = FindGameObjectsWithLayer(anim.affectLayer);

			//Make sure the layer has selected something to affect
			if(anim.affectLayer.value > 0 && anim.layerObjects != null){
				
				foreach (GameObject theObject in anim.layerObjects){

					//Reference the tracker script
					GAC_TargetTracker gacTracker = theObject.GetComponent<GAC_TargetTracker>();

					if (gacTracker != null){
					
						//Check if in 3D mode
						if(gacReference.gameModeIndex == 0){

							for (int i = 0; i < gacTracker.parameterVertices3D.Count; i++){

								//Calculate the range between target and the vertices
								parameterRange3D = gacTracker.parameterVertices3D[i] - gacReference.transform.position;

								//Calculate the angle
								gacTracker.parameterAngles[i] = Vector3.Angle(parameterRange3D, gacReference.transform.forward);

								//Multiply by 2 because the angle set in the inspector gets divided by 2 for the Debug Arc, and this angle starts from forward position and moves left or right
								gacTracker.parameterAngles[i] = gacTracker.parameterAngles[i] * 2;

								//Make sure the horizontal distance is within range 
								if (gacTracker.parameterDistances[i] <= anim.affectDistance){	
									
									//Make sure within angle
									if (gacTracker.parameterAngles[i] <= anim.affectAngle){

										//Check if tracking the height of a target too
										if(anim.heightToggle){

											//Then make sure the height is withing range
											if(gacTracker.gameObject.transform.position.y + gacTracker.parameterPos.y >= gacReference.gameObject.transform.position.y && 
											   gacTracker.gameObject.transform.position.y + gacTracker.parameterPos.y <= gacReference.gameObject.transform.position.y + anim.angleHeight){
											
												//Make sure animation is playing
												if (anim.isPlaying){
													
													//Only proceed if a hit wasn't triggered yet
													if(!gacTracker.didHit){
														
														//Within what part of animation should hits be checked for
														if (anim.animTime > anim.hitBegin && anim.animTime < anim.hitEnd){
															
															//Check if Logging is On
															if (gacReference.debugMode == DebugMode.HitLog || gacReference.debugMode == DebugMode.All){
																Debug.Log("GACLog - Hit Success! '" +  theObject.name + "' was hit with animation '" + gacReference.currentAnimation + "'");
															}
															
															
															//Register the hit trigger
															gacTracker.didHit = true;
															gacTracker.playDamage = true;
															gacReference.hitCalled = true;
															gacReference.animHit = anim;
															
														}	
													}
												}
												
												
												//Check if Logging is On
												if (gacReference.debugMode == DebugMode.HitRangeLog || gacReference.debugMode == DebugMode.All){
													Debug.Log("GACLog - " + theObject.name + " is within hit range at " + gacTracker.parameterDistances.Min() + " " + anim.affectDistance + 
													          " distance and angle " + gacTracker.parameterAngles.Min() + " for animation " + anim.theAnim);
												}
											}
										}else{

											//Make sure animation is playing
											if (anim.isPlaying){
												
												//Only proceed if a hit wasn't triggered yet
												if(!gacTracker.didHit){
													
													//Within what part of animation should hits be checked for
													if (anim.animTime > anim.hitBegin && anim.animTime < anim.hitEnd){
														
														//Check if Logging is On
														if (gacReference.debugMode == DebugMode.HitLog || gacReference.debugMode == DebugMode.All){
															Debug.Log("GACLog - Hit Success! '" +  theObject.name + "' was hit with animation '" + gacReference.currentAnimation + "'");
														}
														
														
														//Register the hit trigger
														gacTracker.didHit = true;
														gacTracker.playDamage = true;
														gacReference.hitCalled = true;
														gacReference.animHit = anim;
														
													}	
												}
											}
											
											
											//Check if Logging is On
											if (gacReference.debugMode == DebugMode.HitRangeLog || gacReference.debugMode == DebugMode.All){
												Debug.Log("GACLog - " + theObject.name + " is within hit range at " + gacTracker.parameterDistances.Min() + " " + anim.affectDistance + 
												          " distance and angle " + gacTracker.parameterAngles.Min() + " for animation " + anim.theAnim);
											}
										}
									}
								}
							}

						}else if (gacReference.gameModeIndex == 1){


							//Get the position based on where the angle is placed - FOR FUTURE USE
							//Vector2 mainPosition = new Vector2(gacReference.transform.position.x - anim.angleHeight.x, 
							 //                                 gacReference.transform.position.y + anim.angleHeight.y);

							for (int i = 0; i < gacTracker.parameterVertices2D.Count; i++){

								if(gacReference.facingDirectionRight){
									parameterRange2D = gacTracker.parameterVertices2D[i] - new Vector2 (gacReference.transform.position.x, gacReference.transform.position.y);
									gacTracker.parameterAngles[i] = Mathf.Rad2Deg * (Mathf.Atan2(parameterRange2D.y, parameterRange2D.x));
									
								}else{
									parameterRange2D = gacTracker.parameterVertices2D[i] + new Vector2 (gacReference.transform.position.x, gacReference.transform.position.y);

									//Can't have negative range
									if(parameterRange2D.x < 0){
										parameterRange2D.x = parameterRange2D.x * -1;
									}
									
									//Check if the target object is on the right of this object
									if(gacReference.transform.position.x > theObject.transform.position.x){

										gacTracker.parameterAngles[i] = Mathf.Rad2Deg * (Mathf.Atan2(parameterRange2D.y, parameterRange2D.x));
									}else{

										gacTracker.parameterAngles[i] = Mathf.Rad2Deg * (Mathf.Atan2(parameterRange2D.y, -parameterRange2D.x));
									}

								}

								//Make sure the angle results are not negative
								if(gacTracker.parameterAngles[i] < 0){
									gacTracker.parameterAngles[i] = gacTracker.parameterAngles[i] + 360;
								}

								if (gacTracker.parameterDistances[i] <= anim.affectDistance){	

									//Make sure within angle
									if (gacTracker.parameterAngles[i] <= anim.affectAngle){
										
										
										//Make sure animation is playing
										if (anim.isPlaying){
											
											//Only proceed if a hit wasn't triggered yet
											if(!gacTracker.didHit){
												
												//Within what part of animation should hits be checked for
												if (anim.animTime > anim.hitBegin && anim.animTime < anim.hitEnd){
													
													//Check if Logging is On
													if (gacReference.debugMode == DebugMode.HitLog || gacReference.debugMode == DebugMode.All){
														Debug.Log("GACLog - Hit Success! '" +  theObject.name + "' was hit with animation '" + gacReference.currentAnimation + "'");
													}


													//Register the hit trigger
													gacTracker.didHit = true;
													gacTracker.playDamage = true;
													gacReference.hitCalled = true;
													gacReference.animHit = anim;
												}	
											}
										}
										

										//Check if Logging is On
										if (gacReference.debugMode == DebugMode.HitRangeLog || gacReference.debugMode == DebugMode.All){
											Debug.Log("GACLog - " + theObject.name + " is within hit range at " + gacTracker.parameterDistances.Min() + " " + anim.affectDistance + 
											          " distance and angle " + gacTracker.parameterAngles.Min() + " for animation " + anim.theAnim);
										}
									}
								}
							}
						}

					}else{
						Debug.LogError("GACError - There is no GAC_TargetTracker script on the gameObject " + theObject.name + ". Please add one to continue.");
					}
		    	}
			}
		    	
	    }
		#endregion Affect Object

		#region Attack Movement
		//Call to move the player when animation is playing
	    void AttackMovement(GAC_AnimationSetup anim){

			if(anim.isPlaying){

				//Within what part of animation should the character be able to move
				if (anim.animTime > anim.moveBegin && anim.animTime < anim.moveEnd){

					//Check if 3D Mode index selected
					if(gacReference.gameModeIndex == 0){
						movementController.Move(transform.right * anim.moveAmountX);
						movementController.Move(transform.up * anim.moveAmountY);
						movementController.Move(transform.forward * anim.moveAmountZ);
						
					}else if(gacReference.gameModeIndex == 1){//Check if 2D Mode index selected

						if(facingDirectionRight){
							//Move using velocity
							movementController2D.velocity = new Vector2(anim.moveAmountX, anim.moveAmountY);
						}else{
							//Move using velocity
							movementController2D.velocity = new Vector2(anim.moveAmountX * -1, anim.moveAmountY);
						}
					}
				}else if (anim.animTime > anim.moveEnd){
					
					if(gacReference.gameModeIndex == 1){//Check if 2D Mode index selected

						//Stop velocity movement
						movementController2D.velocity = Vector2.zero;

					}
					
				}
			}
	    }
		#endregion Attack Movement

		//Call to trigger the event for the activators
		void ActivatorControl(GAC_ActivatorSetup actSet, GameObject theObject, int actIndex){

			setEvent = theObject.GetComponent<GAC_SetEvent>();

			//Set the extra distance to check between the directional positions
			directionalRange = 0.3f;

			//Make sure the component is there
			if(setEvent != null){

				//Make sure the activator is set
				if(actSet.activatorSet){

					#region Key Activator
					//Keyboard Use
					if(actSet.useKey){

						//Check states then call the keycode that was set for activator
						if(actSet.stateIndex == 0){ //Default State

							if(Input.GetKey(actSet.keyInput)){

								if(!actSet.singleInputTriggered){
									//Register time pressed and set input to triggered which will be 0.083 seconds
									actSet.timeInput = 0.13f;
									actSet.singleInputTriggered = true;
								}
							}
							
							if(actSet.singleInputTriggered){
								
								//Countdown the time
								actSet.timeInput -= Time.deltaTime;
								
								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 
									
									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}
							
							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){

								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();

								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;

								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is Key " + actSet.keyInput);
								}

							}
						}else if(actSet.stateIndex == 1){ //Down State

							if(Input.GetKeyDown(actSet.keyInput)){
								
								//Register time pressed and set input to triggered which will be 0.083 seconds
								actSet.timeInput = 0.13f;
								actSet.singleInputTriggered = true;
							}
							
							if(actSet.singleInputTriggered){
								
								//Countdown the time
								actSet.timeInput -= Time.deltaTime;
								
								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 
									
									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}
							
							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){

								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();

								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;

								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is Key " + actSet.keyInput);
								}
							}

						}else if(actSet.stateIndex == 2){//Up State

							if(Input.GetKeyUp(actSet.keyInput)){
								
								//Register time pressed and set input to triggered which will be 0.083 seconds
								actSet.timeInput = 0.13f;
								actSet.singleInputTriggered = true;
							}

							if(actSet.singleInputTriggered){

								//Countdown the time
								actSet.timeInput -= Time.deltaTime;

								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 

									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}

							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){

								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();
								
								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is Key " + actSet.keyInput);
								}
							}

						}
					}
					#endregion Key Activator

					#if UNITY_STANDALONE || UNITY_EDITOR
					#region Mouse Activator
					//Mouse Use
					if (actSet.useMouse){

						//Check states then call the mouse input that was set for activator
						if(actSet.stateIndex == 0){//Default State

							if(Input.GetMouseButton(actSet.mouseIndex)){
								if(!actSet.singleInputTriggered){
									//Register time pressed and set input to triggered which will be 0.083 seconds
									actSet.timeInput = 0.13f;
									actSet.singleInputTriggered = true;
								}
							}
							
							if(actSet.singleInputTriggered){
								
								//Countdown the time
								actSet.timeInput -= Time.deltaTime;
								
								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 
									
									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}
							
							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){
								
								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();
								
								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is " + actSet.mouseInputNames[actSet.mouseIndex] + " Mouse Button");
								}


							}
						}else if(actSet.stateIndex == 1){//Down State

							if(Input.GetMouseButtonDown(actSet.mouseIndex)){
								
								//Register time pressed and set input to triggered which will be 0.083 seconds
								actSet.timeInput = 0.13f;
								actSet.singleInputTriggered = true;
							}
							
							if(actSet.singleInputTriggered){
								
								//Countdown the time
								actSet.timeInput -= Time.deltaTime;
								
								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 
									
									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}
							
							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){
								
								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();

								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is " + actSet.mouseInputNames[actSet.mouseIndex] + " Mouse Button");
								}
							}
							
						}else if(actSet.stateIndex == 2){//Up State

							if(Input.GetMouseButtonUp(actSet.mouseIndex)){
								
								//Register time pressed and set input to triggered which will be 0.083 seconds
								actSet.timeInput = 0.13f;
								actSet.singleInputTriggered = true;
							}
							
							if(actSet.singleInputTriggered){
								
								//Countdown the time
								actSet.timeInput -= Time.deltaTime;
								
								//Check if time elapse to finalize animation trigger
								if (actSet.timeInput <= 0){ 
									
									//Reset and register that all inputs triggered
									actSet.allInputsTriggered = true;
									actSet.singleInputTriggered = false;
									
								}
							}
							
							//Call the animation if all inputs triggered
							if(actSet.allInputsTriggered){
								
								setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
								setEvent.activator = actSet.activatorIndex;
								setEvent.PlayAnimation();

								actSet.activatorTriggered = true;
								actSet.allInputsTriggered = false;
								
								//Check if Logging is On
								if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
									Debug.Log("GACLog - Input used is " + actSet.mouseInputNames[actSet.mouseIndex] + " Mouse Button");
								}
							}
						}

					}
					#endregion Mouse Activator
					#endif

					#region Button Activator
					//Use Input Manager Buttons
					if (actSet.useButton){

						//ff using Unity Input directional joystick/pad
						if (actSet.inputIndex == 0){

							//Get the Vector direction the Stick is pointing in
							Vector2 theDirection = new Vector2(Input.GetAxis(actSet.inputText), Input.GetAxis(actSet.inputTextY));

							if (IsDirection(actSet, theDirection, actSet.directionNames[actSet.directionIndex])){
								
								//Register time pressed and set input to triggered which will be 0.13 seconds
								actSet.timeInput = 0.13f;
								actSet.singleInputTriggered = true;
		
							}
						}else{ //If using Unity Input buttons
						
							//Check states then call the input manager's axes name that was set for activator
							if(actSet.stateIndex == 0){//Default State

								if(Input.GetButton(actSet.inputText)){

									if(!actSet.singleInputTriggered){
										//Register time pressed and set input to triggered which will be 0.083 seconds
										actSet.timeInput = 0.13f;
										actSet.singleInputTriggered = true;
									}
								}
								
								if(actSet.singleInputTriggered){
									
									//Countdown the time
									actSet.timeInput -= Time.deltaTime;
									
									//Check if time elapse to finalize animation trigger
									if (actSet.timeInput <= 0){ 
										
										//Reset and register that all inputs triggered
										actSet.allInputsTriggered = true;
										actSet.singleInputTriggered = false;
										
									}
								}
								
								//Call the animation if all inputs triggered
								if(actSet.allInputsTriggered){
									
									setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
									setEvent.activator = actSet.activatorIndex;
									setEvent.PlayAnimation();

									actSet.activatorTriggered = true;
									actSet.allInputsTriggered = false;
									
									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Unity Input Button " + actSet.inputText);
									}
								}
							}else if(actSet.stateIndex == 1){//Down State

								if(Input.GetButtonDown(actSet.inputText)){
									
									//Register time pressed and set input to triggered which will be 0.083 seconds
									actSet.timeInput = 0.13f;
									actSet.singleInputTriggered = true;
								}
								
								if(actSet.singleInputTriggered){
									
									//Countdown the time
									actSet.timeInput -= Time.deltaTime;
									
									//Check if time elapse to finalize animation trigger
									if (actSet.timeInput <= 0){ 
										
										//Reset and register that all inputs triggered
										actSet.allInputsTriggered = true;
										actSet.singleInputTriggered = false;
										
									}
								}
								
								//Call the animation if all inputs triggered
								if(actSet.allInputsTriggered){
									
									setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
									setEvent.activator = actSet.activatorIndex;
									setEvent.PlayAnimation();

									actSet.activatorTriggered = true;
									actSet.allInputsTriggered = false;

									//Check if Logging is On
									if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
										Debug.Log("GACLog - Input used is Unity Input Button " + actSet.inputText);
									}
								}
								
							}else if(actSet.stateIndex == 2){//Up State

								if(Input.GetButtonUp(actSet.inputText)){
									
									//Register time pressed and set input to triggered which will be 0.083 seconds
									actSet.timeInput = 0.13f;
									actSet.singleInputTriggered = true;
								}
								

							}
						}

						if(actSet.singleInputTriggered){
							
							//Countdown the time
							actSet.timeInput -= Time.deltaTime;
							
							//Check if time elapse to finalize animation trigger
							if (actSet.timeInput <= 0){ 
								
								//Reset and register that all inputs triggered
								actSet.allInputsTriggered = true;
								actSet.singleInputTriggered = false;
								
							}
						}

						//Call the animation if all inputs triggered
						if(actSet.allInputsTriggered){
							
							setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
							setEvent.activator = actSet.activatorIndex;
							setEvent.PlayAnimation();

							actSet.activatorTriggered = true;
							actSet.allInputsTriggered = false;
							
							//Check if Logging is On
							if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
								Debug.Log("GACLog - Input used is Unity Input Button " + actSet.inputText);
							}
						}

					}
					#endregion Button Activator

					#region Touch Activator
					//Touch Use
					if (actSet.useTouch){

						//Set the extra distance to check between the swipe positions
						swipeRange = 0.3f;

						//Set The time that will trigger a hold
						holdTimeTrigger = 0.2f;

						//Set the time limit to recognize when a tap should initiate
						gacTapTimeLimit = 0.065f;

						//Set the minimum length to recognize a swipe
						minSwipeLength = 50;

						//Get touch area aspect calculations
						GetTouchAreas(actSet);

						//Get the Rect Area of the Touch Area
						actSet.areaRect = new Rect(actSet.relativePosition.x, actSet.relativePosition.y, actSet.relativeScale.x, actSet.relativeScale.y);

						#region Regular Touch
						//Only run if there is atleast 1 finger on screen
						if (Input.touches.Length > 0) {

							//Get the reference to the touch
							Touch theTouch = Input.GetTouch(0);
							
							//Get the relative touch position by flipping the y origin bottom left to top left
							Vector2 touchRelative = new Vector2(theTouch.position.x, Screen.height - theTouch.position.y);

							//Make sure another Touch Area other than it's 'Own' is selected
							if(actSet.touchSlotIndex > 0){
								
								//Make sure the Touch Area set to use is valid
								if(gacReference.activatorNames.IndexOf(actSet.setTouchName) > -1){
									
									//Reference the Touch Area set's activator settings
									GAC_ActivatorSetup newActSet = gacReference.activatorSlots[gacReference.activatorNames.IndexOf(actSet.setTouchName)];
									
									
									#if UNITY_STANDALONE || UNITY_WEBPLAYER
									
									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.standaloneSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){
									
										#if UNITY_EDITOR
										GAC_SavedTouchArea standSet = gacReference.standaloneSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - standSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
										
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif
									
									#if UNITY_IOS
									
									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.iosSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){

										
										#if UNITY_EDITOR
										
										GAC_SavedTouchArea iOSSet = gacReference.iosSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - iOSSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
										
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif
									
									#if UNITY_ANDROID
									
									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.androidSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){

										
										#if UNITY_EDITOR
										
										GAC_SavedTouchArea androidSet = gacReference.androidSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - androidSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
										
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif
									
								}
							}


							if (theTouch.phase == TouchPhase.Began || theTouch.phase == TouchPhase.Stationary) {

								//The start position of the touch
								touchStartPos = new Vector2(theTouch.position.x, theTouch.position.y);

								//Register if the mouse position is in the Touch Area
								actSet.touchedArea = actSet.areaRect.Contains(touchRelative);
								
								//Reset the input release trigger 
								inputRelease = false;

								//Retrieve the current amount of fingers on screen
								fingerAmount = Input.touches.Length;
							}

	
							#endregion Regular Touch
						}else{
							#region Simulate Touch With Mouse

							//Set the modifiers to simulate the number of fingers
							if(actSet.touchIndex == 0){//One Finger
								actSet.modifyKey = KeyCode.None;
								
							}else if(actSet.touchIndex == 1){//Two Fingers

								//Check if either regular or num pad numbers pressed
								if(Input.GetKey(KeyCode.Alpha2)){
									actSet.modifyKey = KeyCode.Alpha2;
								}

								if(Input.GetKey(KeyCode.Keypad2)){
									actSet.modifyKey = KeyCode.Keypad2;
								}
							
							}else if(actSet.touchIndex == 2){//Three Fingers

								//Check if either regular or num pad numbers pressed
								if(Input.GetKey(KeyCode.Alpha3)){
									actSet.modifyKey = KeyCode.Alpha3;
								}
								
								if(Input.GetKey(KeyCode.Keypad3)){
									actSet.modifyKey = KeyCode.Keypad3;
								}
							
							}else if(actSet.touchIndex == 3){//Four Fingers

								//Check if either regular or num pad numbers pressed
								if(Input.GetKey(KeyCode.Alpha4)){
									actSet.modifyKey = KeyCode.Alpha4;
								}
								
								if(Input.GetKey(KeyCode.Keypad4)){
									actSet.modifyKey = KeyCode.Keypad4;
								}
							
							}else if(actSet.touchIndex == 4){//Five Fingers

								//Check if either regular or num pad numbers pressed
								if(Input.GetKey(KeyCode.Alpha5)){
									actSet.modifyKey = KeyCode.Alpha5;
								}
								
								if(Input.GetKey(KeyCode.Keypad5)){
									actSet.modifyKey = KeyCode.Keypad5;
								}

							}

							//Get the Rect Area of the Touch Area
							actSet.areaRect = new Rect(actSet.relativePosition.x, actSet.relativePosition.y, actSet.relativeScale.x, actSet.relativeScale.y);

							//Make sure another Touch Area other than it's 'Own' is selected
							if(actSet.touchSlotIndex > 0){

								//Make sure the Touch Area set to use is valid
								if(gacReference.activatorNames.IndexOf(actSet.setTouchName) > -1){

									//Reference the Touch Area set's activator settings
									GAC_ActivatorSetup newActSet = gacReference.activatorSlots[gacReference.activatorNames.IndexOf(actSet.setTouchName)];


									#if UNITY_STANDALONE || UNITY_WEBPLAYER

									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.standaloneSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){
										
										#if UNITY_EDITOR
										
										GAC_SavedTouchArea standSet = gacReference.standaloneSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - standSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
											
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif
										
									#if UNITY_IOS

									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.iosSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){

										#if UNITY_EDITOR
										
										GAC_SavedTouchArea iOSSet = gacReference.iosSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - iOSSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
										
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif

									#if UNITY_ANDROID

									//Make sure resolution selected is within the saved slots range amounts and make sure the resolution index is valid
									if(gacReference.androidSavedSlots.Count > gacReference.inGameResolutionIndex && gacReference.inGameResolutionIndex > -1){

										#if UNITY_EDITOR
										
										GAC_SavedTouchArea androidSet = gacReference.androidSavedSlots[gacReference.inGameResolutionIndex];

										//Get the relative position from the resolutions between TAG Window and Game view
										newActSet.relativePosition = newActSet.touchPosition - androidSet.resOrigin;
										
										//Get the relative scale from the resolutions between TAG Window and Game view
										newActSet.relativeScale = new Vector2(newActSet.touchDimensions.x, newActSet.touchDimensions.y);
										
										#endif
										
										//Register the Rect of the Touch Area using all relative positions
										actSet.areaRect = new Rect(newActSet.relativePosition.x, newActSet.relativePosition.y, newActSet.relativeScale.x, newActSet.relativeScale.y);
									}
									#endif

								}
							}

							#if UNITY_STANDALONE || UNITY_EDITOR
							
							//Get the relative touch position by flipping the y origin bottom left to top left
							Vector2 touchRelative = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);

							//Push down mouse button
							if(Input.GetMouseButtonDown(0)){

								//The start position of the touch
								touchStartPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

								//Register if the mouse position is in the Touch Area
								actSet.touchedArea = actSet.areaRect.Contains(touchRelative);

								//Reset the input release trigger 
								inputRelease = false;

								//Reset fingers
								fingerAmount = 0;

							}
							#endif
							#endregion Simulate Touch With Mouse
						}


						if(actSet.singleInputTriggered){
							
							//Countdown the time
							actSet.timeInput -= Time.deltaTime;
							
							//Check if time elapse to finalize animation trigger
							if (actSet.timeInput <= 0){ 
								
								//Reset and register that all inputs triggered
								actSet.allInputsTriggered = true;
								actSet.singleInputTriggered = false;
								
							}
						}
						
						//Call the animation if all inputs triggered
						if(actSet.allInputsTriggered){

							gacTouchTweak = true;
							setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
							setEvent.activator = actSet.activatorIndex;
							setEvent.PlayAnimation();

							for (int index = 0; index < activatorSlots.Count; index++) {	
								activatorSlots[index].touchedArea = false;

							}

							actSet.activatorTriggered = true;
							actSet.allInputsTriggered = false;
							holdTimeElapsed = 0;


						}

						
					}
					#endregion Touch Activator

					#region Synchro Activator
					if (actSet.useSync){
	
						for (int index = 0; index < actSet.inputStrings.Count; index++) {	
							GAC_SyncSetup syncSet = actSet.syncSlots[index];

							if(actSet.sourceStrings[index] == "Key"){
								
								//Get the keycode value from the string
								retrieveKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), actSet.inputStrings[index]);
								
								if(actSet.stateIndex == 0){
									//Check for key press
									if(Input.GetKey((KeyCode)retrieveKeyCode)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}else if(actSet.stateIndex == 1){
									
									//Check for key press
									if(Input.GetKeyDown((KeyCode)retrieveKeyCode)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}else if(actSet.stateIndex == 2){
									
									//Check for key press
									if(Input.GetKeyUp((KeyCode)retrieveKeyCode)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}
							}
							
							if(actSet.sourceStrings[index] == "Mouse"){

								//Get the mouse value from the string
								retrieveMouse = Int32.Parse(actSet.inputStrings[index]);
								
								if(actSet.stateIndex == 0){
									//Check for mouse press
									if(Input.GetMouseButton((int)retrieveMouse)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}else if(actSet.stateIndex == 1){
									//Check for mouse press
									if(Input.GetMouseButtonDown((int)retrieveMouse)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}else if(actSet.stateIndex == 2){
									//Check for mouse press
									if(Input.GetMouseButton((int)retrieveMouse)){
										
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){
											
											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
											
										}
										
									}
								}
								
							}

							//Check if a Unity Input string
							if(actSet.sourceStrings[index] == "Button"){

								if(syncSet.inputIndex == 0){

									//Get the Vector direction the Stick is pointing in
									Vector2 theDirection = new Vector2(Input.GetAxis(syncSet.inputText), Input.GetAxis(syncSet.inputTextY));


									if (IsDirection(actSet, theDirection, syncSet.directionNames[syncSet.directionIndex])){	
										//Check if input already triggered
										if(!actSet.inputTriggered[index]){

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[index] = true;
										}
										
									}
		
								}else{

									//Check if a Unity Input string
									if(actSet.sourceStrings[index] == "Button"){

										if(actSet.stateIndex == 0){
											//Check for Unity Input press
											if(Input.GetButton(actSet.inputStrings[index])){
												
												//Check if input already triggered
												if(!actSet.inputTriggered[index]){
													
													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;
													actSet.inputTriggered[index] = true;
												}
												
											}
										}else if(actSet.stateIndex == 1){
											//Check for Unity Input press
											if(Input.GetButtonDown(actSet.inputStrings[index])){
												
												//Check if input already triggered
												if(!actSet.inputTriggered[index]){
													
													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;
													actSet.inputTriggered[index] = true;
												}
												
											}

										}else if(actSet.stateIndex == 2){
											//Check for Unity Input press
											if(Input.GetButtonUp(actSet.inputStrings[index])){
												
												//Check if input already triggered
												if(!actSet.inputTriggered[index]){
													
													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;
													actSet.inputTriggered[index] = true;
												}
												
											}
										}
									
									}
								}
								
							}


							//Set the time allowed between input presses
							if (Time.time > actSet.timeSinceInput + 0.083){ 
								
								//Reset input amount
								actSet.inputCountGoal = 0;
								
								//Set all triggered inputs to false
								for (int i = 0; i < actSet.inputTriggered.Count; i++) {	
									
									actSet.inputTriggered[i] = false;
								}

								//Check if there is an interrupted activator
								if(actSet.interruptedActivator != null){
									
									//Enable the interrupted activators input
									actSet.interruptedActivator.singleInputTriggered = true;
									actSet.interruptedActivator.timeInput = 0;
									actSet.interruptedActivator = null;
								}
							}
							
							//If atleast 2 inputs registered
							if(actSet.inputCountGoal > 1){

								if(!actSet.syncInputTriggered){
									//Register time pressed and set input to triggered which will be 0.083 seconds
									actSet.timeInput = 0.11f;
									actSet.syncInputTriggered = true;
								}

								//Set all single triggered inputs to false
								for (int i = 0; i < activatorSlots.Count; i++) {
									if(activatorSlots[i].singleInputTriggered){
										//Register the activator slot that was interrupted from single input
										actSet.interruptedActivator = activatorSlots[i];
									}
									
									//Disable single input
									activatorSlots[i].singleInputTriggered = false;

									if(actSet != activatorSlots[i]){
										activatorSlots[i].activatorTriggered = false;
									}
								}
							}

							//Check if the total inputs triggered matches the amount of inputs to call
							if (actSet.inputCountGoal >= actSet.inputStrings.Count){
								if(actSet.syncInputTriggered){
									
									//Countdown the time
									actSet.timeInput -= Time.deltaTime;
									
									//Check if time elapse to finalize animation trigger
									if (actSet.timeInput <= 0){ 
										
										//Reset and register that all inputs triggered
										actSet.allInputsTriggered = true;
										actSet.syncInputTriggered = false;
										
									}
								}

							
							}else {
								actSet.allInputsTriggered = false;

							}

							
							//Check if all inputs triggered
							if(actSet.allInputsTriggered){
								
								foreach (GAC_ActivatorSetup actSlots in activatorSlots){
									
									//Wait till the last index
									if(activatorSlots.IndexOf(actSlots) == activatorSlots.Count - 1){
										
										//Check if the loop count matches total sychro activators times 3; this gives enough time to check input triggers 
										//for each slot
										if(inputLoopCount == synchroAmounts){
											
											//Retrieve all the activator slots that had all inputs triggered...
											//This is used for when sync activators have similar inputs setup with one or more input difference
											var triggeredSlots = activatorSlots.Where(i => i.allInputsTriggered == true).ToList();
											
											//Make sure there are more than one slot in the triggered list
											if(triggeredSlots.ToList().Count > 1){
												
												//Get the maximum amount of inputs between the triggered slots...the one that does have more inputs overrides 
												//the other being activated
												//Eg. Slot 1 has A+B+C and Slot 2 has A+B+C+D set. If player pushes for Slot 2, Slot 1 would naturally override because 
												//those inputs would always be called first based on input frames. So we need to specify Slot 2 to be activated over 
												//Slot 1 cause thats what the player wanted
												int maxInputs = triggeredSlots.Max(i => i.inputStrings.Count);
												
												//Now we need to reference that slot that has more inputs setup
												var maxSlot = triggeredSlots.Where(i => i.inputStrings.Count == maxInputs).ToList();
												
												//Reference that new activator slot to use by getting the index
												var newActSet = activatorSlots[activatorSlots.IndexOf(maxSlot[maxSlot.Count - 1])];

												newActSet.activatorTriggered = true;

												//Play the animation using the new slot
												setEvent.animName = gacReference.addedStarters[newActSet.animationIndex];
												setEvent.activator = newActSet.activatorIndex;
												setEvent.PlayAnimation();
												
												//Set all single triggered inputs to false
												for (int i = 0; i < activatorSlots.Count; i++) {	
													activatorSlots[i].singleInputTriggered = false;
												}

												actSet.activatorTriggered = true;
												actSet.allInputsTriggered = false;
												
												//Check if Logging is On
												if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
													Debug.Log("GACLog - Input used is Synced " + actSet.syncedString);
												}
											}else{//Otherwise continue to play animation for the slot normally

												actSet.activatorTriggered = true;

												setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
												setEvent.activator = actSet.activatorIndex;
												setEvent.PlayAnimation();
												
												//Set all single triggered inputs to false
												for (int i = 0; i < activatorSlots.Count; i++) {	
													activatorSlots[i].singleInputTriggered = false;
												}

												//Check if Logging is On
												if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
													Debug.Log("GACLog - Input used is Synced " + actSet.syncedString);
												}
											}
											
											//Reset the looping
											inputLoopCount = 0;
										}
										
										//Increase the looping
										inputLoopCount++;	
									}
									
									
								}	
								
							}

						}

					}

					#endregion Synchro Activator


					#region Sequence Activator
					if (actSet.useSequence){

							
						if(actSet.inputCountGoal < actSet.inputStrings.Count) {	
							
							//GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[actSet.inputCountGoal];
							
							if(actSet.sourceStrings[actSet.inputCountGoal] == "Key"){
								
								//Get the keycode value from the string
								retrieveKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), actSet.inputStrings[actSet.inputCountGoal]);
								
								if(actSet.stateIndex == 0){
									//Check for key press
									if(Input.GetKey((KeyCode)retrieveKeyCode)){

										//Register time pressed and increase the total inputs triggered
										actSet.timeSinceInput = Time.time;
										actSet.inputCountGoal++;

									}
								}else if(actSet.stateIndex == 1){
									
									//Check for key press
									if(Input.GetKeyDown((KeyCode)retrieveKeyCode)){
										

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;

									}
								}else if(actSet.stateIndex == 2){
									
									//Check for key press
									if(Input.GetKeyUp((KeyCode)retrieveKeyCode)){
										

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;

									}
								}

							}
						}

						if(actSet.inputCountGoal < actSet.inputStrings.Count) {	
							
							GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[actSet.inputCountGoal];

							if(actSet.sourceStrings[actSet.inputCountGoal] == "Mouse"){
								
								//Get the mouse value from the string
								retrieveMouse = Int32.Parse(actSet.inputStrings[actSet.inputCountGoal]);
								
								if(actSet.stateIndex == 0){
									//Check for mouse press
									if(Input.GetMouseButton((int)retrieveMouse)){
										

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;

									}
								}else if(actSet.stateIndex == 1){
									//Check for mouse press
									if(Input.GetMouseButtonDown((int)retrieveMouse)){
										

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;

									}
								}else if(actSet.stateIndex == 2){
									//Check for mouse press
									if(Input.GetMouseButton((int)retrieveMouse)){
										

											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;

									}
								}
								
							}
							
							//Check if a Unity Input string
							if(actSet.sourceStrings[actSet.inputCountGoal] == "Button"){
								
								if(sequenceSet.inputIndex == 0){
									
									//Get the Vector direction the Stick is pointing in
									Vector2 theDirection = new Vector2(Input.GetAxis(sequenceSet.inputText), Input.GetAxis(sequenceSet.inputTextY));
									
									
									if (IsDirection(actSet, theDirection, sequenceSet.directionNames[sequenceSet.directionIndex])){	


											//Register time pressed and increase the total inputs triggered
											actSet.timeSinceInput = Time.time;
											actSet.inputCountGoal++;
											actSet.inputTriggered[actSet.inputCountGoal] = true;
									
										
									}
									
								}else{
									
									//Check if a Unity Input string
									if(actSet.sourceStrings[actSet.inputCountGoal] == "Button"){
										
										if(actSet.stateIndex == 0){
											//Check for Unity Input press
											if(Input.GetButton(actSet.inputStrings[actSet.inputCountGoal])){

													
													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;
												}
												
										}else if(actSet.stateIndex == 1){
											//Check for Unity Input press
											if(Input.GetButtonDown(actSet.inputStrings[actSet.inputCountGoal])){

													
													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;

											}
											
										}else if(actSet.stateIndex == 2){
											//Check for Unity Input press
											if(Input.GetButtonUp(actSet.inputStrings[actSet.inputCountGoal])){

													//Register time pressed and increase the total inputs triggered
													actSet.timeSinceInput = Time.time;
													actSet.inputCountGoal++;

											}
										}
										
									}
								}
								
							
							}//Check if a Sync Input string
							else if(actSet.sourceStrings[actSet.inputCountGoal] == "Sync"){

								//Get the mathing activator name for the set input string
								List<string> getActivatorName = gacReference.activatorNames.Where (i => i.IndexOf(actSet.inputStrings[actSet.inputCountGoal]) > -1).ToList();

								//Then find it's index
								int nameIndex = gacReference.activatorNames.IndexOf(getActivatorName[0]);

								//Check for Synchro Inputs called
								if(SyncActivated(actSet.stateIndex, activatorSlots[nameIndex])){

									//Register time pressed and increase the total inputs triggered
									actSet.timeSinceInput = Time.time;
									actSet.inputCountGoal++;

								}
							}
							
						}	
						
						
						//Set the time allowed between input presses
						if (Time.time > actSet.timeSinceInput + 0.125){ 
							
							//Reset input amount
							actSet.inputCountGoal = 0;
							
							//Set all triggered inputs to false
							for (int i = 0; i < actSet.inputTriggered.Count; i++) {	
								
								actSet.inputTriggered[i] = false;
							}
							//Check if there is an interrupted activator
							if(actSet.interruptedActivator != null){
								
								//Enable the interrupted activators input
								actSet.interruptedActivator.singleInputTriggered = true;
								actSet.interruptedActivator.timeInput = 0;
								actSet.interruptedActivator = null;
							}
						}
						
						//If atleast 2 inputs registered
						if(actSet.inputCountGoal > 0){
							
							//Set all single triggered inputs to false
							for (int i = 0; i < activatorSlots.Count; i++) {
								if(activatorSlots[i].singleInputTriggered){
									//Register the activator slot that was interrupted from single input
									actSet.interruptedActivator = activatorSlots[i];
								}
								
								//Disable single input
								activatorSlots[i].singleInputTriggered = false;

								if(actSet != activatorSlots[i]){
									activatorSlots[i].activatorTriggered = false;
								}
							}
						}
						
						//Check if the total inputs triggered matches the amount of inputs to call
						if (actSet.inputCountGoal >= actSet.inputStrings.Count){
							actSet.allInputsTriggered = true;

							foreach(var act in activatorSlots){
								if(act != actSet){
									act.allInputsTriggered = false;
									act.activatorTriggered = false;
								}
							}
						}else {
							actSet.allInputsTriggered = false;
							
						}
						
						//Check if all inputs triggered
						if(actSet.allInputsTriggered){

							actSet.activatorTriggered = true;

							//Otherwise continue to play animation for the slot normally
							setEvent.animName = gacReference.addedStarters[actSet.animationIndex];
							setEvent.activator = actSet.activatorIndex;
							setEvent.PlayAnimation();
							
							
							//Set all single triggered inputs to false
							for (int i = 0; i < activatorSlots.Count; i++) {	
								activatorSlots[i].singleInputTriggered = false;
							}

							//Check if Logging is On
							if (gacReference.debugMode == DebugMode.InputLog || gacReference.debugMode == DebugMode.All){
								Debug.Log("GACLog - Input used is Sequenced " + actSet.sequencedString);
							}
							
						}


					}
					#endregion Sequence Activator
				}
					
		
			}
	
		}

		bool IsDirection(GAC_ActivatorSetup actSet, Vector2 theDirection, string directionType){
			
			if(theDirection.y > directionalRange && theDirection.x > -directionalRange && theDirection.x < directionalRange){//Point Up
				if(directionType == "Up"){
					return true;
				}else{
					return false;
				}
			}else if (theDirection.y > directionalRange && theDirection.x < directionalRange) {//Point Up Left
				if(directionType == "Up Left"){
					return true;
				}else{
					return false;
				}
			}else if (theDirection.y > directionalRange && theDirection.x > directionalRange) {//Point Up Right
				if(directionType == "Up Right"){
					return true;
				}else{
					return false;
				}
			}else if (theDirection.x < -directionalRange && theDirection.y > -directionalRange && theDirection.y < directionalRange){//Point Left
				if(directionType == "Left"){

					return true;
				}else{
					return false;
				}
			}else if (theDirection.x > directionalRange && theDirection.y > -directionalRange && theDirection.y < directionalRange) {//Point Right
				if(directionType == "Right"){

					return true;
				}else{
					return false;
				}
			}else if (theDirection.y < -directionalRange && theDirection.x > -directionalRange && theDirection.x < directionalRange) {//Point Down
				if(directionType == "Down"){
					
					return true;
				}else{
					return false;
				}
			}else if (theDirection.y < -directionalRange && theDirection.x < directionalRange) {//Point Down Left
				if(directionType == "Down Left"){
					return true;
				}else{
					return false;
				}
			}else if (theDirection.y < -directionalRange  && theDirection.x > directionalRange) {//Point Down Right
				if(directionType == "Down Right"){
					
					return true;
				}else{
					return false;
				}
			}else{
				return false;
			}
			
		}

		private bool SyncActivated(int state, GAC_ActivatorSetup actSet){
		

			if(state == 1){//Down State

				for (int index = 0; index < actSet.inputStrings.Count; index++) {	
					
					if(actSet.sourceStrings[index] == "Key"){
						
						//Get the keycode value from the string
						retrieveKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), actSet.inputStrings[index]);
						
						//Check for key press
						if(Input.GetKeyDown((KeyCode)retrieveKeyCode)){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
								
							}
							
						}
					}
					
					if(actSet.sourceStrings[index] == "Mouse"){
						
						//Get the mouse value from the string
						retrieveMouse = Int32.Parse(actSet.inputStrings[index]);
						
						//Check for mouse press
						if(Input.GetMouseButtonDown((int)retrieveMouse)){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
								
							}
							
						}
						
					}
					
					//Check if a Unity Input string
					if(actSet.sourceStrings[index] == "Button"){
						
						//Check for Unity Input press
						if(Input.GetButtonDown(actSet.inputStrings[index])){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
							}
							
						}
					}
					
					
				}
			
				//Set the time allowed between input presses
				if (Time.time > actSet.timeSinceInput + 0.05){ 
					
					//Reset input amount
					actSet.inputCountGoal = 0;

					//Set all triggered inputs to false
					for (int i = 0; i < actSet.inputTriggered.Count; i++) {	
						
						actSet.inputTriggered[i] = false;

					}

					return false;
				}


				//Check if the total inputs triggered matches the amount of inputs to call
				if (actSet.inputCountGoal >= actSet.inputStrings.Count){
					actSet.allInputsTriggered = true;
				}else {
					actSet.allInputsTriggered = false;
				}

				//Check if all inputs triggered
				if(actSet.allInputsTriggered){

					//Set all single triggered inputs to false
					for (int i = 0; i < activatorSlots.Count; i++) {	
						activatorSlots[i].singleInputTriggered = false;
					}

					return true;

				}else{
					return false;
				}

			}else if(actSet.stateIndex == 2){ //Up State
			
				for (int index = 0; index < actSet.inputStrings.Count; index++) {	
				
					if(actSet.sourceStrings[index] == "Key"){
						
						//Get the keycode value from the string
						retrieveKeyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), actSet.inputStrings[index]);
						
						//Check for key press
						if(Input.GetKeyUp((KeyCode)retrieveKeyCode)){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
								
							}
							
						}
					}
						
					if(actSet.sourceStrings[index] == "Mouse"){
						
						//Get the mouse value from the string
						retrieveMouse = Int32.Parse(actSet.inputStrings[index]);
						
						//Check for mouse press
						if(Input.GetMouseButtonUp((int)retrieveMouse)){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
								
							}
							
						}
						
					}
						
					//Check if a Unity Input string
					if(actSet.sourceStrings[index] == "Button"){
						
						//Check for Unity Input press
						if(Input.GetButtonUp(actSet.inputStrings[index])){
							
							//Check if input already triggered
							if(!actSet.inputTriggered[index]){
								
								//Register time pressed and increase the total inputs triggered
								actSet.timeSinceInput = Time.time;
								actSet.inputCountGoal++;
								actSet.inputTriggered[index] = true;
							}
							
						}
					}
					
					
				}

				//Check if the total inputs triggered matches the amount of inputs to call
				if (actSet.inputCountGoal >= actSet.inputStrings.Count){
					actSet.allInputsTriggered = true;
				}else {
					actSet.allInputsTriggered = false;
				}
					
				//Set the time allowed between input presses
				if (Time.time > actSet.timeSinceInput + 0.05){ 
					
					//Reset input amount
					actSet.inputCountGoal = 0;
					
					//Set all triggered inputs to false
					for (int i = 0; i < actSet.inputTriggered.Count; i++) {	
						
						actSet.inputTriggered[i] = false;
						
					}
					
					return false;
				}
				
				//Check if all inputs triggered
				if(actSet.allInputsTriggered){
					
					//Set all single triggered inputs to false
					for (int i = 0; i < activatorSlots.Count; i++) {	
						activatorSlots[i].singleInputTriggered = false;
					}
					
					return true;
					
				}else{
					return false;
				}
		
			}else{
				return false;
			}
		}

		void GetTouchAreas(GAC_ActivatorSetup actSet){

			//Check if to disable showing the touch areas in Play Mode/Build
			if(!gacReference.tagInBuild){
				activatorSlots.Select(i => {i.showTouchArea = false; return i;}).ToList ();
			}

			//Convert from arrays to list
			gacReference.resolutionNamesList = new List<string>(gacReference.resolutionNames);
			
			//Search for a match to the current resolution from the list
			List<string> currentResolutionFound = gacReference.resolutionNamesList.FindAll(delegate(string s) { return s.Contains(Screen.width + "x" + Screen.height); });
			
			//If there was a match
			if (currentResolutionFound.Count > 0){
				
				//Then register as the in game resolution index to use
				gacReference.inGameResolutionIndex = gacReference.resolutionNamesList.IndexOf(currentResolutionFound[0]);
			}else{
				//Otherwise reigster it out of index
				gacReference.inGameResolutionIndex = -1;
			}

			
			//Make sure the resolution index is valid
			if(gacReference.inGameResolutionIndex > -1){
				
				#if UNITY_STANDALONE
				GAC_SavedTouchArea standSet = gacReference.standaloneSavedSlots[gacReference.inGameResolutionIndex];
				
				if(standSet.saved){

					//Check if to disable showing the touch areas in Play Mode/Build
					if(!gacReference.tagInBuild){
						standSet.actSlots.Select(i => {i.showTouchArea = false; return i;}).ToList ();
					}

					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * (actSet.touchPosition.x - 
					                                                                                                              standSet.resOrigin.x), gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * 
					                                      (actSet.touchPosition.y - standSet.resOrigin.y));
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.x, 
					                                   gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.y);
					
					#if UNITY_EDITOR
					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(actSet.touchPosition.x - standSet.resOrigin.x, actSet.touchPosition.y - standSet.resOrigin.y);
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);
					
					#endif


				}
				#endif
				
				#if UNITY_IOS
				GAC_SavedTouchArea iOSSet = gacReference.iosSavedSlots[gacReference.inGameResolutionIndex];
				
				if(iOSSet.saved){

					//Check if to disable showing the touch areas in Play Mode/Build
					if(!gacReference.tagInBuild){
						iOSSet.actSlots.Select(i => {i.showTouchArea = false; return i;}).ToList ();
					}

					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * (actSet.touchPosition.x - 
					                                                                                                              iOSSet.resOrigin.x), gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * 
					                                      (actSet.touchPosition.y - iOSSet.resOrigin.y));
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.x, 
					                                   gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.y);
					
					#if UNITY_EDITOR
					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(actSet.touchPosition.x - iOSSet.resOrigin.x, actSet.touchPosition.y - iOSSet.resOrigin.y);
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);
					
					#endif

				}
				#endif
				
				#if UNITY_ANDROID
				GAC_SavedTouchArea androidSet = gacReference.androidSavedSlots[gacReference.inGameResolutionIndex];
				
				if(androidSet.saved){

					//Check if to disable showing the touch areas in Play Mode/Build
					if(!gacReference.tagInBuild){
						androidSet.actSlots.Select(i => {i.showTouchArea = false; return i;}).ToList ();
					}

					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * (actSet.touchPosition.x - 
					                                                                                                              androidSet.resOrigin.x), gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * 
					                                      (actSet.touchPosition.y - androidSet.resOrigin.y));
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.x, 
					                                   gacReference.resolutionScaleFactor[gacReference.inGameResolutionIndex] * actSet.touchDimensions.y);

					#if UNITY_EDITOR
					//Get the relative position from the resolutions between Scene and Game view
					actSet.relativePosition = new Vector2(actSet.touchPosition.x - androidSet.resOrigin.x, actSet.touchPosition.y - androidSet.resOrigin.y);
					
					//Get the relative scale from the resolutions between Scene and Game view
					actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);
					
					#endif
				}
				#endif
			}

		}
	
	}

}