using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using JrDevAssets;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.6
//GAC_TAG.cs
/////////////////////////////////////////////////////////////////////////////////////////
/// 
public class GAC_TAG : EditorWindow{

	public GAC gacSettings; //Reference to GAC script
	public GameObject target; //Reference to current target selected
	private GAC_TAG tagWindow;//This editor window

	//Gets mid of resolutions
	private float vatMidWidth;
	private float vatMidHeight;
	private float resolutionMidWidth;
	private float resolutionMidHeight;

	public void OnGUI(){

		//Check for a selected gameobject
		target = Selection.activeGameObject;
		tagWindow = this;

		if(this != null){

			//These are used to get the mid position of both scene and game view
			vatMidWidth = this.position.width/2;
			vatMidHeight = tagWindow.position.height/2;

			if(GAC.images != null && GAC.images.windowGrid != null && GAC.images.tagHeader != null){
				GUI.DrawTexture(new Rect(0, 0, GAC.images.windowGrid.width, GAC.images.windowGrid.height), GAC.images.windowGrid);
				GUI.DrawTexture(new Rect(0, 0, GAC.images.tagHeader.width, GAC.images.tagHeader.height), GAC.images.tagHeader);

				if(target != null){
					//Reference the GAC script
					gacSettings = target.GetComponent<GAC>();

					if(gacSettings != null){
						if(gacSettings.tagWindowReady){
							TouchAreas();
						}
					}
				}
			}

			//Show the refocus message
			if(!gacSettings.tagWindowReady){
				if(GAC.images != null){
					GUI.Label(new Rect (vatMidWidth - (GAC.images.tagInfo.width/2), 200, GAC.images.tagInfo.width, 150), GAC.images.tagInfo);

				}

			}

		}
	}

	public void TouchAreas(){

		EditorGUILayout.BeginHorizontal();
		
		//Reset the dimensions to 1
		Rect rectDimensions = GUILayoutUtility.GetRect (1,1,1,1);

		//Create a new toolbar style to use
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Box"));

		if (GUI.Button(new Rect (rectDimensions.x + 300, rectDimensions.y, 25, 20), new GUIContent(GAC.images.tagTip, "Display a TAG Tip") , EditorStyles.toolbarButton)){

			//Change the tip index
			gacSettings.tagTipIndex++;

			//Reset if over 5
			if(gacSettings.tagTipIndex > 5){
				gacSettings.tagTipIndex = 0;
			}
		}
		if (GUI.Button(new Rect (rectDimensions.x + 325, rectDimensions.y, 25, 20), new GUIContent(GAC.images.tagOff, "Turn OFF TAG") , EditorStyles.toolbarButton)){
			tagWindow.Close();
			gacSettings.simulate = false;
			gacSettings.tagWindowReady = false;
		}


		if (GUI.Button(new Rect (rectDimensions.x + 350, rectDimensions.y, 25, 20), new GUIContent(GAC.images.gacRefresh, "Refresh the settings of Touch Areas for this resolution"), EditorStyles.toolbarButton)){
			RefreshTouchAreas();
		}

		if (GUI.Button(new Rect (rectDimensions.x + 375, rectDimensions.y, 25, 20), new GUIContent(GAC.images.gacSave, "Save the settings of Touch Areas for this resolution"), EditorStyles.toolbarButton)){
			SaveTouchAreas();
		}

		//Register trigger if any Touch Areas have been set to show
		bool anyAreasShown = gacSettings.activatorSlots.Any(s => s.showTouchArea == true);

		//Show the tip if no Touch Area's shown
		if(!anyAreasShown){
			GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
			labelStyle.normal.textColor = Color.green;
			labelStyle.alignment = TextAnchor.MiddleCenter;
			labelStyle.fontStyle = FontStyle.Bold;
			GUI.color = Color.green;


			GUI.Label(new Rect (vatMidWidth -  300, 600, 600, 50), "TIP - Make sure to click on 'Show Touch Area' for atleast 1 Touch Activator Slot to begin using TAG!", 
			          labelStyle);

			GUI.color = Color.white;
		}else{

			TagTips(gacSettings.tagTipIndex);
		}

		//Create a new toolbar style to use
		style = new GUIStyle(EditorStyles.toolbarPopup);

		#if UNITY_STANDALONE
		
		GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];

		//Check if saving is on for this selection then change color if so
		if(standSet.saved){
			style.normal.textColor = Color.blue;
		}
		#endif

		#if UNITY_IOS
		
		GAC_SavedTouchArea iOSSet = gacSettings.iosSavedSlots[gacSettings.resolutionIndex];
		
		//Check if saving is on for this selection then change color if so
		if(iOSSet.saved){
			style.normal.textColor = Color.blue;
		}
		#endif

		#if UNITY_ANDROID
		
		GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.resolutionIndex];
		
		//Check if saving is on for this selection then change color if so
		if(androidSet.saved){
			style.normal.textColor = Color.blue;
		}
		#endif
		//Get the current resolution index
		gacSettings.currentIndex = gacSettings.resolutionIndex;

		//Show the Popup to select the mouse button 
		gacSettings.resolutionIndex = EditorGUI.Popup(new Rect (rectDimensions.x + 400, rectDimensions.y, 280, 20), gacSettings.resolutionIndex, 
		                                              gacSettings.resolutionNames, style);

		//Check if dropdown selection changed
		if(gacSettings.currentIndex != gacSettings.resolutionIndex){
			gacSettings.loadTouchArea = true;
		}

		EditorGUILayout.EndHorizontal();

		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];

			//Check if simulation is and the VAT window refocuses to restore the state of the Touch Areas 
			if(gacSettings.simulate){
				if(!gacSettings.tagWindowReady){
					actSet.showTouchArea = false;
				}else{
					if(actSet.restoreShowState){
						actSet.showTouchArea = actSet.restoreShowState;
						actSet.restoreShowState = false;
					}
				}
			}

			//Make sure dimensions are not smaller than 100 
			if(actSet.touchDimensions.x < 100){
				actSet.touchDimensions.x = 100;
			}
			
			//Make sure dimensions are not smaller than 100 
			if(actSet.touchDimensions.y < 100){
				actSet.touchDimensions.y = 100;
			}
			
			//Check if Touch Area Square is being shown
			if (actSet.showTouchArea){
				
				if(tagWindow == null){
					return;
				}

				//Change the font
				style = new GUIStyle(GAC.gacSkins.GetStyle("box"));
				
				//Check if the mouse is being use
				if (Event.current.isMouse) {
					
					//Check if the mouse is pressed down
					if(Event.current.type == EventType.MouseDown){
						
						if(!actSet.isDragging){
							// is this an actual click into the button?
							actSet.isDragging = actSet.moveRect.Contains(Event.current.mousePosition);
							
						}
						
						//Register a reset position to prevent any small movement glitches
						if(!actSet.isLeft){
							actSet.resetPosition.x = actSet.touchPosition.x;
						}
						
						//Register a reset position to prevent any small movement glitches
						if(!actSet.isTop){
							actSet.resetPosition.y = actSet.touchPosition.y;
						}
						
						//Checking when to trigger each edge of Touch Area Square to drag
						actSet.isLeft = actSet.leftScale.Contains(Event.current.mousePosition);
						actSet.isRight = actSet.rightScale.Contains(Event.current.mousePosition);
						actSet.isTop = actSet.topScale.Contains(Event.current.mousePosition);
						actSet.isBottom = actSet.bottomScale.Contains(Event.current.mousePosition);
						
					}else if(Event.current.type == EventType.MouseUp){//Check if mouse released after pressed
						
						//Reset
						actSet.isDragging = false;
						actSet.isLeft = false;
						actSet.isRight = false;
						actSet.isTop = false;
						actSet.isBottom = false;
						
						
					}else if(Event.current.type == EventType.MouseDrag){//Check if mouse is pressed and being dragged around
						
						//Check if at the TAG window edge and register if not
						if(actSet.touchPosition.x >= 0 && (actSet.touchPosition.x + actSet.touchDimensions.x) <= 
						   680 && actSet.touchPosition.y >= 36 && 
						   (actSet.touchPosition.y + actSet.touchDimensions.y) <= 640){
							
							actSet.atEdge = false;
						}
					}
					
				}
				
				//Make sure the TAG window has been referenced
				if(tagWindow != null){
					
					//Prevent Touch Area Square from being dragged out of TAG window from left
					if(actSet.touchPosition.x < 0){
						actSet.touchPosition.x = 0;
						actSet.atEdge = true;
					}
					
					//Prevent Touch Area Square from being dragged out of TAG window from right
					if((actSet.touchPosition.x + actSet.touchDimensions.x) > 680){
						actSet.touchPosition.x = actSet.touchPosition.x - ((actSet.touchDimensions.x + actSet.touchPosition.x) - (680 - 1));
						actSet.atEdge = true;
					}

					//Prevent Touch Area Square from being dragged out of TAG window from top
					if(actSet.touchPosition.y < 36){
						actSet.touchPosition.y = 36;
						actSet.atEdge = true;
					}
					
					//Prevent Touch Area Square from being dragged out of TAG window from bottom
					if((actSet.touchPosition.y + actSet.touchDimensions.y) > 640){
						
						actSet.touchPosition.y = 640 - actSet.touchDimensions.y;
						actSet.atEdge = true;
						
					}
					
				}

				//Set the GUI color for what is chosen and reduce the alpha
				actSet.areaColor.a = 0.6f;
				GUI.color = actSet.areaColor;
				
				//Make sure not dragging the Touch Area Square
				if(!actSet.isDragging){
					
					//Draw the Touch Area Square
					GUI.Label(new Rect (actSet.touchPosition.x, actSet.touchPosition.y, actSet.touchDimensions.x, actSet.touchDimensions.y), gacSettings.activatorNames[actIndex] + 
					          System.Environment.NewLine + System.Environment.NewLine + "Touch Area", style);
					
					//Get the Rect of the Touch Area
					actSet.rectPos = new Rect (actSet.touchPosition.x, actSet.touchPosition.y, actSet.touchDimensions.x, actSet.touchDimensions.y);
					
					//Check if the mouse is in the middle of the Touch Area Square
					if(Event.current.mousePosition.x < (actSet.rectPos.width/2 + (actSet.rectPos.x + 9)) && Event.current.mousePosition.x > (actSet.rectPos.width/2 + (actSet.rectPos.x - 9))
					   && Event.current.mousePosition.y < (actSet.rectPos.height/2 + (actSet.rectPos.y + 9)) && Event.current.mousePosition.y > (actSet.rectPos.height/2 + (actSet.rectPos.y - 9))){

						GUI.color = Color.white;

						//A precaution to make sure the icons are all loaded to be used
						if(GAC.images != null){
							//Draw Selected Move icon to show that the mouse is over
							GUI.DrawTexture(new Rect ((actSet.rectPos.width/2 + (actSet.rectPos.x - 9)), (actSet.rectPos.height/2 + (actSet.rectPos.y - 9)), GAC.images.tagSelectedMove.width, GAC.images.tagSelectedMove.height), GAC.images.tagSelectedMove);
							
							//Get the Rect of the Move GUI
							actSet.moveRect = new Rect ((actSet.rectPos.width/2 + (actSet.rectPos.x - 9)), (actSet.rectPos.height/2 + (actSet.rectPos.y - 9)), GAC.images.tagSelectedMove.width, GAC.images.tagSelectedMove.height);
						}
						
					}else{
						
						//Check if the mouse is at the right side of Square
						if(Event.current.mousePosition.x > actSet.rectPos.xMax - 3 && Event.current.mousePosition.x <= actSet.rectPos.xMax + 1){
							
							//Then set right side dimensions
							actSet.rightScale = new Rect(actSet.rectPos.xMax - 2,actSet.rectPos.yMin, 6, actSet.rectPos.height);
						}
						
						//Check if the mouse is at the left side of Square
						if(Event.current.mousePosition.x > actSet.rectPos.x - 1 && Event.current.mousePosition.x <= actSet.rectPos.x + 3){
							
							//Then set left side dimensions
							actSet.leftScale = new Rect(actSet.rectPos.x - 2,actSet.rectPos.yMin, 6, actSet.rectPos.height);
						}
						
						//Check if the mouse is at the bottom side of Square
						if(Event.current.mousePosition.y > actSet.rectPos.yMax - 3 && Event.current.mousePosition.y <= actSet.rectPos.yMax + 1){
							
							//Then set bottom side dimensions
							actSet.bottomScale = new Rect(actSet.rectPos.xMin, actSet.rectPos.yMax - 2, actSet.rectPos.width, 6);
						}
						
						//Check if the mouse is at the top side of Square
						if(Event.current.mousePosition.y > actSet.rectPos.y - 1 && Event.current.mousePosition.y <= actSet.rectPos.y + 3){
							
							//Then set top side dimensions
							actSet.topScale = new Rect(actSet.rectPos.xMin,actSet.rectPos.y - 2, actSet.rectPos.width, 6);
						}
						
						//Check if dragging the left side of the Square
						if(actSet.isLeft){
							
							//Make sure mouse is being dragged to the right
							if(Event.current.mousePosition.x > actSet.rectPos.x){
								
								//Make the dimensions smaller
								actSet.touchDimensions.x = actSet.rectPos.xMax - Event.current.mousePosition.x;
								
								//Make sure the dimensions is above 100
								if(actSet.touchDimensions.x > 100){
									
									//Then adjust the position so the Square doesn't move from it's set touch position
									actSet.touchPosition.x = Event.current.mousePosition.x;
								}else{
									
									//Reset the position
									actSet.touchPosition.x = actSet.resetPosition.x;
								}
								
							}else{//Otherwise if being dragged to the left
								
								//Make sure to stop adjusting size of Square if at the edge of TAG window
								if(Event.current.mousePosition.x > 0){
									
									//Make the dimensions larger
									actSet.touchDimensions.x = (actSet.touchDimensions.x + actSet.rectPos.xMax) - (actSet.touchDimensions.x + Event.current.mousePosition.x);
									
									//Adjust the position so the Square doesn't move from it's set touch position
									actSet.touchPosition.x = Event.current.mousePosition.x;
								}
								
							}
							
						}
						
						//Check if dragging the left side of the Square
						if(actSet.isRight){
							
							//Make sure mouse is being dragged to the right
							if(Event.current.mousePosition.x > actSet.rectPos.xMax){
								
								//Make sure to stop adjusting size of Square if at the edge of TAG window
								if(Event.current.mousePosition.x < tagWindow.position.xMax){
									
									//Make the dimensions larger
									actSet.touchDimensions.x = actSet.touchDimensions.x + (Event.current.mousePosition.x - actSet.rectPos.xMax);
								}
							}else{//Otherwise if being dragged to the left
								
								//Make the dimensions smaller
								actSet.touchDimensions.x = actSet.touchDimensions.x - (actSet.rectPos.xMax - Event.current.mousePosition.x);
								
							}
							
						}
						
						//Check if dragging the top side of the Square
						if(actSet.isTop){
							
							//Make sure mouse is being dragged down
							if(Event.current.mousePosition.y > actSet.rectPos.y){
								
								//Make the dimensions smaller
								actSet.touchDimensions.y = actSet.rectPos.yMax - Event.current.mousePosition.y;
								
								//Make sure the dimensions is above 100
								if(actSet.touchDimensions.y > 100){
									
									//Then adjust the position so the Square doesn't move from it's set touch position
									actSet.touchPosition.y = Event.current.mousePosition.y;
								}else{
									
									//Reset the position
									actSet.touchPosition.y = actSet.resetPosition.y;
								}
							}else{//Otherwise if being dragged to up
								
								//Make sure to stop adjusting size of Square if at the edge of TAG window
								if(Event.current.mousePosition.y > 2){
									
									//Make the dimensions larger
									actSet.touchDimensions.y = (actSet.touchDimensions.y + actSet.rectPos.yMax) - (actSet.touchDimensions.y + Event.current.mousePosition.y);
									
									//Adjust the position so the Square doesn't move from it's set touch position
									actSet.touchPosition.y = Event.current.mousePosition.y;
								}
							}
							
						}
						
						//Check if dragging the bottom side of the Square
						if(actSet.isBottom){
							
							//Make sure mouse is being dragged down
							if(Event.current.mousePosition.y > actSet.rectPos.yMax){
								
								//Make sure to stop adjusting size of Square if at the edge of TAG window
								if(Event.current.mousePosition.y < tagWindow.position.yMax){
									
									//Make the dimensions larger
									actSet.touchDimensions.y = actSet.touchDimensions.y + (Event.current.mousePosition.y - actSet.rectPos.yMax);
								}
							}else{//Otherwise if being dragged up
								
								//Make the dimensions smaller
								actSet.touchDimensions.y = actSet.touchDimensions.y - (actSet.rectPos.yMax - Event.current.mousePosition.y);
								
							}
							
						}
						
						//Draw the Resize Cursor on each end of Square line
						EditorGUIUtility.AddCursorRect (new Rect(actSet.rectPos.xMax,actSet.rectPos.yMin, 5, actSet.rectPos.height), MouseCursor.ResizeHorizontal);	
						EditorGUIUtility.AddCursorRect (new Rect(actSet.rectPos.xMin,actSet.rectPos.yMin, 5, actSet.rectPos.height), MouseCursor.ResizeHorizontal);
						EditorGUIUtility.AddCursorRect (new Rect(actSet.rectPos.xMin,actSet.rectPos.yMax, actSet.rectPos.width, 5), MouseCursor.ResizeVertical);	
						EditorGUIUtility.AddCursorRect (new Rect(actSet.rectPos.xMin,actSet.rectPos.yMin, actSet.rectPos.width, 5), MouseCursor.ResizeVertical);

						GUI.color = Color.white;

						//A precaution to make sure the icons are all loaded to be used
						if(GAC.images != null){
							//Draw Move GUI in middle
							GUI.DrawTexture(new Rect ((actSet.rectPos.width/2 + (actSet.rectPos.x - 9)), (actSet.rectPos.height/2 + (actSet.rectPos.y - 9)), GAC.images.tagDefaultMove.width, GAC.images.tagDefaultMove.height), GAC.images.tagDefaultMove);
						}
					}
				}else{
					
					//Check if the Touch Area Square is at the Edge of Screen View
					if(!actSet.atEdge){
						
						//Get the Rect of the Touch Area with mouse movement
						actSet.rectPos = new Rect (Event.current.mousePosition.x - (actSet.rectPos.width/2), Event.current.mousePosition.y - (actSet.rectPos.height/2), actSet.touchDimensions.x, actSet.touchDimensions.y);
						
						//Draw the Touch Area Square
						GUI.Label(new Rect (actSet.rectPos.x, actSet.rectPos.y, actSet.touchDimensions.x, actSet.touchDimensions.y),gacSettings.activatorNames[actIndex] + 
						          System.Environment.NewLine + System.Environment.NewLine +"Touch Area", style);

						GUI.color = Color.white;

						//A precaution to make sure the icons are all loaded to be used
						if(GAC.images != null){

							//Draw Drag Move icon to show that the mouse is over
							GUI.DrawTexture(new Rect (Event.current.mousePosition.x - 14, Event.current.mousePosition.y - 14, GAC.images.tagDragMove.width, GAC.images.tagDragMove.height), GAC.images.tagDragMove);
						}
						
						actSet.touchPosition = new Vector2(actSet.rectPos.x, actSet.rectPos.y);
						
					}else{
						
						//Draw the Touch Area Square
						GUI.Label(new Rect (actSet.touchPosition.x, actSet.touchPosition.y, actSet.touchDimensions.x, actSet.touchDimensions.y),gacSettings.activatorNames[actIndex] + 
						          System.Environment.NewLine + System.Environment.NewLine +"Touch Area", style);

						GUI.color = Color.white;

						//A precaution to make sure the icons are all loaded to be used
						if(GAC.images != null){
							//Draw Drag Move icon to show that the mouse is over
							GUI.DrawTexture(new Rect ((actSet.rectPos.width/2) + (actSet.touchPosition.x - 14), (actSet.rectPos.height/2) + (actSet.touchPosition.y - 14), GAC.images.tagDragMove.width, GAC.images.tagDragMove.height), GAC.images.tagDragMove);
							
						}
					}
					
				}

				//Get the mid of the current Screens' resolution 
				resolutionMidWidth = gacSettings.theResolution.x/2;
				resolutionMidHeight = gacSettings.theResolution.y/2;

				//Change Line color to green
				Handles.color = Color.green;
				
				GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
				labelStyle.normal.textColor = Color.green;
				labelStyle.alignment = TextAnchor.MiddleCenter;
				labelStyle.fontStyle = FontStyle.Bold;
				GUI.color = Color.green;
				
				#if UNITY_STANDALONE
				if(gacSettings.inGameResolutionIndex > -1){
					
					standSet = gacSettings.standaloneSavedSlots[gacSettings.inGameResolutionIndex];
					
					standSet.resOrigin = new Vector2(vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight);
					
					//Draw the Touch Area Square
					GUI.Label(new Rect (vatMidWidth - 75, vatMidHeight - resolutionMidHeight, 150, 50), "Resolution Frame", labelStyle);
					
					//GUI.Label(new Rect (vatMidWidth - 200, vatMidHeight - resolutionMidHeight + 20, 400, 50), standSet.resOrigin +
					  //        " " + actSet.relativePosition + " " + tagWindow.position, labelStyle);
					GUI.Label(new Rect (vatMidWidth -  150, vatMidHeight - resolutionMidHeight + 20, 300, 50), standSet.resolutionName, 
					          labelStyle);

				}
				#endif
				
				#if UNITY_IOS
				
				if( gacSettings.inGameResolutionIndex > -1){
					
					iOSSet = gacSettings.iosSavedSlots[gacSettings.inGameResolutionIndex];

					iOSSet.resOrigin = new Vector2(vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight);
					
					//Draw the Touch Area Square
					GUI.Label(new Rect (vatMidWidth - 50, vatMidHeight - resolutionMidHeight, 100, 50), "Resolution Frame", labelStyle);
					
					//GUI.Label(new Rect (vatMidWidth - (30 + "Resolution Frame".Length), vatMidHeight - resolutionMidHeight + 20, 300, 50), iOSSet.resOrigin +
					//    " " + actSet.relativePosition, labelStyle);
					GUI.Label(new Rect (vatMidWidth -  150, vatMidHeight - resolutionMidHeight + 20, 300, 50), iOSSet.resolutionName, 
					          labelStyle);
					
				}
				#endif

				#if UNITY_ANDROID
				
				if( gacSettings.inGameResolutionIndex > -1){
					
					androidSet = gacSettings.androidSavedSlots[gacSettings.inGameResolutionIndex];

					androidSet.resOrigin = new Vector2(vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight);
					
					//Draw the Touch Area Square
					GUI.Label(new Rect (vatMidWidth - 50, vatMidHeight - resolutionMidHeight, 100, 50), "Resolution Frame", labelStyle);
					
					//GUI.Label(new Rect (vatMidWidth - (30 + "Resolution Frame".Length), vatMidHeight - resolutionMidHeight + 20, 300, 50), iOSSet.resOrigin +
					//    " " + actSet.relativePosition, labelStyle);
					GUI.Label(new Rect (vatMidWidth -  225, vatMidHeight - resolutionMidHeight + 20, 450, 50), androidSet.resolutionName, 
					          labelStyle);

					
				}
				#endif
				////DRAW A SQUARE FOR THE SCREEN RESOLUTION USING LINES-STARTS FROM HALF(MID) SCREEN POSITIONS TO END; 2 LINES CREATE THE FULL LINE////
				
				//Top Line
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight - resolutionMidHeight, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight - resolutionMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight - resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight - resolutionMidHeight + 1, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight + 1, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight - resolutionMidHeight + 1, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight - resolutionMidHeight + 1, 0));

				//Bottom Line
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight + resolutionMidHeight, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight + resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight + resolutionMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight + resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight + resolutionMidHeight + 1, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight + resolutionMidHeight + 1, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth, vatMidHeight + resolutionMidHeight + 1, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight + resolutionMidHeight + 1, 0));

				//Left Line
				Handles.DrawLine(new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight + resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight, 0), new Vector3 (vatMidWidth - resolutionMidWidth, vatMidHeight - resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth - resolutionMidWidth + 1, vatMidHeight + 1, 0), new Vector3 (vatMidWidth - resolutionMidWidth + 1, vatMidHeight + resolutionMidHeight + 1, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth - resolutionMidWidth + 1, vatMidHeight + 1, 0), new Vector3 (vatMidWidth - resolutionMidWidth + 1, vatMidHeight - resolutionMidHeight + 1, 0));

				//Right Line
				Handles.DrawLine(new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight + resolutionMidHeight + 2, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth, vatMidHeight - resolutionMidHeight, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth + resolutionMidWidth - 1, vatMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth - 1, vatMidHeight + resolutionMidHeight + 2, 0));
				Handles.DrawLine(new Vector3 (vatMidWidth + resolutionMidWidth - 1, vatMidHeight, 0), new Vector3 (vatMidWidth + resolutionMidWidth - 1, vatMidHeight - resolutionMidHeight, 0));

				////DRAW A SQUARE FOR THE SCREEN RESOLUTION USING LINES////
				
				//Handles.EndGUI();
				
				
			}
		}

		tagWindow.Repaint();
	}

	void SaveTouchAreas(){

		#if UNITY_STANDALONE
		
		GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.resolutionIndex];
		
		//Reset the activator slots for this Saved Setting
		standSet.actSlots.Clear();
		
		for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
			GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
			
			GAC_ActivatorSetup newActSet = new GAC_ActivatorSetup();

			if(actSet.useTouch){
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
			}


			//There has to be at least one slot added before setting trigger to save
			if(standSet.actSlots.Count > 0){
				
				//Wait till the last index
				if(actIndex == gacSettings.activatorSlots.Count - 1){
					standSet.saved = true;
					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + standSet.resolutionName + " with "+ standSet.actSlots.Count + " activator slots...");
				}
			}

			//Wait till the last index
			if(actIndex == gacSettings.activatorSlots.Count - 1){
				//There has to be at least one slot added before setting trigger to save
				if(standSet.actSlots.Count == 0){
					Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
				}
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

			if(actSet.useTouch){
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
			}

			//There has to be at least one slot added before setting trigger to save
			if(iosSet.actSlots.Count > 0){
				
				//Wait till the last index
				if(actIndex == gacSettings.activatorSlots.Count - 1){
					iosSet.saved = true;
					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + iosSet.resolutionName + " with "+ iosSet.actSlots.Count + " activator slots...");
				}
			}
			
			//Wait till the last index
			if(actIndex == gacSettings.activatorSlots.Count - 1){
				//There has to be at least one slot added before setting trigger to save
				if(iosSet.actSlots.Count == 0){
					Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
				}
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

			if(actSet.useTouch){
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

						//Then add the activator to save it for this resolution setting
						androidSet.actSlots.Add (newActSet);

					}else{
						Debug.LogWarning("GACLog - " + actSet.name + " wasn't saved because it was not 'Set' to use. Please 'Set' this activator before trying to save it!");
					}
				}
			}

			//There has to be at least one slot added before setting trigger to save
			if(androidSet.actSlots.Count > 0){
				
				//Wait till the last index
				if(actIndex == gacSettings.activatorSlots.Count - 1){
					androidSet.saved = true;
					Debug.Log("GACLog - GAC saved Touch Area Settings for resolution " + androidSet.resolutionName + " with "+ androidSet.actSlots.Count + " activator slots...");
				}
			}
			
			//Wait till the last index
			if(actIndex == gacSettings.activatorSlots.Count - 1){
				//There has to be at least one slot added before setting trigger to save
				if(androidSet.actSlots.Count == 0){
					Debug.LogWarning("GACLog - There were no slots saved! Make sure at least one Touch Activator is 'Set' before trying to save again!");
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

	void TagTips(int index){

		GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
		labelStyle.normal.textColor = Color.green;
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		GUI.color = Color.green;

		if(index == 1){
			GUI.Label(new Rect (vatMidWidth -  305, 600, 610, 50), "TIP - Make sure to click the 'SAVE' icon above to see the 'Touch Areas' in " +
			          "Game View/Build!", 
			          labelStyle);
		}else if(index == 2){
			GUI.Label(new Rect (vatMidWidth -  305, 600, 610, 50), "TIP - The TAG window cannot be closed using the normal window's close button, use the 'POWER' " +
			          System.Environment.NewLine + "icon above instead!", labelStyle);
		}else if(index == 3){
			GUI.Label(new Rect (vatMidWidth -  305, 600, 610, 50), "TIP - Need to properly view the resolution selected in TAG dropdown? Make sure to create custom" +
			          System.Environment.NewLine + "resolutions for the Game View that match the re-scaled resolution listed in the brackets i.e. (568x320)!", 
			          labelStyle);
		}else if(index == 4){
			GUI.Label(new Rect (vatMidWidth -  305, 600, 610, 50), "TIP - Remember to SAVE, SAVE, SAVE your Touch Area resolution settings! If you selected another " +
			          System.Environment.NewLine + "resolution from the drop down, all changes made to Touch Activator slots will be lost!", 
			          labelStyle);
		}else if(index == 5){
			GUI.Label(new Rect (vatMidWidth -  305, 600, 610, 50), "TIP - The 'REFRESH' icon above is used to clear all settings saved in the selected resolution in " +
				"the drop down menu!", 
			          labelStyle);
		}
		GUI.color = Color.white;
	}
}
