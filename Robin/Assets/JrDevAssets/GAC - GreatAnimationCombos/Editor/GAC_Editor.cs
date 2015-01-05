using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using JrDevAssets;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AnimatorController = UnityEditorInternal.AnimatorController;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.7
//GAC_Editor.cs
/////////////////////////////////////////////////////////////////////////////////////////

[CustomEditor(typeof(GAC))]
public class GAC_Editor : Editor {

	[HideInInspector]
	public GAC gacSettings;

	[HideInInspector]
	public GAC_SetEvent gacComponent;
	public GAC_InEditorTouchAreas gacTAComponent;

	public Animation animationComponent;
	public Animator animatorComponent;
	public UnityEditorInternal.AnimatorController animatorController;

	public CharacterController movementController; //Character controller for movement
	public Rigidbody2D movementController2D; //Rigidbody controller for 2D movement

	public GAC_TAG tagWindow;

	bool animEditorExpand = true; //animation editor foldout

	Rect guiDefaultPosition; //Setup rectangle dimensions to use
	bool warningMode; //Trigger for knowing the GAC System has errors
	int moveIndex;//Used to register the index to move around/reorganize the activators
	public List<int> layersUsed = new List<int>(); //Keep a list of all layers in animator
	public List<int> stateCount = new List<int>(); //Keep a list of the animation counts of each layer
	public int layersSum; //Total sum of animations between all layers


	void OnEnable(){

		//Reference the components
		gacSettings = Selection.activeGameObject.GetComponent<GAC>();
		animationComponent = Selection.activeGameObject.GetComponent<Animation>();
		animatorComponent = Selection.activeGameObject.GetComponent<Animator>();

		if(animationComponent != null){
			gacSettings.conType = GAC.ControllerType.Legacy;
		}else if(animatorComponent != null){
			gacSettings.conType = GAC.ControllerType.Mecanim;
		}

	}

	public override void  OnInspectorGUI (){
		
		//Cast the GAC script from target to have the Inspector show on this script
		gacSettings = target as GAC;

		//Check if GAC script has any warnings
		if(warningMode){
			JrDevArts_Utilities.PlayModeWarning();
		}

		//Set the max amount for the global activators to 100
		if(gacSettings.globalActivators.Length == 0){
			gacSettings.globalActivators = new int[101];
		} 

		//Reference the Assets
		GAC.images = Resources.Load("GAC_Images",typeof(GAC_Images)) as GAC_Images;
		GAC.gacSkins = Resources.Load("GAC_Skins",typeof(GUISkin)) as GUISkin;

		#region GAC EDITOR
		GACEditor();
		GetTAGEditor();
		#endregion GAC EDITOR

	}

	public void GACEditor(){

		//Record changes for undo
		Undo.RecordObject (gacSettings, "Record GAC");

		EditorGUILayout.BeginHorizontal();

		//Reset the position dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		//Show the header
		GUI.DrawTexture(new Rect(guiDefaultPosition.x, guiDefaultPosition.y, GAC.images.gacHeader.width, GAC.images.gacHeader.height), GAC.images.gacHeader);
		EditorGUILayout.EndHorizontal();
		

		EditorGUILayout.BeginHorizontal();
		
		//Reset the position dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		//Show the Animation Normal image when menu is not active
		if(!gacSettings.animSetup){
			
			//Make the Animation Setup menu active if button is pressed
			if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y + 40, GAC.images.gacAnimSetupNormal.width, GAC.images.gacAnimSetupNormal.height), GAC.images.gacAnimSetupNormal)) {
				gacSettings.animSetup = true;
				gacSettings.comboSetup = false;
				gacSettings.activatorSetup = false;
			}
		}else{
			//Show the Animation Selected image when menu is active
			GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y + 40, GAC.images.gacAnimSetupSelected.width, GAC.images.gacAnimSetupSelected.height), GAC.images.gacAnimSetupSelected);
		}
		
		if(!gacSettings.comboSetup){
			
			//Make the Combo Setup menu active if button is pressed
			if (GUI.Button(new Rect (guiDefaultPosition.x + 110, guiDefaultPosition.y + 40, GAC.images.gacComboSetupNormal.width, GAC.images.gacComboSetupNormal.height), GAC.images.gacComboSetupNormal)) {
				gacSettings.animSetup = false;
				gacSettings.comboSetup = true;
				gacSettings.activatorSetup = false;
			}
		}else{
			//Show the Combo Selected image when menu is active
			GUI.Button(new Rect (guiDefaultPosition.x + 110, guiDefaultPosition.y + 40, GAC.images.gacComboSetupSelected.width, GAC.images.gacComboSetupSelected.height),GAC.images.gacComboSetupSelected);
		}
		
		if(!gacSettings.activatorSetup){
			
			//Make the Combo Setup menu active if button is pressed
			if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y + 40, GAC.images.gacActivatorSetupNormal.width, GAC.images.gacActivatorSetupNormal.height), GAC.images.gacActivatorSetupNormal)) {
				gacSettings.animSetup = false;
				gacSettings.comboSetup = false;
				gacSettings.activatorSetup = true;
			}
		}else{
			//Show the Combo Selected image when menu is active
			GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y + 40, GAC.images.gacActivatorSetupSelected.width, GAC.images.gacActivatorSetupSelected.height),GAC.images.gacActivatorSetupSelected);
		}
		
		EditorGUILayout.EndHorizontal();

		//Make sure this SetEvent is not on game object if there are no activators slots
		if(gacSettings.activatorSlots.Count == 0){
			
			//Reference the GAC Event component
			gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
			
			if(gacComponent != null){
				DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());
			}
		}

		
		//If no menu is active, divert to activating the Animation Setup menu
		if(!gacSettings.animSetup && !gacSettings.comboSetup && !gacSettings.activatorSetup){
			gacSettings.animSetup = true;
		}

		//Set each global activator in list to the corresponding index
		for (int i = 0; i < gacSettings.globalActivators.Length; i++){
			gacSettings.globalActivators[i] = i;
		}

		//Check if activators totals is less than the global activator amount set
		if(gacSettings.activators.Count < gacSettings.globalActivatorIndex){

			//Then add more to match
			gacSettings.activators.Add (gacSettings.activators.Count + 1);
		}else if(gacSettings.activators.Count > gacSettings.globalActivatorIndex){

			//Then remove them to match
			gacSettings.activators.RemoveAt (gacSettings.activators.Count - 1);
		}

		//Make sure the activator names list is not more than the actual amount of activators
		if (gacSettings.activatorNames.Count > gacSettings.activatorSlots.Count){
			gacSettings.activatorNames.RemoveAt(gacSettings.activatorNames.Count - 1);
		}

		if(gacSettings.activatorsForStarters.Count < gacSettings.addedStarters.Count){
			for (int i = 0; i < gacSettings.addedStarters.Count; i++) {

				//Add a new starter to track its activators
				gacSettings.activatorsForStarters.Add ("Starter " + gacSettings.addedStarters[i]);
			}

			for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
				GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];

				if(actSet.activatorSet){
					SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
				}
			}
		}

		if (Event.current.type == EventType.Repaint){

			//Check if 3D Mode index selected
			if(gacSettings.gameModeIndex == 0){
				movementController = Selection.activeGameObject.GetComponent<CharacterController>();
				movementController2D = Selection.activeGameObject.GetComponent<Rigidbody2D>();

				if(movementController2D != null){
					//Show the dialog to decide
					if(EditorUtility.DisplayDialog("You are currently in 2D Mode!", "This will remove the RigidBody2D component. Do you want to continue?", "Yes", "No")){

						DestroyImmediate(movementController2D);

						if(movementController == null){
							movementController = Selection.activeGameObject.AddComponent<CharacterController>();
						}

						//Move the component up to be above the GAC component
						UnityEditorInternal.ComponentUtility.MoveComponentUp (movementController);
					}else{
						gacSettings.gameModeIndex = 1;
					}
				}else{

					if(movementController == null){
						movementController = Selection.activeGameObject.AddComponent<CharacterController>();
					}
				}

			}else if(gacSettings.gameModeIndex == 1){//Check if 2D Mode index selected
				movementController = Selection.activeGameObject.GetComponent<CharacterController>();
				movementController2D = Selection.activeGameObject.GetComponent<Rigidbody2D>();
				
				if(movementController != null){
					//Show the dialog to decide
					if(EditorUtility.DisplayDialog("You are currently in 3D Mode!", "This will remove the CharacterController component. Do you want to continue?", "Yes", "No")){

						DestroyImmediate(movementController);

						if(movementController2D == null){
							movementController2D = Selection.activeGameObject.AddComponent<Rigidbody2D>();
						}

						//Move the component up to be above the GAC component
						UnityEditorInternal.ComponentUtility.MoveComponentUp (movementController2D);

					}else{
						gacSettings.gameModeIndex = 0;
					}
				}else{
					
					if(movementController2D == null){
						movementController2D = Selection.activeGameObject.AddComponent<Rigidbody2D>();
					}
				}
				
			}
		}

		ResolutionSelection();

		#region Animation Setup
		//Only show in Animation Setup Menu
		if(gacSettings.animSetup){

			//Check all animation slot indexes to see if they are using any hit detection
			bool usingHit = gacSettings.animSlots.Any(i => i.hitToggle == true);

			GUILayout.Space(110);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Animation Controller Type", "What type of animation controller to use"));
			
			//Show the popup to modify the Animation Controller Mode
			gacSettings.conType = (GAC.ControllerType) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.conType,EditorStyles.toolbarPopup);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Game Mode", "Are we making 3D or 2D games"));
			
			//Show the popup to modify the Game Mode
			gacSettings.gameModeIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20),  gacSettings.gameModeIndex , gacSettings.gameModeNames.ToArray(),EditorStyles.toolbarPopup);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Debug Mode", "Log info at certain points"));

			//Show the popup to modify the Debug Mode
			gacSettings.debugMode = (GAC.DebugMode) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.debugMode,EditorStyles.toolbarPopup);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20),"Activators");

			//Register the previous activator number that was set
			int previousActivatorIndex = gacSettings.globalActivatorIndex;

			gacSettings.globalActivatorIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.globalActivatorIndex, 
			                                                   gacSettings.globalActivators.Select(a => a.ToString()).ToArray(), EditorStyles.toolbarPopup);

			//Check to compare if previous set activators were more than the current
			if(previousActivatorIndex > gacSettings.globalActivatorIndex){
				//Show the dialog to decide 
				if(EditorUtility.DisplayDialog("You are reducing Activators!", "Reducing activators will remove any animations set in combos " +
					"and any activator inputs set using this activator. Do you want to continue reducing activators?", "Yes", "No")){

				}else{
					gacSettings.globalActivatorIndex = previousActivatorIndex;
				}
			}

			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(5);

			//Only if atleast one animation slot is using hit detection
			if(usingHit){

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				//Check if using range mode
				if(!gacSettings.useRange){

					GUI.color = Color.green;
					//Enable range mode if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 330,20), new GUIContent("Click to Enable Range Mode for Tracking Targets", 
                         "Use to turn on range mode to track targets"), EditorStyles.toolbarButton)){
						gacSettings.useRange = true;
					}
					GUI.color = Color.white;
				}else{
					GUI.color = Color.red;
					//Make the Combo Setup menu active if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 330,20), new GUIContent("Click to Disable Range Mode for Tracking Targets", 
                         "Use to turn off range mode to track targets"), EditorStyles.toolbarButton)){
						gacSettings.useRange = false;
					}
					GUI.color = Color.white;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(10);

				//Check if using range mode
				if(gacSettings.useRange){

					EditorGUILayout.BeginHorizontal();
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 100, 20),"Tracker Radius");
					
					var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSettings.trackerRadius, 0.03f, 0, 50, gacSettings.isDraggingRadius);
					gacSettings.isDraggingRadius = fieldValues.Value;
					
					//Show the field to modify the tracker amount
					gacSettings.trackerRadius = fieldValues.Key;

					EditorGUILayout.EndHorizontal();
					GUILayout.Space(25);
				}
			}

			if(gacSettings.gameModeIndex == 1){

				EditorGUILayout.BeginHorizontal();
				
				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
				
				//if(gacSettings.animSlots.Any (i => i.hitToggle == true)){
					//Only if using hit detection
					if(!gacSettings.detectFacingDirection){
						
						GUI.color = Color.green;
						//Make the Combo Setup menu active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329,20), new GUIContent("Turn On Facing Direction Detection", 
						                                                                                             "Use to turn on facing direction detection"), EditorStyles.toolbarButton)){
							gacSettings.detectFacingDirection = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Make the Combo Setup menu active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329,20), new GUIContent("Turn Off Facing Direction Detection", 
						                                                                                             "Use to turn off facing direction detection"), EditorStyles.toolbarButton)){
							gacSettings.detectFacingDirection = false;
						}
						GUI.color = Color.white;
						
					}
				//}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);
				

				//Only if using facing detection
				if(gacSettings.detectFacingDirection){
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					GUIStyle boxStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
					boxStyle.fontSize = 14;
					boxStyle.alignment = TextAnchor.MiddleCenter;

					EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 130, 30), new GUIContent("Right Facing Scale"));
					
					//Show the popup to modify the Debug Mode
					gacSettings.directionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 28, 20),  gacSettings.directionIndex , gacSettings.directionScales.ToArray(),EditorStyles.toolbarPopup);
					
					
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 140, guiDefaultPosition.y, 150, 30), new GUIContent("Current Facing Direction"));
					//gacSettings.facingDirectionRight = EditorGUI.Toggle(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 100, 20), gacSettings.facingDirectionRight);
					
					if(gacSettings.facingDirectionRight){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("RIGHT"), boxStyle);
					}else{
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("LEFT"), boxStyle);
					}
					
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(20);
				}
				GUILayout.Space(5);
			}

			//Show the separator
			JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
			
			GUILayout.Space(10);

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 100, 20), "The Animations");

			//Only allow adding new animations as long as there are more to add
			if(gacSettings.storeAnimNames.Count > gacSettings.animSlots.Count){
		        //Add an Animation Slot if button is pressed
				if (GUI.Button(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20),new GUIContent("New Animation Slot", "Adds a new animation slot"), EditorStyles.toolbarButton)) {

					AddAnimation(gacSettings);
				}
			}

			EditorGUILayout.EndHorizontal();

			if(gacSettings.conType == GAC.ControllerType.Legacy){
				
				//Make sure animation component is on the gameobject
				if(Selection.activeGameObject.animation != null){

					if(gacSettings.storeAnimNames.Count + 1 > Selection.activeGameObject.animation.GetClipCount()){
						gacSettings.storeAnimNames.Clear();
					}
					
					var clips = AnimationUtility.GetAnimationClips(gacSettings.gameObject).ToList();
					
					for (int i = 0; i < clips.Count; i++) {
						if(clips[i] != null){
							if(!gacSettings.storeAnimNames.Contains(clips[i].name)){
								//Make sure to ignore the Take 001 animation string and not null
								if(clips[i].name != "Take 001"){
									gacSettings.storeAnimNames.Add(clips[i].name);
								}
							}
						}
						
					}
				}
			}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
				
				//Get reference to animator component
				animatorComponent = Selection.activeGameObject.GetComponent<Animator>();

				//Make sure the component is available
				if(animatorComponent != null){
					//Get a reference to the Animator Controller:
					animatorController = animatorComponent.runtimeAnimatorController as UnityEditorInternal.AnimatorController;

					//Check if 3D Mode index selected
					if(gacSettings.gameModeIndex == 0){

						//Make sure the avatar is attached for mecanim to use
						if(animatorComponent.avatar != null){
							
							// Number of layers
							int layerCount = animatorController.layerCount;

							int statesSum = stateCount.Sum() - gacSettings.dummyStates.Count;

							//If the stored animations are more than the sum of all the states then reset the lists
							if(statesSum < gacSettings.storeAnimNames.Count){
								gacSettings.keepAnimsInSync.Clear();
								gacSettings.dummyStates.Clear();
							}

							//Loop through the available layers
							for (int layer = 0; layer < layerCount; layer++) {

								//Make sure animator controller is available
								if(animatorController != null){
									//Get states/animations on layer on the layer
									UnityEditorInternal.StateMachine sm = animatorController.GetLayer(layer).stateMachine;

									//Add this layer to used list if not used yet
									if(!layersUsed.Contains(layer)){
										//Add the amount of states/animations in this layer to list
										stateCount.Add(sm.stateCount);
										layersUsed.Add(layer);	  
									}

									for (int i = 0; i < sm.stateCount; i++) {
										//Get each state/animation
										UnityEditorInternal.State state = sm.GetState(i);

										//Loop and add state names to the list to keep updated
										if(!gacSettings.keepAnimsInSync.Contains(state.name + " 'L-" + layer + "'")){
											
											//Make sure there is a clip in motion slot before adding to list
											if(state.GetMotion() != null){
												gacSettings.keepAnimsInSync.Add(state.name + " 'L-" + layer + "'");
											}
											
										}

										//Loop and add state names to the list
										if(!gacSettings.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){

											//Make sure there is a clip in motion slot before adding to list
											if(state.GetMotion() != null){
												gacSettings.storeAnimNames.Add(state.name + " 'L-" + layer + "'");
											
											}else{

												if(!gacSettings.dummyStates.Contains(state.name + " " + layer + " " + i)){
													gacSettings.dummyStates.Add(state.name + " " + layer + " " + i);
												}
											}

										}else if(gacSettings.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){//If state already in list

											//If there is no clip in motion slot
											if(state.GetMotion() == null){
												gacSettings.storeAnimNames.Remove(state.name + " 'L-" + layer + "'");
											}


										}

									}

									//Loop through and make sure the 2 lists are a match with the animations
									for (int i = 0; i < gacSettings.storeAnimNames.Count; i++) {
										if(!gacSettings.keepAnimsInSync.Contains(gacSettings.storeAnimNames[i])){
											gacSettings.storeAnimNames.Remove(gacSettings.storeAnimNames[i]);
										}
									}
								}
							}
						}

					}else if(gacSettings.gameModeIndex == 1){//Check if 2D Mode index selected

						// Number of layers
						int layerCount = animatorController.layerCount;
						
						int statesSum = stateCount.Sum() - gacSettings.dummyStates.Count;
						
						//If the stored animations are more than the sum of all the states then reset the lists
						if(statesSum < gacSettings.storeAnimNames.Count){
							gacSettings.keepAnimsInSync.Clear();
							gacSettings.dummyStates.Clear();
						}
						
						//Loop through the available layers
						for (int layer = 0; layer < layerCount; layer++) {
							
							//Make sure animator controller is available
							if(animatorController != null){
								//Get states/animations on layer on the layer
								UnityEditorInternal.StateMachine sm = animatorController.GetLayer(layer).stateMachine;
								
								//Add this layer to used list if not used yet
								if(!layersUsed.Contains(layer)){
									//Add the amount of states/animations in this layer to list
									stateCount.Add(sm.stateCount);
									layersUsed.Add(layer);	  
								}
								
								for (int i = 0; i < sm.stateCount; i++) {
									//Get each state/animation
									UnityEditorInternal.State state = sm.GetState(i);
									
									//Loop and add state names to the list to keep updated
									if(!gacSettings.keepAnimsInSync.Contains(state.name + " 'L-" + layer + "'")){
										
										//Make sure there is a clip in motion slot before adding to list
										if(state.GetMotion() != null){
											gacSettings.keepAnimsInSync.Add(state.name + " 'L-" + layer + "'");
										}
										
									}
									
									//Loop and add state names to the list
									if(!gacSettings.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){
										
										//Make sure there is a clip in motion slot before adding to list
										if(state.GetMotion() != null){
											gacSettings.storeAnimNames.Add(state.name + " 'L-" + layer + "'");
											
										}else{
											
											if(!gacSettings.dummyStates.Contains(state.name + " " + layer + " " + i)){
												gacSettings.dummyStates.Add(state.name + " " + layer + " " + i);
											}
										}
										
									}else if(gacSettings.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){//If state already in list
										
										//If there is no clip in motion slot
										if(state.GetMotion() == null){
											gacSettings.storeAnimNames.Remove(state.name + " 'L-" + layer + "'");
										}
										
										
									}
									
								}
								
								//Loop through and make sure the 2 lists are a match with the animations
								for (int i = 0; i < gacSettings.storeAnimNames.Count; i++) {
									if(!gacSettings.keepAnimsInSync.Contains(gacSettings.storeAnimNames[i])){
										gacSettings.storeAnimNames.Remove(gacSettings.storeAnimNames[i]);
									}
								}
							}
						}
					}
				}
				
			}
			

			for (int animIndex = 0; animIndex < gacSettings.animSlots.Count; animIndex++) {
				GAC_AnimationSetup gacSet = gacSettings.animSlots[animIndex];

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					
				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				//Create a new toolbar style to use
				GUIStyle style = new GUIStyle(EditorStyles.foldout);
				style.normal.textColor = Color.blue;
				style.onNormal.textColor = Color.blue;

				//Display the animation name in the foldout if it's been added to clip slot
				if (string.IsNullOrEmpty(gacSet.theAnim)){
					EditorGUI.Foldout(new Rect (guiDefaultPosition.x + 10, guiDefaultPosition.y, 215, 20), gacSet.showAnim, "Anim #" + (animIndex + 1), style);
        		}else{
					gacSet.showAnim = EditorGUI.Foldout(new Rect (guiDefaultPosition.x + 10, guiDefaultPosition.y, 215, 20), gacSet.showAnim, gacSet.theAnim, true, style);
        		}
				EditorGUILayout.EndHorizontal();

				//Make animation slot folder is not opened
				if (!gacSet.showAnim){
					EditorGUILayout.BeginHorizontal();

					//Then show the progress bar if in play mode of the editor
					if(EditorApplication.isPlaying){
						EditorGUI.ProgressBar(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 110, 17), gacSet.animTime, "" + gacSet.animTime);
					
					}else{

						//Remove the Animation Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 18, 20),new GUIContent("-", "Remove this animation slot" ), EditorStyles.toolbarButton)){
							RemoveAnimation(gacSettings, gacSet, animIndex);                              
						}
						
						//Check to make sure we added as much animations slots that is not above the amount of animations available
						if(gacSettings.storeAnimNames.Count > gacSettings.animSlots.Count){
							//Add an Animation Slot if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 233, guiDefaultPosition.y, 18, 20), new GUIContent("+", "Add a animation slot below"),EditorStyles.toolbarButton)){
								AddAnimation(gacSettings);
							} 
						}
						//Move the Animation Slot down if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 251, guiDefaultPosition.y, 20, 20),new GUIContent('\u25BC'.ToString(), "Move animation slot down"),EditorStyles.toolbarButton)){
							MoveAnimation(gacSettings.animSlots, animIndex, 0);
							
						}    
						
						//Move the Animation Slot up if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 271, guiDefaultPosition.y, 20, 20),new GUIContent('\u25B2'.ToString(), "Move animation slot up"),EditorStyles.toolbarButton)){
							MoveAnimation(gacSettings.animSlots, animIndex, 1);
						} 
						
						//Expand all of the Animation Slots if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 291, guiDefaultPosition.y, 16, 20), new GUIContent('\u2294'.ToString(), "Expand all"),EditorStyles.toolbarButton)){
							ExpandAnimationSlots(gacSettings);
							
						}    
						
						//Close all fo the Animation Slots if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 307, guiDefaultPosition.y, 16, 20), new GUIContent('\u2293'.ToString(), "Close all"),EditorStyles.toolbarButton)){
							CloseAnimationSlots(gacSettings);
						}	
					}
					EditorGUILayout.EndHorizontal();
				}

				//if the animation slot is folder out
				if (gacSet.showAnim){
					
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make the Combo Setup menu active if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 48, 20), new GUIContent(GAC.images.gacComboSetupQuick, "Go to Combos Setup Menu"), 
					               EditorStyles.toolbarButton)) {
						gacSettings.animSetup = false;
						gacSettings.comboSetup = true;
						gacSettings.activatorSetup = false;
					}
					//Make the Activator Setup menu active if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x + 64, guiDefaultPosition.y, 48, 20), new GUIContent(GAC.images.gacActivatorSetupQuick, "Go to Activators Setup Menu"), 
					               EditorStyles.toolbarButton)) {
						gacSettings.animSetup = false;
						gacSettings.comboSetup = false;
						gacSettings.activatorSetup = true;
					}
					//Remove the Animation Slot if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x + 112, guiDefaultPosition.y, 37, 20),new GUIContent("-", "Remove this animation slot" ), EditorStyles.toolbarButton)){
						RemoveAnimation(gacSettings, gacSet, animIndex);                              
					}
					
					//Check to make sure we added as much animations slots that is not above the amount of animations available
					if(gacSettings.storeAnimNames.Count > gacSettings.animSlots.Count){
						//Add an Animation Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 149, guiDefaultPosition.y, 37, 20), new GUIContent("+", "Add a animation slot below"),EditorStyles.toolbarButton)){
							AddAnimation(gacSettings);
						} 
					}
					
					//Show only if animation has been applied; Apply button is pushed
					if(gacSet.appliedAnim){
						
						//Move the Animation Slot down if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 186, guiDefaultPosition.y, 37, 20),new GUIContent('\u25BC'.ToString(), "Move animation slot down"),EditorStyles.toolbarButton)){
							MoveAnimation(gacSettings.animSlots, animIndex, 0);
							
						}    
						
						//Move the Animation Slot up if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 221, guiDefaultPosition.y, 36, 20),new GUIContent('\u25B2'.ToString(), "Move animation slot up"),EditorStyles.toolbarButton)){
							MoveAnimation(gacSettings.animSlots, animIndex, 1);
						} 
						
						//Expand all of the Animation Slots if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 257, guiDefaultPosition.y, 36, 20), new GUIContent('\u2294'.ToString(), "Expand all"),EditorStyles.toolbarButton)){
							ExpandAnimationSlots(gacSettings);
							
						}    
						
						//Close all fo the Animation Slots if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 293, guiDefaultPosition.y, 36, 20), new GUIContent('\u2293'.ToString(), "Close all"),EditorStyles.toolbarButton)){
							CloseAnimationSlots(gacSettings);
						}	
					}        	
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();

					//Get the position and dimensions of the last gui
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Show only during editor in play
					if (UnityEditor.EditorApplication.isPlaying){

						EditorGUI.ProgressBar(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 314, 20), gacSet.animTime,"Animation Play Frame: " + gacSet.animTime);
					}else{

						//Disable the GUI
						GUI.enabled = false;
						EditorGUI.ProgressBar(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 314, 20), gacSet.animTime,"Animation Play Frame: " + gacSet.animTime);
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();

					GUILayout.Space(22);
					EditorGUILayout.BeginHorizontal();
					
					//Get the position and dimensions of the last gui
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), "Play Mode");

					//Show the popup to modify the Input Source
					gacSet.playMode = (GAC_AnimationSetup.PlayMode) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 110, guiDefaultPosition.y, 110, 20), gacSet.playMode,EditorStyles.toolbarPopup);

					if(gacSet.playMode == GAC_AnimationSetup.PlayMode.CrossFade){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 227, guiDefaultPosition.y, 100, 20), "Blend Time");

						gacSet.blendTime = EditorGUI.FloatField(new Rect (guiDefaultPosition.x + 300, guiDefaultPosition.y, 30, 20), gacSet.blendTime);
					
					}else if (gacSet.playMode == GAC_AnimationSetup.PlayMode.Normal){
						gacSet.blendTime = 0;

						//Disable the GUI
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 227, guiDefaultPosition.y, 100, 20), "Blend Time");
						
						EditorGUI.FloatField(new Rect (guiDefaultPosition.x + 300, guiDefaultPosition.y, 30, 20), gacSet.blendTime);
						//Enable the GUI
						GUI.enabled = true;
					}

						EditorGUILayout.EndHorizontal();


					//Clear first, as to not add more ontop of the current animations, then readd
					gacSet.animNames.Clear ();
					gacSet.animNames.AddRange(gacSettings.storeAnimNames);

					//Loop and remove if contains animations from added list
					foreach(string added in gacSettings.addedAnims){
						if(gacSet.animNames.Contains(added)){
							gacSet.animNames.Remove(added);
						}
					}

					//Only do when not set to be used as a starter
					if (!gacSet.useAsStarter){

						//Check first if this animation has been added as a starter before 
						if(gacSettings.addedStarters.Contains(gacSet.theAnim)){

							if (gacSettings.starterSlots.Count > gacSettings.addedStarters.IndexOf (gacSet.theAnim)){
								
								//Remove the animation from the slot
								gacSettings.starterSlots.RemoveAt(gacSettings.addedStarters.IndexOf (gacSet.theAnim));
								
							}

							//Remove the animation from the list
							gacSettings.addedStarters.Remove(gacSet.theAnim);

						}

					}

					GUILayout.Space(22);

					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Check if animation has not been applied first
					if(!gacSet.appliedAnim){

						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), "Animation");

						//Change background color
						GUI.backgroundColor = Color.yellow;
						gacSet.currentAnim = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 110, guiDefaultPosition.y, 110, 20), 
						                                     gacSet.currentAnim , gacSet.animNames.ToArray(),EditorStyles.toolbarPopup);


						//Make sure index is within limits of 0 and above, and below the animation list length
						if(gacSet.currentAnim < 0){
							gacSet.currentAnim = 0;
						}else if (gacSet.currentAnim >= gacSet.animNames.ToArray().Length){
							gacSet.currentAnim = gacSet.animNames.ToArray().Length - 1;
						}

						//Restore default background color
						GUI.backgroundColor = Color.white;

						//Add an animation slot when this button pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 109, 20),new GUIContent("Apply", "Add this animation to use"), EditorStyles.toolbarButton)) {

							gacSet.theAnim = gacSet.animNames[gacSet.currentAnim];
							gacSettings.addedAnims.Add(gacSet.animNames[gacSet.currentAnim]);

							if(gacSettings.conType == GAC.ControllerType.Legacy){
								//Set all min/max slider end values to the length of the animation
								gacSet.linkEnd = Selection.activeGameObject.animation[gacSet.theAnim].length;
								gacSet.hitEnd = Selection.activeGameObject.animation[gacSet.theAnim].length;
								gacSet.moveEnd = Selection.activeGameObject.animation[gacSet.theAnim].length;

							}

							//Set animation as applied
							gacSet.appliedAnim = true;
						}


					}else{//else if animation has been applied

						//Only do when set to be used as a starter
						if (gacSet.useAsStarter){
							
							//Check if the list contains this first, then add
							if(!gacSettings.starterAnims.Contains(gacSet.theAnim)){
								
								gacSettings.starterAnims.Add(gacSet.theAnim);
								gacSettings.startersAvailable.Add(gacSet.theAnim);
								
							}
						}

						//Make sure the set animation for this is not blank otherwise remove this animation slot - PREVENTS ERRORS
						if(gacSet.currentAnim == -1){
							
							//Only do when set to be used as a starter
							if (gacSet.useAsStarter){
								
								//Check if the list contains this first
								if(gacSettings.starterAnims.Contains(gacSet.theAnim)){
									for (int startIndex= 0; startIndex < gacSettings.starterSlots.Count; startIndex++) {
										GAC_StarterSetup starterSet = gacSettings.starterSlots[startIndex];
										
										//Compare to see if the animation set matches the starter animation then remove the starter
										if(starterSet.starterName == gacSet.theAnim){
											RemoveStarter(gacSettings, starterSet, startIndex, false); 
										}
									}
									
								}
							}
							
							RemoveAnimation(gacSettings, gacSet, animIndex);
							
							Debug.LogWarning("GACWarning - GAC has done a Cleanup and Removed Animation Slots with missing animation states. If the animation slot was being used for combos " +
								"and activators, those setups have been deleted to not cause errors.");
						}

						//Disable the Animation Popup; make it greyed out
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), "Animation");

						//Change background color
						GUI.backgroundColor = Color.yellow;
						gacSet.currentAnim = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 110, guiDefaultPosition.y, 111, 20), 
						                                     gacSettings.storeAnimNames.IndexOf(gacSet.theAnim) , gacSettings.storeAnimNames.ToArray(),EditorStyles.toolbarPopup);

						//Restore default background color
						GUI.backgroundColor = Color.white;
						GUI.enabled = true;

						//Remove this animation from added if this button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 221, guiDefaultPosition.y, 108, 20),new GUIContent("Edit", "Don't use this animation"), EditorStyles.toolbarButton)) {

							//Check if the used list contains this animation first
							if(gacSettings.animationsUsed.Contains(gacSet.theAnim)){

								//Then show the warning to prevent removing the animations from combos that are already set
								if (EditorUtility.DisplayDialog(gacSet.theAnim + " is being used in a current combo!", "Please manually remove this animation from the " +
									"'Combo Setup Menu' before trying to remove from the animation slot!", "Cancel", "Go To Combo Setup Menu")){

									break;
								}else{
									gacSettings.animSetup = false;
									gacSettings.comboSetup = true;
									gacSettings.activatorSetup = false;
								}
							
							}else if(gacSettings.addedStarters.Contains(gacSet.theAnim)){
								//Then show the warning to prevent removing the animations from combos that are already set
								if(EditorUtility.DisplayDialog(gacSet.theAnim + " is being used as a current combo!", "Please manually remove this animation from the " +
								                            "'Combo Setup Menu' before trying to remove from the animation slot!", "Cancel", "Go To Combo Setup Menu")){
									break;
								}else{
									gacSettings.animSetup = false;
									gacSettings.comboSetup = true;
									gacSettings.activatorSetup = false;
								}
							}else{//If not in use

								//Remove the animation name from the added list
								gacSettings.addedAnims.Remove(gacSet.theAnim);

								//Check if the list contains this animation first
								if(gacSettings.starterAnims.Contains(gacSet.theAnim)){

									//Remove the animation from the starter list
									gacSettings.starterAnims.Remove(gacSet.theAnim);
								}
								
								//Check if the list contains this animation first
								if(gacSettings.startersAvailable.Contains(gacSet.theAnim)){

									//Remove the animation from the starters available popup list;
									gacSettings.startersAvailable.Remove(gacSet.theAnim);
								}

								//Check first if this animation has been added as a starter before 
								if(gacSettings.addedStarters.Contains(gacSet.theAnim)){
									
									if (gacSettings.starterSlots.Count > gacSettings.addedStarters.IndexOf (gacSet.theAnim)){
										
										//Remove the animation from the slot
										gacSettings.starterSlots.RemoveAt(gacSettings.addedStarters.IndexOf (gacSet.theAnim));
										
									}
									
									//Remove the animation from the list
									gacSettings.addedStarters.Remove(gacSet.theAnim);
									
								}

								//Reset all min/max slider values to 0
								gacSet.linkBegin = 0;
								gacSet.linkEnd = 0;
								gacSet.hitBegin = 0;
								gacSet.hitEnd = 0;
								gacSet.moveBegin = 0;
								gacSet.moveEnd = 0;

								//Reset the toggles
								gacSet.hitToggle = false;
								gacSet.moveToggle = false;

								//Set both to false
								gacSet.useAsStarter = false;
								gacSet.appliedAnim = false;

								//Reset the animation name label
								gacSet.theAnim = null;
							}

						}
					}

					EditorGUILayout.EndHorizontal();
					//////////////////////////////////////////////////////////////////////////////

					GUILayout.Space(20);

					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Is this being used as a starter animation?
					if (gacSet.useAsStarter){

						//And animation has been applied
						if (gacSet.appliedAnim){

							//Change background color
							GUI.backgroundColor = Color.red;
							if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 313, 20),new GUIContent("Don't Use As Starter", "Don't use this animation for starter"), EditorStyles.toolbarButton)) {

								//Check if the list contains this animation first
								if(gacSettings.addedStarters.Contains(gacSet.theAnim)){

									GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.starterNames.IndexOf(gacSet.theAnim)];

									if(starterSet.comboAmount > 0){
										EditorUtility.DisplayDialog(gacSet.theAnim + " is being used in a current combo!", "Please manually remove this starter " +
											"animation from the 'Combo Setup Menu' before deciding not to use this animation as a starter!", "OK", "");
									}else{
										//Check if the list contains this animation first
										if(gacSettings.starterAnims.Contains(gacSet.theAnim)){
											
											//Remove the animation from the starter list
											gacSettings.starterAnims.Remove(gacSet.theAnim);
										}
										
										//Check if the list contains this animation first
										if(gacSettings.startersAvailable.Contains(gacSet.theAnim)){
											
											//Remove the animation from the starters available popup list;
											gacSettings.startersAvailable.Remove(gacSet.theAnim);
										}
										
										gacSet.useAsStarter = false;
									}
								}else{

									//Check if the list contains this animation first
									if(gacSettings.starterAnims.Contains(gacSet.theAnim)){
										
										//Remove the animation from the starter list
										gacSettings.starterAnims.Remove(gacSet.theAnim);
									}
									
									//Check if the list contains this animation first
									if(gacSettings.startersAvailable.Contains(gacSet.theAnim)){
										
										//Remove the animation from the starters available popup list;
										gacSettings.startersAvailable.Remove(gacSet.theAnim);
									}

									gacSet.useAsStarter = false;
								}
							}
							GUI.backgroundColor = Color.white;
						}

					}else{

						GUI.backgroundColor = Color.green;

						//If animation has been applied
						if (gacSet.appliedAnim){

							//Set to use a starter if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 313, 20),new GUIContent("Use As Starter", "Add this animation to use for starter"), EditorStyles.toolbarButton)) {
								gacSet.useAsStarter = true;
							}
						}else{
							//Disable the GUI
							GUI.enabled = false;
							GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 313, 20),new GUIContent("Use As Starter", "Add this animation to use for starter"), EditorStyles.toolbarButton);
							GUI.enabled = true;
						}

						GUI.backgroundColor = Color.white;
					}

					EditorGUILayout.EndHorizontal();

					//LEFT FOR FUTURE USE IF NEEDED//
					/*GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();
						
					//Get the position and dimensions of the last gui
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					if (!string.IsNullOrEmpty(gacSet.theAnim)){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 120, 20),"Animation End Frame");
						
						gacSet.animEndLength = EditorGUI.Slider(new Rect (guiDefaultPosition.x + 145, guiDefaultPosition.y, 185, 20),gacSet.animEndLength, 0, Selection.activeGameObject.animation[gacSet.theAnim].length);
					}else{
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 120, 20),"Animation End Frame");
						EditorGUI.Slider(new Rect (guiDefaultPosition.x + 145, guiDefaultPosition.y, 185, 20),0, 0, 0);
						GUI.enabled = true;
					}

					EditorGUILayout.EndHorizontal();*/
					//LEFT FOR FUTURE USE IF NEEDED//
					GUILayout.Space(25);
					
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Show the Link labels
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Link Begin:");
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 76, guiDefaultPosition.y, 100, 20), gacSet.linkBegin.ToString("f2"));	
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 100, 20), "Link End:");
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 270, guiDefaultPosition.y, 100, 20), gacSet.linkEnd.ToString("f2"));
					EditorGUILayout.EndHorizontal();

					//Only if not using delay
					if(!gacSet.delayToggle){
						
						GUI.color = Color.green;
						//Toggle delay on if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Delay On", 
						                                                                                                  "Use to turn on delay use for this animation's links"), EditorStyles.toolbarButton)){
							gacSet.delayToggle = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Toggle delay off if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Delay Off", 
						                                                                                                  "Use to turn off delay use for this animation's links"), EditorStyles.toolbarButton)){
							gacSet.delayToggle = false;
						}
						GUI.color = Color.white;
						
					}

					GUILayout.Space(15);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);	
					
					//Make sure the animation is not null before showing the min/max slider for the link
					if (!string.IsNullOrEmpty(gacSet.theAnim)){
						
						if(gacSettings.conType == GAC.ControllerType.Legacy){
							
							//Make sure animation component is on the gameobject
							if(Selection.activeGameObject.animation != null){
								EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.linkBegin, ref gacSet.linkEnd, 0, Selection.activeGameObject.animation[gacSet.theAnim].length - 0.1f);
							}
						}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.linkBegin, ref gacSet.linkEnd, 0, 0.9f);
						}

						JrDevArts_Utilities.NANCheck(gacSet.linkBegin);
					}else{
						GUI.enabled = false;
						EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.linkBegin, ref gacSet.linkEnd, 0, 0);
						GUI.enabled = true;
					}
					
					EditorGUILayout.EndHorizontal();

					if(gacSet.delayToggle){
						GUILayout.Space(20);

						EditorGUILayout.BeginHorizontal();
						
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);	
						
						//Make sure the animation is not null before showing the min/max slider for the link
						if (!string.IsNullOrEmpty(gacSet.theAnim)){

							//Set when editor not playing
							if(!UnityEditor.EditorApplication.isPlaying){
								gacSet.delayCountDown = 0.5f;
							}

							//Change color depending if countdown is within range
							if(gacSet.delayCountDown > gacSet.delayBegin && gacSet.delayCountDown < gacSet.delayEnd){
								GUI.color = Color.green;
							}

							//Show the progress bar of count down
							EditorGUI.ProgressBar(new Rect (guiDefaultPosition.x + 102, guiDefaultPosition.y, 96, 20), gacSet.delayCountDown/0.5f,"Count Down " + gacSet.delayCountDown);
							GUI.color = Color.white;

							//Show the Link labels
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Delay Frames:");

							//Round the delay time to 1 decimal
							decimal rounded = Math.Round((decimal)gacSet.delayBegin, 1);

							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 204, guiDefaultPosition.y, 21, 20),rounded.ToString("f2"));

							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 80, 20), ref gacSet.delayBegin, ref gacSet.delayEnd, 0, 0.5f);

							rounded = Math.Round((decimal)gacSet.delayEnd, 1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 312, guiDefaultPosition.y, 21, 20),rounded.ToString("f2"));

							JrDevArts_Utilities.NANCheck(gacSet.delayBegin);
						}
						
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(10);
					}
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the move is toggled on
					if (gacSet.moveToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Move Begin:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 86, guiDefaultPosition.y, 100, 20),gacSet.moveBegin.ToString("f2"));
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 100, 20),"Move End:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 100, 20),gacSet.moveEnd.ToString("f2"));
					}

					//Only if not using move
					if(!gacSet.moveToggle){
						
						GUI.color = Color.green;
						//Make the move attributes active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Move On", 
						                                                                                                  "Use to turn on move use for movement during animation"), EditorStyles.toolbarButton)){
							gacSet.moveToggle = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Make the move attributes not active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Move Off", 
						                                                                                                  "Use to turn off move use for movement during animation"), EditorStyles.toolbarButton)){
							gacSet.moveToggle = false;
						}
						GUI.color = Color.white;
						
					}
					//Make sure the move is toggled off
					if (!gacSet.moveToggle){
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Move Begin:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 86, guiDefaultPosition.y, 100, 20),"0.00");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 100, 20),"Move End:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 100, 20),"0.00");
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(15);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the animation is not null before showing the min/max slider for the move; also check if move is toggled on
					if (!string.IsNullOrEmpty(gacSet.theAnim) && gacSet.moveToggle){
						if(gacSettings.conType == GAC.ControllerType.Legacy){
							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.moveBegin, ref gacSet.moveEnd, 0, Selection.activeGameObject.animation[gacSet.theAnim].length - 0.1f);
							
						}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.moveBegin, ref gacSet.moveEnd, 0, 0.9f);
						}

						JrDevArts_Utilities.NANCheck(gacSet.moveBegin);
					}else if (!gacSet.moveToggle){
						GUI.enabled = false;
						EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.moveBegin, ref gacSet.moveEnd, 0, 0);
						GUI.enabled = true;
					}
					
					EditorGUILayout.EndHorizontal();	
					
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the move is toggled on
					if (gacSet.moveToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount X", "The amount to move the player while attacking"));
			
						var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountX, 0.03f, -50, 50, gacSet.isDraggingMoveX);
						gacSet.isDraggingMoveX = fieldValues.Value;

						//Show the field to modify the move amount
						gacSet.moveAmountX = fieldValues.Key;
						
					}else{
						//Disable the GUI
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount X", "The amount to move the player while attacking"));						
						JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountX, 0.03f, -50, 50, gacSet.isDraggingMoveX);
						GUI.enabled = true;					
					}

					//Check if Moves are being shown
					if(!gacSet.showMoves){
						
						GUI.color = Color.yellow;
						//Make all axis shown if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 147, guiDefaultPosition.y, 50,20), new GUIContent("Show All", 
						                                                                                                  "Show all axis fields for moves"), EditorStyles.toolbarButton)){
							gacSet.showMoves = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Make all axis hidden if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 147, guiDefaultPosition.y, 50,20), new GUIContent("Hide All", 
						                                                                                                  "Hide all axis fields for moves"), EditorStyles.toolbarButton)){
							gacSet.showMoves = false;
						}
						GUI.color = Color.white;
						
					}
					EditorGUILayout.EndHorizontal();

					//Check if Moves are being shown
					if(gacSet.showMoves){
						GUILayout.Space(20);
						EditorGUILayout.BeginHorizontal();
						
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
						
						//Make sure the move is toggled on
						if (gacSet.moveToggle){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount Y", "The amount to move the player while attacking"));
							
							var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountY, 0.03f, -50, 50, gacSet.isDraggingMoveY);
							gacSet.isDraggingMoveY = fieldValues.Value;
							
							//Show the field to modify the move amount
							gacSet.moveAmountY = fieldValues.Key;
							
						}else{
							//Disable the GUI
							GUI.enabled = false;
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount Y", "The amount to move the player while attacking"));						
							JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountY, 0.03f, -50, 50, gacSet.isDraggingMoveY);
							GUI.enabled = true;					
						}
						
						EditorGUILayout.EndHorizontal();

						//Check if 3D Mode index selected
						if(gacSettings.gameModeIndex == 0){
							GUILayout.Space(20);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							
							//Make sure the move is toggled on
							if (gacSet.moveToggle){
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount Z", "The amount to move the player while attacking"));

								//Show the field to modify the move amount
								var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountZ, 0.03f, -50, 50, gacSet.isDraggingMoveZ);

								//Reference the results
								gacSet.isDraggingMoveZ = fieldValues.Value;
								gacSet.moveAmountZ = fieldValues.Key;
								
							}else{
								//Disable the GUI
								GUI.enabled = false;
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Move Amount Z", "The amount to move the player while attacking"));						
								JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.moveAmountZ, 0.03f, -50, 50, gacSet.isDraggingMoveZ);
								GUI.enabled = true;					
							}
							
							EditorGUILayout.EndHorizontal();
						}
					}

					#region Hit Detection
					GUILayout.Space(20);
					
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the hit toggle is on
					if(gacSet.hitToggle){
						//Show the Hit labels
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), "Hit Begin:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 86, guiDefaultPosition.y, 100, 20),gacSet.hitBegin.ToString("f2"));
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 100, 20),"Hit End:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 100, 20),gacSet.hitEnd.ToString("f2"));
					}

					//Only if using hit detection
					if(!gacSet.hitToggle){
						
						GUI.color = Color.green;
						//Make the hit attributes active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Hit On", 
						                                                                                                  "Use to turn on hit detection"), EditorStyles.toolbarButton)){
							gacSet.hitToggle = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Make the hit attributes not active if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Hit Off", 
						                                                                                                  "Use to turn off hit detection"), EditorStyles.toolbarButton)){
							gacSet.hitToggle = false;
						}
						GUI.color = Color.white;
						
					}

					//Make sure the hit is toggled off
					if(!gacSet.hitToggle){
						//Disable the GUI
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), "Hit Begin:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 86, guiDefaultPosition.y, 100, 20),"0.00");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 100, 20),"Hit End:");
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 100, 20),"0.00");
						GUI.enabled = true;

						//Disable Gizmo
						gacSet.gizmoFocus = false;
						
					}
					
					EditorGUILayout.EndHorizontal();
					
					GUILayout.Space(15);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the animation is not null before showing the min/max slider for the hit; also check if hit is toggled on
					if (!string.IsNullOrEmpty(gacSet.theAnim) && gacSet.hitToggle){
						if(gacSettings.conType == GAC.ControllerType.Legacy){
							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.hitBegin, ref gacSet.hitEnd, 0, Selection.activeGameObject.animation[gacSet.theAnim].length - 0.1f);
							
						}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
							EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.hitBegin, ref gacSet.hitEnd, 0, 0.9f);
						}

						JrDevArts_Utilities.NANCheck(gacSet.hitBegin);

					}else if (!gacSet.hitToggle){
						GUI.enabled = false;
						EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y, 312, 20),ref gacSet.hitBegin, ref gacSet.hitEnd, 0, 0);
						GUI.enabled = true; 
					}
					
					EditorGUILayout.EndHorizontal();
					#endregion Hit Detection

					#region Affect Layer
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					if(gacSet.hitToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 120, 20),"Affect Layer");

						//Show the layer mask to modify layers
						gacSet.affectLayer = JrDevArts_Utilities.LayerMaskField(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 126, 20),gacSet.affectLayer, EditorStyles.toolbarPopup);
					}else{
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 120, 20),"Affect Layer");

						JrDevArts_Utilities.LayerMaskField(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 126, 20),0, EditorStyles.toolbarPopup);
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();
					#endregion Affect Layer

					#region Affect Distance
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					if(gacSet.hitToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Affect Distance");
						
						
						//Show the field to modify the distance amount
						var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 
						                                                        gacSet.affectDistance, 0.03f, 0, 50, gacSet.isDraggingDistance);

						gacSet.isDraggingDistance = fieldValues.Value;
						gacSet.affectDistance = fieldValues.Key;
					}else{
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Affect Distance");
						JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 
						                                      0, 0.03f, 0, 50, gacSet.isDraggingDistance);
						GUI.enabled = true;
					}

					EditorGUILayout.EndHorizontal();
					#endregion Affect Distance

					#region Affect Angle
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();
						
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					if(gacSet.hitToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Affect Angle");
							
						var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 
						                                                        gacSet.affectAngle, 3, 0, 360, gacSet.isDraggingAngle);
						//Reference the results
						gacSet.isDraggingAngle = fieldValues.Value;
						gacSet.affectAngle = fieldValues.Key;

						//Check if 3D Mode index selected
						if(gacSettings.gameModeIndex == 0){
							//Only if using height detection
							if(!gacSet.heightToggle){
								
								GUI.color = Color.green;
								//Make the Combo Setup menu active if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Height On", 
								                                                                                             "Use to turn on height use for hit detection"), EditorStyles.toolbarButton)){
									gacSet.heightToggle = true;
								}
								GUI.color = Color.white;
								
							}else{
								GUI.color = Color.red;
								//Make the Combo Setup menu active if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 129, guiDefaultPosition.y, 68,20), new GUIContent("Height Off", 
								                                                                                             "Use to turn off height use for hit detection"), EditorStyles.toolbarButton)){
									gacSet.heightToggle = false;
								}
								GUI.color = Color.white;
								
							}
						}
					}else{
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20),"Affect Angle");
						JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 
						                                     	0, 3, 0, 360, gacSet.isDraggingAngle);
						GUI.enabled = true;
					}

					EditorGUILayout.EndHorizontal();

					//Check if 3D Mode index selected
					if(gacSettings.gameModeIndex == 0){

						if(gacSet.heightToggle){
							GUILayout.Space(20);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Angle Height", "The parameter size for this gameobject"));
							var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.angleHeight, 0.03f, -100, 100, 
							                                                        gacSet.isDraggingHeight);
							//Reference the results
							gacSet.isDraggingHeight = fieldValues.Value;
							gacSet.angleHeight = fieldValues.Key;
							EditorGUILayout.EndHorizontal();
						}
					}

					#endregion Affect Angle
					/*FOR FUTURE USE
					else if(gacSettings.gameModeIndex == 1){
						GUILayout.Space(20);
						EditorGUILayout.BeginHorizontal();
					
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

						if(gacSet.hitToggle){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Angle Position", "The parameter size for this gameobject"));

							gacSet.anglePosition = JrDevArts_Utilities.Vector2Field(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 148, 17), gacSet.anglePosition, "X", "Y", 0.03f);
						}else{
							GUI.enabled = false;
							JrDevArts_Utilities.Vector2Field(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 148, 17), Vector2.zero, "X", "Y", 0.03f);
							GUI.enabled = true;
						}
						EditorGUILayout.EndHorizontal();
					}*/

					#region Knockback
					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
					//Make sure the hit detection is toggled on
					if (gacSet.hitToggle){
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount X", "The amount to move the object that was hit in X-Axis"));
						
						var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.hitKnockBackX, 0.03f, -100, 100, gacSet.isDraggingKnockBackX);
						gacSet.isDraggingKnockBackX = fieldValues.Value;
						
						//Show the field to modify the knockback amount
						gacSet.hitKnockBackX = fieldValues.Key;
						
					}else{
						//Disable the GUI
						GUI.enabled = false;
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount X", "The amount to move the object that was hit in X-Axis"));					
						JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 0, 0.03f, -100, 100, gacSet.isDraggingKnockBackX);
						GUI.enabled = true;					
					}

					//Check if Knockbacks are being shown
					if(!gacSet.showKnockBacks){
						
						GUI.color = Color.yellow;
						//Make all axis shown if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 147, guiDefaultPosition.y, 50,20), new GUIContent("Show All", 
						                                                                                                  "Show all axis fields for knockbacks"), EditorStyles.toolbarButton)){
							gacSet.showKnockBacks = true;
						}
						GUI.color = Color.white;
						
					}else{
						GUI.color = Color.red;
						//Make all axis not shown if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 147, guiDefaultPosition.y, 50,20), new GUIContent("Hide All", 
						                                                                                                  "Hide all axis fields for knockbacks"), EditorStyles.toolbarButton)){
							gacSet.showKnockBacks = false;
						}
						GUI.color = Color.white;
						
					}
					EditorGUILayout.EndHorizontal();

					if(gacSet.showKnockBacks){
						GUILayout.Space(20);
						EditorGUILayout.BeginHorizontal();
						
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
						
						//Make sure the hit detection is toggled on
						if (gacSet.hitToggle){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount Y", "The amount to move the object that was hit in Y-Axis"));
							
							var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.hitKnockBackY, 0.03f, -100, 100, gacSet.isDraggingKnockBackY);
							gacSet.isDraggingKnockBackY = fieldValues.Value;
							
							//Show the field to modify the move amount
							gacSet.hitKnockBackY = fieldValues.Key;
							
						}else{
							//Disable the GUI
							GUI.enabled = false;
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount Y", "The amount to move the object that was hit in Y-Axis"));					
							JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 0, 0.03f, -100, 100, gacSet.isDraggingKnockBackY);
							GUI.enabled = true;					
						}
						
						EditorGUILayout.EndHorizontal();

						//Check if 3D Mode index selected
						if(gacSettings.gameModeIndex == 0){
							GUILayout.Space(20);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							
							//Make sure the hit detection is toggled on
							if (gacSet.hitToggle){
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount Z", "The amount to move the object that was hit in Z-Axis"));
								
								var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacSet.hitKnockBackZ, 0.03f, -100, 100, gacSet.isDraggingKnockBackZ);
								gacSet.isDraggingKnockBackZ = fieldValues.Value;
								
								//Show the field to modify the knockback amount
								gacSet.hitKnockBackZ = fieldValues.Key;
								
							}else{
								//Disable the GUI
								GUI.enabled = false;
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 150, 20), new GUIContent("KnockBack Amount Z", "The amount to move the object that was hit in Z-Axis"));					
								JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), 0, 0.03f, -100, 100, gacSet.isDraggingKnockBackZ);
								GUI.enabled = true;					
							}
							
							EditorGUILayout.EndHorizontal();
						}
					}
					#endregion KnockBack

					GUILayout.Space(20);
					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Make sure the hit detection is toggled on 
					if(gacSet.hitToggle){

						//Make sure the gizmo is not shown yet
						if(!gacSet.gizmoFocus){

							GUI.color = Color.yellow;
							//Show the gizmo for this animation
							if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 115, 20),new GUIContent("Focus Gizmo", "Focus the gizmo on this animation slot"),EditorStyles.toolbarButton)){
								gacSet.gizmoFocus = true;
								
								for (int idx = 0; idx < gacSettings.animSlots.Count; idx++){ 
									//If the animation slot is not null, show the gizmo for it and hide the other slots' gizmos
									if (!string.IsNullOrEmpty(gacSettings.animSlots[idx].theAnim)){
										if (gacSettings.animSlots[idx].theAnim != gacSet.theAnim){
											gacSettings.animSlots[idx].gizmoFocus = false;
										}
									}
								}	
								
							}
							GUI.color = Color.white;
						}else{

							GUI.color = Color.red;
							//Hide the gizmo for this animation
							if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 115, 20),new GUIContent("Un-Focus Gizmo", "Turn of focus of the gizmo on this animation slot"),EditorStyles.toolbarButton)){
								gacSet.gizmoFocus = false;

							}
							GUI.color = Color.white;
						}
							
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 40, 20), new GUIContent("Color", "Set the color to use for the gizmo"));
							
						//Show the field to modify the gizmo color
						gacSet.gizmoColor = EditorGUI.ColorField(new Rect (guiDefaultPosition.x + 243, guiDefaultPosition.y, 84, 18),gacSet.gizmoColor);
					}else{
						GUI.enabled = false;
						//EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 100, 20), new GUIContent("Gizmo", "This shows the effect radius and distance of the attack"));
						GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 132, 20),new GUIContent("Focus Gizmo", "Focus the gizmo on this animation slot"),EditorStyles.toolbarButton);
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 202, guiDefaultPosition.y, 40, 20), new GUIContent("Color", "Set the color to use for the gizmo"));
						EditorGUI.ColorField(new Rect (guiDefaultPosition.x + 243, guiDefaultPosition.y, 84, 18),gacSet.gizmoColor);
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();						


					//Show the separator image if not the first index
					if(gacSettings.animSlots.Count > 1){
						GUILayout.Space(30);
						JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
						GUILayout.Space(10);
					}

				}

			}
			
			//Add more space if there is no more than 1 animation slot
			if(gacSettings.animSlots.Count < 2){
				GUILayout.Space(20);
			}
		
		#endregion Animation Setup
		
		#region Combo Setup
		}else if (gacSettings.comboSetup){

			GUILayout.Space(110);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 120, 20), "Starter Animations");
			gacSettings.starterIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 112, 20), gacSettings.starterIndex, gacSettings.startersAvailable.ToArray(),EditorStyles.toolbarPopup);

			//Add an animation slot when this button pressed
			if (GUI.Button(new Rect (guiDefaultPosition.x + 214, guiDefaultPosition.y, 110, 20),new GUIContent("Add", "Add this animation to use as a Starter"), EditorStyles.toolbarButton)) {

				if(!gacSettings.startersAvailable.Contains("None")){
					AddStarter(gacSettings, gacSettings.startersAvailable[gacSettings.starterIndex]);

				}
			}
		
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Debug Mode", "Log info at certain points"));
			
			//Show the popup to modify the Debug Mode
			gacSettings.debugMode = (GAC.DebugMode) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.debugMode,EditorStyles.toolbarPopup);
			
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(14);
				
			for (int startIndex = 0; startIndex < gacSettings.starterSlots.Count; startIndex++) {
				GAC_StarterSetup starterSet = gacSettings.starterSlots[startIndex];
				GAC_AnimationSetup animSet = null;



				//Assign a reference for each animSlot using the starters names
				foreach(GAC_AnimationSetup anim in gacSettings.animSlots){
					if(anim.theAnim == starterSet.starterName){
						animSet = anim;
					}
				}

				if(starterSet.starterCombos.Count == 0){
					gacSettings.starterGroupShow[startIndex] = false;

				}


				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
					
				//Create a new toolbar style to use
				GUIStyle style = new GUIStyle(EditorStyles.foldout);
				style.normal.textColor = Color.blue;
				style.onNormal.textColor = Color.blue;

				//Display the animation name in the foldout if it's been added to clip slot
				starterSet.showStarter = EditorGUI.Foldout(new Rect (guiDefaultPosition.x + 10, guiDefaultPosition.y, 40, 20), starterSet.showStarter, starterSet.starterName, style);

				GUI.color = Color.red;

				//Remove the starter if button pressed
				if (GUI.Button(new Rect (guiDefaultPosition.x + 214, guiDefaultPosition.y, 109, 20), 
				    new GUIContent("Delete Starter", "Delete this starter slot" ), EditorStyles.toolbarButton)){

					RemoveStarter(gacSettings, starterSet, startIndex, true);                              
				}
				GUI.color = Color.white;

				//Set to default color
				style.normal.textColor = Color.white;
				style.onNormal.textColor = Color.white;

				//if the starter slot is folder out
				if (starterSet.showStarter){

					//Get the position and dimensions of the last gui
					guiDefaultPosition = GUILayoutUtility.GetLastRect ();

					//Check if there are any activators to use
					if(gacSettings.globalActivatorIndex > 0){

						//Only Show this button initially
						if(starterSet.starterCombos.Count == 0){

							GUI.color = Color.green;
							//Add an animation slot when this button pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 140, guiDefaultPosition.y, 73, 20),
							   	new GUIContent("Add Combos", "Adds an animation to which is used to start combo"), EditorStyles.toolbarButton)) {

								gacSettings.starterGroupShow[startIndex] = true;

								AddCombo(starterSet);

								//If there are no starters added yet
								if(starterSet.starterCombos[0].theCombos.Count == 0){
									starterSet.starterCombos[0].theCombos.Add (starterSet.starterName);
								}
							}
							GUI.color = Color.white;
						}
					}
							

				}else{

					if (GUI.Button(new Rect (guiDefaultPosition.x + 140, guiDefaultPosition.y, 37, 20),new GUIContent('\u25BC'.ToString(), "Move starter slot down"),EditorStyles.toolbarButton)){
						MoveStarter(gacSettings.starterSlots, startIndex, 0);
						
					}    
					
					if (GUI.Button(new Rect (guiDefaultPosition.x + 177, guiDefaultPosition.y, 36, 20),new GUIContent('\u25B2'.ToString(), "Move starter slot up"),EditorStyles.toolbarButton)){
						MoveStarter(gacSettings.starterSlots, startIndex, 1);
					} 
				}
				EditorGUILayout.EndHorizontal();

				//if the starter slot is folder out
				if (starterSet.showStarter){

					//Make sure a reference was set, and it's not null
					if(animSet != null){

						//Check if there are any activators to use
						if(gacSettings.globalActivatorIndex == 0){
							EditorGUILayout.HelpBox("You have no Activators for this Animation. An Activator is required before continuing. Please add an Activator.", MessageType.Warning, true);
						}
					}

					for (int comboIndex = 0; comboIndex < starterSet.starterCombos.Count; comboIndex++) {
						GAC_ComboSetup comboSet = starterSet.starterCombos[comboIndex];

						//Add to delay animation count if its less than the current animation spots
						if(comboSet.delayedAnim.Count < comboSet.animSpot.Count){
							comboSet.delayedAnim.Add(false);
						}

						//Remove set animation amount if its more than the available animation spots
						if(comboSet.setAnim.Count > comboSet.animSpot.Count){
							comboSet.setAnim.RemoveAt(comboSet.setAnim.Count - 1);
						}

						//Add space and separator if the combo index is more than 1 and within range
						if(comboIndex > 0 && comboIndex < starterSet.starterCombos.Count){
							//Show the separator
							JrDevArts_Utilities.ShowTexture(GAC.images.gacGreenSeparator);
							GUILayout.Space(10);
						}

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

						//Display the combo # in the foldout if it's been added to clip slot
						comboSet.showCombo = EditorGUI.Foldout(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 40, 20), comboSet.showCombo, "Combo # " + (comboIndex + 1) + " - ");

						//Keep a record of the lenght of the combo; minus the starter
						if(comboSet.theCombos.Count > 1){
							comboSet.linkAmount = comboSet.theCombos.Count - 1;
						}else{
							comboSet.linkAmount = 0;
						}

						//Show a warning if the combo starter is conflicted
						for (int i = 0; i < comboSet.animSpot.Count; i++) {
							if(comboSet.conflicted[i]){
								GUI.DrawTexture(new Rect (guiDefaultPosition.x - 6, guiDefaultPosition.y + 2, GAC.images.gacWarning.width, GAC.images.gacWarning.height), GAC.images.gacWarning, ScaleMode.ScaleToFit, true, 0);
								warningMode = true;
							}
						}

						//Check if combo name set yet
						if(comboSet.nameSet){
							//Create a new toolbar style to use
							style = new GUIStyle(EditorStyles.boldLabel);
							style.normal.textColor = Color.blue;
							style.onNormal.textColor = Color.blue;

							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 101, guiDefaultPosition.y, 150, 20), "", comboSet.comboName, style);

							GUI.color = Color.yellow;

							//Only show if  text field blank
							if(string.IsNullOrEmpty(comboSet.comboName)){

								//Make the Text field active if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 251, guiDefaultPosition.y, 72, 20), new GUIContent("Add Name", "Add a custom name"), 
								               EditorStyles.toolbarButton)) {
									comboSet.nameSet = false;
								}

							}else{

								//Make the Text field menu active if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 251, guiDefaultPosition.y, 72, 20), new GUIContent("Edit Name", "Add a custom name"), 
								               EditorStyles.toolbarButton)) {
									comboSet.nameSet = false;
								}
							
							}
							GUI.color = Color.white;
						}else{
							comboSet.comboName = GUI.TextField(new Rect (guiDefaultPosition.x + 103, guiDefaultPosition.y, 148, 18), comboSet.comboName, 23);

							GUI.color = Color.red;
							//Set the name of the combo if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 251, guiDefaultPosition.y, 72, 20), new GUIContent("Set Name", "Add a custom name"), 
							               EditorStyles.toolbarButton)) {
								comboSet.nameSet = true;
							}
							GUI.color = Color.white;
						}
						EditorGUILayout.EndHorizontal();

						//if the combo slot is folder out
						if (comboSet.showCombo){
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							//Make the Animation Setup menu active if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 16, guiDefaultPosition.y, 48, 20), new GUIContent(GAC.images.gacAnimSetupQuick, "Go to Animations Setup Menu"), 
							               EditorStyles.toolbarButton)) {
								gacSettings.animSetup = true;
								gacSettings.comboSetup = false;
								gacSettings.activatorSetup = false;
							}
							//Make the Activator Setup menu active if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 64, guiDefaultPosition.y, 48, 20), new GUIContent(GAC.images.gacActivatorSetupQuick, "Go to Activators Setup Menu"), 
							               EditorStyles.toolbarButton)) {
								gacSettings.animSetup = false;
								gacSettings.comboSetup = false;
								gacSettings.activatorSetup = true;
							}

							if (GUI.Button(new Rect (guiDefaultPosition.x + 109, guiDefaultPosition.y, 37,  20),new GUIContent("-", "Remove this combo slot" ), EditorStyles.toolbarButton)){
								RemoveCombo(starterSet, comboIndex);                              
							}

							if (GUI.Button(new Rect (guiDefaultPosition.x + 146, guiDefaultPosition.y, 37, 20),
							    new GUIContent("+", "Add a combo slot below"),EditorStyles.toolbarButton)){
								AddCombo(starterSet);

								//If there are no starters added yet
								if(comboSet.theCombos.Count == 0){
									comboSet.theCombos.Add (starterSet.starterName);
								}
							} 

							if (GUI.Button(new Rect (guiDefaultPosition.x + 183, guiDefaultPosition.y, 37, 20),new GUIContent('\u25BC'.ToString(), "Move combo slot down"),EditorStyles.toolbarButton)){
								MoveCombo(starterSet.starterCombos, comboIndex, 0);
								
							}    
							
							if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 37,  20),new GUIContent('\u25B2'.ToString(), "Move combo slot up"),EditorStyles.toolbarButton)){
								MoveCombo(starterSet.starterCombos, comboIndex, 1);
							} 
							  
							//Expand all of the Animation Slots if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 257, guiDefaultPosition.y, 36, 20), new GUIContent('\u2294'.ToString(), "Expand all"),EditorStyles.toolbarButton)){
								ExpandComboSlots(gacSettings, starterSet);
								
							}    
							
							//Close all fo the Animation Slots if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 293, guiDefaultPosition.y, 36, 20), new GUIContent('\u2293'.ToString(), "Close all"),EditorStyles.toolbarButton)){
								CloseComboSlots(gacSettings, starterSet);
							}	
							EditorGUILayout.EndHorizontal();
							
							GUILayout.Space(10);

							//Show if there isn't atleast 2 animations added
							if (gacSettings.addedAnims.Count <= 1){
								EditorGUILayout.HelpBox("You have no Animations available to add to this combo. Atleast one other Animation is required before continuing. Please add an new Animation.", MessageType.Warning, true);
							}


							//If there are no animations added for combos yet
							if (gacSettings.addedAnims.Count <= 1){

								//Reset the list
								comboSet.animNames.Clear();

								//Add a 'None' string to the list as default
								comboSet.animNames.Add("None");

							}else if(comboSet.animNames.Count > gacSettings.addedAnims.Count){
								//Reset the list
								comboSet.animNames.Clear();
							}else{

								//Check if the list has the default 'None' string
								if(comboSet.animNames.Contains("None")){
									//And remove it
									comboSet.animNames.Remove("None");
								}

								//Add all animations from the Animation Component to a list
								foreach(string anim in gacSettings.addedAnims){
									
									if(!comboSet.animNames.Contains(anim)){
									
										comboSet.animNames.Add(anim);
									
									}
									
								}
							}

							GUILayout.Space(10);
							EditorGUILayout.BeginHorizontal();

							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							//Only show if there are no animations added to combo yet
							if(comboSet.animSpot.Count == 0){

								//Check if there are any activators to use
								if(gacSettings.globalActivatorIndex > 0 && gacSettings.addedAnims.Count > 1){
									
									GUI.backgroundColor = Color.green;

									//Add a delayed animation to the combo
									if (GUI.Button(new Rect (guiDefaultPosition.x + 64, guiDefaultPosition.y, 119, 20),
									               new GUIContent("Add Delay", "Add an delayed animation spot for the combo"), EditorStyles.toolbarButton)) {
										
										comboSet.animSpot.Add(0);
										comboSet.comboSequence.Add(1);
										comboSet.activatorIndex.Add(1);
										comboSet.setAnim.Add (false);
										comboSet.buttonShown.Add (false);
										comboSet.conflicted.Add(false);
										comboSet.delayedAnim.Add (true);
										
										
									}

									//Add an animation to the combo when this button pressed
									if (GUI.Button(new Rect (guiDefaultPosition.x + 183, guiDefaultPosition.y, 110, 20),
									               new GUIContent("Add Animation", "Add an animation spot for the combo"), EditorStyles.toolbarButton)) {
										
										comboSet.animSpot.Add(0);
										comboSet.comboSequence.Add(1);
										comboSet.activatorIndex.Add(1);
										comboSet.setAnim.Add (false);
										comboSet.buttonShown.Add (false);
										comboSet.conflicted.Add(false);
										comboSet.delayedAnim.Add (false);


									}

									GUI.backgroundColor = Color.white;
								}
							}
							EditorGUILayout.EndHorizontal();

							//Check if there are any activators to use
							if(gacSettings.globalActivatorIndex == 0){

								//Then reset everything
								comboSet.animSpot.Clear();
								comboSet.conflicted.Clear ();
								comboSet.comboSequence.Clear();
								comboSet.activatorIndex.Clear();
								comboSet.showCombo = false;
								
							}

							//Check if there are not animation spots added to combo yet
							if(comboSet.animSpot.Count == 0){
								GUILayout.Space(20);
							}else{
								GUILayout.Space(10);
							}
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							//Make sure there is atleast 2 animations added
							if (gacSettings.addedAnims.Count > 1){

								//Display the labels
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 26, guiDefaultPosition.y, 100, 20), "Activator");
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 102, guiDefaultPosition.y, 100, 20),"Animation");
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 190, guiDefaultPosition.y, 100, 20),"Seq.");
								
							}
							EditorGUILayout.EndHorizontal();

							GUILayout.Space(20);
							for (int i = 0; i < comboSet.animSpot.Count; i++) {

								EditorGUILayout.BeginHorizontal();

								//Reset the position dimensions to 1
								guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

								//Create a String List from the Activator Integer list
								int[] activators = gacSettings.activators.ToArray();


								string[] activatorStrings = gacSettings.activators.Select(a => a.ToString()).ToArray();

								//Make sure there is atleast 2 animations added
								if (gacSettings.addedAnims.Count > 1){

									//Make sure the anim Spot is not set
									if(!comboSet.setAnim[i]){
										//Create a new toolbar style to use
										style = new GUIStyle(EditorStyles.toolbarPopup);
										style.normal.textColor = Color.green;
										style.onNormal.textColor = Color.green;

										//Make sure index not out of range
										if((comboSet.delayedAnim.Count - 1) >= i){
											
											//Check if it is a delayed animation spot
											if(comboSet.delayedAnim[i]){

												//Create a new toolbar style to use
												style = new GUIStyle(EditorStyles.largeLabel);
												style.normal.textColor = Color.blue;
												EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 5, guiDefaultPosition.y, 32, guiDefaultPosition.height + 20), "D", style);

												//Create a new toolbar style to use
												style = new GUIStyle(EditorStyles.toolbarPopup);

												style.normal.textColor = Color.white;
												style.onNormal.textColor = Color.white;
												GUI.backgroundColor = Color.blue;
											}

										}
										comboSet.activatorIndex[i] = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 38, guiDefaultPosition.y, 32, guiDefaultPosition.height + 20), comboSet.activatorIndex[i] , activatorStrings, activators,style);

										comboSet.animSpot[i] = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 80, guiDefaultPosition.y, 100, guiDefaultPosition.height + 20),comboSet.animSpot[i] , comboSet.animNames.ToArray(), style);

										//Make sure index not out of range
										if((comboSet.delayedAnim.Count - 1) >= i){
											
											//Check if it is a delayed animation spot
											if(comboSet.delayedAnim[i]){
												GUI.backgroundColor = Color.white;
											}
										}

										//Keep track of the combo sequence using the index; adding 2 to it because the sequence begins at 2 while indexes begin at 0
										comboSet.comboSequence[i] = i + 2;

										EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 195, guiDefaultPosition.y, guiDefaultPosition.width + 40, guiDefaultPosition.height + 20),"" + comboSet.comboSequence[i]);
									}


									//Make sure the index is within its limits
									if (i <= (comboSet.setAnim.Count - 1)){

										//If animation not set yet
										if(!comboSet.setAnim[i]){

											//Make sure index after current index is not past its limit
											if ((i + 1) <= (comboSet.setAnim.Count - 1)){

												//Make sure this indexes buttons are showing or this index is not set yet
												if(comboSet.buttonShown[i + 1] || !comboSet.setAnim[i + 1]){

													//Remove this animation from spot if button pressed
													if (GUI.Button(new Rect (guiDefaultPosition.x + 229, guiDefaultPosition.y, 33, 20),
													               new GUIContent("-", "Remove this animation spot from the combo"), EditorStyles.toolbarButton)) {

														//Register animation spot as not set
														comboSet.setAnim[i] = false;

														//Check to make sure there are no more conflicts
														EditConflicts(starterSet, i);

														//Remove this spot
														RemoveAnimSpot(comboSet, i);
														
													}
												}
											}else if (i == (comboSet.setAnim.Count - 1)){ //Else make sure the current index is not past its limit

												//Remove this animation from spot if button pressed
												if (GUI.Button(new Rect (guiDefaultPosition.x + 229, guiDefaultPosition.y, 33, 20),
												               new GUIContent("-", "Remove this animation spot from the combo"), EditorStyles.toolbarButton)) {

													//Register animation spot as not set
													comboSet.setAnim[i] = false;

													//Check to make sure there are no more conflicts
													EditConflicts(starterSet, i);

													//Remove this spot
													RemoveAnimSpot(comboSet, i);
													
												}
											}

											//Create a new toolbar style to use
											style = new GUIStyle(EditorStyles.toolbarButton);
											style.normal.textColor = Color.green;
											style.onNormal.textColor = Color.green;

											//Make sure not the first animation slot in the index
											if(i > 0){

												//If previous animation buttons are showing
												if(comboSet.buttonShown[i - 1]){

													//Set this animation to this spott of the combo if button pressed
													if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 33, guiDefaultPosition.height + 20),
														new GUIContent("SET", "Set the animation to this spot of the combo"), style)){

														//Register animation spot as set
														comboSet.setAnim[i] = true;

														//Loop through all animSpots after the index and show all their buttons
														for (int n = i; n < comboSet.animSpot.Count; n++){
															comboSet.buttonShown[n] = true;
														}

														//Reference all the animations for this combo to use later
														AddReferences(comboSet, starterSet, i);
														
														//Make sure no reference is causing conflicts in the sequence of animation combos
														ConflictCheck(starterSet, i);

														//If there were any references kept, reAdd them starting from index after the current
														if(comboSet.keepReference.Count > 0){
															comboSet.animationReference.InsertRange(i + 1, comboSet.keepReference);

															//Then clear the lists
															comboSet.keepReference.Clear();
														}

														//If there were any references kept, reAdd them starting from index after the current
														//Starter animation uses the first index so don't include it; index begins a 1 instead
														if(comboSet.referenceCombos.Count > 0){

															comboSet.theCombos.InsertRange(i + 2, comboSet.referenceCombos);
															
															//Then clear the lists
															comboSet.referenceCombos.Clear();

														}


													}
												}
													
											}else{

												//Set this animation to this spott of the combo if button pressed
												if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 33, guiDefaultPosition.height + 20),
												               new GUIContent("SET", "Set the animation to this spot of the combo"), style)){
													
													//Register animation spot as set
													comboSet.setAnim[i] = true;

													//Loop through all animSpots after the index and show all their buttons
													for (int n = i; n < comboSet.animSpot.Count; n++){
														comboSet.buttonShown[n] = true;
													}

													//Reference all the animations for this combo to use later
													AddReferences(comboSet, starterSet, i);

													//Make sure no reference is causing conflicts in the sequence of animation combos
													ConflictCheck(starterSet, i);
														
													//If there were any references kept, reAdd them starting from index after the current
													if(comboSet.keepReference.Count > 0){

														comboSet.animationReference.InsertRange(i + 1, comboSet.keepReference);
														
														//Then clear the lists
														comboSet.keepReference.Clear();
													}

													//If there were any references kept, reAdd them starting from index after the current
													//Starter animation uses the first index so don't include it; index begins a 1 instead
													if(comboSet.referenceCombos.Count > 0){

														comboSet.theCombos.InsertRange(i + 2, comboSet.referenceCombos);
														
														//Then clear the lists
														comboSet.referenceCombos.Clear();

													}
													
													
												}
											}


										}else{

											//Make sure index not out of range
											if(comboSet.activatorIndex[i] > gacSettings.activators.Count){
												//Remove this spot
												RemoveAnimSpot(comboSet, i);
											}

											if(i > 0){
												//Make sure this is the last animation spot in the index
												if(i == (comboSet.animSpot.Count - 1) && comboSet.setAnim[i-1]){

													//Create a new toolbar style to use
													style = new GUIStyle(EditorStyles.toolbarButton);
													style.normal.textColor = Color.blue;
													style.onNormal.textColor = Color.blue;
													style.fontStyle = FontStyle.Bold;

													if (GUI.Button(new Rect (guiDefaultPosition.x + 229, guiDefaultPosition.y, 33, 20),
													               new GUIContent("D", "Adds an delay animation to this combo"), style)) {
														
														AddAnimSpot(comboSet, true);
													}

													if (GUI.Button(new Rect (guiDefaultPosition.x + 261, guiDefaultPosition.y, 33, 20),
													               new GUIContent("+", "Adds an animation to this combo"), EditorStyles.toolbarButton)) {
														
														AddAnimSpot(comboSet, false);
													}
												}
											}else{

												//Make sure this is the last animation spot in the index
												if(i == (comboSet.animSpot.Count - 1) && comboSet.setAnim[i]){
													
													//Create a new toolbar style to use
													style = new GUIStyle(EditorStyles.toolbarButton);
													style.normal.textColor = Color.blue;
													style.onNormal.textColor = Color.blue;
													style.fontStyle = FontStyle.Bold;
													
													if (GUI.Button(new Rect (guiDefaultPosition.x + 229, guiDefaultPosition.y, 33, 20),
													               new GUIContent("D", "Adds an delay animation to this combo"), style)) {
														
														AddAnimSpot(comboSet, true);
													}
													
													if (GUI.Button(new Rect (guiDefaultPosition.x + 261, guiDefaultPosition.y, 33, 20),
													               new GUIContent("+", "Adds an animation to this combo"), EditorStyles.toolbarButton)) {
														
														AddAnimSpot(comboSet, false);
													}
												}
											}

											//Disable the GUI
											GUI.enabled = false;
											//Create a new toolbar style to use
											style = new GUIStyle(EditorStyles.toolbarPopup);
											style.normal.textColor = Color.black;
											style.onNormal.textColor = Color.black;

											//Make sure index not out of range
											if((comboSet.delayedAnim.Count - 1) >= i){

												//Check if it is a delayed animation spot
												if(comboSet.delayedAnim[i]){
													//Create a new toolbar style to use
													style = new GUIStyle(EditorStyles.largeLabel);
													style.normal.textColor = Color.blue;
													EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 5, guiDefaultPosition.y, 32, guiDefaultPosition.height + 20), "D", style);
													
													//Create a new toolbar style to use
													style = new GUIStyle(EditorStyles.toolbarPopup);
													style.normal.textColor = Color.white;
													style.onNormal.textColor = Color.white;
													GUI.backgroundColor = Color.blue;
												}
											}

											//Make sure index not out of range
											if(comboSet.activatorIndex.Count > i){
												EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 38, guiDefaultPosition.y, 32, guiDefaultPosition.height + 20), comboSet.activatorIndex[i] , activatorStrings, activators,style);
											
												EditorGUI.Popup(new Rect (guiDefaultPosition.x + 80, guiDefaultPosition.y, 100, guiDefaultPosition.height + 20),comboSet.animSpot[i] , comboSet.animNames.ToArray(),style);
											
												EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 195, guiDefaultPosition.y, guiDefaultPosition.width + 40, guiDefaultPosition.height + 20),"" + comboSet.comboSequence[i]);
											}

											GUI.enabled = true;

											//Make sure index not out of range
											if((comboSet.delayedAnim.Count - 1) >= i){

												//Check if it is a delayed animation spot
												if(comboSet.delayedAnim[i]){
													GUI.backgroundColor = Color.white;
												}
											}
											//Create a new toolbar style to use
											style = new GUIStyle(EditorStyles.toolbarButton);
											style.normal.textColor = Color.red;
											style.onNormal.textColor = Color.red;

											//Make sure not the first animation slot in the index
											if(i > 0){

												//If button is shown for previous index
												if(comboSet.buttonShown[i - 1]){
													if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 33, guiDefaultPosition.height + 20),
														new GUIContent("EDIT", "Edit the animation for this spot of the combo"), style)) {

														//Register animation spot as not set
														comboSet.setAnim[i] = false;

														//Loop through all animSpots after the index and hide all their buttons
														for (int n = i; n < comboSet.animSpot.Count; n++){
															comboSet.buttonShown[n] = false;
														}

														//Check to make sure there are no more conflicts
														EditConflicts(starterSet, i);
														
														//Remove any animations below this index that were added to combo for now
														RemoveBelow(comboSet, i);

													}
												}

											}else{

												if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 33, guiDefaultPosition.height + 20),
												               new GUIContent("EDIT", "Edit the animation for this spot of the combo"), style)) {

													//Register animation spot as not set
													comboSet.setAnim[i] = false;

													//Loop through all animSpots after the index and hide all their buttons
													for (int n = i; n < comboSet.animSpot.Count; n++){
														comboSet.buttonShown[n] = false;
													}

													//Check to make sure there are no more conflicts
													EditConflicts(starterSet, i);

													//Remove any animations below this index that were added to combo for now
													RemoveBelow(comboSet, i);
													
												}
											}


											if (Event.current.type == EventType.Repaint){

												//If animation spot is blank because of a removal of animation clips/states the remove the spot
												if(comboSet.animSpot[i] >= comboSet.animNames.Count){

													RemoveAnimSpot(comboSet, i);
													EditorGUILayout.EndHorizontal();
													return;
												
												}
											
											}

											//Restore default text color
											style.normal.textColor = Color.white;
											style.onNormal.textColor = Color.white;
										}
									}
								}
								EditorGUILayout.EndHorizontal();

								//Make sure the index is within it's limits
								if(i < comboSet.conflicted.Count){

									//Check if conflicted first, and if so display message at each conflicted index
									if(comboSet.conflicted[i]){

										//Reset the position dimensions to 1
										guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

										EditorGUILayout.BeginHorizontal();

										GUI.DrawTexture(new Rect (guiDefaultPosition.x + 18, guiDefaultPosition.y + 1, GAC.images.gacWarning.width, GAC.images.gacWarning.height), GAC.images.gacWarning, ScaleMode.ScaleToFit, true, 0);

										EditorGUI.HelpBox(new Rect (guiDefaultPosition.x + 37, guiDefaultPosition.y + 20, 290, guiDefaultPosition.height + 45),"Conflicting Combos at 'Sequence' " + comboSet.comboSequence[i] + "!" +
										                  " Please use a different 'Activator' if using different 'Animations' in the same 'Sequence' as another combo.", MessageType.Warning);
										EditorGUILayout.EndHorizontal();
										GUILayout.Space(70);
									}else{
										GUILayout.Space(20);
									}
								}

							

							}
							GUILayout.Space(20);
						}else{
							GUILayout.Space(10);
						}


						//Make sure there is atleast one combo to get maximum amount of animation spots between all combos from this starter
						if(starterSet.starterCombos.Count > 0){
							int maxIndexes = starterSet.starterCombos.Max(a => a.animSpot.Count);
							
							//if conflicts indexes less than the maximum indexes to create, then add more
							if(starterSet.conflicts.Count < maxIndexes){
								starterSet.conflicts.Add (new Conflicts());
							}else if(starterSet.conflicts.Count > maxIndexes){//if there are too much, reduce the amount of indexes
								starterSet.conflicts.RemoveAt(starterSet.conflicts.Count - 1);
							}
						}
					}

				}

				if(starterSet.comboAmount == 0){
					GUILayout.Space(10);
				}

				//Show the separator
				JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
				GUILayout.Space(10);

				//Make sure the starter is not shown and is not the last index
				if(!starterSet.showStarter && startIndex == (gacSettings.starterSlots.Count - 1)){
					GUILayout.Space(10);
				}

				if(gacSettings.starterSlots.Count == 0){
					gacSettings.animationsUsed.Clear();
					gacSettings.starterNames.Clear();
				}

				for (int comboIndex = 0; comboIndex < starterSet.starterCombos.Count; comboIndex++) {
						
					GAC_ComboSetup theComboSet = starterSet.starterCombos[comboIndex];
						
					if(theComboSet.theCombos.Count > 1){
						foreach(string combos in theComboSet.theCombos){
							//And check if this animation is used in a combo
							if(!gacSettings.animationsUsed.Contains(combos)){

								//If not, skip the first index (which is the starter animation that we dont want) and add the actual animation to the used list
								if(gacSettings.animationsUsed.IndexOf(combos) != 0){
									gacSettings.animationsUsed.Add(combos);
								}
							}
							
						}
						
					}
				}
				
			}
			
			//If the starter list is empty, add a 'None' string
			if(gacSettings.startersAvailable.Count == 0){
				gacSettings.startersAvailable.Add("None");

			}else if (gacSettings.startersAvailable.Count > 1){

				//Check if the 'None' string is in the starter list, if so remove it...
				if(gacSettings.startersAvailable.Contains("None")){
					gacSettings.startersAvailable.Remove("None");
				}
			}
		
		}
		#endregion Combo Setup

		
		#region Activator Setup
		if (gacSettings.activatorSetup){

			//Used to track the the total amounts of each type of Activator
			gacSettings.touchAmounts = gacSettings.activatorNames.Where(i => i.IndexOf("Touch") > -1).ToList().Count;		
			gacSettings.synchroAmounts = gacSettings.activatorNames.Where(i => i.IndexOf("Sync") > -1).ToList().Count;
			gacSettings.sequenceAmounts = gacSettings.activatorNames.Where(i => i.IndexOf("Sequence") > -1).ToList().Count;
			
			GUILayout.Space(110);

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 100, 20), "The Activators");

			//Show the popup to modify the Input Source
			gacSettings.inputSource = (GAC.InputSource) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 110, 20), gacSettings.inputSource,EditorStyles.toolbarPopup);

			//Check if there are any activators to use
			if(gacSettings.globalActivatorIndex <= 0){
				EditorGUI.HelpBox(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y + 20, 330, 30), "You have no Activators added to use. Atleast one Activator " +
					"is required before continuing. Please add atleast one Activator.", MessageType.Warning);
			}else{
				//Make sure there are starters added for use;
				if(gacSettings.addedStarters.Count > 0){
					//Add an Activator Slot if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x + 214, guiDefaultPosition.y, 108, 20),new GUIContent("New Activator", "Adds a new animation slot"), EditorStyles.toolbarButton)) {
						AddActivator(gacSettings);

					}
				}else{

					//Otherwise clear the activator slots
					gacSettings.activatorSlots.Clear();
				}
			}
			EditorGUILayout.EndHorizontal();

			//Check if there are any activators to use
			if(gacSettings.globalActivatorIndex <= 0){
				GUILayout.Space(40);
			}else{
				GUILayout.Space(5);
			}

			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Debug Mode", "Log info at certain points"));

			//Show the popup to modify the Debug Mode
			gacSettings.debugMode = (GAC.DebugMode) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.debugMode,EditorStyles.toolbarPopup);

			EditorGUILayout.EndHorizontal();

			GUILayout.Space(5);

			if(gacSettings.touchAmounts > 0){

				//Reference the GAC Event component
				gacTAComponent = Selection.activeGameObject.GetComponent<GAC_InEditorTouchAreas>();

				if(gacTAComponent == null){
					//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
					Selection.activeGameObject.AddComponent<GAC_InEditorTouchAreas>().hideFlags = HideFlags.HideInInspector;
					//Selection.activeGameObject.AddComponent<GAC_InEditorTouchAreas>();//ONLY USE FOR DEBUGGING PURPOSE

				}

				EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				if (GUI.Button(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 25, 20), new GUIContent(GAC.images.gacRefresh, "Refresh these settings for Touch Areas"), EditorStyles.toolbarButton)){
					RefreshTouchAreas();
				}

				if (GUI.Button(new Rect (guiDefaultPosition.x + 190, guiDefaultPosition.y, 25, 20), new GUIContent(GAC.images.gacSave, "Save these settings for Touch Areas"), EditorStyles.toolbarButton)){
					SaveTouchAreas();
				}
				

				//Show the label
				EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Resolution Mode", "Show resolution options"));

				//Create a new toolbar style to use
				GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);

				#if UNITY_STANDALONE

				GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];

				//Loop through all the resolution names 
				for (int index = 0; index < gacSettings.resolutionNames.Length; index++) {

					if(gacSettings.standaloneSavedSlots.Count > index){
						//Put an *asterisk next to name if it has saved settings
						if(gacSettings.standaloneSavedSlots[index].saved){
							gacSettings.resolutionNames[index] = "* " + gacSettings.resolutionNames[index];
						}else{
							//Otherwise remove the *asterisk if available
							if(gacSettings.resolutionNames[index].IndexOf("*") != -1){
								gacSettings.resolutionNames[index].Replace("*","");
							}
						}
					}
				}
				 
				//Check if saving is on for this selection then change color if so
				if(standSet.saved){
					style.normal.textColor = Color.blue;
				}
				#endif

				#if UNITY_IOS
				GAC_SavedTouchArea iOSSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];

				//Loop through all the resolution names 
				for (int index = 0; index < gacSettings.resolutionNames.Length; index++) {
					
					if(gacSettings.iosSavedSlots.Count > index){
						//Put an *asterisk next to name if it has saved settings
						if(gacSettings.iosSavedSlots[index].saved){
							gacSettings.resolutionNames[index] = "* " + gacSettings.resolutionNames[index];
						}else{
							//Otherwise remove the *asterisk if available
							if(gacSettings.resolutionNames[index].IndexOf("*") != -1){
								gacSettings.resolutionNames[index].Replace("*","");
							}
						}
					}
				}
				
				//Check if saving is on for this selection then change color if so
				if(iOSSet.saved){
					style.normal.textColor = Color.blue;
				}
				#endif

				#if UNITY_ANDROID

				GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];

				//Loop through all the resolution names 
				for (int index = 0; index < gacSettings.resolutionNames.Length; index++) {

					if(gacSettings.androidSavedSlots.Count > index){
						//Put an *asterisk next to name if it has saved settings
						if(gacSettings.androidSavedSlots[index].saved){
							gacSettings.resolutionNames[index] = "* " + gacSettings.resolutionNames[index];
						}else{
							//Otherwise remove the *asterisk if available
							if(gacSettings.resolutionNames[index].IndexOf("*") != -1){
								gacSettings.resolutionNames[index].Replace("*","");
							}
						}
					}
				}
				
				//Check if saving is on for this selection then change color if so
				if(androidSet.saved){
					style.normal.textColor = Color.blue;
				}
				#endif

				//Get the current resolution index
				gacSettings.currentIndex = gacSettings.resolutionIndex;

				//Show the Popup to select the mouse button 
				gacSettings.resolutionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 215, guiDefaultPosition.y, 108, 20), gacSettings.resolutionIndex, 
				                                    gacSettings.resolutionNames, style);

				//Check if the dropdown selection changed
				if(gacSettings.currentIndex != gacSettings.resolutionIndex){
					gacSettings.loadTouchArea = true;
				}

				if(gacSettings.loadTouchArea){
					#if UNITY_STANDALONE
					//Retrieve the index
					GAC_SavedTouchArea newStandSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];

					//Check if saving is on for this selection
					if(newStandSet.saved){
						//Then load the saved touch areas settings
						LoadTouchAreas();
					}
					#endif

					#if UNITY_IOS

					if(gacSettings.resolutionIndex < gacSettings.resolutionNames.Length){

						//Retrieve the index
						GAC_SavedTouchArea newiOSSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];
						
						//Check if saving is on for this selection
						if(newiOSSet.saved){
							//Then load the saved touch areas settings
							LoadTouchAreas();
						}
					}
					#endif

					#if UNITY_ANDROID
					//Retrieve the index
					GAC_SavedTouchArea newAndroidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];
					
					//Check if saving is on for this selection
					if(newAndroidSet.saved){
						//Then load the saved touch areas settings
						LoadTouchAreas();
					}
					#endif

					gacSettings.loadTouchArea = false;
				}
				EditorGUILayout.EndHorizontal();

				GUILayout.Space(5);
				
				EditorGUILayout.BeginHorizontal();

				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				//Create a new toolbar style to use
				style = new GUIStyle(EditorStyles.toolbarButton);

				//Only show TAGs in build if allowed
				if(!gacSettings.tagInBuild){
					GUI.color = Color.green;
					
					if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329, 20), new GUIContent("Click to Enable TAG Viewing in Play Mode/Build!", 
					                                                                                            "Turn on to show the TAGs when build on device"), style)){
						gacSettings.tagInBuild = true;
						
					}
					GUI.color = Color.white;
				}else{
					GUI.color = Color.red;
					if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329, 20), new GUIContent("Click to Disable TAG Viewing in Play Mode/Build!", 
					                                                                                            "Turn off showing the TAGs when build on device"), style)){
						gacSettings.tagInBuild = false;

					}
					GUI.color = Color.white;
				}
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(23);

				EditorGUILayout.BeginHorizontal();

				//Reset the position dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				GUIStyle sceneStyle = new GUIStyle(GUI.skin.GetStyle("Box"));

				//Change label color based on if the TAG window is ready
				if(gacSettings.tagWindowReady){
					GUI.backgroundColor = Color.green;
					GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 170, 18), "TAG is Ready!", sceneStyle);
					GUI.backgroundColor = Color.white;
				}else{
					GUI.backgroundColor = Color.red;
					GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 170, 18), "TAG is Not Ready!", sceneStyle);
					GUI.backgroundColor = Color.white;
				}

				//Create a new toolbar style to use
				style = new GUIStyle(EditorStyles.toolbarButton);

				//Only allow to turn on TAG window if not already on
				if(!gacSettings.simulate){
					style.normal.textColor = Color.blue;

					if (GUI.Button(new Rect (guiDefaultPosition.x + 171, guiDefaultPosition.y, 158, 20), new GUIContent("Start TAG Resolution Simulation", 
					                                                                                            "Turn on Simulation of Touch Areas for selected resolution"), style)){
						gacSettings.tagWindowReady = true;
						gacSettings.simulate = true;

					}
				}else{

					GUI.enabled = false;
					GUI.Button(new Rect (guiDefaultPosition.x + 171, guiDefaultPosition.y, 158, 20), "Start TAG Resolution Simulation", style);
					GUI.enabled = true;

				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);
			}else{

				//Close the TAG window if it is open
				if(tagWindow != null){
					tagWindow.Close();
					gacSettings.simulate = false;
				}

				if(gacTAComponent != null){
					DestroyImmediate(gacTAComponent);
				}
			}

			for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
				GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];

				//Get array of the activators
				int[] activators = gacSettings.activators.ToArray();

				//Convert the activators into a string array
				string[] activatorStrings = gacSettings.activators.Select(a => a.ToString()).ToArray();

				#region Key Input Activator
				if(actSet.useKey){

					GUILayout.Space(3);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);

						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;

						GUI.color = Color.red;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}

						//Show the Popup to select the state
						actSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, guiDefaultPosition.height + 20), actSet.stateIndex, 
						  	                                    actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);

						//Show the Popup to select the key 
						actSet.keyInput = (KeyCode)EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 90, guiDefaultPosition.height + 20),"", actSet.keyInput,EditorStyles.toolbarPopup);

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;

						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("SET", "Set the activator slot to use"),style)){

							//Make sure the input selected is not None
							if(actSet.keyInput != KeyCode.None){

								//Add the Event component if not on the gameobject 
								if(gacComponent == null){
									//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
									Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
									//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
									actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
								}else{
									actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
								}

								//Make sure the Event component is referenced
								if(actSet.evt != null){
									actSet.activatorSet = true;

									//Call to set activators
									SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
								}
						
							}
						}
					
						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							RemoveActivator(gacSettings, actSet, actIndex);
						}

					}else if(actSet.activatorSet){

						//Check if activator has been called
						if(actSet.activatorTriggered){

							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);
						
						//style.fixedHeight = 26;
						style.fontSize = 11;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;
						
						//Hide the GUI
						GUI.enabled = false;
						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.red;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, guiDefaultPosition.height + 20), actSet.stateIndex, 
						                   actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);
						EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 90, guiDefaultPosition.height + 20),"", actSet.keyInput,
						                    EditorStyles.toolbarPopup);

						GUI.enabled = true;

						//Check if activator has been called
						if(actSet.activatorTriggered){

							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}

						//Add the Event component if not on the gameobject 
						if(gacComponent == null){
							//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
							Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
							//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
							actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
						}else{
							actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;

						//Make sure within index range
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						}

						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
                                                                                 			"Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();

							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);

							//Register the index to move
							moveIndex = actIndex;

							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						//Edit the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){

							//Remove the GAC Event script from the gameObject
							DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());

							gacSettings.activatorsForStarters.Clear();
							
							GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
							starterSet.firstActivatorSet = false;

							actSet.activatorSet = false;
						}

					}
					GUI.backgroundColor = Color.white;
					
					EditorGUILayout.EndHorizontal();
				}
				#endregion Key Input Activator
				
				#region Mouse Input Activator
				if(actSet.useMouse){

					GUILayout.Space(3);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.blue;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}

						//Show the Popup to select the state
						actSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, guiDefaultPosition.height + 20), actSet.stateIndex, 
						                                       actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);

						//Show the Popup to select the mouse button 
						actSet.mouseIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 90, guiDefaultPosition.height + 20), actSet.mouseIndex, 
						                                       actSet.mouseInputNames, actSet.mouseInput, EditorStyles.toolbarPopup);//.Select(a => a.ToString()).ToArray(), EditorStyles.toolbarPopup);
						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;

						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("SET", "Set the activator slot to use"), style)){
							
							//Add the Event component if not on the gameobject 
							if(gacComponent == null){
								//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
								Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
								//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
								actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
							}else{
								actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
							}
							
							//Make sure the Event component is referenced
							if(actSet.evt != null){
								actSet.activatorSet = true;
								
								//Call to set activators
								SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
							}
							
						}

						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
					}else if(actSet.activatorSet){

						//Check if activator has been called
						if(actSet.activatorTriggered){

							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);
						
						//style.fixedHeight = 26;
						style.fontSize = 11;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;
						
						//Hide the GUI
						GUI.enabled = false;

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.blue;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;
						
						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators, EditorStyles.toolbarPopup);
						
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, guiDefaultPosition.height + 20), actSet.stateIndex, 
						                   actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 90, guiDefaultPosition.height + 20), actSet.mouseIndex, 
						                   actSet.mouseInputNames, actSet.mouseInput, EditorStyles.toolbarPopup);
						
						GUI.enabled = true;
						
						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}

						//Add the Event component if not on the gameobject 
						if(gacComponent == null){
							//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
							Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
							//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
							actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
						}else{
							actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;

						//Make sure within index range
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						}

						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
						                                                                                           "Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();
							
							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);
							
							//Register the index to move
							moveIndex = actIndex;
							
							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						//Edit the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){
							DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());

							gacSettings.activatorsForStarters.Clear();
							
							GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
							starterSet.firstActivatorSet = false;

							actSet.activatorSet = false;
						}

					}

					GUI.backgroundColor = Color.white;

					EditorGUILayout.EndHorizontal();
				}
				#endregion Mouse Input Activator

				#region Button Input Activator
				if(actSet.useButton){

					GUILayout.Space(3);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.yellow;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}

						//Show the Popup to select the Direction
						actSet.inputIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 175, guiDefaultPosition.y, 85, guiDefaultPosition.height + 20), actSet.inputIndex, 
						                                    actSet.inputTypeNames, EditorStyles.toolbarPopup);
						
						if(actSet.inputIndex == 0){
							//Show the Popup to select the Direction
							actSet.directionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 75, guiDefaultPosition.height + 20), actSet.directionIndex, 
							                                        actSet.directionNames, EditorStyles.toolbarPopup);

						}else{

							//Show the Popup to select the state
							actSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 75, guiDefaultPosition.height + 20), actSet.stateIndex, 
							                                       actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);

						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;

						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("SET", "Set the activator slot to use"), style)){

							if(actSet.inputIndex == 0){
								//Make sure an input string has been added for use
								if(actSet.inputText  != null && actSet.inputText  != "The Input" && actSet.inputTextY  != null && actSet.inputTextY  != "The Input Y"){
									
									//Add the Event component if not on the gameobject 
									if(gacComponent == null){
										//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
										Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
										//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
										actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
									}else{
										actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
									}
									
									//Make sure the Event component is referenced
									if(actSet.evt != null){
										actSet.activatorSet = true;
										
										//Call to set activators
										SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
									}
									
								}
							}else{
								//Make sure an input string has been added for use
								if(actSet.inputText  != null && actSet.inputText  != "The Input Name"){

									//Add the Event component if not on the gameobject 
									if(gacComponent == null){
										//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
										Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
										//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
										actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
									}else{
										actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
									}
									
									//Make sure the Event component is referenced
									if(actSet.evt != null){
										actSet.activatorSet = true;
										
										//Call to set activators
										SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
									}

								}

							}
							
						}

						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							RemoveActivator(gacSettings, actSet, actIndex);
						}
					}else if(actSet.activatorSet){

						//Check if activator has been called
						if(actSet.activatorTriggered){
							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}

						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);
						
						//style.fixedHeight = 26;
						style.fontSize = 11;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;
						
						//Hide the GUI
						GUI.enabled = false;
						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.yellow;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators, EditorStyles.toolbarPopup);
 						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 175, guiDefaultPosition.y, 85, guiDefaultPosition.height + 20), actSet.inputIndex, 
						                                    actSet.inputTypeNames, EditorStyles.toolbarPopup);
						
						if(actSet.inputIndex == 0){
							EditorGUI.Popup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 75, guiDefaultPosition.height + 20), actSet.directionIndex, 
							                                        actSet.directionNames, EditorStyles.toolbarPopup);
							
						}else{
							EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 75, guiDefaultPosition.height + 20), actSet.stateIndex, 
							                                       actSet.stateInputNames, actSet.stateInput, EditorStyles.toolbarPopup);
							
						}

						GUI.enabled = true;
						
						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}

						//Add the Event component if not on the gameobject 
						if(gacComponent == null){
							//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
							Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
							//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
							actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
						}else{
							actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;

						//Make sure within index range
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						} 
					
						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
						                                                                                           "Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();
							
							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);
							
							//Register the index to move
							moveIndex = actIndex;
							
							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){
							DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());

							gacSettings.activatorsForStarters.Clear();

							GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
							starterSet.firstActivatorSet = false;


							actSet.activatorSet = false;
						}

					}

					GUI.backgroundColor = Color.white;

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						if(actSet.inputIndex == 0){
							//Show the label
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 66, guiDefaultPosition.y, 28, 20), "X", EditorStyles.label);

							//Show the TextField to write the input event name
							actSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 76, guiDefaultPosition.y, 85, guiDefaultPosition.height + 16), actSet.inputText, 
							                                        EditorStyles.textField);

							//Show the label
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 171, guiDefaultPosition.y, 28, 20), "Y", EditorStyles.label);

							//Show the TextField to write the input event name
							actSet.inputTextY  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 181, guiDefaultPosition.y, 86, guiDefaultPosition.height + 16), actSet.inputTextY, 
							                                         EditorStyles.textField);
						}else{
							//Show the TextField to write the input event name
							actSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 181, guiDefaultPosition.y, 86, guiDefaultPosition.height + 16), actSet.inputText, 
							                                        EditorStyles.textField);

						}
					}else{

						GUI.enabled = false;

						if(actSet.inputIndex == 0){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 66, guiDefaultPosition.y, 28, 20), "X", EditorStyles.label);
							EditorGUI.TextField(new Rect (guiDefaultPosition.x + 76, guiDefaultPosition.y, 85, guiDefaultPosition.height + 16), actSet.inputText, 
							                                        EditorStyles.textField);

							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 171, guiDefaultPosition.y, 28, 20), "Y", EditorStyles.label);
							EditorGUI.TextField(new Rect (guiDefaultPosition.x + 181, guiDefaultPosition.y, 86, guiDefaultPosition.height + 16), actSet.inputTextY, 
							                                         EditorStyles.textField);
						}else{
							EditorGUI.TextField(new Rect (guiDefaultPosition.x + 181, guiDefaultPosition.y, 86, guiDefaultPosition.height + 16), actSet.inputText, 
							                                        EditorStyles.textField);					
						}
						GUI.enabled = true;
					}

					EditorGUILayout.EndHorizontal();
					GUILayout.Space(20);
				}
				#endregion Button Input Activator

				#region Touch Input Activator
				if(actSet.useTouch){

					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();

					actSet.name = gacSettings.activatorNames[actIndex];
					gacSettings.activatorNames[actIndex] = "Touch Input " + (actIndex + 1);

					//Make sure the activator isn't set
					if(!actSet.activatorSet){
						
						actSet.touchNameNotSet = gacSettings.activatorNames[actIndex];
					}

					//Make sure x positions are not less than 0
					if(actSet.touchPosition.x < 0){
						actSet.touchPosition.x = 0;
					}
					
					//Make sure y positions are not less than 50
					if(actSet.touchPosition.y < 50){
						actSet.touchPosition.y = 50;
					}
					//Make sure dimensions are not smaller than 100 
					if(actSet.touchDimensions.x < 100){
						actSet.touchDimensions.x = 100;
					}
					
					//Make sure dimensions are not smaller than 100 
					if(actSet.touchDimensions.y < 100){
						actSet.touchDimensions.y = 100;
					}

					//Give each touch activator slot a reference index based on when it was created; for easier searching later
					if(actSet.touchReferenceIndex == 0){

						//Make sure the current counter number is not in the list yet
						if(!gacSettings.touchSlotReferences.Contains(gacSettings.touchIndexCounter)){

							//Then register this activator with this number
							actSet.touchReferenceIndex = gacSettings.touchIndexCounter;

							gacSettings.touchSlotReferences.Add (actSet.touchReferenceIndex);
							gacSettings.touchIndexCounter = 0;
						}else{
							//Otherwise increase the counter to use another number for reference
							gacSettings.touchIndexCounter++;
						}
					}

					//Reset the list regularly to prevent bloat
					gacSettings.touchSlotNames.Clear();

					//Add the own tag to the list of names to select
					if(gacSettings.touchSlotNames.IndexOf("Own") == -1){
						gacSettings.touchSlotNames.Add ("Own");
					}

					//Add all the current activator names and sort them
					gacSettings.touchSlotNames.AddRange(gacSettings.activatorNames);

					//Make sure the remove empty indexes and indexes that are This Touch Name, and are not Touch Activators
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf("Sync") == -1).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf("Sequence") == -1).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf("Key") == -1).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf("Mouse") == -1).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf("Button") == -1).Distinct().ToList();
					gacSettings.touchSlotNames = gacSettings.touchSlotNames.Where(s => s.IndexOf(gacSettings.activatorNames[actIndex]) == -1).Distinct().ToList();

					//Also check if the Touch Activator that was set and added, make sure it hasn't been un-set for editing; if so remove it
					for (int i = 0; i < gacSettings.activatorSlots.Count; i++){

						if(gacSettings.activatorSlots[i].touchNameNotSet != null){
							gacSettings.touchSlotNames.Remove(gacSettings.activatorSlots[i].touchNameNotSet);
						}
					}

					//Sort the list; NOTE: need to find a better way to do this; doesn't sort numerically
					gacSettings.touchSlotNames.Sort();

					if(gacSettings.simulate){
						if(!gacSettings.tagWindowReady){
							GUI.enabled = false;
						}
					}

					if(!actSet.showActivator){
						if(actSet.activatorSet){

							//Check if activator has been called
							if(actSet.activatorTriggered){
								
								//Then check if the animation for this activator is playing, then change activator background to green to signify this
								if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
									GUI.backgroundColor = Color.green;
								}
							}

							GUILayout.Space(2);
							EditorGUILayout.BeginHorizontal();

							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							//Change the font
							GUIStyle newStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
							newStyle.alignment = TextAnchor.MiddleCenter;
							newStyle.fontStyle = FontStyle.Bold;
							newStyle.fontSize = 10;

							if(gacSettings.activatorNames[actIndex]!= null){
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 295, 18), actSet.gestures + " | " + 
								                     gacSettings.activatorNames[actIndex] + " - " + 
								                     gacSettings.addedStarters[actSet.animationIndex], newStyle);
								
							}

							if(actSet.showTouchArea){
								GUI.color = Color.green;
								//Hide the Touch Area if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 35, 20), new GUIContent("ON", "Hide the Touch Area"),EditorStyles.toolbarButton)){
									actSet.showTouchArea = false;

								}
								GUI.color = Color.white;
							}else{
								GUI.color = Color.red;
								//Show the Touch Area if button is pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 294, guiDefaultPosition.y, 35, 20), new GUIContent("OFF", "Show the Touch Area"),EditorStyles.toolbarButton)){
									actSet.showTouchArea = true;
									
								}
								GUI.color = Color.white;
							}

							EditorGUILayout.EndHorizontal();
							
							if(gacSettings.activatorNames[actIndex] != null){
								GUILayout.Space(19);
								
							}
						}
					}

					GUILayout.Space(1);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = new Color(0.6f, 1, 0, 1);
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}
						
						//Show the popup to modify the Touch Gesture
						actSet.gestures = (GAC_ActivatorSetup.Gestures) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, 20), actSet.gestures,EditorStyles.toolbarPopup);

						//Show the Popup to select the mouse button 
						actSet.touchIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 50, guiDefaultPosition.height + 20), actSet.touchIndex, 
						                                       actSet.touchAmountNames, EditorStyles.toolbarPopup);

						//Make sure there are syncs in the list to use and the list index is not empty, otherwise disable set
						if(gacSettings.touchSlotNames.Count == 0 || actSet.touchSlotIndex < 0 || actSet.touchSlotIndex > gacSettings.touchSlotNames.Count - 1){
							actSet.touchSlotIndex = 0;
						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;
						
						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("SET", "Set the activator slot to use"),style)){
							
							//Add the Event component if not on the gameobject 
							if(gacComponent == null){
								//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
								Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
								//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
								actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
							}else{
								actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
							}
							
							//Make sure the Event component is referenced
							if(actSet.evt != null){
								actSet.activatorSet = true;
								actSet.showActivator = false;
								
								//Call to set activators
								SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);

								//Get the touch name that will be set to use
								actSet.setTouchName = gacSettings.touchSlotNames[actSet.touchSlotIndex];

								//Make sure the set name is valid/found
								if(gacSettings.activatorNames.IndexOf(actSet.setTouchName) > -1){

									actSet.setTouchReferenceIndex = gacSettings.activatorSlots[gacSettings.activatorNames.IndexOf(actSet.setTouchName)].touchReferenceIndex;

								}
							}
							
							
						}

						//Only if activator set to show
						if(!actSet.showActivator){
							if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 40, 20), new GUIContent("Show", "Show the inputs for this activator"), style)){
								actSet.showActivator = true;
							}
						}else{
							if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 40, 20), new GUIContent("Hide", "Hide the inputs for this activator"), style)){
								actSet.showActivator = false;
							}
						}
						
						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							gacSettings.touchSlotReferences.Remove (actSet.touchReferenceIndex);
							RemoveActivator(gacSettings, actSet, actIndex);
						}

						//Only if index is not set to it's default 'Own', which is 0
						if(actSet.touchSlotIndex > 0){

							//Get the touch slot that has the name of the sync that was set
							var getTouchSlot = gacSettings.activatorSlots.Where (i => i.name == gacSettings.touchSlotNames[actSet.touchSlotIndex]).ToList();

							//Set as being used
							getTouchSlot[0].touchInUse = false;
						}
					}else if(actSet.activatorSet){

						actSet.touchNameNotSet = "";

						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}

						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);
						
						//style.fixedHeight = 26;
						style.fontSize = 11;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;


						//Hide the GUI
						GUI.enabled = false;
						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = new Color(0.6f, 1, 0, 1);
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, 20), actSet.gestures,EditorStyles.toolbarPopup);
						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 50, guiDefaultPosition.height + 20), actSet.touchIndex, 
						                   actSet.touchAmountNames, EditorStyles.toolbarPopup);


						GUI.enabled = true;

						if(gacSettings.simulate){
							if(!gacSettings.tagWindowReady){
								GUI.enabled = false;
							}
						}

						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;

						//Only if activator set to show
						if(!actSet.showActivator){
							if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 40, 20), new GUIContent("Show", "Show the inputs for this activator"), style)){
								actSet.showActivator = true;
							}
						}else{
							if (GUI.Button(new Rect (guiDefaultPosition.x + 220, guiDefaultPosition.y, 40, 20), new GUIContent("Hide", "Hide the inputs for this activator"), style)){
								actSet.showActivator = false;
							}
						}

						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}
						
						//Add the Event component if not on the gameobject 
						if(gacComponent == null){
							//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
							Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
							//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
							actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
						}else{
							actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
						}
						
						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;
						
						//Make sure within index range
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						}

						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
						                                                                                           "Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();
							
							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);
							
							//Register the index to move
							moveIndex = actIndex;
							
							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						//Edit the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){



							if(actSet.touchInUse){
								//Show the dialog to decide 
								if(EditorUtility.DisplayDialog("You are currently using this Touch Activator's settings in another Touch Slot Setup!", "Clicking " +
									"'EDIT' will reset the Touch Input to it's 'Own' settings. Do you want to 'EDIT' this Touch Activator?", "Yes", "No")){

									//Get all the touch slots that has the reference of this activator set to use
									var getTouchSlot = gacSettings.activatorSlots.Where (i => i.setTouchReferenceIndex == actSet.touchReferenceIndex).ToList();

									//Then loop through
									for(var i = 0; i < getTouchSlot.Count; i++){

										//Revert the activator to not set and show it GUI settings
										getTouchSlot[i].activatorSet = false;
										getTouchSlot[i].showActivator = true;

									}

									//Remove the GAC Event script from the gameObject
									DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());
									gacSettings.touchNamesSet.Remove(gacSettings.activatorNames[actIndex]);
									gacSettings.touchNameRemoved = gacSettings.activatorNames[actIndex];

									actSet.setTouchName = "";
									actSet.setTouchReferenceIndex = 0;

									gacSettings.activatorsForStarters.Clear();
									
									GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
									starterSet.firstActivatorSet = false;
									
									actSet.activatorSet = false;
									actSet.showActivator = true;
								}
							}else{

								//Remove the GAC Event script from the gameObject
								DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());
								gacSettings.touchNamesSet.Remove(gacSettings.activatorNames[actIndex]);
								gacSettings.touchNameRemoved = gacSettings.activatorNames[actIndex];
								
								actSet.setTouchName = "";
								actSet.setTouchReferenceIndex = 0;
								
								gacSettings.activatorsForStarters.Clear();
								
								GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
								starterSet.firstActivatorSet = false;
								
								actSet.activatorSet = false;
								actSet.showActivator = true;
							}
						}

						//Only if index is not set to it's default 'Own', which is 0
						if(actSet.touchSlotIndex > 0){

							//Get the touch slot that has the name of the touch Slot that was set
							var getTouchSlot = gacSettings.activatorSlots.Where (i => i.name == gacSettings.touchSlotNames[actSet.touchSlotIndex]).ToList();

							//Then loop thoough them and set as being used
							for(var i = 0; i < getTouchSlot.Count; i++){
								getTouchSlot[i].touchInUse = true;
							}
						}

					}
					EditorGUILayout.EndHorizontal();


					if(actSet.activatorSet){
						GUILayout.Space(5);
					}

					//Only if activator set to show
					if(actSet.showActivator){

						if(!actSet.activatorSet){
							GUILayout.Space(5);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 107, guiDefaultPosition.y, 130, 20), "TOUCH AREA SETUP", EditorStyles.boldLabel);
							
							EditorGUILayout.EndHorizontal();

							GUILayout.Space(40);

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 100, 20), "USE TOUCH AREA");

							//Show the Popup for all the sync slot inputs available
							actSet.touchSlotIndex  = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 90, guiDefaultPosition.height + 16), actSet.touchSlotIndex, gacSettings.touchSlotNames.ToArray(), 
							                                             EditorStyles.toolbarPopup);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);

							//Only if index is not set to it's default 'Own', which is 0
							if(actSet.touchSlotIndex > 0){

								//Check if showing the Touch Area square
								if(actSet.showTouchArea){

									//Then save this state as was showing
									actSet.wasShowing = actSet.showTouchArea;
								}

								//Make sure within range
								if(actSet.touchSlotIndex < gacSettings.touchSlotNames.Count){

									//Make sure the name from slot index is valid/found
									if(gacSettings.activatorNames.IndexOf(gacSettings.touchSlotNames[actSet.touchSlotIndex]) > -1){
										
										//Set this Touch activator to mimic the other Touch Name that was set
										GAC_ActivatorSetup newActSet = gacSettings.activatorSlots[gacSettings.activatorNames.IndexOf(gacSettings.touchSlotNames[actSet.touchSlotIndex])];

										actSet = newActSet;

									}
								}
							}else{

								//Check if the state saved was set to true
								if(actSet.wasShowing){

									//Then reset the state of the Touch Area to it's saved state
									actSet.showTouchArea = actSet.wasShowing;

									//Reset the state saving 
									actSet.wasShowing = false;
								}
							}

							GUI.color = Color.yellow;
							if(actSet.showTouchArea){
								//Show the visual area when pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y - 20, 134, 20), new GUIContent("HIDE VISUAL TOUCH AREA", "Show the visual touch area"), EditorStyles.toolbarButton)){
									actSet.showTouchArea = false;
								}
							}else{
								//Hide the visual area when pressed
								if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y - 20, 134, 20), new GUIContent("SHOW VISUAL TOUCH AREA", "Show the visual touch area"), EditorStyles.toolbarButton)){
									actSet.showTouchArea = true;
								}
							}
							GUI.color = Color.white;

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 100, 20), "TOUCH POSITION");

							actSet.touchPosition = EditorGUI.Vector2Field(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 20),"",actSet.touchPosition);
							
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 130, 20), "TOUCH DIMENSONS");
							
							actSet.touchDimensions = EditorGUI.Vector2Field(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 20),"",actSet.touchDimensions);
							
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 130, 20), "TOUCH AREA COLOR");

							//Show the field to modify the gizmo color
							actSet.areaColor = EditorGUI.ColorField(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 18),actSet.areaColor);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);

						}else{

							GUI.enabled = false;
							GUILayout.Space(5);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 107, guiDefaultPosition.y, 130, 20), "TOUCH AREA SETUP", EditorStyles.boldLabel);
							
							EditorGUILayout.EndHorizontal();
							
							GUILayout.Space(40);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 122, guiDefaultPosition.y + 20, 130, 20), actSet.name, EditorStyles.boldLabel);

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 100, 20), "USE TOUCH AREA");
							EditorGUI.Popup(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 90, guiDefaultPosition.height + 16), actSet.touchSlotIndex, gacSettings.touchSlotNames.ToArray(), 
							                                         EditorStyles.toolbarPopup);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);

							//Only if index is not set to it's default 'Own', which is 0
							if(actSet.touchSlotIndex > 0){

								//Get the touch slot that has the reference of this activator set to use
								var getTouchSlot = gacSettings.activatorSlots.Where (i => i.touchReferenceIndex == actSet.setTouchReferenceIndex).ToList();

								//Make sure this is valid/found
								if(gacSettings.activatorSlots.IndexOf(getTouchSlot[0]) > -1){

									//Get the index to use for the drop down
									actSet.touchSlotIndex = gacSettings.touchSlotNames.IndexOf(gacSettings.activatorNames[gacSettings.activatorSlots.IndexOf(getTouchSlot[0])]);

									//Make sure within range
									if(actSet.touchSlotIndex > -1 && actSet.touchSlotIndex < gacSettings.touchSlotNames.Count){

										//Get the touch name that will be set to use
										actSet.setTouchName = gacSettings.touchSlotNames[actSet.touchSlotIndex];

										//Make sure this input name is actually one of the Touch activators
										if(gacSettings.activatorNames.IndexOf(actSet.setTouchName) > -1){

											//Set this Touch activator to mimic the other Touch Name that was set
											GAC_ActivatorSetup newActSet = gacSettings.activatorSlots[gacSettings.activatorNames.IndexOf(actSet.setTouchName)];

											//Set this Touch activator to mimic the other Touch Name that was set
											actSet = newActSet;
										}
									}
								}
							}

							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 100, 20), "TOUCH POSITION");
							EditorGUI.Vector2Field(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 20),"",actSet.touchPosition);
							
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);
							
							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 130, 20), "TOUCH DIMENSONS");
							EditorGUI.Vector2Field(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 20),"",actSet.touchDimensions);
							
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);
							
							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 30, guiDefaultPosition.y, 130, 20), "TOUCH AREA COLOR");
							EditorGUI.ColorField(new Rect (guiDefaultPosition.x + 165, guiDefaultPosition.y, 100, 18),actSet.areaColor);
							EditorGUILayout.EndHorizontal();
							GUILayout.Space(20);
							GUI.enabled = true;
						}
					}

					GUI.backgroundColor = Color.white;
				}
				#endregion Touch Input Activator

				#region Synchro Input Activator
				if (actSet.useSync){

					actSet.name = gacSettings.activatorNames[actIndex];
					gacSettings.activatorNames[actIndex] = "Sync-Input " + (actIndex + 1);

					if(actSet.syncReferenceIndex == 0){
						if(!gacSettings.syncSlotReferences.Contains(gacSettings.syncIndexCounter)){
							actSet.syncReferenceIndex = gacSettings.syncIndexCounter;
							
							gacSettings.syncSlotReferences.Add (actSet.syncReferenceIndex);
							gacSettings.syncIndexCounter = 0;
						}else{
							gacSettings.syncIndexCounter++;
						}
					}

					if(actSet.activatorSet){
						
						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}

						//Loop through the list and add all button strings to the Synced String
						for (int index = 0; index < actSet.inputStrings.Count; index++) {
							
							//Check if a mouse input to add a string to signify it
							if (actSet.sourceStrings[index] == "Mouse"){
								string mouseName = "";

								//Register what the name of mouse button set for input based on the index
								if(actSet.inputStrings[index].Contains("0")){
									mouseName = "Left";
								}else if(actSet.inputStrings[index].Contains("1")){
									mouseName = "Right";
								}else if(actSet.inputStrings[index].Contains("2")){
									mouseName = "Middle";
								}

								//Don't do on the first index
								if(index > 0){
									actSet.syncedString = actSet.syncedString + " + " + mouseName + "Mouse ";
								}else{
									actSet.syncedString =  actSet.syncedString + System.Environment.NewLine + mouseName + "Mouse ";
								}
							}else{ 
								
								//If there is an input string registered
								if(!string.IsNullOrEmpty(actSet.inputStrings[index])){
									if(index > 0){
										actSet.syncedString = actSet.syncedString + " + " + actSet.inputStrings[index] + " ";
									}else{
										actSet.syncedString =  actSet.syncedString + System.Environment.NewLine + actSet.inputStrings[index] + " ";
									}
								}
								
							}
						}

						if(actSet.showSync){
							GUILayout.Space(1);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							
							//Change the font
							GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
							style.alignment = TextAnchor.MiddleCenter;
							style.fontStyle = FontStyle.Bold;
							
							if(actSet.syncedString != null){
								
								//Check the length of the string and increase the box height if string too long to fit
								if(actSet.syncedString.Length <= 58){
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 32), actSet.syncedString, style);
									
								}else if(actSet.syncedString.Length > 58 && actSet.syncedString.Length < 144){
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 46), actSet.syncedString, style);
									
								}else if(actSet.syncedString.Length > 144 && actSet.syncedString.Length < 194){
									
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 60), actSet.syncedString, style);
									
								}else{
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 74), actSet.syncedString, style);
								}
							}
							EditorGUILayout.EndHorizontal();
							
							if(actSet.syncedString != null){
								//Check the length of the string and increase the box height if string too long to fit
								if(actSet.syncedString.Length <= 58){
									GUILayout.Space(30);
								}else if(actSet.syncedString.Length > 58 && actSet.syncedString.Length < 144){
									GUILayout.Space(44);
								}else if(actSet.syncedString.Length > 144 && actSet.syncedString.Length < 194){
									GUILayout.Space(58);	
								}else{
									GUILayout.Space(72);
								}
							}
							
						}
					
					}else{

						//Register the sync activator name to remove from the sync slot names of sequence activators
						actSet.syncNameNotSet = gacSettings.activatorNames[actIndex];
					}

					
					GUILayout.Space(1);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
					
					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.green;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}

						//Show the popup to modify the Input Source
						actSet.syncSource = (GAC_ActivatorSetup.SyncSource) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, 20), actSet.syncSource,EditorStyles.toolbarPopup);
						
						//Prevent from adding more inputs that 5
						if(actSet.syncSlots.Count < 5){
							
							//Add a Sync Slot if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 32, 20),new GUIContent("Add", "Add input to this activator slot" ), EditorStyles.toolbarButton)){
								AddSync(gacSettings, actSet);                              
							}
						}
						
						//Only if activator set to show
						if(!actSet.showActivator){
							if (GUI.Button(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 34, 20), new GUIContent("Show", "Show the inputs for this activator"), EditorStyles.toolbarButton)){
								actSet.showActivator = true;
							}
						}else{
							if (GUI.Button(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 34, 20), new GUIContent("Hide", "Hide the inputs for this activator"), EditorStyles.toolbarButton)){
								actSet.showActivator = false;
							}
						}
						
						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 234, guiDefaultPosition.y, 26, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						
						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;
						
						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 63, 20), new GUIContent("APPLY", "Set the activator slot to use"), style)){

							//Has to be more than 1 inputs set
							if(actSet.inputStrings.Count > 1){
								
								//Reference if all sync slots are all set
								bool allSet = actSet.syncSlots.All (i => i.isSet == true);
								
								//if all sync slots are set
								if(allSet){
									
									//Add the Event component if not on the gameobject 
									if(gacComponent == null){
										//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
										Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
										//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
										actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
									}else{
										actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
									}

									//Make sure the Event component is referenced
									if(actSet.evt != null){

										//Call to set activators
										SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);

										foreach (var act in gacSettings.activatorSlots){
											
											act.syncWarning = false;
										}
										actSet.syncNameNotSet = "";
										
										actSet.showActivator = false;
										actSet.showSync = true;
										actSet.activatorSet = true;
									}
									
									
								}
							}
						}

						//Make sure to keep the amount of Input Strings in check and not exceed the amount of sequence slots
						if(actSet.inputStrings.Count > actSet.syncSlots.Count){
							actSet.inputStrings.RemoveAt(actSet.inputStrings.Count - 1);
						}
						
						//Loop through the list and add all button strings to the Synced String
						for (int index = 0; index < actSet.inputStrings.Count; index++) {
							
							//Loop through the list and add all button strings to the Synced String
							for (int i = 0; i < actSet.syncSlots.Count; i++) {
								GAC_SyncSetup syncSet = actSet.syncSlots[i];
								
								if(index == i){
									if(syncSet.useKey){
										actSet.inputStrings[index] = syncSet.keyInput + "";
										
									}else if(syncSet.useMouse){
										actSet.inputStrings[index] = syncSet.mouseIndex + "";
										
									}else if(syncSet.useButton){

										if(syncSet.inputIndex == 0){
											actSet.inputStrings[index] = "(" + syncSet.inputText + " + " + syncSet.inputTextY + ") " + syncSet.directionNames[syncSet.directionIndex];
										}else{
											actSet.inputStrings[index] = syncSet.inputText;
										}
									}
								}
							}
							
						}

					}else{

						//Make sure index is in range
						if(actIndex < gacSettings.activatorNames.Count){
							actSet.syncedString = gacSettings.activatorNames[actIndex] + " - " + gacSettings.addedStarters[actSet.animationIndex];
						}


						//Make sure starter added is always more than the animation's index
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						} 

						//Check if activator has been called
						if(actSet.activatorTriggered){
							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}

						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);

						//style.fixedHeight = 26;
						style.fontSize = 11;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;

						//Hide the GUI
						GUI.enabled = false;
						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 11;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.green;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(),EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						
						GUI.enabled = true;
						
						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}
						
						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;
						
						if(!actSet.showSync){
							//Show syncs if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 160, 20), new GUIContent("Click to Show Input Syncs", "Show input syncs"), style)){
								
								actSet.showSync = true;
								
							}
						}else{
							//Hide syncs if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 160, 20), new GUIContent("Click to Hide Input Syncs", "Hide input syncs"), style)){
								
								actSet.showSync = false;
								
							}
						}

						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
						                                                                                           "Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();
							
							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);
							
							//Register the index to move
							moveIndex = actIndex;
							
							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;
						
						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){

							if(actSet.syncInUse){
								//Show the dialog to decide 
								if(EditorUtility.DisplayDialog("You are currently using this Sync Activator in a Sequence Slot Setup!", "Clicking 'EDIT' will remove " +
									"the Sync Input from the Sequence Slot and break it's settings. Do you want to 'EDIT' this Sync Activator?", "Yes", "No")){

									//Remove the GAC Event script from the gameObject
									DestroyImmediate(gacComponent);

									//Reset the list
									gacSettings.activatorsForStarters.Clear();
													
									GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
									starterSet.firstActivatorSet = false;
									
									actSet.activatorSet = false;
									actSet.showActivator = true;
									actSet.showSync = false;
								}
							}else{
								DestroyImmediate(gacComponent);


								gacSettings.activatorsForStarters.Clear();
								
								GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[actSet.animationIndex])];
								starterSet.firstActivatorSet = false;
								
								actSet.activatorSet = false;
								actSet.showActivator = true;
								actSet.showSync = false;
							}
							
							
						}

						//Get the sequence activators in use
						var getSequenceSlots = gacSettings.activatorSlots.Where (i => i.useSequence == true && i.activatorSet == true).ToList();

						//If there is atleast one sequence
						if(getSequenceSlots.Count > 0){

							//Check only one index to see if the sync is being is used
							actSet.syncInUse = getSequenceSlots[0].sequenceSlots.Any(n => n.setSyncReferenceIndex == actSet.syncReferenceIndex);

						}else{
							//Otherwise synce isn't being used
							actSet.syncInUse = false;
						}
					

					}
					EditorGUILayout.EndHorizontal();

					if(actSet.activatorSet){
						if(actSet.showSync){
							GUILayout.Space(5);
						}
					}

					//Display INFO label when there is not atleast 2 slots added
					if(actSet.inputStrings.Count <= 1){
						
						//Change the font
						GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
						style.fontSize = 9;
						
						//GUILayout.Space(5);
						EditorGUILayout.BeginHorizontal();
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

						GUI.backgroundColor = Color.yellow;
						//Only if activator set to show
						if(actSet.showActivator){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30),"Synchro-Activators need atleast 2 Inputs 'Set' to work. Please add more Inputs!", style);						
						}
						GUI.backgroundColor = Color.white;
						EditorGUILayout.EndHorizontal();
						
						//Only if activator set to show
						if(actSet.showActivator){
							GUILayout.Space(25);
						}
					}

					if(actSet.dupeInputWarning){

						//Change the font
						GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
						style.fontSize = 9;
						
						EditorGUILayout.BeginHorizontal();
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
						
						GUI.backgroundColor = Color.yellow;
						
						//Only if activator set to show
						if(actSet.showActivator){
							EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30),"There is already a similar input 'Set' in this Sync Activator. " +
								"Please choose a different input!", style);						
						}
						GUI.backgroundColor = Color.white;
						
						EditorGUILayout.EndHorizontal();
						GUILayout.Space(25);

					}
					//Only if activator set to show
					if(actSet.showActivator){

						EditorGUILayout.BeginHorizontal();
						
						//Reset the position dimensions to 1
						guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

						EditorGUILayout.EndHorizontal();
						
						if(!actSet.activatorSet){
							GUILayout.Space(15);
							
							for (int syncIndex = 0; syncIndex < actSet.syncSlots.Count; syncIndex++) {
								GAC_SyncSetup syncSet = actSet.syncSlots[syncIndex];
								
								EditorGUILayout.BeginHorizontal();
								
								//Reset the position dimensions to 1
								guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

								GUIStyle controlStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
								controlStyle.fontSize = 10;
								controlStyle.alignment = TextAnchor.MiddleCenter;

								//Make sure the sync slot isn't set
								if(!syncSet.isSet){

									if(syncSet.useKey){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Key", controlStyle);

										//Show the Popup to select the key 
										syncSet.keyInput = (KeyCode)EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20),"", syncSet.keyInput,EditorStyles.toolbarPopup);

										//Show the Popup to select the state
										syncSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), syncSet.stateIndex, 
										                                        syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
									}else if(syncSet.useMouse){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Mouse", controlStyle);

										//Show the Popup to select the mouse button 
										syncSet.mouseIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20), syncSet.mouseIndex, 
										                                        syncSet.mouseInputNames, syncSet.mouseInput, EditorStyles.toolbarPopup);

										//Show the Popup to select the state
										syncSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), syncSet.stateIndex, 
										                                        syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
									}else if(syncSet.useButton){
										controlStyle.fontSize = 8;

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Unity Input", controlStyle);

										//Show the Popup to select the Direction
										syncSet.inputIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 47, guiDefaultPosition.height + 20), syncSet.inputIndex, 
										                                    syncSet.inputTypeNames, EditorStyles.toolbarPopup);
										
										if(syncSet.inputIndex == 0){
											//Show the Popup to select the Direction
											syncSet.directionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), syncSet.directionIndex, 
											                                        syncSet.directionNames, EditorStyles.toolbarPopup);
										
											//Show the TextField to write the input event name
											syncSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputText, 
											                                        EditorStyles.textField);

											//Show the TextField to write the input event name
											syncSet.inputTextY  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 213, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputTextY, 
											                                         EditorStyles.textField);

										}else{
											
											//Show the Popup to select the state
											syncSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), syncSet.stateIndex, 
											                                       syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
											//Show the TextField to write the input event name
											syncSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputText, 
										                                        EditorStyles.textField);
										}
									}
									
									//Remove the Sync Slot if button is pressed
									if (GUI.Button(new Rect (guiDefaultPosition.x + 273, guiDefaultPosition.y, 24, 20),new GUIContent("-", "Remove this sync slot" ), EditorStyles.toolbarButton)){
										RemoveSync(gacSettings, actSet, syncIndex);                              
									}
									
								
									//Create a new toolbar style to use
									GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
									style.normal.textColor = Color.green;
									style.onNormal.textColor = Color.green;
		
									//Make sure not the first index
									if(syncIndex > 0){

										//Make sure the previous index has be Set first
										if(actSet.syncSlots[syncIndex - 1].isSet){
											//Set the Sync Slot if button is pressed
											if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("SET", "Set the sync slot to use"),style)){

												//Check if key selected from dropdown
												if(syncSet.useKey){

													//Make sure the key input hasn't been added yet or not set to NONE
													if(!actSet.inputStrings.Contains("" + syncSet.keyInput) && syncSet.keyInput != KeyCode.None){
														actSet.inputStrings.Add ("" + syncSet.keyInput);
														actSet.sourceStrings.Add("Key");
														actSet.inputTriggered.Add (false);
														syncSet.isSet = true;
														actSet.dupeInputWarning = false;
													}else

													//Throw a warning if trying to add same input
													if(actSet.inputStrings.Contains("" + syncSet.keyInput)){
														actSet.dupeInputWarning = true;
													}
												}else if(syncSet.useMouse){//Check if mouse selected from dropdown
													
													//Make sure the mouse input hasn't been added yet
													if(!actSet.inputStrings.Contains("" + syncSet.mouseIndex)){
														actSet.inputStrings.Add ("" + syncSet.mouseIndex);
														actSet.sourceStrings.Add("Mouse");
														actSet.inputTriggered.Add (false);
														syncSet.isSet = true;
														actSet.dupeInputWarning = false;
													}else{
														//Throw a warning if trying to add same input
														actSet.dupeInputWarning = true;
													}
													
												}else if(syncSet.useButton){//Check if button selected from dropdown

													if(syncSet.inputIndex == 0){
														
														//Make sure an input string has been added for use in the Input Manager
														if(!String.IsNullOrEmpty(syncSet.inputText) && syncSet.inputText  != "The Input"
														   && !String.IsNullOrEmpty(syncSet.inputTextY) && syncSet.inputTextY  != "Input Y"){
															
															//Make sure the unity input hasn't been added yet
															if(!actSet.inputStrings.Contains("(" + syncSet.inputText + " + " + syncSet.inputTextY + ")")){
																actSet.inputStrings.Add ("(" + syncSet.inputText + " + " + syncSet.inputTextY + ")");
																actSet.sourceStrings.Add("Button");
																actSet.inputTriggered.Add (false);
																syncSet.isSet = true;
																actSet.dupeInputWarning = false;
															}else{
																//Throw a warning if trying to add same input
																actSet.dupeInputWarning = true;
															}
														}
													}else{
														//Make sure an input string has been added for use in the Input Manager
														if(!String.IsNullOrEmpty(syncSet.inputText) && syncSet.inputText  != "The Input"){
															
															//Make sure the unity input hasn't been added yet
															if(!actSet.inputStrings.Contains("" + syncSet.inputText)){
																actSet.inputStrings.Add (syncSet.inputText);
																actSet.sourceStrings.Add("Button");
																actSet.inputTriggered.Add (false);
																syncSet.isSet = true;
																actSet.dupeInputWarning = false;
															}else{
																//Throw a warning if trying to add same input
																actSet.dupeInputWarning = true;
															}
														}
														
													}
												}
											}
										}
									}else{

										//Set the Sync Slot if button is pressed
										if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("SET", "Set the sync slot to use"),style)){
											//Check if key selected from dropdown
											if(syncSet.useKey){
												
												//Make sure the key input hasn't been added yet or not set to NONE
												if(!actSet.inputStrings.Contains("" + syncSet.keyInput) && syncSet.keyInput != KeyCode.None){
													actSet.inputStrings.Add ("" + syncSet.keyInput);
													actSet.sourceStrings.Add("Key");
													actSet.inputTriggered.Add (false);
													syncSet.isSet = true;
												}
												
											}else if(syncSet.useMouse){//Check if mouse selected from dropdown
												
												//Make sure the mouse input hasn't been added yet
												if(!actSet.inputStrings.Contains("" + syncSet.mouseIndex)){
													actSet.inputStrings.Add ("" + syncSet.mouseIndex);
													actSet.sourceStrings.Add("Mouse");
													actSet.inputTriggered.Add (false);
													syncSet.isSet = true;
												}
												
											}else if(syncSet.useButton){//Check if button selected from dropdown
												
												if(syncSet.inputIndex == 0){
												
													//Make sure an input string has been added for use in the Input Manager
													if(!String.IsNullOrEmpty(syncSet.inputText) && syncSet.inputText  != "The Input"
													   && !String.IsNullOrEmpty(syncSet.inputTextY) && syncSet.inputTextY  != "Input Y"){
														
														//Make sure the unity input hasn't been added yet
														if(!actSet.inputStrings.Contains("(" + syncSet.inputText + " + " + syncSet.inputTextY + ")")){
															actSet.inputStrings.Add ("(" + syncSet.inputText + " + " + syncSet.inputTextY + ")");
															actSet.sourceStrings.Add("Button");
															actSet.inputTriggered.Add (false);
															syncSet.isSet = true;
															actSet.dupeInputWarning = false;
														}else{
															//Throw a warning if trying to add same input
															actSet.dupeInputWarning = true;
														}
													}
												}else{
													//Make sure an input string has been added for use in the Input Manager
													if(!String.IsNullOrEmpty(syncSet.inputText) && syncSet.inputText  != "The Input"){
														
														//Make sure the unity input hasn't been added yet
														if(!actSet.inputStrings.Contains("" + syncSet.inputText)){
															actSet.inputStrings.Add (syncSet.inputText);
															actSet.sourceStrings.Add("Button");
															actSet.inputTriggered.Add (false);
															syncSet.isSet = true;
															actSet.dupeInputWarning = false;
														}else{
															//Throw a warning if trying to add same input
															actSet.dupeInputWarning = true;
														}
													}
												
												}
											}
										}
										
									}
									
								}else{
									GUI.enabled = false;
									
									if(syncSet.useKey){
										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Key", controlStyle);

										EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20),"", syncSet.keyInput,EditorStyles.toolbarPopup);

										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), syncSet.stateIndex, 
										                                        syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
									}else if(syncSet.useMouse){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Mouse", controlStyle);

										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20), syncSet.mouseIndex, 
										                   syncSet.mouseInputNames, syncSet.mouseInput, EditorStyles.toolbarPopup);

										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), syncSet.stateIndex, 
										                                        syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
									}else if(syncSet.useButton){
										controlStyle.fontSize = 8;

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Unity Input", controlStyle);

										EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 47, guiDefaultPosition.height + 20), syncSet.inputIndex, 
										                                     syncSet.inputTypeNames, EditorStyles.toolbarPopup);
										
										if(syncSet.inputIndex == 0){
											EditorGUI.Popup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), syncSet.directionIndex, 
											                                         syncSet.directionNames, EditorStyles.toolbarPopup);
											
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputText, 
											                                         EditorStyles.textField);
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 213, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputTextY, 
											                                          EditorStyles.textField);
											
										}else{
											EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), syncSet.stateIndex, 
											                                       syncSet.stateInputNames, syncSet.stateInput, EditorStyles.toolbarPopup);
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), syncSet.inputText, 
											                                         EditorStyles.textField);
										}
									}
									GUI.enabled = true;
									
									//Create a new toolbar style to use
									GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
									style.normal.textColor = Color.red;
									style.onNormal.textColor = Color.red;
									
									//Edit the Activator Slot if button is pressed
									if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("EDIT", "Edit this sync slot"),style)){
										syncSet.isSet = false;
										
										if(syncIndex < actSet.inputStrings.Count){
											actSet.inputStrings.RemoveAt(syncIndex);
											actSet.sourceStrings.RemoveAt(syncIndex);
											actSet.inputTriggered.RemoveAt(syncIndex);
										}
										
									}
								}
								EditorGUILayout.EndHorizontal();
								GUILayout.Space(18);
							}
						}
						GUILayout.Space(15);
					}
					

					GUI.backgroundColor = Color.white;
				}
				#endregion Synchro Input Activator

				#region Sequence Input Activator			
				if (actSet.useSequence){
				
					//Reference the GAC Event component
					gacComponent = gacSettings.gameObject.GetComponent<GAC_SetEvent>();

					//Give the activator a name
					actSet.name = gacSettings.activatorNames[actIndex];
					gacSettings.activatorNames[actIndex] = "Sequence-Input " + (actIndex + 1);

					gacSettings.syncSlotNames.Clear();

					//Add all the current activator names and sort them
					gacSettings.syncSlotNames.AddRange(gacSettings.activatorNames);


					//Make sure the remove empty indexes, this indexes name, and indexes that are not Sync activators
					gacSettings.syncSlotNames = gacSettings.syncSlotNames.Where(s => !string.IsNullOrEmpty(s) && s.IndexOf("Sequence") == -1).Distinct().ToList();
					gacSettings.syncSlotNames = gacSettings.syncSlotNames.Where(s => s.IndexOf("Key") == -1).Distinct().ToList();
					gacSettings.syncSlotNames = gacSettings.syncSlotNames.Where(s => s.IndexOf("Mouse") == -1).Distinct().ToList();
					gacSettings.syncSlotNames = gacSettings.syncSlotNames.Where(s => s.IndexOf("Button") == -1).Distinct().ToList();
					gacSettings.syncSlotNames = gacSettings.syncSlotNames.Where(s => s.IndexOf("Touch") == -1).Distinct().ToList();

					//Loop through and remove any Sync that's not 'Set' yet but are in the sync slot name list
					for (int i = 0; i < gacSettings.activatorSlots.Count; i++){
						
						if(gacSettings.activatorSlots[i].syncNameNotSet != null){
							gacSettings.syncSlotNames.Remove(gacSettings.activatorSlots[i].syncNameNotSet);
						}
					}

					//Sort the list
					gacSettings.syncSlotNames.Sort();

					//Turn off the warning for syncs if there are sync names to use
					if(gacSettings.syncSlotNames.Count > 0){
						actSet.syncWarning = false;
					}


					if(actSet.inputStrings.Count < actSet.sourceStrings.Count){
						actSet.sourceStrings.RemoveAt(actSet.sourceStrings.Count - 1);
					}

					if(actSet.inputStrings.Count < actSet.inputTriggered.Count){
						actSet.inputTriggered.RemoveAt(actSet.inputTriggered.Count - 1);
					}

					for (int sequenceIndex = 0; sequenceIndex < actSet.sequenceSlots.Count; sequenceIndex++) {
						GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[sequenceIndex];

						//Make sure the sequence slot isn't set
						if(!sequenceSet.isSet){

							if(sequenceSet.useSync){
							
								//Make sure it is not out of range
								if(sequenceSet.syncSlotIndex < 0 || gacSettings.syncSlotNames.Count == 0){
									actSet.activatorSet = false;
									
									EditSequenceSlot(gacSettings, actSet, sequenceIndex); 

									Debug.LogWarning ("GACWarning - GAC has done a Cleanup of Sequence Slots that don't have the Set Synchro activators to use.");
								}

							}
						}
					}

					//Make sure the activator is set
					if(actSet.activatorSet){

						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}

						//Make sure to keep the amount of Input Strings in check and not exceed the amount of sequence slots
						if(actSet.inputStrings.Count > actSet.sequenceSlots.Count){
							actSet.inputStrings.RemoveAt(actSet.inputStrings.Count - 1);
						}
						
						//Loop through the list and add all button strings to the Sequenced String
						for (int index = 0; index < actSet.inputStrings.Count; index++) {
							
							//Check if a mouse input to add a string to signify it
							if (actSet.sourceStrings[index] == "Mouse"){

								string mouseName = "";

								//Register what the name of mouse button set for input based on the index
								if(actSet.inputStrings[index].Contains("0")){
									mouseName = "Left";
								}else if(actSet.inputStrings[index].Contains("1")){
									mouseName = "Right";
								}else if(actSet.inputStrings[index].Contains("2")){
									mouseName = "Middle";
								}

								//Don't do on the first index
								if(index > 0){
									actSet.sequencedString = actSet.sequencedString + "> " + mouseName + "Mouse ";
								}else{
									actSet.sequencedString =  actSet.sequencedString + System.Environment.NewLine + mouseName + "Mouse ";
								}

							}else{ 
								
								//If there is an input string registered
								if(!string.IsNullOrEmpty(actSet.inputStrings[index])){
									if(index > 0){
										actSet.sequencedString = actSet.sequencedString + "> " + actSet.inputStrings[index] + " ";
									}else{
										actSet.sequencedString =  actSet.sequencedString + System.Environment.NewLine + actSet.inputStrings[index] + " ";
									}
								}
								
							}
						}

						if(actSet.showSequence){

							GUILayout.Space(1);
							EditorGUILayout.BeginHorizontal();
							
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
							
							//Change the font
							GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
							style.alignment = TextAnchor.MiddleCenter;
							style.fontStyle = FontStyle.Bold;

							if(actSet.sequencedString != null){
								
								//Check the length of the string and increase the box height if string too long to fit
								if(actSet.sequencedString.Length <= 58){
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 32), actSet.sequencedString, style);
									
								}else if(actSet.sequencedString.Length > 58 && actSet.sequencedString.Length < 144){
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 46), actSet.sequencedString, style);
									
								}else if(actSet.sequencedString.Length > 144 && actSet.sequencedString.Length < 194){
									
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 60), actSet.sequencedString, style);
									
								}else{
									//Show the synced inputs in a GUI box
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 74), actSet.sequencedString, style);
								}
							}
							EditorGUILayout.EndHorizontal();
							
							if(actSet.sequencedString != null){
								//Check the length of the string and increase the box height if string too long to fit
								if(actSet.sequencedString.Length <= 58){
									GUILayout.Space(30);
								}else if(actSet.sequencedString.Length > 58 && actSet.sequencedString.Length < 144){
									GUILayout.Space(44);
								}else if(actSet.sequencedString.Length > 144 && actSet.sequencedString.Length < 194){
									GUILayout.Space(58);	
								}else{
									GUILayout.Space(72);
								}
							}
								
						}

					}


					GUILayout.Space(1);
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					
					//Reset the position dimensions to 1
					guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

					//Make sure the activator isn't set
					if(!actSet.activatorSet){

						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 9;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.magenta;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbarPopup);
						style.normal.textColor = Color.blue;
						style.onNormal.textColor = Color.blue;
						
						//Show the Popup to select the animation
						actSet.animationIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(), style);
						
						//Show the Popup to select the activator
						actSet.activatorIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);
						
						//Always reset the activator index to 1 if its set at 0 or more than the actual activators to use 
						if(actSet.activatorIndex <= 0 || actSet.activatorIndex > gacSettings.globalActivatorIndex){
							actSet.activatorIndex = 1;
						}
						
						//Always reset the animation index to 1 if its set at 0 or more than the actual starters to use 
						if (actSet.animationIndex < 0 || actSet.animationIndex > (gacSettings.addedStarters.Count - 1)){
							actSet.animationIndex = 0;
						}

						//Show the popup to modify the Input Source
						actSet.sequenceSource = (GAC_ActivatorSetup.SequenceSource) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 70, 20), actSet.sequenceSource,EditorStyles.toolbarPopup);


						//Prevent from adding more inputs that 5
						if(actSet.sequenceSlots.Count < 5){
							
							//Add a Sync Slot if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 170, guiDefaultPosition.y, 32, 20),new GUIContent("Add", "Add input to this activator slot" ), EditorStyles.toolbarButton)){
								AddSequence(gacSettings, actSet);                              
							}
						}
						
						//Only if activator set to show
						if(!actSet.showActivator){
							if (GUI.Button(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 34, 20), new GUIContent("Show", "Show the inputs for this activator"), EditorStyles.toolbarButton)){
								actSet.showActivator = true;
							}
						}else{
							if (GUI.Button(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 34, 20), new GUIContent("Hide", "Hide the inputs for this activator"), EditorStyles.toolbarButton)){
								actSet.showActivator = false;
							}
						}
						
						//Remove the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 234, guiDefaultPosition.y, 26, 20),new GUIContent("-", "Remove this activator slot"), EditorStyles.toolbarButton)){
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						
						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;
						
						//Set the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 63, 20), new GUIContent("APPLY", "Set the activator slot to use"), style)){

							//Has to be more than 1 inputs set
							if(actSet.inputStrings.Count > 1){
								
								//Reference if all sync slots are all set
								bool allSet = actSet.sequenceSlots.All (i => i.isSet == true);
								
								//if all sequence slots are set
								if(allSet){

									//Add the Event component if not on the gameobject 
									if(gacComponent == null){
										//USE FOR ASSET RELEASE; Add component then hide it to prevent errors
										Selection.activeGameObject.AddComponent<GAC_SetEvent>().hideFlags = HideFlags.HideInInspector;
										//Selection.activeGameObject.AddComponent<GAC_SetEvent>();//ONLY USE FOR DEBUGGING PURPOSE
										actSet.evt = gacSettings.gameObject.GetComponent<GAC_SetEvent>();
									}else{
										actSet.evt = Selection.activeGameObject.GetComponent<GAC_SetEvent>();
									}

									//Make sure the Event component is referenced
									if(actSet.evt != null){

										//Call to set activators
										SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);

										actSet.showActivator = false;
										actSet.showSequence = true;
										actSet.activatorSet = true;
									}
									
								}
							}

						}
						
					}else{

						//Make sure index within range
						if(actIndex < gacSettings.activatorNames.Count){
							//The register the name of the synced activator
							actSet.sequencedString = gacSettings.activatorNames[actIndex] + " - " + gacSettings.addedStarters[actSet.animationIndex];
						}


						//Make sure within index range
						if(gacSettings.addedStarters.Count > actSet.animationIndex){
							//Call to set activators
							SetActivators(gacSettings.addedStarters[actSet.animationIndex], actSet.activatorIndex);
						}else{
							RemoveActivator(gacSettings, actSet, actIndex);
						}
						
						//Make sure within index range
						if(actSet.activatorIndex > gacSettings.activators.Count){
							RemoveActivator(gacSettings, actSet, actIndex);
						} 

						//Make sure to keep the amount of Input Strings in check and not exceed the amount of sequence slots
						if(actSet.inputStrings.Count > actSet.sequenceSlots.Count){
							actSet.inputStrings.RemoveAt(actSet.inputStrings.Count - 1);
						}

						//Loop through the list and add all button strings to the Synced String
						for (int index = 0; index < actSet.inputStrings.Count; index++) {

							//Loop through the list and add all button strings to the Synced String
							for (int i = 0; i < actSet.sequenceSlots.Count; i++) {
								GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[i];
								
								if(index == i){
									if(sequenceSet.useKey){
										actSet.inputStrings[index] = sequenceSet.keyInput + "";
										
									}else if(sequenceSet.useMouse){
										actSet.inputStrings[index] = sequenceSet.mouseIndex + "";
										
									}else if(sequenceSet.useButton){
										if(sequenceSet.inputIndex == 0){
											actSet.inputStrings[index] = "(" + sequenceSet.inputText + " + " + sequenceSet.inputTextY + ") " + sequenceSet.directionNames[sequenceSet.directionIndex];
										}else{
											actSet.inputStrings[index] = sequenceSet.inputText;
										}
										
									}else if(sequenceSet.useSync){
										
										//Make sure the indexes are within range before setting the input string 
										if(sequenceSet.syncSlotIndex > -1 && gacSettings.syncSlotNames.Count > sequenceSet.syncSlotIndex){
											actSet.inputStrings[index] = gacSettings.syncSlotNames[sequenceSet.syncSlotIndex];
										}
									}
								}
							}

						}

						//Check if activator has been called
						if(actSet.activatorTriggered){

							//Then check if the animation for this activator is playing, then change activator background to green to signify this
							if(GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								GUI.backgroundColor = Color.green;
							}
						}
						
						//Create a new toolbar style to use
						GUIStyle style = new GUIStyle(EditorStyles.toolbar);

						style.fontSize = 9;
						style.fontStyle = FontStyle.Bold;
						style.alignment = TextAnchor.MiddleLeft;
						
						//Hide the GUI
						GUI.enabled = false;
						//Create a new toolbar style to use
						GUIStyle boxStyle = new GUIStyle(EditorStyles.toolbar);
						
						boxStyle.fontSize = 9;
						boxStyle.fontStyle = FontStyle.Bold;
						boxStyle.alignment = TextAnchor.MiddleLeft;
						
						GUI.color = Color.magenta;
						//Show the label
						EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 7, guiDefaultPosition.y, 28, 20), new GUIContent(actSet.inputInitials, "This activator's input source"), boxStyle);
						GUI.color = Color.white;

						EditorGUI.Popup(new Rect (guiDefaultPosition.x + 20, guiDefaultPosition.y, 50, 20), actSet.animationIndex, gacSettings.addedStarters.ToArray(),EditorStyles.toolbarPopup);
						EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 70, guiDefaultPosition.y, 30, guiDefaultPosition.height + 20), actSet.activatorIndex, activatorStrings, activators,EditorStyles.toolbarPopup);

						
						GUI.enabled = true;
						
						//Check if activator has been called
						if(actSet.activatorTriggered){
							
							//Then check if the animation for this activator is not playing, then turn the trigger off
							if(!GAC.IsPlaying(Selection.activeGameObject, gacSettings.addedStarters[actSet.animationIndex])){
								actSet.activatorTriggered = false;
							}
						}

						//Create a new toolbar style to use
						style = new GUIStyle(EditorStyles.toolbarButton);
						style.normal.textColor = Color.green;
						style.onNormal.textColor = Color.green;
						
						if(!actSet.showSequence){
							//Show syncs if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 160, 20), new GUIContent("Click to Show Input Sequences", "Show input sequences"), style)){
								
								actSet.showSequence = true;
								
							}
						}else{
							//Hide syncs if button is pressed
							if (GUI.Button(new Rect (guiDefaultPosition.x + 100, guiDefaultPosition.y, 160, 20), new GUIContent("Click to Hide Input Sequences", "Hide input sequences"), style)){
								
								actSet.showSequence = false;
								
							}
						}

						//Move the activators up or down for rearrangement
						if (GUI.Button(new Rect (guiDefaultPosition.x + 260, guiDefaultPosition.y, 28, 20), new GUIContent(GAC.images.gacDropDown, 
						                                                                                           "Move this activators up or down"), style)) {
							// Now create the menu, add items and show it
							GenericMenu activatorDropdown = new GenericMenu ();
							
							//Register the dropdown items
							activatorDropdown.AddItem(new GUIContent ("Move Up"), false, MoveActivatorUp);
							activatorDropdown.AddItem(new GUIContent ("Move Down"), false, MoveActivatorDown);
							
							//Register the index to move
							moveIndex = actIndex;
							
							//Show the dropdown
							activatorDropdown.ShowAsContext ();
						}

						style.normal.textColor = Color.red;
						style.onNormal.textColor = Color.red;
						
						//Edit the Activator Slot if button is pressed
						if (GUI.Button(new Rect (guiDefaultPosition.x + 288, guiDefaultPosition.y, 35, 20), new GUIContent("EDIT", "Edit this activator slot"), style)){

							EditSequence(gacSettings, actSet, actSet.animationIndex);
							
							
						}

					}
					EditorGUILayout.EndHorizontal();

					if(actSet.activatorSet){
						if(actSet.showSequence){
							GUILayout.Space(5);
						}
					}

					//Only if activator set to show
					if(actSet.showActivator){

						//Display INFO label when there is not atleast 2 slots added
						if(actSet.inputStrings.Count <= 1){
							
							//Change the font
							GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
							style.fontSize = 9;
							
							//GUILayout.Space(5);
							EditorGUILayout.BeginHorizontal();
							//Reset the position dimensions to 1
							guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

							GUI.backgroundColor = Color.yellow;
							//Only if activator set to show
							if(actSet.showActivator){
								EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30),"Sequence-Activators need atleast 2 Inputs 'Set' to work. Please add more Inputs!", style);						
							}
							GUI.backgroundColor = Color.white;
							EditorGUILayout.EndHorizontal();
							
							//Only if activator set to show
							if(actSet.showActivator){
								GUILayout.Space(25);
							}
						}

						//Check if to send a warning about no syncs available
						if(actSet.syncWarning){

							if (actSet.sequenceSource == GAC_ActivatorSetup.SequenceSource.SYNC){
								//Change the font
								GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));
								style.fontSize = 9;

								EditorGUILayout.BeginHorizontal();
								//Reset the position dimensions to 1
								guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

								GUI.backgroundColor = Color.yellow;

								//Only if activator set to show
								if(actSet.showActivator){
									EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30),"There are no Sync Activators set to use in this Sequence Activator. Please Add to use!", style);						
								}
								GUI.backgroundColor = Color.white;

								EditorGUILayout.EndHorizontal();
								GUILayout.Space(25);
							}
						}


						//Make sure the activator isn't set
						if(!actSet.activatorSet){
							GUILayout.Space(15);
							
							for (int sequenceIndex = 0; sequenceIndex < actSet.sequenceSlots.Count; sequenceIndex++) {
								GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[sequenceIndex];
								
								EditorGUILayout.BeginHorizontal();
								
								//Reset the position dimensions to 1
								guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

								GUIStyle controlStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
								controlStyle.fontSize = 10;
								controlStyle.alignment = TextAnchor.MiddleCenter;
								
								//Make sure the sync slot isn't set
								if(!sequenceSet.isSet){
									
									if(sequenceSet.useKey){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Key", controlStyle);

										//Show the Popup to select the key 
										sequenceSet.keyInput = (KeyCode)EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20),"", sequenceSet.keyInput,EditorStyles.toolbarPopup);

										//Show the Popup to select the state
										sequenceSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
										                                            sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
									}else if(sequenceSet.useMouse){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Mouse", controlStyle);

										//Show the Popup to select the mouse button 
										sequenceSet.mouseIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20), sequenceSet.mouseIndex, 
										                                        sequenceSet.mouseInputNames, sequenceSet.mouseInput, EditorStyles.toolbarPopup);

										//Show the Popup to select the state
										sequenceSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
										                                            sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
									}else if(sequenceSet.useButton){
										controlStyle.fontSize = 8;
										
										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Unity Input", controlStyle);
										
										//Show the Popup to select the Direction
										sequenceSet.inputIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 47, guiDefaultPosition.height + 20), sequenceSet.inputIndex, 
										                                     sequenceSet.inputTypeNames, EditorStyles.toolbarPopup);
										
										if(sequenceSet.inputIndex == 0){
											//Show the Popup to select the Direction
											sequenceSet.directionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), sequenceSet.directionIndex, 
											                                         sequenceSet.directionNames, EditorStyles.toolbarPopup);
											
											//Show the TextField to write the input event name
											sequenceSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputText, 
											                                         EditorStyles.textField);
											
											//Show the TextField to write the input event name
											sequenceSet.inputTextY  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 213, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputTextY, 
											                                          EditorStyles.textField);
											
										}else{
											
											//Show the Popup to select the state
											sequenceSet.stateIndex = EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
											                                        sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
											//Show the TextField to write the input event name
											sequenceSet.inputText  = EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputText, 
											                                         EditorStyles.textField);
										}
									}else if(sequenceSet.useSync){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Sync", controlStyle);

										sequenceSet.syncSlotIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 16), sequenceSet.syncSlotIndex, gacSettings.syncSlotNames.ToArray(), 
										                EditorStyles.toolbarPopup);
										
									}
									
									//Remove the Sync Slot if button is pressed
									if (GUI.Button(new Rect (guiDefaultPosition.x + 273, guiDefaultPosition.y, 24, 20),new GUIContent("-", "Remove this sync slot" ), EditorStyles.toolbarButton)){
										EditSequenceSlot(gacSettings, actSet, sequenceIndex);                              
									}
									
									//Create a new toolbar style to use
									GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
									style.normal.textColor = Color.green;
									style.onNormal.textColor = Color.green;

									//Make sure not the first index
									if(sequenceIndex > 0){

										//Make sure the previous index has be Set first
										if(actSet.sequenceSlots[sequenceIndex - 1].isSet){

											//Set the Sync Slot if button is pressed
											if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("SET", "Set the sync slot to use"),style)){
												
												//Check if key selected from dropdown
												if(sequenceSet.useKey){
												
													//Make sure the key input hasn't been added yet or not set to NONE
													if(sequenceSet.keyInput != KeyCode.None){
														actSet.inputStrings.Add ("" + sequenceSet.keyInput);
														actSet.sourceStrings.Add("Key");
														actSet.inputTriggered.Add (false);
														sequenceSet.isSet = true;
													}

													
												}else if(sequenceSet.useMouse){//Check if mouse selected from dropdown

														actSet.inputStrings.Add ("" + sequenceSet.mouseIndex);
														actSet.sourceStrings.Add("Mouse");
														actSet.inputTriggered.Add (false);
														sequenceSet.isSet = true;

												}else if(sequenceSet.useButton){//Check if button selected from dropdown
												
													if(sequenceSet.inputIndex == 0){
													
														//Make sure an input string has been added for use in the Input Manager
														if(!String.IsNullOrEmpty(sequenceSet.inputText) && sequenceSet.inputText  != "The Input"
														   && !String.IsNullOrEmpty(sequenceSet.inputTextY) && sequenceSet.inputTextY  != "Input Y"){

															actSet.inputStrings.Add ("(" + sequenceSet.inputText + " + " + sequenceSet.inputTextY + ")");
															actSet.sourceStrings.Add("Button");
															actSet.inputTriggered.Add (false);
															sequenceSet.isSet = true;
															actSet.dupeInputWarning = false;
														
														}

													}else{

														//Make sure an input string has been added for use in the Input Manager
														if(!String.IsNullOrEmpty(sequenceSet.inputText) && sequenceSet.inputText  != "The Input"){
														
															actSet.inputStrings.Add (sequenceSet.inputText);
															actSet.sourceStrings.Add("Button");
															actSet.inputTriggered.Add (false);
															sequenceSet.isSet = true;
															actSet.dupeInputWarning = false;
															
														}
													
													}
												}else if(sequenceSet.useSync){//Check if SYNC selected from dropdown
													actSet.inputStrings.Add(gacSettings.syncSlotNames[sequenceSet.syncSlotIndex]);
													actSet.sourceStrings.Add ("Sync");
													actSet.inputTriggered.Add (false);

													//Get the sync name that will be set to use
													actSet.setSyncName = gacSettings.syncSlotNames[sequenceSet.syncSlotIndex];

													//Make sure the set name is an actual activator slot
													if(gacSettings.activatorNames.IndexOf(actSet.setSyncName) > -1){

														//Get the reference index from the Set sync
														sequenceSet.setSyncReferenceIndex = gacSettings.activatorSlots[gacSettings.activatorNames.IndexOf(actSet.setSyncName)].syncReferenceIndex;

													}

													sequenceSet.isSet = true;
												}

											}
										}
									}else{
										//Set the Sequence Slot if button is pressed
										if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("SET", "Set the sequence slot to use"),style)){
											
											//Check if key selected from dropdown
											if(sequenceSet.useKey){
												
												//Make sure the key input hasn't been added yet or not set to NONE
												if(sequenceSet.keyInput != KeyCode.None){
													actSet.inputStrings.Add ("" + sequenceSet.keyInput);
													actSet.sourceStrings.Add("Key");
													actSet.inputTriggered.Add (false);
													sequenceSet.isSet = true;
												}
												
												
											}else if(sequenceSet.useMouse){//Check if mouse selected from dropdown
												
												actSet.inputStrings.Add ("" + sequenceSet.mouseIndex);
												actSet.sourceStrings.Add("Mouse");
												actSet.inputTriggered.Add (false);
												sequenceSet.isSet = true;
												
											}else if(sequenceSet.useButton){//Check if button selected from dropdown
												
												if(sequenceSet.inputIndex == 0){
													
													//Make sure an input string has been added for use in the Input Manager
													if(!String.IsNullOrEmpty(sequenceSet.inputText) && sequenceSet.inputText  != "The Input"
													   && !String.IsNullOrEmpty(sequenceSet.inputTextY) && sequenceSet.inputTextY  != "Input Y"){
														
														actSet.inputStrings.Add ("(" + sequenceSet.inputText + " + " + sequenceSet.inputTextY + ")");
														actSet.sourceStrings.Add("Button");
														actSet.inputTriggered.Add (false);
														sequenceSet.isSet = true;
														actSet.dupeInputWarning = false;
														
													}
													
												}else{
													
													//Make sure an input string has been added for use in the Input Manager
													if(!String.IsNullOrEmpty(sequenceSet.inputText) && sequenceSet.inputText  != "The Input"){
													
															actSet.inputStrings.Add (sequenceSet.inputText);
															actSet.sourceStrings.Add("Button");
															actSet.inputTriggered.Add (false);
															sequenceSet.isSet = true;
															actSet.dupeInputWarning = false;
													}
													
												}
											}else if(sequenceSet.useSync){//Check if SYNC selected from dropdown
												actSet.inputStrings.Add(gacSettings.syncSlotNames[sequenceSet.syncSlotIndex]);
												actSet.sourceStrings.Add ("Sync");

												actSet.inputTriggered.Add (false);

												//Get the sync name that will be set to use
												actSet.setSyncName = gacSettings.syncSlotNames[sequenceSet.syncSlotIndex];
												
												//Make sure the set name is an actual activator slot
												if(gacSettings.activatorNames.IndexOf(actSet.setSyncName) > -1){
													
													//Get the reference index from the Set sync
													sequenceSet.setSyncReferenceIndex = gacSettings.activatorSlots[gacSettings.activatorNames.IndexOf(actSet.setSyncName)].syncReferenceIndex;
													
												}
												
												sequenceSet.isSet = true;
											}
											
										}
									}
									
								}else{
									GUI.enabled = false;


									if(sequenceSet.useKey){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Key", controlStyle);

										//Show the Popup to select the key 
										EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20),"", sequenceSet.keyInput,EditorStyles.toolbarPopup);

										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
										                   sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
									}else if(sequenceSet.useMouse){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Mouse", controlStyle);

										//Show the Popup to select the mouse button 
										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 20), sequenceSet.mouseIndex, 
										                   sequenceSet.mouseInputNames, sequenceSet.mouseInput, EditorStyles.toolbarPopup);

										EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 152, guiDefaultPosition.y, 60, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
										                   sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
									}else if(sequenceSet.useButton){
										controlStyle.fontSize = 8;
										
										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Unity Input", controlStyle);

										EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 47, guiDefaultPosition.height + 20), sequenceSet.inputIndex, 
										                                     sequenceSet.inputTypeNames, EditorStyles.toolbarPopup);
										
										if(sequenceSet.inputIndex == 0){
											EditorGUI.Popup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), sequenceSet.directionIndex, 
											                                         sequenceSet.directionNames, EditorStyles.toolbarPopup);
											
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputText, 
											                                         EditorStyles.textField);
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 213, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputTextY, 
											                                          EditorStyles.textField);
											
										}else{
											EditorGUI.IntPopup(new Rect (guiDefaultPosition.x + 97, guiDefaultPosition.y, 55, guiDefaultPosition.height + 20), sequenceSet.stateIndex, 
											                                       sequenceSet.stateInputNames, sequenceSet.stateInput, EditorStyles.toolbarPopup);
											EditorGUI.TextField(new Rect (guiDefaultPosition.x + 153, guiDefaultPosition.y, 60, guiDefaultPosition.height + 16), sequenceSet.inputText, 
											                                         EditorStyles.textField);
										}
									}else if(sequenceSet.useSync){

										GUI.Label(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 50, 18), "Sync", controlStyle);

										EditorGUI.Popup(new Rect (guiDefaultPosition.x + 50, guiDefaultPosition.y, 102, guiDefaultPosition.height + 16), sequenceSet.syncSlotIndex, gacSettings.syncSlotNames.ToArray(), 
										                                       EditorStyles.toolbarPopup);
									
									}
									GUI.enabled = true;

									//Create a new toolbar style to use
									GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
									style.normal.textColor = Color.red;
									style.onNormal.textColor = Color.red;
									
									//Edit the Sequence Slot if button is pressed
									if (GUI.Button(new Rect (guiDefaultPosition.x + 297, guiDefaultPosition.y, 32, 20), new GUIContent("EDIT", "Edit this sequence slot"),style)){
										sequenceSet.isSet = false;

										if(sequenceIndex < actSet.inputStrings.Count){
											actSet.inputStrings.RemoveAt(sequenceIndex);
											actSet.sourceStrings.RemoveAt(sequenceIndex);
											actSet.inputTriggered.RemoveAt(sequenceIndex);
										}

										//Reset
										sequenceSet.setSyncReferenceIndex = 0;

									}
								}
								EditorGUILayout.EndHorizontal();
								GUILayout.Space(18);
							}
						}
						GUILayout.Space(15);
					}
				
					if (Event.current.type == EventType.Repaint){
						for (int sequenceIndex = 0; sequenceIndex < actSet.sequenceSlots.Count; sequenceIndex++) {
							GAC_SequenceSetup sequenceSet = actSet.sequenceSlots[sequenceIndex];


							//Make sure the sequence slot isn't set
							if(sequenceSet.isSet){

								//Loop through the list and register all source strings for each sequence slot
								for (int index = 0; index < actSet.sourceStrings.Count; index++) {

									if(index == sequenceIndex){
										if(sequenceSet.useKey){
											actSet.sourceStrings[index] = "Key";
										}else if(sequenceSet.useMouse){
											actSet.sourceStrings[index] = "Mouse";
										}else if(sequenceSet.useButton){
											actSet.sourceStrings[index] = "Button";
										}else if(sequenceSet.useSync){
											actSet.sourceStrings[index] = "Sync";
										}
									}
								}
							
								//Check if using a Synced Slot Input
								if(sequenceSet.useSync){
										
									//Get the sync slot that has the matching reference index that was set
									var getSyncSlot = gacSettings.activatorSlots.Where (i => i.syncReferenceIndex == sequenceSet.setSyncReferenceIndex).ToList();

									//Make sure this activator slot is valid/found
									if(gacSettings.activatorSlots.IndexOf(getSyncSlot[0]) > -1){
										
										//Get the index to use for the drop down
										sequenceSet.syncSlotIndex = gacSettings.syncSlotNames.IndexOf(gacSettings.activatorNames[gacSettings.activatorSlots.IndexOf(getSyncSlot[0])]);


										//Make sure it is not out of range
										if(sequenceSet.syncSlotIndex < 0 || gacSettings.syncSlotNames.Count == 0){

											actSet.showActivator = true;
											
											sequenceSet.isSet = false; 

										}
									}
								}

							}
							
						}
					}
				
					GUI.backgroundColor = Color.white;
				}
				#endregion Sequence Input Activator


				//Show the separator
				JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
			}
			GUILayout.Space(10);
		}
		#endregion Activator Setup

		ChangeController(gacSettings);

		if (GUI.changed){
			EditorUtility.SetDirty(gacSettings);//Let other objects know the script has been changed and need to save to disk

		}
	}

	#region Change Controller
	void ChangeController(GAC gacSettings){

		//If set to legacy animations
		if(gacSettings.conType == GAC.ControllerType.Legacy){
			
			//Reference the components
			animationComponent = gacSettings.gameObject.GetComponent<Animation>();
			animatorComponent = gacSettings.gameObject.GetComponent<Animator>();
			
			//Check if animation component is available, if not...
			if(animationComponent == null){
				
				//Check if animator component is available, if it is...
				if(animatorComponent != null){
					
					//Show the dialog to decide 
					if(EditorUtility.DisplayDialog("You are currently using Mecanim Animations!", "This will remove the Animator Component and Reset" +
					                               "all Setups made in GAC. Do you want to switch to Legacy Animations?", "Yes", "No")){
						
						//Deciding Yes, removes the animator component and adds the animation component
						DestroyImmediate(gacSettings.gameObject.GetComponent<Animator>());
						
						animationComponent = gacSettings.gameObject.AddComponent<Animation>();
						
						//Move the component up to be above the GAC component
						UnityEditorInternal.ComponentUtility.MoveComponentUp (animationComponent);
						
						//Reset slots
						ResetAllSlots(gacSettings);
					}else{
						
						//Deciding No, resets the selection to Mecanim
						gacSettings.conType = GAC.ControllerType.Mecanim;
						
					}
				}
				
				
			}
			
			//If set to Mecanim animations
		}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
			
			//Get the sum of all the animation counts of each layer
			layersSum = stateCount.Sum();
			
			//Check to maker sure there are not more animations stored than there is available total between each layer, reset if so
			if(layersSum < gacSettings.storeAnimNames.Count){
				stateCount.Clear();
				layersUsed.Clear();
				gacSettings.storeAnimNames.Clear();
				
			}
			
			//Reference the components
			animationComponent = gacSettings.gameObject.GetComponent<Animation>();
			animatorComponent = gacSettings.gameObject.GetComponent<Animator>();
			
			//Check if animator component is available, if not..
			if(animatorComponent == null){
				
				//Check if animation component is available, if it is...
				if(animationComponent != null){
					
					//Show the dialog to decide
					if(EditorUtility.DisplayDialog("You are currently using Legacy Animations!", "This will remove the Animations Component and Reset" +
					                               "all Setups made in GAC. Do you want to switch to Mecanim Animations?", "Yes", "No")){
						
						//Deciding Yes, removes the animation component and adds the animator component
						DestroyImmediate(gacSettings.gameObject.GetComponent<Animation>());
						
						animatorComponent = gacSettings.gameObject.AddComponent<Animator>();
						
						//Move the component up to be above the GAC component
						UnityEditorInternal.ComponentUtility.MoveComponentUp (animatorComponent);
						
						//Reset slots
						ResetAllSlots(gacSettings);
					}else{
						//Deciding No, resets the component to Legacy
						gacSettings.conType = GAC.ControllerType.Legacy;
						
					}
				}
			}
		}
	}
	#endregion Change Controller

	#region Scene GUI
	public void OnSceneGUI(){

		GAC.gacGameObjects = FindObjectsOfType<GAC>().ToList();

		foreach(GAC gacSettings in GAC.gacGameObjects){

			//If the editor is folded out...
			if (animEditorExpand){

				for (int animIndex= 0; animIndex < gacSettings.animSlots.Count; animIndex++) {
					GAC_AnimationSetup gacSet = gacSettings.animSlots[animIndex];		

					//Setup the attack effect gizmo; the color and the arc
					Handles.color = gacSet.gizmoColor;
					gacSet.gizmoColor.a = 0.1f;

					//Only do if gizmo is set to be shown
					if (gacSet.gizmoFocus){

						//Check if 3D Mode index selected
						if(gacSettings.gameModeIndex == 0){
							//THESE ARE THE DISTANCE ARCS
							//Draw arc to the right
							Handles.DrawSolidArc (gacSettings.gameObject.transform.position, gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, gacSet.affectAngle/2, gacSet.affectDistance);
						
							//Draw arc to the left
							Handles.DrawSolidArc (gacSettings.gameObject.transform.position, gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, -gacSet.affectAngle/2, gacSet.affectDistance);
						
							//Setup the attack effect gizmo; the color and the arc
							Handles.color = new Color(0, 255, 0, 0.1f);

							if(gacSet.heightToggle){
								//THESE ARE THE HEIGHT ARCS
								//Draw arc to the right
								Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x, gacSettings.gameObject.transform.position.y + gacSet.angleHeight, gacSettings.gameObject.transform.position.z), 
								                      gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, gacSet.affectAngle/2, gacSet.affectDistance);
								
								//Draw arc to the left
								Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x, gacSettings.gameObject.transform.position.y + gacSet.angleHeight, gacSettings.gameObject.transform.position.z),  
								                      gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, -gacSet.affectAngle/2, gacSet.affectDistance);
							}
						}else if(gacSettings.gameModeIndex == 1){//Check if 2D Mode index selected

							//Check if in play mode of editor
							if(EditorApplication.isPlaying){

								//Adjust based on if facing direction is to the right
								if(gacSettings.facingDirectionRight ){
									//Draw arc from right up
									Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x + gacSet.anglePosition.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y,gacSettings.gameObject.transform.position.z),
								                      -gacSettings.gameObject.transform.forward, gacSettings.gameObject.transform.right, -gacSet.affectAngle, gacSet.affectDistance);
								}else{
									//Draw arc from left up
									Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x + gacSet.anglePosition.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y,gacSettings.gameObject.transform.position.z),
									                      gacSettings.gameObject.transform.forward, -gacSettings.gameObject.transform.right, -gacSet.affectAngle, gacSet.affectDistance);
								}
							}else{

								//Use the index to check if facing direction is to the right
								if(gacSettings.directionIndex == 0){
									//Draw arc from right up
									Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x + gacSet.anglePosition.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y,gacSettings.gameObject.transform.position.z),
									                      -gacSettings.gameObject.transform.forward, gacSettings.gameObject.transform.right, -gacSet.affectAngle, gacSet.affectDistance);
								}else{
									//Draw arc from left up
									Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x + gacSet.anglePosition.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y,gacSettings.gameObject.transform.position.z),
									                      gacSettings.gameObject.transform.forward, -gacSettings.gameObject.transform.right, -gacSet.affectAngle, gacSet.affectDistance);
								}
							}
						}
					}
				}
			}	

			//Check if in range mode
			if(gacSettings.useRange){
				//Set the color of the range mode handle
				Handles.color = Color.blue;

				//Check if 3D or 2D mode then face the Disc in correct direction
				if(gacSettings.gameModeIndex == 0){
					Handles.DrawWireDisc(gacSettings.gameObject.transform.position, gacSettings.gameObject.transform.up, gacSettings.trackerRadius);
				}else if(gacSettings.gameModeIndex == 1){
					Handles.DrawWireDisc(gacSettings.gameObject.transform.position, gacSettings.gameObject.transform.forward, gacSettings.trackerRadius);
				}
			}


		}

		//Retrieve all the gameobjects that have the Target Tracker component
		GAC.targetGameObjects = FindObjectsOfType<GAC_TargetTracker>().ToList();

		foreach(GAC_TargetTracker gacTarget in GAC.targetGameObjects){

			//Only do if gizmo is set to be shown
			if(gacTarget.showGizmo){

				//Check if 3D or 2D mode then face the Disc in correct direction
				if(gacTarget.gameModeIndex == 0){

					//Get to positions of target to create an area parameter
					float targetsLeft = gacTarget.gameObject.transform.position.x + gacTarget.parameterPos.x - gacTarget.parameterSize.x;
					float targetsRight = gacTarget.gameObject.transform.position.x + gacTarget.parameterPos.x + gacTarget.parameterSize.x;
					float targetsBack = gacTarget.gameObject.transform.position.z + gacTarget.parameterPos.z - gacTarget.parameterSize.y;
					float targetsFront = gacTarget.gameObject.transform.position.z + gacTarget.parameterPos.z + gacTarget.parameterSize.y;
						
					//Create array of verts for the area of the target
					Vector3[] verts = {new Vector3(targetsLeft, gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y, targetsFront),
						new Vector3(targetsRight, gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y, targetsFront),
						new Vector3(targetsRight, gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y, targetsBack),
						new Vector3(targetsLeft, gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y, targetsBack)};

					Handles.color = Color.red;
					//Use the verts to show a handle
					Handles.DrawSolidRectangleWithOutline(verts, new Color(1,0,0,0.1f), Color.red);

				}else if(gacTarget.gameModeIndex == 1){

					float targetsLeft = gacTarget.gameObject.transform.position.x + gacTarget.parameterPos.x - gacTarget.parameterSize.x;
					float targetsRight = gacTarget.gameObject.transform.position.x + gacTarget.parameterPos.x + gacTarget.parameterSize.x;
					float targetsBottom = gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y - gacTarget.parameterSize.y;
					float targetsTop = gacTarget.gameObject.transform.position.y + gacTarget.parameterPos.y + gacTarget.parameterSize.y;
					
					//Create array of verts for the area of the target
					Vector3[] verts = {new Vector3(targetsLeft, targetsTop, gacTarget.gameObject.transform.position.z),
						new Vector3(targetsRight, targetsTop, gacTarget.gameObject.transform.position.z),
						new Vector3(targetsRight, targetsBottom, gacTarget.gameObject.transform.position.z),
						new Vector3(targetsLeft, targetsBottom, gacTarget.gameObject.transform.position.z)};
					
					Handles.color = Color.red;
					//Use the verts to show a handle
					Handles.DrawSolidRectangleWithOutline(verts, new Color(1,0,0,0.1f), new Color(1,0,0,0));
					
				}
			}

		}

	}
	#endregion Scene GUI

	void  AddAnimation (GAC gacSettings){
		
		GAC_AnimationSetup newAnim = new GAC_AnimationSetup();

		//Add the animation
		gacSettings.animSlots.Add(newAnim);

		//Keep track of the amount
		gacSettings.animAmount++; 
	}
	
	void  RemoveAnimation (GAC gacSettings , GAC_AnimationSetup gacSet, int animIndex){

		//Remove this animation from the added list if it's already been applied
		if(gacSet.appliedAnim){
			gacSettings.addedAnims.Remove(gacSet.theAnim);
		}

		//Check if the list contains this first
		if(gacSettings.starterAnims.Contains(gacSet.theAnim)){
			//Remove the animation from the starter list; using RemoveAll in case of multiple occurrences with accidental bug
			gacSettings.starterAnims.RemoveAll(i => i == gacSet.theAnim);
		}

		if(gacSettings.startersAvailable.Contains(gacSet.theAnim)){
			//Remove the animation from the starters available popup list; using RemoveAll in case of multiple occurrences with accidental bug
			gacSettings.startersAvailable.RemoveAll(i => i == gacSet.theAnim);
		}

		//Remove the animation
		gacSettings.animSlots.RemoveAt(animIndex);

		//Keep track of the amount
		gacSettings.animAmount--;

	
	}
	
	void MoveAnimation (List<GAC_AnimationSetup> list, int animIndex, int selection){
        
        if(selection == 0){//Move down
	        if (animIndex + 1 < list.Count){
	            GAC_AnimationSetup moveSlot = list[animIndex];
	            list.RemoveAt(animIndex);
	            list.Insert(animIndex + 1, moveSlot);
	        }
	    }else if (selection == 1){//Move up

	        if (animIndex > 0){
	            GAC_AnimationSetup moveSlot = list[animIndex];
	            list.RemoveAt(animIndex);
	            list.Insert(animIndex - 1, moveSlot);
	        }
	        	    
	    }    
    }

	
	void ExpandAnimationSlots(GAC gacSettings){
	
		for (int i= 0; i < gacSettings.animSlots.Count; i++){
			gacSettings.animSlots[i].showAnim = true;

		}	
	}
	
	void CloseAnimationSlots(GAC gacSettings){
	
		for (int i= 0; i < gacSettings.animSlots.Count; i++){

			if(!string.IsNullOrEmpty(gacSettings.animSlots[i].theAnim)){
				gacSettings.animSlots[i].showAnim = false;
			}

		}	
	}

	void  AddStarter (GAC gacSettings , string theAnim){
		
		GAC_StarterSetup newStarter = new GAC_StarterSetup();
		
		//Add the animation
		gacSettings.starterSlots.Add(newStarter);

		if(!gacSettings.starterNames.Contains(theAnim)){
			gacSettings.starterNames.Add(theAnim);
		}

		//Add for each starter to be used by the GAC Preview Window
		gacSettings.starterGroupShow.Add (false); 

		newStarter.starterName = theAnim;
		newStarter.showStarter = true;
		gacSettings.addedStarters.Add(theAnim);

		//Add a new starter to track its activators
		gacSettings.activatorsForStarters.Add ("Starter " + theAnim);

		//If in the available animations to use, then remove it
		if(gacSettings.startersAvailable.Contains(theAnim)){
			gacSettings.startersAvailable.Remove(theAnim);
		}

		//Keep track of the amount
		gacSettings.starterAmount++; 
	}
	
	void  RemoveStarter (GAC gacSettings , GAC_StarterSetup starterSet, int startIndex, bool makeAvailable){

		//Remove the animation
		gacSettings.addedStarters.Remove(starterSet.starterName);
		gacSettings.starterNames.RemoveAt(startIndex);
		gacSettings.starterSlots.RemoveAt(startIndex);

		//Remove from the GAC Preview Window starter groups
		gacSettings.starterGroupShow.RemoveAt(startIndex);

		starterSet.conflicts.Clear ();

		//Should the starter animation be made avaialble again (Only would want to do this if animations are still setup to use with GAC
		if(makeAvailable){
			//Check if the starer name of an animation is listed
			if(gacSettings.starterAnims.Contains(starterSet.starterName)){

				//ReAdd to the starters available 
				if(!gacSettings.startersAvailable.Contains(starterSet.starterName)){
					gacSettings.startersAvailable.Add(starterSet.starterName);
				}
			}
		}

		//Remove the activators that are for this starter
		if (gacSettings.activatorsForStarters.Any(str => str.Contains("Starter " + starterSet.starterName))){
			int index = gacSettings.activatorsForStarters.FindIndex(x => x.StartsWith("Starter " + starterSet.starterName));
			gacSettings.activatorsForStarters.RemoveAt(index);

		}

		//Keep track of the amount
		gacSettings.starterAmount--;
		
		
	}
	
	void MoveStarter (List<GAC_StarterSetup> list, int startIndex, int selection){
		
		if(selection == 0){//Move down
			if (startIndex + 1 < list.Count){
				GAC_StarterSetup moveSlot = list[startIndex];
				list.RemoveAt(startIndex);
				list.Insert(startIndex + 1, moveSlot);
			}
		}else if (selection == 1){//Move up
			
			if (startIndex > 0){
				GAC_StarterSetup moveSlot = list[startIndex];
				list.RemoveAt(startIndex);
				list.Insert(startIndex - 1, moveSlot);
			}
			
		}    
	}

	void  AddCombo (GAC_StarterSetup starterSet){
		
		GAC_ComboSetup newCombo = new GAC_ComboSetup();
		
		//Add the animation
		starterSet.starterCombos.Add(newCombo);

		newCombo.showCombo = true;

		starterSet.comboAmount++;
	}
	
	void  RemoveCombo (GAC_StarterSetup starterSet, int comboIndex){
		GAC_ComboSetup comboSet = starterSet.starterCombos[comboIndex];

		//Loop through the combo sequence indexes
		foreach(int set in comboSet.comboSequence){

			//Set each sequence of this comboSet to false
			comboSet.setAnim[comboSet.comboSequence.IndexOf(set)] = false;

			//Then check conflicts to make changes
			EditConflicts(starterSet, comboSet.comboSequence.IndexOf(set));
		}

		//Reset the combo lists
		comboSet.activatorIndex.Clear();
		comboSet.animSpot.Clear();
		comboSet.comboSequence.Clear();
		comboSet.conflicted.Clear ();
		comboSet.referenceCombos.Clear ();
		comboSet.animationReference.Clear ();
		comboSet.keepReference.Clear();


		//Remove the animation
		starterSet.starterCombos.RemoveAt(comboIndex);

		//Keep track of the amount
		starterSet.comboAmount--;
			
	}
	
	void MoveCombo (List<GAC_ComboSetup> list, int comboIndex, int selection){
		
		if(selection == 0){//Move down
			if (comboIndex + 1 < list.Count){
				GAC_ComboSetup moveSlot = list[comboIndex];
				list.RemoveAt(comboIndex);
				list.Insert(comboIndex + 1, moveSlot);
			}
		}else if (selection == 1){//Move up
			
			if (comboIndex > 0){
				GAC_ComboSetup moveSlot = list[comboIndex];
				list.RemoveAt(comboIndex);
				list.Insert(comboIndex - 1, moveSlot);
			}
			
		}    
	}

	void ExpandComboSlots(GAC gacSettings, GAC_StarterSetup starterSet){
		
		for (int i= 0; i < starterSet.starterCombos.Count; i++){
			starterSet.starterCombos[i].showCombo = true;
			
		}	
	}
	
	void CloseComboSlots(GAC gacSettings, GAC_StarterSetup starterSet){
		
		for (int i= 0; i < starterSet.starterCombos.Count; i++){

			starterSet.starterCombos[i].showCombo = false;

		}	
	}
	void  AddAnimSpot (GAC_ComboSetup comboSet, bool delayed){

		//Add all values to the index
		comboSet.animSpot.Add(0);
		comboSet.activatorIndex.Add(1);
		comboSet.comboSequence.Add(1);
		comboSet.setAnim.Add(false);
		comboSet.buttonShown.Add(false);
		comboSet.conflicted.Add(false);

		//Check to see if we should set as a delayed animation
		if(delayed){
			comboSet.delayedAnim.Add(true);
		}else{
			comboSet.delayedAnim.Add(false);
		}
	}

	void  RemoveAnimSpot (GAC_ComboSetup comboSet, int i){

		//If there is anything above this index
		if(i <= comboSet.animSpot.Count - 1){

			//Remove the index from all these lists
			comboSet.animSpot.RemoveAt(i);
			comboSet.activatorIndex.RemoveAt(i);
			comboSet.comboSequence.RemoveAt(i);
			comboSet.setAnim.RemoveAt(i);
			comboSet.buttonShown.RemoveAt(i);
			comboSet.conflicted.RemoveAt(i);
			comboSet.delayedAnim.RemoveAt(i);
		}

		//If there is anything above this index
		if(i <= comboSet.theCombos.Count - 2){

			//The plus 1 signifies the additional index used by the starter animations
			comboSet.theCombos.RemoveAt(i + 1);
		}

	}

	void  AddActivator (GAC gacSettings){
		
		GAC_ActivatorSetup newActivator = new GAC_ActivatorSetup();
		
		//Add the activator
		gacSettings.activatorSlots.Add(newActivator);
		gacSettings.activatorNames.Add ("");

		//Use an input source based on the selection
		if (gacSettings.inputSource == GAC.InputSource.KEYINPUT){
			newActivator.useKey = true;
			newActivator.inputInitials = "K";

		}else if (gacSettings.inputSource == GAC.InputSource.MOUSEINPUT){
			newActivator.useMouse = true;
			newActivator.inputInitials = "M";

		}else if (gacSettings.inputSource == GAC.InputSource.BUTTONINPUT){
			newActivator.useButton = true;
			newActivator.inputInitials = "B";

		}else if (gacSettings.inputSource == GAC.InputSource.TOUCHINPUT){
			newActivator.useTouch = true;
			newActivator.inputInitials = "TI";
			newActivator.showActivator = true;

			newActivator.areaColor = Color.white;
			newActivator.touchPosition = new Vector2(0,50);
			newActivator.touchDimensions = new Vector2(100,100);
		}else if (gacSettings.inputSource == GAC.InputSource.SYNCHROINPUT){
			newActivator.useSync = true;
			newActivator.inputInitials = "SI";
			newActivator.showActivator = true;
		}else if (gacSettings.inputSource == GAC.InputSource.SEQUENCEINPUT){
			newActivator.useSequence = true;
			newActivator.inputInitials = "Seq";
			newActivator.showActivator = true;
			newActivator.stateIndex = 1;
		}

		//Keep track of the amount
		gacSettings.activatorAmount++; 
	}

	void  RemoveActivator(GAC gacSettings , GAC_ActivatorSetup actSet, int actIndex){

		//Remove the activator
		gacSettings.activatorSlots.RemoveAt(actIndex);
		gacSettings.activatorNames.Remove(actSet.name);

		//Keep track of the amount
		gacSettings.activatorAmount--;
		
	}

	void MoveActivator (List<GAC_ActivatorSetup> list, int actIndex, int selection){

		if(selection == 0){//Move down
			if (actIndex + 1 < list.Count){
				GAC_ActivatorSetup moveSlot = list[actIndex];

				list.RemoveAt(actIndex);

				list.Insert(actIndex + 1, moveSlot);


			}
		}else if (selection == 1){//Move up
			
			if (actIndex > 0){
				GAC_ActivatorSetup moveSlot = list[actIndex];

				list.RemoveAt(actIndex);
				
				list.Insert(actIndex - 1, moveSlot);
			}
			
		}   

	}

	
	void MoveActivatorUp(){
		MoveActivator(gacSettings.activatorSlots, moveIndex, 1);
	}
	
	void MoveActivatorDown(){
		MoveActivator(gacSettings.activatorSlots, moveIndex, 0);
	}

	void  AddSync(GAC gacSettings , GAC_ActivatorSetup actSet){
		
		GAC_SyncSetup newSync = new GAC_SyncSetup();
		
		//Add the activator
		actSet.syncSlots.Add(newSync);

		actSet.showActivator = true;

		if (actSet.syncSource == GAC_ActivatorSetup.SyncSource.KEY){
			newSync.useKey = true;
		}else if (actSet.syncSource == GAC_ActivatorSetup.SyncSource.MOUSE){
			newSync.useMouse = true;
		}else if (actSet.syncSource == GAC_ActivatorSetup.SyncSource.BUTTON){
			newSync.useButton = true;
		}

		//Keep track of the amount
		actSet.syncAmounts++;
		
		
	}
	
	void  RemoveSync(GAC gacSettings , GAC_ActivatorSetup actSet, int index){
	
		//Remove the activator
		actSet.syncSlots.RemoveAt(index);

	}

	void  AddSequence(GAC gacSettings , GAC_ActivatorSetup actSet){
		
		GAC_SequenceSetup newSequence = new GAC_SequenceSetup();

		//Don't add to sequence slot yet if this is a Sync Sequence Source
		if (actSet.sequenceSource != GAC_ActivatorSetup.SequenceSource.SYNC){
			//Add the activator
			actSet.sequenceSlots.Add(newSequence);
		
		}

		actSet.showActivator = true;

		if (actSet.sequenceSource == GAC_ActivatorSetup.SequenceSource.KEY){
			newSequence.useKey = true;
		}else if (actSet.sequenceSource == GAC_ActivatorSetup.SequenceSource.MOUSE){
			newSequence.useMouse = true;
		}else if (actSet.sequenceSource == GAC_ActivatorSetup.SequenceSource.BUTTON){
			newSequence.useButton = true;
		}else if (actSet.sequenceSource == GAC_ActivatorSetup.SequenceSource.SYNC){
			if(gacSettings.syncSlotNames.Count > 0){
				//Add the activator
				actSet.sequenceSlots.Add(newSequence);
				newSequence.useSync = true;

			}else{
				actSet.syncWarning = true;
			}
		}
	
		
	}

	void  EditSequenceSlot(GAC gacSettings , GAC_ActivatorSetup actSet, int index){

		actSet.sequenceSlots.RemoveAt(index);

		if(actSet.inputStrings.Count > index){
			actSet.inputStrings.RemoveAt(index);
			actSet.sourceStrings.RemoveAt(index);
			actSet.inputTriggered.RemoveAt(index);
		}

	}

	void EditSequence(GAC gacSettings , GAC_ActivatorSetup actSet, int animationIndex){

		DestroyImmediate(gacSettings.gameObject.GetComponent<GAC_SetEvent>());
		
		gacSettings.activatorsForStarters.Clear();
		
		GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(gacSettings.addedStarters[animationIndex])];
		starterSet.firstActivatorSet = false;


		actSet.activatorSet = false;
		actSet.showActivator = true;
		actSet.showSequence = false;
		actSet.sequencedString = "";
	}

	void SetActivators(string theString, int index){

		//Check to see if this starter has been added in the list to use before adding to the list of activators used for this starter
		if (gacSettings.activatorsForStarters.Any(str => str.Contains("Starter " + theString))){
			
			//Get the index of this starter in the list
			int i = gacSettings.activatorsForStarters.FindIndex(x => x.StartsWith("Starter " + theString));
			
			//Make sure the 'Activators' string hasnt been added yet
			if(gacSettings.activatorsForStarters[i].IndexOf(" Activators") <= 0){
				
				//Concatenate the 'Activators' string to the string
				gacSettings.activatorsForStarters[i] = gacSettings.activatorsForStarters[i] + " Activators";
			}

			//Make sure the activator isn't already there
			if(gacSettings.activatorsForStarters[i].IndexOf(" " + index) <= 0){

				GAC_StarterSetup starterSet = gacSettings.starterSlots[gacSettings.addedStarters.IndexOf(theString)];

				if(starterSet.firstActivatorSet){

					//Concatenate the activator number to the string
					gacSettings.activatorsForStarters[i] = gacSettings.activatorsForStarters[i] + ", " + index;
				}else{
					//Concatenate the activator number to the string
					gacSettings.activatorsForStarters[i] = gacSettings.activatorsForStarters[i] + " " + index;
					starterSet.firstActivatorSet = true;
				}
				
			}
			
			
		}
	}
	

	void AddReferences(GAC_ComboSetup comboSet, GAC_StarterSetup starterSet, int i){

		GAC_AnimationReference newReference = new GAC_AnimationReference();

		//Make sure the reference list does not have more than the combo sequence list before adding any more
		if(comboSet.animationReference.Count < comboSet.comboSequence.Count){
			comboSet.animationReference.Add (newReference);
		}

		GAC_AnimationReference animationReference = comboSet.animationReference[i];

		//Check if null, if so the reference all the attributes of animation in the combo
		if(string.IsNullOrEmpty(animationReference.starterName)){
			animationReference.starterName = starterSet.starterName;
			animationReference.activator = comboSet.activatorIndex[i];
			animationReference.animName = comboSet.animNames[comboSet.animSpot[i]];
			animationReference.sequence = comboSet.comboSequence[i];
			animationReference.delayed = comboSet.delayedAnim[i];

		}


			
		//If there are no starters added yet
		if(comboSet.theCombos.Count == 0){
			comboSet.theCombos.Add (starterSet.starterName);
		
		}

		//Add the animation name to the combo list for use in GAC_PAC
		comboSet.theCombos.Add (comboSet.animNames[comboSet.animSpot[i]]);


	}

	void RemoveBelow (GAC_ComboSetup comboSet, int i){

		gacSettings.animationsUsed.Clear ();

		//if there is anything above this index
		if(i <= comboSet.animationReference.Count - 1){

			//Save them for readding
			comboSet.keepReference.AddRange(comboSet.animationReference);


			//Calculate the amount of indexes to remove that come before the current index
			//The plus 1 represents the current index that is to be removed too
			int countDifference = comboSet.keepReference.Count - (comboSet.keepReference.Count - (i + 1));

			//Get the max to prevent any negative results
			int removeAmount = Math.Max(0, countDifference);

			//Remove all animations that combo before the index before the index
			comboSet.keepReference.RemoveRange(0, removeAmount);

			//Remove them, including the index
			comboSet.animationReference.RemoveRange(i, comboSet.animationReference.Count - i);

		}

		//if there is anything above this index
		if(i <= comboSet.theCombos.Count - 2){

			//Save them for readding
			comboSet.referenceCombos.AddRange(comboSet.theCombos);

			//Calculate the amount of indexes to remove that come before the current index
			//The 2 that is added to 'i' serves two purposes, 1 represents the starter animation that is not indexed in the animSpot List but is in the theCombo list
			//Plus another 1 represents the current index that is to be removed too
			int countDifference = comboSet.referenceCombos.Count - (comboSet.referenceCombos.Count - (i + 2));

			//Get the max to prevent any negative results
			int removeAmount = Math.Max(0, countDifference);

			// Remove all animations that combo before the index before the index
			comboSet.referenceCombos.RemoveRange(0, removeAmount);

			//Remove them, including the index
			comboSet.theCombos.RemoveRange(i + 1, comboSet.theCombos.Count - (i + 1));
		}



	}

	//Used to check if there are any conflicting combo animation setups
	void ConflictCheck(GAC_StarterSetup starterSet, int index){

		foreach(GAC_ComboSetup combos in starterSet.starterCombos){

			//Check to make sure index is not out of bounds; that there is atleast one animation spot added to a combo
			if(combos.animSpot.Count > index){

				//Make sure this animation has been set to this combo first
				if(combos.setAnim[index]){

					//Make sure not a delayed animation
					if(!combos.delayedAnim[index]){
						//Check if the list doesn't contain the string first
						if(!starterSet.conflicts[index].animationsUsed.Contains("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Normal")){

							//Then add it
							starterSet.conflicts[index].animationsUsed.Add ("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Normal");
							starterSet.conflicts[index].normalIndexes.Add(starterSet.starterCombos.IndexOf(combos)+"");
						}
					}else{//If so, add a delayed tag

						//Check if the list doesn't contain the string first
						if(!starterSet.conflicts[index].animationsUsed.Contains("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Delayed")){

							//Then add it
							starterSet.conflicts[index].animationsUsed.Add ("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Delayed");
							starterSet.conflicts[index].delayIndexes.Add(starterSet.starterCombos.IndexOf(combos)+"");
						}
					}

				}
			}
		}

		//Make sure these is atleast 2 animations total from each combo
		if(starterSet.conflicts[index].animationsUsed.Count > 1){

			//Loop through forwards
			for (int i = 0; i < starterSet.conflicts[index].animationsUsed.Count; i++){

				//Check for these strings to find the activator and animation needed
				string activeFirst = starterSet.conflicts[index].animationsUsed[i].Between("Activator","Animation");
				string animFirst = starterSet.conflicts[index].animationsUsed[i].Between("Animation","'");
				string delayFirst = starterSet.conflicts[index].animationsUsed[i].After("'");

				//Loop through backwards
				for(int n = (starterSet.conflicts[index].animationsUsed.Count - 1); n > 0; n--){

					//Check for these strings to find the activator and animation needed
					string activeSec = starterSet.conflicts[index].animationsUsed[n].Between("Activator","Animation");
					string animSec = starterSet.conflicts[index].animationsUsed[n].Between("Animation", "'");
					string delaySec = starterSet.conflicts[index].animationsUsed[n].After("'");                                                             
					
					//Make sure not to check the same indexes
					if(i != n){

						//Compare each activator with each animation combination
						if(activeFirst == activeSec && animFirst != animSec && delayFirst == delaySec){ 

							foreach(GAC_ComboSetup comboSet in starterSet.starterCombos){
								//Check to make sure index is not out of bounds; that there is atleast one animation spot added to a combo
								if(comboSet.animSpot.Count > index){

									//Make sure this animation has been set to this combo first
									if(comboSet.setAnim[index]){

										if(delayFirst == "Normal"){

											//Make sure only checking Normal animation indexes
											if(starterSet.conflicts[index].normalIndexes.Contains(starterSet.starterCombos.IndexOf(comboSet) + "")){
												//Set the combo to be conflicted
												comboSet.conflicted[index] = true;

											}
										}else if(delayFirst == "Delayed"){

											//Make sure only checking Delayed animation idexes
											if(starterSet.conflicts[index].delayIndexes.Contains(starterSet.starterCombos.IndexOf(comboSet) + "")){
												//Set the combo to be conflicted
												comboSet.conflicted[index] = true;

											}
										}
									}

								}
							}

							
						}
					}
				}
			}

		}
					

	}

	//Used when editing what combo animations are setup to remove this animation from the conflict checking list
	void EditConflicts(GAC_StarterSetup starterSet, int index){

		//Loop through the combos of the starter
		foreach(GAC_ComboSetup combos in starterSet.starterCombos){

			//Check to make sure index is not out of bounds; that there is atleast one animation spot added to a combo
			if(combos.animSpot.Count > index){

				//Make sure not a delayed animation
				if(!combos.delayedAnim[index]){

					//Check if the list doesn't contain the string first
					if(starterSet.conflicts[index].animationsUsed.Contains("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Normal")){

						//Then add it
						starterSet.conflicts[index].animationsUsed.Remove ("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Normal");

						//Set the combo to not be conflicted
						combos.conflicted[index] = false;
						
						warningMode = false;
						
						//Call to make sure there are no more conflicted animations between combos
						ConflictCheck(starterSet, index);
					}

				}else{//If so, add a delayed tag

					//Check if the list doesn't contain the string first
					if(starterSet.conflicts[index].animationsUsed.Contains("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Delayed")){

						//Then add it
						starterSet.conflicts[index].animationsUsed.Remove ("Activator " + combos.activatorIndex[index] + " Animation" + combos.animNames[combos.animSpot[index]] + "'Delayed");

						//Set the combo to not be conflicted
						combos.conflicted[index] = false;

						warningMode = false;

						//Call to make sure there are no more conflicted animations between combos
						ConflictCheck(starterSet, index);
					}
				}
			}

		}

		//If there isn't atleast 2 total animations from each starter
		if(starterSet.conflicts[index].animationsUsed.Count < 2){

			//Loop through all the combos
			foreach(GAC_ComboSetup comboSet in starterSet.starterCombos){

				//Check to make sure index is not out of bounds; that there is atleast one animation spot added to a combo
				if(comboSet.animSpot.Count > index){

					//Set the combo to not be conflicted
					comboSet.conflicted[index] = false;

					warningMode = false;
				}
			}
			
		}


	}


	void ResetAllSlots(GAC gacSettings){

		gacSettings.animSlots.Clear();
		gacSettings.comboSlots.Clear();
		gacSettings.starterSlots.Clear();
		gacSettings.activatorSlots.Clear();

		gacSettings.starterGroupShow.Clear();
		gacSettings.starterNames.Clear();
		gacSettings.storeAnimNames.Clear();
		gacSettings.addedAnims.Clear();
		gacSettings.addedStarters.Clear();
		gacSettings.startersAvailable.Clear();
		gacSettings.animationsUsed.Clear();
	}

	#region Touch Areas GUI Editor
	void GetTAGEditor(){

		if(gacSettings.simulate){

			if(tagWindow == null){
				tagWindow = (GAC_TAG)EditorWindow.GetWindow(typeof(GAC_TAG), true);
				tagWindow.name = "(GAC_TAG)";
				tagWindow.minSize = new Vector2(680,640);
				tagWindow.maxSize = new Vector2(680,640);
			}else{

				if(EditorWindow.focusedWindow != null){ 

					//Get the name of the focused window
					string windowNames = EditorWindow.focusedWindow.ToString();

					//Remove any duplicate strings
					windowNames = JrDevArts_Utilities.RemoveDuplicateWords(windowNames);

					//Check if the window is the TAG window
					if(windowNames == tagWindow.name){

						//Set the TAG when to ready state
						gacSettings.tagWindowReady = true;
							
					}

					//Check if TAG is ready
					if(gacSettings.tagWindowReady){

						//Compare the selected window names to allow the stay in ready state when clicked on certain windows, otherwise turn state off
						if(windowNames != tagWindow.name && windowNames != "(UnityEditor InspectorWindow)" && windowNames != "(UnityEditor GameView)" && 
						   windowNames != "(UnityEditor PopupWindow)" && windowNames != "(UnityEditor ColorPicker)"){

							//Loop through and register the current state of all activators to restore later
							for (int i = 0; i < gacSettings.activatorSlots.Count; i++) {
								gacSettings.activatorSlots[i].restoreShowState = gacSettings.activatorSlots[i].showTouchArea;
							}

							gacSettings.tagWindowReady = false;
						}
					}
				}
				
			}
		}

	}
	#endregion Touch Areas GUI Editor

	#region Resolution Settings
	void ResolutionSelection(){

		#if UNITY_STANDALONE

		gacSettings.resolutionNames = new string[] {"640x480 - Laptops, Desktops","800x500 (400x250) - Laptops, Desktops","800x600 (400x300) - Laptops, Desktops",
			"1024x640 (512x320) - Laptops","1024x768 (512x384) - Laptops","1152x720 (576x360) - Laptops", "1280x720 (640x360) - TVs", "1280x800 (640x400) - Laptops", 
			"1280x1024 (640x512) - Desktop Monitors", "1366x768 (314x256) - Laptops", "1440x900 (480x300) - Laptops, Desktops", "1600x1200 (533x400) - Desktop Monitors", 
			"1920x1080 (640x360) - TVs", "1920x1200 (640x400) - Desktop Monitors"};

		if(gacSettings.standaloneSavedSlots.Count < gacSettings.resolutionNames.Length){
			GAC_SavedTouchArea newRecord = new GAC_SavedTouchArea();

			gacSettings.standaloneSavedSlots.Add(newRecord);

		}

		if(gacSettings.resolutionScaleFactor.Count < gacSettings.resolutionNames.Length){
			gacSettings.resolutionScaleFactor.Add(1);
		}

		if(gacSettings.resolutionIndex < gacSettings.resolutionNames.Length && gacSettings.resolutionIndex < gacSettings.standaloneSavedSlots.Count){
			if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "640x480 - Laptops, Desktops"){
				gacSettings.theResolution = new Vector2(640, 480);
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops, Desktops - 640x480";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "800x500 (400x250) - Laptops, Desktops"){
				gacSettings.theResolution = new Vector2(400, 250);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops, Desktops - 800x500";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "800x600 (400x300) - Laptops, Desktops"){
				gacSettings.theResolution = new Vector2(400, 300);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops, Desktops - 800x600";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1024x640 (512x320) - Laptops"){
				gacSettings.theResolution = new Vector2(512, 320);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops - 1024x640";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1024x768 (512x384) - Laptops"){
				gacSettings.theResolution = new Vector2(512, 384);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops - 1024x768";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1152x720 (576x360) - Laptops"){
				gacSettings.theResolution = new Vector2(576, 360);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops - 1152x720";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x720 (640x360) - TVs"){
				gacSettings.theResolution = new Vector2(640, 360);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "TVs - 1280x720";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x800 (640x400) - Laptops"){
				gacSettings.theResolution = new Vector2(640, 400);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops - 1280x800";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x1024 (640x512) - Desktop Monitors"){
				gacSettings.theResolution = new Vector2(640, 512);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Desktop Monitors - 1280x1024";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1366x768 (314x256) - Laptops"){
				gacSettings.theResolution = new Vector2(314, 256);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops - 1366x768";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1440x900 (480x300) - Laptops, Desktops"){
				gacSettings.theResolution = new Vector2(480, 300);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Laptops, Desktops - 1440x900";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1600x1200 (533x400) - Desktop Monitors"){
				gacSettings.theResolution = new Vector2(533, 400);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Desktop Monitors - 1600x1200";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1920x1080 (640x360) - TVs"){
				gacSettings.theResolution = new Vector2(640, 360);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "TVs - 1920x1080";
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1920x1200 (640x400) - Desktop Monitors"){
				gacSettings.theResolution = new Vector2(640, 400);
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex].resolutionName = "Desktop Monitors - 1920x1200";
				
			}
		}else{
			gacSettings.resolutionIndex = 0;
		}
		#endif

		#if UNITY_IOS

		gacSettings.resolutionNames = new string[] {"iPhone - 480x320", "iPhone - 320x480", "iPhone 4 - 960x640 (480x320)", "iPhone 4 - 640x960 (320x480)", 
			"iPhone 5 - 1136x640 (568x320)", "iPhone 5 - 640x1136 (320x568)", "iPad (w Mini) - 1024x768 (512x384)", "iPad (w Mini) - 786x1024 (384x512)", 
			"iPad Retina (w Mini) - 2048x1538 (512x384)", "iPad Retina (w Mini) - 1538x2048 (384x512)"};

		if(gacSettings.iosSavedSlots.Count < gacSettings.resolutionNames.Length){
			GAC_SavedTouchArea newRecord = new GAC_SavedTouchArea();
			
			gacSettings.iosSavedSlots.Add(newRecord);
		}

		if(gacSettings.resolutionScaleFactor.Count < gacSettings.resolutionNames.Length){
			gacSettings.resolutionScaleFactor.Add(1);
		}
		 
		if(gacSettings.resolutionIndex < gacSettings.resolutionNames.Length && gacSettings.resolutionIndex < gacSettings.iosSavedSlots.Count){
			if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone - 480x320"){//IPHONE WIDE
				gacSettings.theResolution = new Vector2(480, 320);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone Wide 480x320";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone - 320x480"){//IPHONE TALL
				gacSettings.theResolution = new Vector2(320, 480);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone Tall 320x480";

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone 4 - 960x640 (480x320)"){//IPHONE 4 WIDE
				gacSettings.theResolution = new Vector2(480, 320);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone 4 Wide 960x640";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone 4 - 640x960 (320x480)"){//IPHONE 4 TALL
				gacSettings.theResolution = new Vector2(320, 480);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone 4 Tall 640x960";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone 5 - 1136x640 (568x320)"){//IPHONE 5 WIDE
				gacSettings.theResolution = new Vector2(568, 320);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone 5 Wide 1136x640";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPhone 5 - 640x1136 (320x568)"){//IPHONE 5 TALL
				gacSettings.theResolution = new Vector2(320, 568);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPhone 5 Tall 640x1136";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				 
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPad (w Mini) - 1024x768 (512x384)"){//IPAD WIDE
				gacSettings.theResolution = new Vector2(512, 384);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPad Wide 1024x768";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPad (w Mini) - 786x1024 (384x512)"){//IPAD TALL
				gacSettings.theResolution = new Vector2(384, 512);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPad Tall 768x1024";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPad Retina (w Mini) - 2048x1538 (512x384)"){//IPAD RETINA WIDE
				gacSettings.theResolution = new Vector2(512, 384);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPad Retina Wide 2048x1538";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "iPad Retina (w Mini) - 1538x2048 (384x512)"){//IPAD RETINA TALL
				gacSettings.theResolution = new Vector2(384, 512);
				gacSettings.iosSavedSlots[gacSettings.resolutionIndex].resolutionName = "iPad Retina Tall 1538x2048";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
			}
		}else{
			gacSettings.resolutionIndex = 0;
		}
		#endif

		#if UNITY_ANDROID

		gacSettings.resolutionNames = new string[] {"320x240 - Galaxy Mini", "480x320 - Galaxy", "800x480 (400x240) - Galaxy S2, Nexus One, Nexus S", 
			"854x480 (427x240) - XPeria Play", "960x540 (480x270) - Droid Razr", "1024x580 (512x290) - Kindle Fire", "1024x600 (512x300) - Nook Tab, Galaxy Tab",
			"1280x768 (640x384) - Nexus 4", "1280x720 (640x360) - Galaxy Note 2, Galaxy Nexus, Fire Phone, Moto G",
			"1280x800 (640x400) - G-Tab 10.1, G-Tab 2 10.1, Galaxy Note 10.1, Kindle Fire HD, Nexus 7", "1440x900 (480x300) - Nook HD",
			"1920x1080 (640x360) - Galaxy S4, Galaxy S5, Nexus 5, XPeria Z2, HTC One M8, Lumia 930", "1920x1200 (640x400) - Kindle Fire HD 8.9, Nexus 7 2", 
			"1920x1280 (640x426) - Nook HD+", "2560x1600 (640x400) - Nexus 10, LG G3"};

		if(gacSettings.androidSavedSlots.Count < gacSettings.resolutionNames.Length){
			GAC_SavedTouchArea newRecord = new GAC_SavedTouchArea();
			
			gacSettings.androidSavedSlots.Add(newRecord);
		}
		
		if(gacSettings.resolutionScaleFactor.Count < gacSettings.resolutionNames.Length){
			gacSettings.resolutionScaleFactor.Add(1);
		}

		if(gacSettings.resolutionIndex < gacSettings.resolutionNames.Length && gacSettings.resolutionIndex < gacSettings.androidSavedSlots.Count){
			if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "320x240 - Galaxy Mini"){
				gacSettings.theResolution = new Vector2(320, 240);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Samsung Galaxy Mini 320x240";
			
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "480x320 - Galaxy"){
				gacSettings.theResolution = new Vector2(480, 320);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Samsung Galaxy 480x320";
			
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "800x480 (400x240) - Galaxy S2, Nexus One, Nexus S"){
				gacSettings.theResolution = new Vector2(400, 240);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Galaxy S2, Nexus One, Nexus S 800x480";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "854x480 (427x240) - XPeria Play"){
				gacSettings.theResolution = new Vector2(427, 240);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "XPeria Play 854x480";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
			
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "960x540 (480x270) - Droid Razr"){
				gacSettings.theResolution = new Vector2(480, 270);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Droid Razr 960x540";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1024x580 (512x290) - Kindle Fire"){
				gacSettings.theResolution = new Vector2(512, 290);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Kindle Fire 1024x580";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1024x600 (512x300) - Nook Tab, Galaxy Tab"){
				gacSettings.theResolution = new Vector2(512, 300);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Nook Tab, Galaxy Tab 1024x600";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x768 (640x384) - Nexus 4"){
				gacSettings.theResolution = new Vector2(640, 384);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Nexus 4 1280x768";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x720 (640x360) - Galaxy Note 2, Galaxy Nexus, Fire Phone, Moto G"){
				gacSettings.theResolution = new Vector2(640, 360);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Galaxy Note 2, Galaxy Nexus, Fire Phone, Moto G 1280x720";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1280x800 (640x400) - G-Tab 10.1, G-Tab 2 10.1, Galaxy Note 10.1, Kindle Fire HD, Nexus 7"){
				gacSettings.theResolution = new Vector2(640, 400);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "G-Tab 10.1, G-Tab 2 10.1, Galaxy Note 10.1, Kindle Fire HD, Nexus 7 1280x800";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 2;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1440x900 (480x300) - Nook HD"){
				gacSettings.theResolution = new Vector2(400, 300);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Nook HD 1440x900";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;

			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1920x1080 (640x360) - Galaxy S4, Galaxy S5, Nexus 5, XPeria Z2, HTC One M8, Lumia 930"){
				gacSettings.theResolution = new Vector2(640, 360);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Galaxy S4, Galaxy S5, Nexus 5, XPeria Z2, HTC One M8, Lumia 930 1920x1080";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
		
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1920x1200 (640x400) - Kindle Fire HD 8.9, Nexus 7 2"){
				gacSettings.theResolution = new Vector2(640, 360);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Kindle Fire HD 8.9, Nexus 7 2 1920x1200";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "1920x1280 (640x426) - Nook HD+"){
				gacSettings.theResolution = new Vector2(640, 426);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Nook HD+ 1920x1280";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 3;
				
			}else if(gacSettings.resolutionNames[gacSettings.resolutionIndex] == "2560x1600 (640x400) - Nexus 10, LG G3"){
				gacSettings.theResolution = new Vector2(640, 400);
				gacSettings.androidSavedSlots[gacSettings.resolutionIndex].resolutionName = "Nexus 10, LG G3 2560x1600";
				gacSettings.resolutionScaleFactor[gacSettings.resolutionIndex] = 4;
				
			}
		}else{
			gacSettings.resolutionIndex = 0;
		}
	#endif

	}

	#region Save Touch Areas
	void SaveTouchAreas(){
		
		#if UNITY_STANDALONE

		GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];

		//Reset the activator slots for this Saved Setting
		standSet.actSlots.Clear();

		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
			
			GAC_ActivatorSetup newActSet = new GAC_ActivatorSetup();
			
			//Make sure it is a Touch Activator
			if(actSet.name.IndexOf("Touch Input") > -1){
				
				if(actSet.activatorSet){
					
					//Copy all attributes from this activators
					newActSet.useTouch = actSet.useTouch;
					newActSet.name = actSet.name;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.touchDimensions = actSet.touchDimensions;
					newActSet.touchReferenceIndex = actSet.touchReferenceIndex;
					newActSet.activatorSet = actSet.activatorSet;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.showTouchArea = actSet.showTouchArea;
					newActSet.wasShowing = actSet.wasShowing;
					newActSet.restoreShowState = actSet.restoreShowState;
					newActSet.touchedArea = actSet.touchedArea;
					newActSet.setTouchName = actSet.setTouchName;
					newActSet.touchNameNotSet = actSet.touchNameNotSet;
					newActSet.touchInUse = actSet.touchInUse;
					newActSet.setTouchReferenceIndex = actSet.setTouchReferenceIndex;
					newActSet.relativePosition = actSet.relativePosition;
					newActSet.relativeScale = actSet.relativeScale;
					newActSet.areaRect = actSet.areaRect;
					newActSet.rectPos = actSet.rectPos;
					newActSet.moveRect = actSet.moveRect;
					newActSet.atEdge = actSet.atEdge;
					newActSet.areaColor = actSet.areaColor;
					newActSet.touchIndex = actSet.touchIndex;
					newActSet.touchSlotIndex = actSet.touchSlotIndex;
					
					//Then copy the activator to save it for this resolution setting
					standSet.actSlots.Add (newActSet);
					
				}else{
					Debug.LogWarning("GACLog - " + actSet.name + " wasn't saved because it was not 'Set' to use. Please 'Set' this activator before trying to save it!");
				}
			}
			
			//There has to be at least one slot added before setting trigger to save
			if(standSet.actSlots.Count > 0){
				
				//Wait till the last index
				if(actIndex == gacSettings.activatorSlots.Count - 1){
					standSet.saved = true;
					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + standSet.resolutionName + " with "+ standSet.actSlots.Count + " activator slots...");
				}
			}else{
				Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
			}
		}

		#endif
		
		#if UNITY_IOS
		GAC_SavedTouchArea iosSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];
		
		//Reset the activator slots for this Saved Setting
		iosSet.actSlots.Clear();
		
		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
			
			GAC_ActivatorSetup newActSet = new GAC_ActivatorSetup();
			
			//Make sure it is a Touch Activator
			if(actSet.name.IndexOf("Touch Input") > -1){
				
				if(actSet.activatorSet){
					
					//Copy all attributes from this activators
					newActSet.useTouch = actSet.useTouch;
					newActSet.name = actSet.name;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.touchDimensions = actSet.touchDimensions;
					newActSet.touchReferenceIndex = actSet.touchReferenceIndex;
					newActSet.activatorSet = actSet.activatorSet;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.showTouchArea = actSet.showTouchArea;
					newActSet.wasShowing = actSet.wasShowing;
					newActSet.restoreShowState = actSet.restoreShowState;
					newActSet.touchedArea = actSet.touchedArea;
					newActSet.setTouchName = actSet.setTouchName;
					newActSet.touchNameNotSet = actSet.touchNameNotSet;
					newActSet.touchInUse = actSet.touchInUse;
					newActSet.setTouchReferenceIndex = actSet.setTouchReferenceIndex;
					newActSet.relativePosition = actSet.relativePosition;
					newActSet.relativeScale = actSet.relativeScale;
					newActSet.areaRect = actSet.areaRect;
					newActSet.rectPos = actSet.rectPos;
					newActSet.moveRect = actSet.moveRect;
					newActSet.atEdge = actSet.atEdge;
					newActSet.areaColor = actSet.areaColor;
					newActSet.touchIndex = actSet.touchIndex;
					newActSet.touchSlotIndex = actSet.touchSlotIndex;
					
					//Then copy the activator to save it for this resolution setting
					iosSet.actSlots.Add (newActSet);
					
				}else{
					Debug.LogWarning("GACLog - " + actSet.name + " wasn't saved because it was not 'Set' to use. Please 'Set' this activator before trying to save it!");
				}
			}
			
			//There has to be at least one slot added before setting trigger to save
			if(iosSet.actSlots.Count > 0){
				
				//Wait till the last index
				if(actIndex == gacSettings.activatorSlots.Count - 1){
					iosSet.saved = true;
					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + iosSet.resolutionName + " with "+ iosSet.actSlots.Count + " activator slots...");
				}
			}else{
				Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
			}
		}
		#endif

		#if UNITY_ANDROID
		GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];
		
		//Reset the activator slots for this Saved Setting
		androidSet.actSlots.Clear();

		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
			
			GAC_ActivatorSetup newActSet = new GAC_ActivatorSetup();

			//Make sure it is a Touch Activator
			if(actSet.name.IndexOf("Touch Input") > -1){

				if(actSet.activatorSet){

					//Copy all attributes from this activators
					newActSet.useTouch = actSet.useTouch;
					newActSet.name = actSet.name;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.touchDimensions = actSet.touchDimensions;
					newActSet.touchReferenceIndex = actSet.touchReferenceIndex;
					newActSet.activatorSet = actSet.activatorSet;
					newActSet.touchPosition = actSet.touchPosition;
					newActSet.showTouchArea = actSet.showTouchArea;
					newActSet.wasShowing = actSet.wasShowing;
					newActSet.restoreShowState = actSet.restoreShowState;
					newActSet.touchedArea = actSet.touchedArea;
					newActSet.setTouchName = actSet.setTouchName;
					newActSet.touchNameNotSet = actSet.touchNameNotSet;
					newActSet.touchInUse = actSet.touchInUse;
					newActSet.setTouchReferenceIndex = actSet.setTouchReferenceIndex;
					newActSet.relativePosition = actSet.relativePosition;
					newActSet.relativeScale = actSet.relativeScale;
					newActSet.areaRect = actSet.areaRect;
					newActSet.rectPos = actSet.rectPos;
					newActSet.moveRect = actSet.moveRect;
					newActSet.atEdge = actSet.atEdge;
					newActSet.areaColor = actSet.areaColor;
					newActSet.touchIndex = actSet.touchIndex;
					newActSet.touchSlotIndex = actSet.touchSlotIndex;

					//Then copy the activator to save it for this resolution setting
					androidSet.actSlots.Add (newActSet);


				}else{
					Debug.LogWarning("GACLog - " + actSet.name + " wasn't saved because it was not 'Set' to use. Please 'Set' this activator before trying to save it!");
				}
			}

			//Wait till the last index
			if(actIndex == gacSettings.activatorSlots.Count - 1){
				
				//There has to be at least one slot added before setting trigger to save
				if(androidSet.actSlots.Count > 0){
					androidSet.saved = true;

					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + androidSet.resolutionName + " with "+ androidSet.actSlots.Count + " activator slots...");
				
				}else{
					Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
				}
			}
			
		}

		#endif
	}
	
	void LoadTouchAreas(){


		#if UNITY_STANDALONE
		GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];
		
		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];

			//Make sure it is a Touch Activator
			if(actSet.name.IndexOf("Touch Input") > -1){
				for (int i = 0; i < standSet.actSlots.Count; i++) {

					//Compare the references for each index to make sure they match
					if(standSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){

						//Then reload the activator settings to the matched reference index
						actSet.useTouch = standSet.actSlots[i].useTouch;
						actSet.name = standSet.actSlots[i].name;
						actSet.touchPosition = standSet.actSlots[i].touchPosition;
						actSet.touchDimensions = standSet.actSlots[i].touchDimensions;
						actSet.touchReferenceIndex = standSet.actSlots[i].touchReferenceIndex;
						actSet.activatorSet = standSet.actSlots[i].activatorSet;
						actSet.touchPosition = standSet.actSlots[i].touchPosition;
						actSet.showTouchArea = standSet.actSlots[i].showTouchArea;
						actSet.wasShowing = standSet.actSlots[i].wasShowing;
						actSet.restoreShowState = standSet.actSlots[i].restoreShowState;
						actSet.touchedArea = standSet.actSlots[i].touchedArea;
						actSet.setTouchName = standSet.actSlots[i].setTouchName;
						actSet.touchNameNotSet = standSet.actSlots[i].touchNameNotSet;
						actSet.touchInUse = standSet.actSlots[i].touchInUse;
						actSet.setTouchReferenceIndex = standSet.actSlots[i].setTouchReferenceIndex;
						actSet.relativePosition = standSet.actSlots[i].relativePosition;
						actSet.relativeScale = standSet.actSlots[i].relativeScale;
						actSet.areaRect = standSet.actSlots[i].areaRect;
						actSet.rectPos = standSet.actSlots[i].rectPos;
						actSet.moveRect = standSet.actSlots[i].moveRect;
						actSet.atEdge = standSet.actSlots[i].atEdge;
						actSet.areaColor = standSet.actSlots[i].areaColor;
						actSet.touchIndex = standSet.actSlots[i].touchIndex;
						actSet.touchSlotIndex = standSet.actSlots[i].touchSlotIndex;
					}

				}
			}
		}
		#endif
		
		#if UNITY_IOS
		GAC_SavedTouchArea iosSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];
		
		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
			
			//Make sure it is a Touch Activator
			if(actSet.name.IndexOf("Touch Input") > -1){
				for (int i = 0; i < iosSet.actSlots.Count; i++) {
					
					//Compare the references for each index to make sure they match
					if(iosSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){
						
						//Then reload the activator settings to the matched reference index
						actSet.useTouch = iosSet.actSlots[i].useTouch;
						actSet.name = iosSet.actSlots[i].name;
						actSet.touchPosition = iosSet.actSlots[i].touchPosition;
						actSet.touchDimensions = iosSet.actSlots[i].touchDimensions;
						actSet.touchReferenceIndex = iosSet.actSlots[i].touchReferenceIndex;
						actSet.activatorSet = iosSet.actSlots[i].activatorSet;
						actSet.touchPosition = iosSet.actSlots[i].touchPosition;
						actSet.showTouchArea = iosSet.actSlots[i].showTouchArea;
						actSet.wasShowing = iosSet.actSlots[i].wasShowing;
						actSet.restoreShowState = iosSet.actSlots[i].restoreShowState;
						actSet.touchedArea = iosSet.actSlots[i].touchedArea;
						actSet.setTouchName = iosSet.actSlots[i].setTouchName;
						actSet.touchNameNotSet = iosSet.actSlots[i].touchNameNotSet;
						actSet.touchInUse = iosSet.actSlots[i].touchInUse;
						actSet.setTouchReferenceIndex = iosSet.actSlots[i].setTouchReferenceIndex;
						actSet.relativePosition = iosSet.actSlots[i].relativePosition;
						actSet.relativeScale = iosSet.actSlots[i].relativeScale;
						actSet.areaRect = iosSet.actSlots[i].areaRect;
						actSet.rectPos = iosSet.actSlots[i].rectPos;
						actSet.moveRect = iosSet.actSlots[i].moveRect;
						actSet.atEdge = iosSet.actSlots[i].atEdge;
						actSet.areaColor = iosSet.actSlots[i].areaColor;
						actSet.touchIndex = iosSet.actSlots[i].touchIndex;
						actSet.touchSlotIndex = iosSet.actSlots[i].touchSlotIndex;
					}
					
				}
			}
		}
		#endif

		#if UNITY_ANDROID
		GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];

		if(androidSet.saved){
			for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
				GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
				
				//Make sure it is a Touch Activator
				if(actSet.name.IndexOf("Touch Input") > -1){
					for (int i = 0; i < androidSet.actSlots.Count; i++) {
						
						//Compare the references for each index to make sure they match
						if(androidSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){

							//Then reload the activator settings to the matched reference index
							actSet.useTouch = androidSet.actSlots[i].useTouch;
							actSet.name = androidSet.actSlots[i].name;
							actSet.touchPosition = androidSet.actSlots[i].touchPosition;
							actSet.touchDimensions = androidSet.actSlots[i].touchDimensions;
							actSet.touchReferenceIndex = androidSet.actSlots[i].touchReferenceIndex;
							actSet.activatorSet = androidSet.actSlots[i].activatorSet;
							actSet.touchPosition = androidSet.actSlots[i].touchPosition;
							actSet.showTouchArea = androidSet.actSlots[i].showTouchArea;
							actSet.wasShowing = androidSet.actSlots[i].wasShowing;
							actSet.restoreShowState = androidSet.actSlots[i].restoreShowState;
							actSet.touchedArea = androidSet.actSlots[i].touchedArea;
							actSet.setTouchName = androidSet.actSlots[i].setTouchName;
							actSet.touchNameNotSet = androidSet.actSlots[i].touchNameNotSet;
							actSet.touchInUse = androidSet.actSlots[i].touchInUse;
							actSet.setTouchReferenceIndex = androidSet.actSlots[i].setTouchReferenceIndex;
							actSet.relativePosition = androidSet.actSlots[i].relativePosition;
							actSet.relativeScale = androidSet.actSlots[i].relativeScale;
							actSet.areaRect = androidSet.actSlots[i].areaRect;
							actSet.rectPos = androidSet.actSlots[i].rectPos;
							actSet.moveRect = androidSet.actSlots[i].moveRect;
							actSet.atEdge = androidSet.actSlots[i].atEdge;
							actSet.areaColor = androidSet.actSlots[i].areaColor;
							actSet.touchIndex = androidSet.actSlots[i].touchIndex;
							actSet.touchSlotIndex = androidSet.actSlots[i].touchSlotIndex;
						}

					}
				}
			}
		}
		#endif
	}
	
	void RefreshTouchAreas(){
		
		#if UNITY_STANDALONE
		GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];

		standSet.actSlots.Clear();
		standSet.saved = false;
		#endif
		
		#if UNITY_IOS
		GAC_SavedTouchArea iOSSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];

		iOSSet.actSlots.Clear();
		iOSSet.saved = false;
		#endif

		#if UNITY_ANDROID
		GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];

		androidSet.actSlots.Clear();
		androidSet.saved = false;
		#endif
	}

	#endregion Save Touch Areas

	#endregion Resolution Settings

	void  AddDFSequence(GAC gacSettings , GAC_ActivatorSetup actSet){
		
		GAC_SequenceSetup newSequence = new GAC_SequenceSetup();
		
		//Add the activator
		actSet.sequenceSlots.Add(newSequence);
		
		actSet.showActivator = true;
		
		//Keep track of the amount
		actSet.sequenceAmounts++;
		
		
	}
	
}

