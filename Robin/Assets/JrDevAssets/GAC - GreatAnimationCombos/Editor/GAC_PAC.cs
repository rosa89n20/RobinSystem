using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using JrDevAssets;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.7
//GAC_PAC.cs
/////////////////////////////////////////////////////////////////////////////////////////
public class GAC_PAC : EditorWindow{

	public GAC gacSettings; //Reference to GAC script
	public static bool gacPacInitialize; //Keeps track of gac pac initializations
	public static GAC_PAC gacWindow; //Reference to the gac pac window

	public Animator animatorComponent;
	public UnityEditorInternal.AnimatorController animatorController;

	public GameObject target; //Reference to current target selected
	public GameObject otherTarget; //Reference to another target selected

	public GUIStyle style = new GUIStyle(); //Used for creating different button and font styles

	public Vector2 scrollPosition = Vector2.zero; //Set the scrolling position values
	public Rect scrollLength; //Set the scrolling length for the window

	[UnityEditor.MenuItem("Tools/Great Animation Combos (GAC)/Preview Animation Combos (PAC)")]
	static void Init(){

		//Show existing window instance. If one doesn't exist, make one.
		gacWindow = (GAC_PAC)EditorWindow.GetWindow(typeof(GAC_PAC));

		gacWindow.name = "GAC PAC";
		gacWindow.Show();
		 
	}

	public void OnGUI(){

		//Check for a selected gameobject
		target = Selection.activeGameObject;

		if(gacWindow == null){
			//Find the window 
			gacWindow = (GAC_PAC)EditorWindow.GetWindow(typeof(GAC_PAC));
			
		}

		if (GAC.images == null) {
			GAC.images = AssetDatabase.LoadAssetAtPath("Assets/JrDevAssets/GAC - GreatAnimationCombos/Resources/GAC_Images.asset",typeof(GAC_Images)) as GAC_Images;
			GAC.gacSkins = AssetDatabase.LoadAssetAtPath("Assets/JrDevAssets/GAC - GreatAnimationCombos/Resources/GAC_Skins.asset",typeof(GUISkin)) as GUISkin;
		}else{
			//Make sure not null
			if(GAC.images.gacpacInfo1 != null){
				//Set the window size contraints
				gacWindow.minSize = new Vector2(GAC.images.gacpacInfo1.width + 70, 360);
			}

			//Make sure not null
			if(GAC.images.gacpacHeader != null){
				//Show the header
				GUI.Label(new Rect((gacWindow.position.width/2) - (GAC.images.gacpacHeader.width/2),0,GAC.images.gacpacHeader.width,GAC.images.gacpacHeader.height), GAC.images.gacpacHeader);
			}

			//Make sure not null
			if(GAC.images.gacpacSeparator != null){

				//Show the Separator
				GUI.DrawTexture(new Rect(0, 96, GAC.images.gacpacSeparator.width,GAC.images.gacpacSeparator.height),GAC.images.gacpacSeparator);
			}


			//Check if any gameobject is selected
			if (target == null){

				//Make sure not null
				if(GAC.images.gacpacInfo1 != null){
					//Show the no target selected message
					GUI.Label(new Rect((gacWindow.position.width/2) - (GAC.images.gacpacInfo1.width/2), 120, GAC.images.gacpacInfo1.width, GAC.images.gacpacInfo1.height), GAC.images.gacpacInfo1);
				}
				return;
			}else{

				//Reference the GAC script
				gacSettings = target.GetComponent<GAC>();

				//Check for gameobject with the GAC script
				if(gacSettings == null){

					//Make sure not null
					if(GAC.images.gacpacInfo2 != null){
						//Show the no script attached to gameobject message
						GUI.Label(new Rect((gacWindow.position.width/2) - (GAC.images.gacpacInfo2.width/2), 120, GAC.images.gacpacInfo2.width, GAC.images.gacpacInfo2.height), GAC.images.gacpacInfo2);
					}
					return;
				}else{


					//Create the scroll view
					scrollPosition = GUI.BeginScrollView(new Rect(0, 100, gacWindow.position.width, gacWindow.position.height - 100), scrollPosition, new Rect(0, 80, 0, scrollLength.y - 45), false, false);
						ShowCombos();
					GUI.EndScrollView();

				}
			}
			
		}



		// This is necessary to make the framerate normal for the editor window.
		this.Repaint();
	}

	void ShowCombos(){


		//Check if there are no starters added
		if (gacSettings.starterSlots.Count == 0){

			//Make sure not null
			if(GAC.images.gacpacInfo3 != null){
				//Show the no target selected message
				GUI.Label(new Rect((gacWindow.position.width/2) - (GAC.images.gacpacInfo3.width/2), 100, GAC.images.gacpacInfo3.width, GAC.images.gacpacInfo3.height), GAC.images.gacpacInfo3);
			}
		}

		for (int startIndex= 0; startIndex < gacSettings.starterSlots.Count; startIndex++) {
			GAC_StarterSetup startSet = gacSettings.starterSlots[startIndex];

			EditorGUILayout.BeginHorizontal();

			//Get the position and dimensions of the last gui
			if(startIndex == 0){

				//Preset this position
				startSet.lastDim.y = 100;

				//Create a new toolbar style to use
				style = new GUIStyle(EditorStyles.toolbarButton);
				style.font = GAC.images.gacpacStartFont;
				style.fixedHeight = 26;
				style.fontSize = 18;
				style.alignment = TextAnchor.LowerCenter;

				if(!gacSettings.starterGroupShow[startIndex]){

					//Change the button color
					GUI.backgroundColor = Color.green;

					//Display the animation name in the foldout if it's been added to clip slot
					if (GUI.Button(new Rect (2, startSet.lastDim.y, gacWindow.position.width - 5, 30),"Click to Show " + startSet.starterName + " Combos " +
					               "- Amount of Combos (" + startSet.comboAmount + ")", style)){
						if(startSet.comboAmount > 0){
							gacSettings.starterGroupShow[startIndex] = true;
						}
					}
					//Restore the default button color
					GUI.backgroundColor = Color.white;

				}else{

					//Change the button color
					GUI.backgroundColor = Color.yellow;

					//Display the animation name in the foldout if it's been added to clip slot
					if (GUI.Button(new Rect (2, startSet.lastDim.y, gacWindow.position.width - 150, 30),"Click to Hide " + startSet.starterName + " Combos " +
					               "- Amount of Combos (" + startSet.comboAmount + ")", style)){
						
						gacSettings.starterGroupShow[startIndex] = false;
					}

					//Check if the activators are already being shown
					if(!startSet.showActivators){
						//Change the button color
						GUI.backgroundColor = Color.green;
						
						//Display the animation name in the foldout if it's been added to clip slot
						if (GUI.Button(new Rect (gacWindow.position.width - 153,startSet.lastDim.y, 150, 30),"Activators", style)){
							startSet.showActivators = true;
						}
					}else{
						//Change the button color
						GUI.backgroundColor = Color.red;
						
						//Display the animation name in the foldout if it's been added to clip slot
						if (GUI.Button(new Rect (gacWindow.position.width - 153,startSet.lastDim.y, 150, 30),"Activators", style)){
							startSet.showActivators = false;
						}
					}

					//Restore the default button color
					GUI.backgroundColor = Color.white;
				}

			}else{

				//Create the button style and change the font and font settings
				style = new GUIStyle(EditorStyles.toolbarButton);
				style.font = GAC.images.gacpacStartFont;
				style.fixedHeight = 26;
				style.fontSize = 18;
				style.alignment = TextAnchor.LowerCenter;

				if(!gacSettings.starterGroupShow[startIndex]){

					//Change the button color
					GUI.backgroundColor = Color.green;

					//Display the animation name in the foldout if it's been added to clip slot
					if (GUI.Button(new Rect (2,startSet.lastDim.y, gacWindow.position.width - 5, 30),"Click to Show " + startSet.starterName + " Combos " +
					               "- Amount of Combos (" + startSet.comboAmount + ")", style)){
						if(startSet.comboAmount > 0){
							gacSettings.starterGroupShow[startIndex] = true;
						}
					}

					//Change the button color
					GUI.backgroundColor = Color.white;
				}else{

					//Change the button color
					GUI.backgroundColor = Color.yellow;

					//Display the animation name in the foldout if it's been added to clip slot
					if (GUI.Button(new Rect (2,startSet.lastDim.y, gacWindow.position.width - 150, 30),"Click to Hide " + startSet.starterName + " Combos " +
					               "- Amount of Combos (" + startSet.comboAmount + ")", style)){
						
						gacSettings.starterGroupShow[startIndex] = false;
					}


					//Check if the activators are already being shown
					if(!startSet.showActivators){
						//Change the button color
						GUI.backgroundColor = Color.green;

						//Display the animation name in the foldout if it's been added to clip slot
						if (GUI.Button(new Rect (gacWindow.position.width - 153,startSet.lastDim.y, 150, 30),"Activators", style)){
							startSet.showActivators = true;
						}
					}else{
						//Change the button color
						GUI.backgroundColor = Color.red;

						//Display the animation name in the foldout if it's been added to clip slot
						if (GUI.Button(new Rect (gacWindow.position.width - 153,startSet.lastDim.y, 150, 30),"Activators", style)){
							startSet.showActivators = false;
						}
					}
					//Change the button color
					GUI.backgroundColor = Color.white;
				}

			}

			EditorGUILayout.EndHorizontal();


			//Check if the current starter is being shown
			if(gacSettings.starterGroupShow[startIndex]){



				for (int comboIndex= 0; comboIndex < startSet.starterCombos.Count; comboIndex++) {
					GAC_ComboSetup comboSet = startSet.starterCombos[comboIndex];

					//If the first combo in the index
					if(comboIndex == 0){

						//Move this combo down by 25 pixels based on the current starters y position; if not done already
						if(comboSet.lastDim.y != startSet.lastDim.y + 30){
							comboSet.lastDim.y = startSet.lastDim.y + 30;
						}

						//Only when activator button has been pushed to show
						if(startSet.showActivators){

							//Check to see if this starter has been added in the list to use
							if (gacSettings.activatorsForStarters.Any(str => str.Contains("Starter " + startSet.starterName))){
								
								//Get the index of this starter in the list
								int index = gacSettings.activatorsForStarters.FindIndex(x => x.StartsWith("Starter " + startSet.starterName));

								//Retrive all the activators
								string theActivators = gacSettings.activatorsForStarters[index].Replace("Starter " + startSet.starterName, "");
								theActivators = theActivators.Replace("Activators ", "");
								theActivators = theActivators.Replace("Activators, ", "");

								//Change the font
								style = new GUIStyle(GUI.skin.GetStyle("Box"));
								style.font = GAC.images.gacpacStartFont;
								style.fontSize = 16;
								style.normal.textColor = Color.green; 
								style.alignment = TextAnchor.MiddleCenter;
								GUI.backgroundColor = Color.black;

								//Reference this string
								string activatorString = "The Activators that can call this starter - " + theActivators;

								//Draw the activator numbers in a box
								GUI.Box(new Rect ((gacWindow.position.width/2) - ((activatorString.Length * 8)/2), comboSet.lastDim.y, activatorString.Length * 8, 25), "The Activators that can call this starter - " + theActivators, style);
								comboSet.lastDim.y = comboSet.lastDim.y + 30;

								GUI.backgroundColor = Color.white;
							}
						}
					}else{

						//Move this combo down by 30 pixels based on the last combo's y position; if not done already
						if(comboSet.lastDim.y != startSet.starterCombos[comboIndex - 1].lastDim.y + 35){
							comboSet.lastDim.y = startSet.starterCombos[comboIndex - 1].lastDim.y + 35;
						}

					}



					//Change the font
					style = new GUIStyle(EditorStyles.label);
					style.font = GAC.images.gacpacStartFont;
					style.fontSize = 12;
					style.normal.textColor = Color.black;

					//Draw the combo number
					GUI.Label(new Rect (15, comboSet.lastDim.y, 80, 20), "Combo #" + (comboIndex + 1), style);

					//Change the color, font and size of font for the Links text
					style.normal.textColor = Color.red;
					style.font = GAC.images.gacpacStartFont;

					GUI.Label(new Rect (15, comboSet.lastDim.y + 12, 80, 20), "L I N K S -> " + comboSet.linkAmount, style);

					//Restore to default color
					style.normal.textColor = Color.black;

					for (int animIndex= 0; animIndex < comboSet.theCombos.Count; animIndex++) {

						//Get the animation string
						string animOfCombo = comboSet.theCombos[animIndex];

						//Create a new box style to use, then set attributes of the font
						style = new GUIStyle(GUI.skin.GetStyle("Box"));
						style.font = GAC.images.gacpacAnimFont;
						style.fontSize = 12;
						style.alignment = TextAnchor.MiddleCenter;

						//Use to reference the width of each animation string length
						float animBoxLength = 0;

						//Check if using Legacy or Mecanim Animations
						if(gacSettings.conType == GAC.ControllerType.Legacy){
							//Check the length of the animation name to determine the length to make the box
							if(animOfCombo.Length < 10){
								animBoxLength = 130;
							}else if(animOfCombo.Length >= 10 && animOfCombo.Length < 30){
								animBoxLength = 294;
							}
						}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
							//Check the length of the animation name to determine the length to make the box
							if(animOfCombo.Length < 15){
								animBoxLength = 130;
							}else if(animOfCombo.Length >= 15 && animOfCombo.Length < 35){
								animBoxLength = 294;
							}
						}

						//Reference the width that should be used to check edge limits for animation boxes
						int widthCheck = 0;

						if(animIndex == 0){


							//Check if current sequence playing matches the animation index
							if((gacSettings.sequenceCounter - 2) == animIndex){

									//Check if animation of the animation box is playing, then change animation box color to green if matched
									if(GAC.IsPlaying(target, comboSet.theCombos[animIndex])){
										GUI.backgroundColor = Color.green;
									}
								
							}

							//Draw the animation box
							GUI.Box(new Rect (90, comboSet.lastDim.y, animBoxLength, 20),animOfCombo.ToUpper(), style);

							//Create a new box style to use, then set attributes of the font
							style = new GUIStyle(EditorStyles.miniBoldLabel);
							style.normal.textColor = Color.red;

							//Restore default color
							GUI.backgroundColor = Color.white;

							//Update the new dimension
							comboSet.lastDim = new Rect(90,comboSet.lastDim.y,animBoxLength,20);

							if(GAC.images.gacpacArrow == null){
								return;
							}


						}else{


							if(animBoxLength > 130){
								widthCheck = 310;
							}else{
								widthCheck = 150;
							}

							//Make sure the combo sequence matches the current sequence and only indexes below the current sequence
							if (comboSet.comboSequence[animIndex - 1] == (gacSettings.sequenceCounter - 1) && animIndex <= (gacSettings.sequenceCounter - 1)){

								if(gacSettings.conType == GAC.ControllerType.Legacy){
									//Make sure to contain within the current starter animation being used
									if(startSet.starterName == gacSettings.starterAnimation){

										//Check if animation of the animation box is playing, then change animation box color to green if matched
										if(Selection.activeGameObject.animation.IsPlaying(comboSet.theCombos[animIndex])){
											GUI.backgroundColor = Color.green;
										}
									}
								}else if(gacSettings.conType == GAC.ControllerType.Mecanim){
									string newStarterName = startSet.starterName.Before("'L");

									//Make sure to contain within the current starter animation being used
									if(newStarterName == gacSettings.starterAnimation){
										if(animatorComponent == null){
											animatorComponent = Selection.activeGameObject.GetComponent<Animator>();
										}

										//Check if animation of the animation box is playing, then change animation box color to green if matched
										if(GAC.IsPlaying(target, comboSet.theCombos[animIndex])){
											GUI.backgroundColor = Color.green;
										}
									}


								}

							}


							//Check if the animation length is more than the window minus animation box and Arrow length combined
							if (comboSet.lastDim.xMax > gacWindow.position.width - widthCheck){
								GUI.Label(new Rect (190, comboSet.lastDim.y + 28, GAC.images.gacpacGreenArrow.width, GAC.images.gacpacGreenArrow.height), GAC.images.gacpacGreenArrow);
							
								//Display Arrow to signify the combo links
								GUI.Label(new Rect (220, comboSet.lastDim.y + 28,GAC.images.gacpacArrow.width,GAC.images.gacpacArrow.height),GAC.images.gacpacArrow);

								//Draw the animation box
								GUI.Box(new Rect (254, comboSet.lastDim.y + 30, animBoxLength, 20), animOfCombo.ToUpper(), style);


								//Update the new dimension
								comboSet.lastDim = new Rect(254,comboSet.lastDim.y + 30, animBoxLength, 20);


							}else{

								//Display Arrow to signify the combo links
								GUI.Label(new Rect (comboSet.lastDim.xMax, comboSet.lastDim.y - 2,GAC.images.gacpacArrow.width,GAC.images.gacpacArrow.height),GAC.images.gacpacArrow);

								//Update the new dimension
								comboSet.lastDim = new Rect(comboSet.lastDim.xMax,comboSet.lastDim.y,GAC.images.gacpacArrow.width, 20);

								//Draw the animation box
								GUI.Box(new Rect (comboSet.lastDim.xMax, comboSet.lastDim.y, animBoxLength, 20), animOfCombo.ToUpper(), style);

								//Update the new dimension
								comboSet.lastDim = new Rect(comboSet.lastDim.xMax,comboSet.lastDim.y, animBoxLength, 20);

							}

							//Create a new box style to use, then set attributes of the font
							style = new GUIStyle(EditorStyles.miniBoldLabel);
							style.normal.textColor = Color.red;

							if(animBoxLength <= 130){
								//Draw the label
								GUI.Label(new Rect (comboSet.lastDim.xMax - 130, comboSet.lastDim.y + 5, animBoxLength, 20),comboSet.activatorIndex[animIndex - 1] + "", style);
							}else{
								//Draw the label
								GUI.Label(new Rect (comboSet.lastDim.xMax - 294, comboSet.lastDim.y + 5, animBoxLength, 20),comboSet.activatorIndex[animIndex - 1] + "", style);
							}


							//Create a new box style to use, then set attributes of the font
							style = new GUIStyle(EditorStyles.miniBoldLabel);
							style.normal.textColor = Color.blue;

							if(comboSet.delayedAnim.Count > 0){
								if(comboSet.delayedAnim[animIndex - 1]){
									//Draw the label
									GUI.Label(new Rect (comboSet.lastDim.xMax - 11, comboSet.lastDim.y + 5, animBoxLength, 20),"D", style);
								}
							}

							//Restore default color
							GUI.backgroundColor = Color.white;

						}



					}

					//Reference the dimensions which is used to determine the area to scroll
					scrollLength = comboSet.lastDim;

					if(comboIndex < startSet.starterCombos.Count - 1){
						//Display theGAC.images.gacpacArrow to signify the combo links
						GUI.DrawTexture(new Rect (0, comboSet.lastDim.y + 25, GAC.images.gacpacGreenSeparator.width, GAC.images.gacpacGreenSeparator.height), GAC.images.gacpacGreenSeparator);
					}
				}




				//Only do this if not the first starter animation
				if (startIndex > 0){

					//Check if the previous starter being shown
					if(gacSettings.starterGroupShow[startIndex - 1]){

						//Show the Separator
						GUI.DrawTexture(new Rect(0, startSet.lastDim.y - 9,GAC.images.gacpacSeparator.width,GAC.images.gacpacSeparator.height),GAC.images.gacpacSeparator);

						for (int comboIndex= 0; comboIndex < gacSettings.starterSlots[startIndex - 1].starterCombos.Count; comboIndex++) {
							GAC_ComboSetup comboSet = gacSettings.starterSlots[startIndex - 1].starterCombos[comboIndex];

							//Then move this starter down by 35 pixels based on the current combo's y position; if not done already
							if(startSet.lastDim.y != comboSet.lastDim.y + 35){
								startSet.lastDim.y = comboSet.lastDim.y + 35;
							}
						}
					}else{//Else if previous starters is not being shown...

						//Show the Separator
						GUI.DrawTexture(new Rect(0, startSet.lastDim.y - 9,GAC.images.gacpacSeparator.width,GAC.images.gacpacSeparator.height),GAC.images.gacpacSeparator);


						//Then move this starter down by 40 pixels based on the previous starters y position; if not done already
						if(startSet.lastDim.y != gacSettings.starterSlots[startIndex - 1].lastDim.y + 40){
							startSet.lastDim.y = gacSettings.starterSlots[startIndex - 1].lastDim.y + 40;
						}
					}

				}

			}else{

				//Only do this if not the first starter animation
				if (startIndex > 0){

					//Check if the previous starter being shown
					if(gacSettings.starterGroupShow[startIndex - 1]){

						//Show the Separator
						GUI.DrawTexture(new Rect(0, startSet.lastDim.y - 9,GAC.images.gacpacSeparator.width,GAC.images.gacpacSeparator.height),GAC.images.gacpacSeparator);

						for (int comboIndex= 0; comboIndex < gacSettings.starterSlots[startIndex - 1].starterCombos.Count; comboIndex++) {
							GAC_ComboSetup comboSet = gacSettings.starterSlots[startIndex - 1].starterCombos[comboIndex];

							//Then move this starter down by 35 pixels based on the current combo's y position; if not done already
							if(startSet.lastDim.y != comboSet.lastDim.y + 35){
								startSet.lastDim.y = comboSet.lastDim.y + 35;
							}
						}
					}else{ //Else if previous starters is not being shown...

						//Show the Separator
						GUI.DrawTexture(new Rect(0, startSet.lastDim.y - 9,GAC.images.gacpacSeparator.width,GAC.images.gacpacSeparator.height),GAC.images.gacpacSeparator);

						//Then move this starter down by 40 pixels based on the previous starters y position; if not done already
						if(startSet.lastDim.y != gacSettings.starterSlots[startIndex - 1].lastDim.y + 40){
							startSet.lastDim.y = gacSettings.starterSlots[startIndex - 1].lastDim.y + 40;
						
						}
					}

					//Check if at the last starter index
					if(startIndex == gacSettings.starterSlots.Count - 1){
						
						//Reference the dimensions which is used to determine the area amount allowed to scroll
						scrollLength.y = startSet.lastDim.y + 10;
					}
				}


			}


		}
		
		
		
	}

	public static bool ImageCheck(Texture texture, string name, string directory, GAC_PAC gacWindow, bool initialize){
		
		//Combine strings to create full directory
		string newDirectory = "";
		
		if (Directory.Exists("Assets/JrDevAssets/" + directory)){
			
			//Combine strings to create full directory
			newDirectory = "Assets/JrDevAssets/" + directory;
			
		}else if (Directory.Exists("Assets/Plugins/" + directory)){
			
			//Combine strings to create full directory
			newDirectory = "Assets/Plugins/" + directory;
			
		}
		
		if(string.IsNullOrEmpty(newDirectory)){
			//Call the Play mode warning check
			JrDevArts_Utilities.PlayModeWarning();
			
			Debug.LogError ("GAC-Error: Directory 'Assets/GAC - GreatAnimationCombos/Images' or Assets/Plugins/GAC - GreatAnimationCombos/Images' not found!");
			
			return initialize;
		}else{
			
			if(texture == null){
				
				//Call the Play mode warning check
				JrDevArts_Utilities.PlayModeWarning();
				
				//Start reinitialization
				initialize = false;
				gacWindow.Close();
				Debug.LogError ("GAC-Error: There was no " + name + " image found in " + newDirectory + " folder. Please add the file to this directory or make sure directory is correct. Closing GACPAC Window.");
				
			}else{
				initialize = true;
			}
			return initialize;
		}
	}
	
	public static bool FontCheck(Font font, string name, string directory, GAC_PAC gacWindow, bool initialize){
		
		//Combine strings to create full directory
		string newDirectory = "";
		
		if (Directory.Exists("Assets/JrDevAssets/" + directory)){
			
			//Combine strings to create full directory
			newDirectory = "Assets/JrDevAssets/" + directory;
			
			
		}else if (Directory.Exists("Assets/Plugins/" + directory)){
			
			//Combine strings to create full directory
			newDirectory = "Assets/Plugins/" + directory;
			
		}
		
		if(string.IsNullOrEmpty(newDirectory)){
			
			//Call the Play mode warning check
			JrDevArts_Utilities.PlayModeWarning();
			Debug.LogError ("GAC-Error: Directory 'Assets/GAC - GreatAnimationCombos/Fonts' or Assets/Plugins/GAC - GreatAnimationCombos/Fonts' not found!");
			
			return initialize;
		}else{
			if(font == null){
				
				//Call the Play mode warning check
				JrDevArts_Utilities.PlayModeWarning();
				
				//Start reinitialization
				initialize = false;
				gacWindow.Close();
				Debug.LogError ("GAC-Error: There was no " + name + " font found in " + newDirectory + " folder. Please add the file to this directory or make sure directory is correct,  then close and re-open GACPAC Window.");
				
			}else{
				initialize = true;
			}
			
			return initialize;
		}
	}
	
}