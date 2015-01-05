using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JrDevAssets;

[CustomEditor(typeof(GAC_TargetTracker))]
public class GAC_Tracker_Editor : Editor {

	GAC_TargetTracker gacTarget;
	GAC gacSettings;
	Rect guiDefaultPosition;

	public List<int> layersUsed = new List<int>(); //Keep a list of all layers in animator
	public List<int> stateCount = new List<int>(); //Keep a list of the animation counts of each layer

	public Animation animationComponent;
	public Animator animatorComponent;

	bool storingAnimations;//Trigger when to start storing animations
	int hideWait; //Loop after hiding components

	void OnEnable(){
		
		//Reference the components
		gacTarget = Selection.activeGameObject.GetComponent<GAC_TargetTracker>();
		animationComponent = Selection.activeGameObject.GetComponent<Animation>();
		animatorComponent = Selection.activeGameObject.GetComponent<Animator>();
		
		if(animationComponent != null){
			gacTarget.conType = GAC_TargetTracker.ControllerType.Legacy;
		}else if(animatorComponent != null){
			gacTarget.conType = GAC_TargetTracker.ControllerType.Mecanim;
		}

	}


	public override void  OnInspectorGUI (){

		//Cast the GAC script from target to have the Inspector show on this script
		gacTarget = target as GAC_TargetTracker;

		gacSettings = gacTarget.gameObject.GetComponent<GAC>();

		//If a GAC script is attached too, copy all following settings to TargetTracker
		if(gacSettings != null){
			gacTarget.gameModeIndex = gacSettings.gameModeIndex;

			if(gacSettings.conType == GAC.ControllerType.Legacy){
				gacTarget.conType = GAC_TargetTracker.ControllerType.Legacy;
			}else{
				gacTarget.conType = GAC_TargetTracker.ControllerType.Mecanim;
			}

			gacTarget.directionIndex = gacSettings.directionIndex;
			gacTarget.detectFacingDirection = gacSettings.detectFacingDirection;
		}

		if (GAC.images == null) {
			GAC.images = AssetDatabase.LoadAssetAtPath("Assets/JrDevAssets/GAC - GreatAnimationCombos/Resources/GAC_Images.asset",typeof(GAC_Images)) as GAC_Images;
			GAC.gacSkins = AssetDatabase.LoadAssetAtPath("Assets/JrDevAssets/GAC - GreatAnimationCombos/Resources/GAC_Skins.asset",typeof(GUISkin)) as GUISkin;
		}

		AnimationInitialization();

		//Record changes for undo
		Undo.RecordObject (gacTarget, "Record Target Tracker");

		EditorGUILayout.BeginHorizontal();
		
		//Reset the position dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		//Show the header
		GUI.DrawTexture(new Rect(guiDefaultPosition.x, guiDefaultPosition.y, GAC.images.gacTrackerHeader.width, GAC.images.gacTrackerHeader.height), GAC.images.gacTrackerHeader);
		EditorGUILayout.EndHorizontal();

		GUIStyle boxStyle = new GUIStyle(GUI.skin.GetStyle("Box"));
		boxStyle.fontSize = 14;
		boxStyle.alignment = TextAnchor.MiddleCenter;
		
		GUILayout.Space(40);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30), new GUIContent("TARGET ID " + gacTarget.targetId), boxStyle);
		
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(35);

		//Make sure a GAC script is not attached too
		if(gacSettings == null){
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 6, guiDefaultPosition.y, 100, 20), new GUIContent("Game Mode", "The parameter size for this gameobject"));
			
			//Show the popup to modify the Game Mode
			gacTarget.gameModeIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 198, guiDefaultPosition.y, 125, 20),  gacTarget.gameModeIndex , gacTarget.gameModeNames.ToArray(),EditorStyles.toolbarPopup);
			
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);
		
		
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
			
			//Show the label
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x - 5, guiDefaultPosition.y, 200, 20), new GUIContent("Animation Controller Type", "What type of animation controller to use"));
			
			//Show the popup to modify the Animation Controller Mode
			gacTarget.conType = (GAC_TargetTracker.ControllerType) EditorGUI.EnumPopup(new Rect (guiDefaultPosition.x + 198, guiDefaultPosition.y, 125, 20), gacTarget.conType,EditorStyles.toolbarPopup);
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(10);
		}
		//Show the separator
		JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
		GUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 30), new GUIContent("Hit Detection Setup"), boxStyle);
		
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(35);

		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		//Make sure the gizmo is not shown yet
		if(!gacTarget.showGizmo){
			GUI.color = Color.green;
			//Show the gizmo for this animation
			if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329, 20),new GUIContent("Show Parameter Gizmo", "Show the gizmo"),EditorStyles.toolbarButton)){
				gacTarget.showGizmo = true;
			}
			GUI.color = Color.white;
		}else{
			GUI.color = Color.yellow;
			//Hide the gizmo for this animation
			if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329, 20),new GUIContent("Hide Parameter Gizmo", "Hide the gizmo"),EditorStyles.toolbarButton)){
				gacTarget.showGizmo = false;
			}
			GUI.color = Color.white;
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(30);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 100, 20), new GUIContent("Parameter Size", "The parameter size for this gameobject"));

		//Check if 3D Mode index selected
		if(gacTarget.gameModeIndex == 0){
			gacTarget.parameterSize = JrDevArts_Utilities.Vector2Field(new Rect (guiDefaultPosition.x + 120, guiDefaultPosition.y, 168, 17), gacTarget.parameterSize, "X", "Z", 0.03f);
		
		}else if(gacTarget.gameModeIndex == 1){//Check if 2D Mode index selected
			gacTarget.parameterSize = JrDevArts_Utilities.Vector2Field(new Rect (guiDefaultPosition.x + 120, guiDefaultPosition.y, 168, 17), gacTarget.parameterSize, "X", "Y", 0.03f);
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(30);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 120, 20), new GUIContent("Parameter Position", "The parameter position for this gameobject"));

		//Check if 3D Mode index selected
		if(gacTarget.gameModeIndex == 0){
			gacTarget.parameterPos = EditorGUI.Vector3Field(new Rect (guiDefaultPosition.x + 120, guiDefaultPosition.y, 210, 20),"", gacTarget.parameterPos);
		}else if(gacTarget.gameModeIndex == 1){
			gacTarget.parameterPos = EditorGUI.Vector2Field(new Rect (guiDefaultPosition.x + 120, guiDefaultPosition.y, 139, 20),"", gacTarget.parameterPos);
		}
		
		EditorGUILayout.EndHorizontal();

		
		if(gacTarget.horizontalSensitivity % 2 != 0){
			gacTarget.horizontalSensitivity = gacTarget.horizontalSensitivity + 1;
		}

		GUILayout.Space(30);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 150, 20), new GUIContent("X Hit Detection Sensitivty ", "The amount hit detection points used horizontally"));

		gacTarget.horizontalSensitivity = EditorGUI.IntSlider(new Rect (guiDefaultPosition.x + 195, guiDefaultPosition.y, 135, 20), gacTarget.horizontalSensitivity, 2, 20);
		
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(30);

		//Check if 3D Mode index selected
		if(gacTarget.gameModeIndex == 0){
			if(gacTarget.forwardSensitivity % 2 != 0){
				gacTarget.forwardSensitivity = gacTarget.forwardSensitivity + 1;
			}
		}else if (gacTarget.gameModeIndex == 1){ //Check if 2D Mode index selected
			if(gacTarget.verticalSensitivity % 2 != 0){
				gacTarget.verticalSensitivity = gacTarget.verticalSensitivity + 1;
			}
		}
		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

		//Check if 3D Mode index selected
		if(gacTarget.gameModeIndex == 0){
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 150, 20), new GUIContent("Z Hit Detection Sensitivty ", "The amount hit detection points used forward"));
			
			gacTarget.forwardSensitivity = EditorGUI.IntSlider(new Rect (guiDefaultPosition.x + 195, guiDefaultPosition.y, 135, 20), gacTarget.forwardSensitivity, 2, 20);

		}else if (gacTarget.gameModeIndex == 1){//Check if 2D Mode index selected
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 150, 20), new GUIContent("Y Hit Detection Sensitivty ", "The amount hit detection points used vertically"));
			
			gacTarget.verticalSensitivity = EditorGUI.IntSlider(new Rect (guiDefaultPosition.x + 195, guiDefaultPosition.y, 135, 20), gacTarget.verticalSensitivity, 2, 20);
		}


		
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(30);

		if(gacTarget.gameModeIndex == 1){

			//Show the separator
			JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
			GUILayout.Space(10);

			//Make sure a GAC script is not attached too
			if(gacSettings == null){
			
				EditorGUILayout.BeginHorizontal();
				
				//Reset the dimensions to 1
				guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

				//Only if using hit detection
				if(!gacTarget.detectFacingDirection){

					GUI.color = Color.green;
					//Make the Combo Setup menu active if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329,20), new GUIContent("Turn On Facing Direction Detection", 
					                                                                                                 "Use to turn on facing direction detection"), EditorStyles.toolbarButton)){
						gacTarget.detectFacingDirection = true;
					}
					GUI.color = Color.white;

				}else{
					GUI.color = Color.red;
					//Make the Combo Setup menu active if button is pressed
					if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 329,20), new GUIContent("Turn Off Facing Direction Detection", 
					                                                                                                 "Use to turn off facing direction detection"), EditorStyles.toolbarButton)){
						gacTarget.detectFacingDirection = false;
					}
					GUI.color = Color.white;

				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space(30);
			}
			EditorGUILayout.BeginHorizontal();
			
			//Reset the dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Only if using hit detection
			if(gacTarget.detectFacingDirection){

				//Make sure a GAC script is not attached too
				if(gacSettings == null){
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 130, 30), new GUIContent("Right Facing Scale"));
					
					//Show the popup to modify the Debug Mode
					gacTarget.directionIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 28, 20),  gacTarget.directionIndex , gacTarget.directionScales.ToArray(),EditorStyles.toolbarPopup);
			
				}else{
					GUI.enabled = false;
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 130, 30), new GUIContent("Right Facing Scale"));
					EditorGUI.Popup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 28, 20),  gacTarget.directionIndex , gacTarget.directionScales.ToArray(),EditorStyles.toolbarPopup);
					GUI.enabled = true;
				}
				EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 140, guiDefaultPosition.y, 150, 30), new GUIContent("Current Facing Direction"));
				//gacTarget.facingDirectionRight = EditorGUI.Toggle(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 100, 20), gacTarget.facingDirectionRight);
				
				if(gacTarget.facingDirectionRight){
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("RIGHT"), boxStyle);
				}else{
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("LEFT"), boxStyle);
				}

			}else{
				
				GUI.enabled = false;
				EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 130, 30), new GUIContent("Right Facing Scale"));
				
				//Show the popup to modify the Debug Mode
				EditorGUI.Popup(new Rect (guiDefaultPosition.x + 104, guiDefaultPosition.y, 28, 20),  gacTarget.directionIndex , gacTarget.directionScales.ToArray(),EditorStyles.toolbarPopup);
				
				
				EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 140, guiDefaultPosition.y, 150, 30), new GUIContent("Current Facing Direction"));
				//gacTarget.facingDirectionRight = EditorGUI.Toggle(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 100, 20), gacTarget.facingDirectionRight);
				
				if(gacTarget.facingDirectionRight){
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("RIGHT"), boxStyle);
				}else{
					EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 280, guiDefaultPosition.y, 50, 20), new GUIContent("LEFT"), boxStyle);
				}
				GUI.enabled = true;
			
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(25);

			EditorGUILayout.BeginHorizontal();
			
			//Reset the dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Only if using hit detection
			if(gacTarget.detectFacingDirection){
				EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 100, 20), new GUIContent("Flip Smoothing", "Adjustment of position for a smooth flipping of facing direction"));
				
				var fieldValues = JrDevArts_Utilities.ContextFloatField(new Rect (guiDefaultPosition.x + 230, guiDefaultPosition.y, 44, 20), gacTarget.smoothFlipAmount, 0.03f, -50, 50, gacTarget.isDraggingSmoothFlip);
				gacTarget.isDraggingSmoothFlip = fieldValues.Value;
				
				//Show the field to modify the move amount
				gacTarget.smoothFlipAmount = fieldValues.Key;
			}

			EditorGUILayout.EndHorizontal();
			GUILayout.Space(30);

		}

		//Show the separator
		JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);

		//Make sure there is atleast one animation to use
		if(gacTarget.storeAnimNames.Count > 0){

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			if(gacTarget.animToggle){
				GUI.color = Color.red;
				//Make the Combo Setup menu active if button is pressed
				if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 190,20), new GUIContent("Use Damage Animation - Turn OFF", 
				                                                                                             "Use to turn off damage animation"), EditorStyles.toolbarButton)){
					gacTarget.animToggle = false;
				}
				GUI.color = Color.white;

				gacTarget.currentAnimIndex = EditorGUI.Popup(new Rect (guiDefaultPosition.x + 200, guiDefaultPosition.y, 129, 20), 
				                                             gacTarget.storeAnimNames.IndexOf(gacTarget.damageAnim) , gacTarget.storeAnimNames.ToArray(),EditorStyles.toolbarPopup);
				
				//Make sure index is within limits of 0 and above, and below the animation list length
				if(gacTarget.currentAnimIndex  < 0){
					gacTarget.currentAnimIndex  = 0;
				}else if (gacTarget.currentAnimIndex  >= gacTarget.storeAnimNames.ToArray().Length){
					gacTarget.currentAnimIndex  = gacTarget.storeAnimNames.ToArray().Length - 1;
				}

			}else{

				GUI.color = Color.green;
				if (GUI.Button(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 200,20), new GUIContent("Use Damage Animation - Turn ON", 
				                                                                                             "Use to turn on damage animation"), EditorStyles.toolbarButton)){

					gacTarget.animToggle = true;
				}
				GUI.color = Color.white;


			}

			EditorGUILayout.EndHorizontal();
		
			//Make sure the move is toggled off
			if (gacTarget.animToggle){
				gacTarget.damageAnim = gacTarget.storeAnimNames[gacTarget.currentAnimIndex];

			}
			GUILayout.Space(10);
		}

		GUILayout.Space(10);
		EditorGUILayout.BeginHorizontal();
		
		//Reset the position dimensions to 1
		guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);
		
		//Make sure the move is toggled off
		if (gacTarget.moveToggle){
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 100, 20),"Move Begin:");
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 66, guiDefaultPosition.y, 100, 20),gacTarget.moveBegin.ToString("f2"));
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 240, guiDefaultPosition.y, 100, 20),"Move End:");
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 300, guiDefaultPosition.y, 100, 20),gacTarget.moveEnd.ToString("f2"));
		}
		
		//Show the label and field to modify the move toggle
		EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 150, guiDefaultPosition.y, 100, 20),"Use Move");
		gacTarget.moveToggle = EditorGUI.Toggle(new Rect (guiDefaultPosition.x + 130, guiDefaultPosition.y, 100, 20), gacTarget.moveToggle);

		//Make sure the move is toggled off
		if (!gacTarget.moveToggle){
			GUI.enabled = false;
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 100, 20),"Move Begin:");
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 66, guiDefaultPosition.y, 100, 20),"0.00");
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 240, guiDefaultPosition.y, 100, 20),"Move End:");
			EditorGUI.LabelField(new Rect (guiDefaultPosition.x + 300, guiDefaultPosition.y, 100, 20),"0.00");
			GUI.enabled = true;
		}
		EditorGUILayout.EndHorizontal();

		
		//Check if move is toggled on
		if (gacTarget.moveToggle){
			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Check if animation is toggled to use and Make sure the animation is not null before showing the min/max slider for the move
			if(gacTarget.animToggle){

				//Make sure there is an animation stored
				if(!string.IsNullOrEmpty(gacTarget.damageAnim)){

					//Check if animations are being stored
					if(storingAnimations){

						//Check what animation controller type is being used
						if(gacTarget.conType == GAC_TargetTracker.ControllerType.Legacy){

							if(animationComponent != null){
								
								EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 20),ref gacTarget.moveBegin, ref gacTarget.moveEnd, 0, Selection.activeGameObject.animation[gacTarget.damageAnim].length - 0.1f);
							}
							
						}else if(gacTarget.conType == GAC_TargetTracker.ControllerType.Mecanim){

							if(animatorComponent != null){
								EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 20),ref gacTarget.moveBegin, ref gacTarget.moveEnd, 0, 0.9f);
							}
						}
					}
					JrDevArts_Utilities.NANCheck(gacTarget.moveBegin);
					JrDevArts_Utilities.NANCheck(gacTarget.moveEnd);

				}
			}
			EditorGUILayout.EndHorizontal();

		}else if (!gacTarget.moveToggle){

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			
			//Reset the position dimensions to 1
			guiDefaultPosition = GUILayoutUtility.GetRect (1,1,1,1);

			//Check if animation is toggled to use and Make sure the animation is not null before showing the min/max slider for the move
			if(gacTarget.animToggle){
				GUI.enabled = false;
				EditorGUI.MinMaxSlider(new Rect (guiDefaultPosition.x, guiDefaultPosition.y, 330, 20),ref gacTarget.moveBegin, ref gacTarget.moveEnd, 0, 0);
				GUI.enabled = true;
			}
			EditorGUILayout.EndHorizontal();
		}

		GUILayout.Space(20);
		
		//Show the separator
		JrDevArts_Utilities.ShowTexture(GAC.images.gacSeparator);
		GUILayout.Space(20);

		//If set to legacy animations
		if(gacTarget.conType == GAC_TargetTracker.ControllerType.Legacy){
			
			//Check if animation component is available, if not...
			if(animationComponent == null){

				//Disable animation storing and reset
				storingAnimations = false;
				gacTarget.storeAnimNames.Clear();
				gacTarget.damageAnim = null;
				gacTarget.moveBegin = 0;
				gacTarget.moveEnd = 0;
				gacTarget.animToggle = false;

				//Add animation component
				animationComponent = gacTarget.gameObject.AddComponent<Animation>();
				
				//Move the component up to be above the GAC component
				UnityEditorInternal.ComponentUtility.MoveComponentUp (animationComponent);
				UnityEditorInternal.ComponentUtility.MoveComponentUp (animationComponent);
			}
			
			//Check if animator component is available, if it is...
			if(animatorComponent != null){

				//Hide the component first to prevent errors
				animatorComponent.hideFlags = HideFlags.HideInInspector;
				
				//Check if looped atleast 1
				if(hideWait > 0){
					//Removes the animation component
					DestroyImmediate(animatorComponent);
					
					//Trigger animation storing
					storingAnimations = true;
					
					hideWait = 0;
				}
				
				hideWait++;
				
			}else if(animationComponent != null){
				//Trigger animation storing
				storingAnimations = true;
				
			}
			
		//If set to Mecanim animations
		}else if(gacTarget.conType == GAC_TargetTracker.ControllerType.Mecanim){
			
			//Check if animator component is available, if not..
			if(animatorComponent == null){

				//Disable animation storing and reset
				storingAnimations = false;
				gacTarget.storeAnimNames.Clear();
				gacTarget.damageAnim = null;
				gacTarget.moveBegin = 0;
				gacTarget.moveEnd = 0;
				gacTarget.animToggle = false;

				//Add animation component
				animatorComponent = gacTarget.gameObject.AddComponent<Animator>();

				//Move the component up to be above the GAC component
				UnityEditorInternal.ComponentUtility.MoveComponentUp (animatorComponent);
				UnityEditorInternal.ComponentUtility.MoveComponentUp (animatorComponent);
				
			}
			
			//Check if animation component is available, if it is...
			if(animationComponent != null){

				//Hide the component first to prevent errors
				animationComponent.hideFlags = HideFlags.HideInInspector;

				//Check if looped atleast 1
				if(hideWait > 0){
					//Removes the animation component
					DestroyImmediate(animationComponent);

					//Trigger animation storing
					storingAnimations = true;

					hideWait = 0;
				}
				
				hideWait++;

			}else if(animatorComponent != null){

				//Trigger animation storing
				storingAnimations = true;
				
			}

		}

		if (GUI.changed){
			EditorUtility.SetDirty(gacTarget);//Let other objects know the script has been changed and need to save to disk
		}

	}

	void OnSceneGUI(){

		GAC.gacGameObjects = FindObjectsOfType<GAC>().ToList();
		
		foreach(GAC gacSettings in GAC.gacGameObjects){

			for (int animIndex = 0; animIndex < gacSettings.animSlots.Count; animIndex++) {
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
						
						//THESE ARE THE HEIGHT ARCS
						//Draw arc to the right
						Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y, gacSettings.gameObject.transform.position.z), 
						                      gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, gacSet.affectAngle/2, gacSet.affectDistance);
						
						//Draw arc to the left
						Handles.DrawSolidArc (new Vector3(gacSettings.gameObject.transform.position.x, gacSettings.gameObject.transform.position.y + gacSet.anglePosition.y, gacSettings.gameObject.transform.position.z),  
						                      gacSettings.gameObject.transform.up, gacSettings.gameObject.transform.forward, -gacSet.affectAngle/2, gacSet.affectDistance);
						
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

	void AnimationInitialization(){

		//Check if animation storing should begin
		if(storingAnimations){
			
			if(gacTarget.conType == GAC_TargetTracker.ControllerType.Legacy){
				
				//Make sure animation component is on the gameobject
				if(animationComponent != null){
					
					if(gacTarget.storeAnimNames.Count > animationComponent.GetClipCount()){
						gacTarget.storeAnimNames.Clear();
					}
					
					var clips = AnimationUtility.GetAnimationClips(gacTarget.gameObject).ToList();
					
					for (int i = 0; i < clips.Count; i++) {
						
						if(!gacTarget.storeAnimNames.Contains(clips[i].name)){
							//Make sure to ignore the Take 001 animation string and not null
							if(clips[i].name != "Take 001" && clips[i] != null){
								gacTarget.storeAnimNames.Add(clips[i].name);
							}
						}
						
					}
				}
			}else if(gacTarget.conType == GAC_TargetTracker.ControllerType.Mecanim){

				//Make sure the component is available
				if(animatorComponent!= null){
					
					//Get a reference to the Animator Controller:
					UnityEditorInternal.AnimatorController animatorControl = animatorComponent.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
					
					//Make sure there is a Runtime Controller added
					if(animatorControl != null){
						
						//Check if 3D Mode index selected
						if(gacTarget.gameModeIndex == 0){
							
							//Make sure the avatar is attached for mecanim to use
							if(animatorComponent.avatar != null){
								
								// Number of layers
								int layerCount = animatorControl.layerCount;
								
								int statesSum = stateCount.Sum() - gacTarget.dummyStates.Count;
								
								//If the stored animations are more than the sum of all the states then reset the lists
								if(statesSum < gacTarget.storeAnimNames.Count){
									gacTarget.keepAnimsInSync.Clear();
									gacTarget.dummyStates.Clear();
								}
								
								//Loop through the available layers
								for (int layer = 0; layer < layerCount; layer++) {
									
									//Get states/animations on layer on the layer
									UnityEditorInternal.StateMachine sm = animatorControl.GetLayer(layer).stateMachine;
									
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
										if(!gacTarget.keepAnimsInSync.Contains(state.name + " 'L-" + layer + "'")){
											
											//Make sure there is a clip in motion slot before adding to list
											if(state.GetMotion() != null){
												gacTarget.keepAnimsInSync.Add(state.name + " 'L-" + layer + "'");
											}
											
										}
										
										//Loop and add state names to the list
										if(!gacTarget.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){
											
											//Make sure there is a clip in motion slot before adding to list
											if(state.GetMotion() != null){
												gacTarget.storeAnimNames.Add(state.name + " 'L-" + layer + "'");
												
											}else{
												
												if(!gacTarget.dummyStates.Contains(state.name + " " + layer + " " + i)){
													gacTarget.dummyStates.Add(state.name + " " + layer + " " + i);
												}
											}
											
										}else if(gacTarget.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){//If state already in list
											
											//If there is no clip in motion slot
											if(state.GetMotion() == null){
												gacTarget.storeAnimNames.Remove(state.name + " 'L-" + layer + "'");
											}
											
											
										}
									}
									
									//Loop through and make sure the 2 lists are a match with the animations
									for (int i = 0; i < gacTarget.storeAnimNames.Count; i++) {
										if(!gacTarget.keepAnimsInSync.Contains(gacTarget.storeAnimNames[i])){
											gacTarget.storeAnimNames.Remove(gacTarget.storeAnimNames[i]);
										}
									}
									
								}
							}else{
								Debug.LogError("GACError - There is no Avatar on the Animator component of the " + gacTarget.gameObject.name + ". Please add one to before continuing.");
							}
							
						}else if(gacTarget.gameModeIndex == 1){//Check if 2D Mode index selected
							
							// Number of layers
							int layerCount = animatorControl.layerCount;
							
							int statesSum = stateCount.Sum() - gacTarget.dummyStates.Count;
							
							//If the stored animations are more than the sum of all the states then reset the lists
							if(statesSum < gacTarget.storeAnimNames.Count){
								gacTarget.keepAnimsInSync.Clear();
								gacTarget.dummyStates.Clear();
							}
							
							//Loop through the available layers
							for (int layer = 0; layer < layerCount; layer++) {
								
								//Get states/animations on layer on the layer
								UnityEditorInternal.StateMachine sm = animatorControl.GetLayer(layer).stateMachine;
								
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
									if(!gacTarget.keepAnimsInSync.Contains(state.name + " 'L-" + layer + "'")){
										
										//Make sure there is a clip in motion slot before adding to list
										if(state.GetMotion() != null){
											gacTarget.keepAnimsInSync.Add(state.name + " 'L-" + layer + "'");
										}
										
									}
									
									//Loop and add state names to the list
									if(!gacTarget.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){
										
										//Make sure there is a clip in motion slot before adding to list
										if(state.GetMotion() != null){
											gacTarget.storeAnimNames.Add(state.name + " 'L-" + layer + "'");
											
										}else{
											
											if(!gacTarget.dummyStates.Contains(state.name + " " + layer + " " + i)){
												gacTarget.dummyStates.Add(state.name + " " + layer + " " + i);
											}
										}
										
									}else if(gacTarget.storeAnimNames.Contains(state.name + " 'L-" + layer + "'")){//If state already in list
										
										//If there is no clip in motion slot
										if(state.GetMotion() == null){
											gacTarget.storeAnimNames.Remove(state.name + " 'L-" + layer + "'");
										}
										
										
									}
									
								}
								
								//Loop through and make sure the 2 lists are a match with the animations
								for (int i = 0; i < gacTarget.storeAnimNames.Count; i++) {
									if(!gacTarget.keepAnimsInSync.Contains(gacTarget.storeAnimNames[i])){
										gacTarget.storeAnimNames.Remove(gacTarget.storeAnimNames[i]);
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
