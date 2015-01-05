using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JrDevAssets;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.6
//GAC_InEditorTouchAreas.cs
/////////////////////////////////////////////////////////////////////////////////////////
[ExecuteInEditMode]
public class GAC_InEditorTouchAreas : MonoBehaviour {

	private GAC gacSettings;

	void OnGUI(){

		if(gacSettings == null){

			//Reference the GAC Script
			gacSettings = gameObject.GetComponent<GAC>();

		//Check if referenced
		}else if(gacSettings != null){

			#if UNITY_EDITOR
			//Used the current resolution index and the in game index to choose
			gacSettings.inGameResolutionIndex = gacSettings.resolutionIndex;

			#endif

			//Make sure the resolution index is valid
			if(gacSettings.inGameResolutionIndex > -1){

				#if UNITY_STANDALONE
				if(gacSettings.standaloneSavedSlots.Count > 0){
					GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.inGameResolutionIndex];

					//Make sure this was saved first
					if(standSet.saved){

						for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
							
							for (int i = 0; i < standSet.actSlots.Count; i++) {
								
								//Compare the references for each index to make sure they match
								if(standSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){

									//Check if Touch Area Square is being shown
									if (standSet.actSlots[i].showTouchArea){
									
										GAC.gacSkins = Resources.Load("GAC_Skins",typeof(GUISkin)) as GUISkin;
										
										if(GAC.gacSkins != null){

											//Change the style
											GUIStyle style = new GUIStyle(GAC.gacSkins.GetStyle("box"));

											//Set the GUI color for what is chosen and reduce the alpha
											standSet.actSlots[i].areaColor.a = 0.7f;
											GUI.color = standSet.actSlots[i].areaColor;

											//Draw the Box in the game view
											GUI.Label(new Rect (standSet.actSlots[i].relativePosition.x, standSet.actSlots[i].relativePosition.y, 
								                    	standSet.actSlots[i].relativeScale.x, standSet.actSlots[i].relativeScale.y), standSet.actSlots[i].name + 
									          			System.Environment.NewLine + "Touch Area", style);

										}
									}
								}
							}
						}
					}
				}
				#endif

				#if UNITY_IOS

				if(gacSettings.iosSavedSlots.Count > 0){
					GAC_SavedTouchArea iOSSet = gacSettings.iosSavedSlots[gacSettings.inGameResolutionIndex];

					//Make sure this was saved first
					if(iOSSet.saved){
						
						for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];
							
							for (int i = 0; i < iOSSet.actSlots.Count; i++) {
								
								//Compare the references for each index to make sure they match
								if(iOSSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){
								
									//Check if Touch Area Square is being shown
									if (iOSSet.actSlots[i].showTouchArea){
										
										GAC.gacSkins = Resources.Load("GAC_Skins",typeof(GUISkin)) as GUISkin;
										
										if(GAC.gacSkins != null){
											
											//Change the style
											GUIStyle style = new GUIStyle(GAC.gacSkins.GetStyle("box"));
											
											//Set the GUI color for what is chosen and reduce the alpha
											iOSSet.actSlots[i].areaColor.a = 0.7f;
											GUI.color = iOSSet.actSlots[i].areaColor;
											
											//Draw the Box in the game view
											GUI.Label(new Rect (iOSSet.actSlots[i].relativePosition.x, iOSSet.actSlots[i].relativePosition.y, 
								                    	iOSSet.actSlots[i].relativeScale.x, iOSSet.actSlots[i].relativeScale.y), iOSSet.actSlots[i].name + 
											          	System.Environment.NewLine + "Touch Area", style);
		
										}
									}
								}
							}
						}
					}
				}
				#endif

				#if UNITY_ANDROID

				if(gacSettings.androidSavedSlots.Count > 0){
					GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.inGameResolutionIndex];
					
					//Make sure this was saved first
					if(androidSet.saved){
						
						for (int actIndex = 0; actIndex < gacSettings.activatorSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = gacSettings.activatorSlots[actIndex];

							for (int i = 0; i < androidSet.actSlots.Count; i++) {

								//Compare the references for each index to make sure they match
								if(androidSet.actSlots[i].touchReferenceIndex == actSet.touchReferenceIndex){

									//Check if Touch Area Square is being shown
									if (androidSet.actSlots[i].showTouchArea){
										
										GAC.gacSkins = Resources.Load("GAC_Skins",typeof(GUISkin)) as GUISkin;
										
										if(GAC.gacSkins != null){
											
											//Change the style
											GUIStyle style = new GUIStyle(GAC.gacSkins.GetStyle("box"));
											
											//Set the GUI color for what is chosen and reduce the alpha
											androidSet.actSlots[i].areaColor.a = 0.7f;
											GUI.color = androidSet.actSlots[i].areaColor;
											
											//Draw the Box in the game view
											GUI.Label(new Rect (androidSet.actSlots[i].relativePosition.x, androidSet.actSlots[i].relativePosition.y, 
									               	androidSet.actSlots[i].relativeScale.x, androidSet.actSlots[i].relativeScale.y), androidSet.actSlots[i].name + 
										          	System.Environment.NewLine + "Touch Area", style);
					
										}
									}
								}
							
							}
						}
					}
				}
				#endif

			}
		}
	}

	void Update(){

		//Reference the GAC Script
		GAC gacSettings = gameObject.GetComponent<GAC>();

		//Check if referenced
		if(gacSettings != null){

			//Convert from arrays to list
			gacSettings.resolutionNamesList = new List<string>(gacSettings.resolutionNames);

			//Search for a match to the current resolution from the list
			List<string> currentResolutionFound = gacSettings.resolutionNamesList.FindAll(delegate(string s) { return s.Contains(Screen.width + "x" + Screen.height); });

			//If there was a match
			if (currentResolutionFound.Count > 0){

				//Then register as the in game resolution index to use
				gacSettings.inGameResolutionIndex = gacSettings.resolutionNamesList.IndexOf(currentResolutionFound[0]);
			}else{
				//Otherwise reigster it out of index
				gacSettings.inGameResolutionIndex = -1;
			}


			#if UNITY_EDITOR
			//Make sure the TAG simulation is turned on
			if(gacSettings.simulate){
				gacSettings.inGameResolutionIndex = gacSettings.resolutionIndex;

			}
			#endif

			//Make sure the resolution index is valid
			if(gacSettings.inGameResolutionIndex > -1){

				#if UNITY_STANDALONE
				if(gacSettings.standaloneSavedSlots.Count > gacSettings.inGameResolutionIndex){
					GAC_SavedTouchArea standSet = gacSettings.standaloneSavedSlots[gacSettings.inGameResolutionIndex];

					if(standSet.saved){
						for (int actIndex = 0; actIndex < standSet.actSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = standSet.actSlots[actIndex];

							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * (actSet.touchPosition.x - 
	                                                         		standSet.resOrigin.x), gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * 
							                                     	(actSet.touchPosition.y - standSet.resOrigin.y));

							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.x, 
							                                   gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.y);

							#if UNITY_EDITOR
							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(actSet.touchPosition.x - standSet.resOrigin.x, actSet.touchPosition.y - standSet.resOrigin.y);
							
							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);
								
							#endif

						}
					}
				}
				#endif
				
				#if UNITY_IOS
				if(gacSettings.iosSavedSlots.Count > gacSettings.inGameResolutionIndex){
					GAC_SavedTouchArea iOSSet = gacSettings.iosSavedSlots[gacSettings.inGameResolutionIndex];

					if(iOSSet.saved){
						for (int actIndex = 0; actIndex < iOSSet.actSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = iOSSet.actSlots[actIndex];
							
							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * (actSet.touchPosition.x - 
							                                                                                                              iOSSet.resOrigin.x), gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * 
							                                      (actSet.touchPosition.y - iOSSet.resOrigin.y));
							
							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.x, 
							                                   gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.y);
							
							#if UNITY_EDITOR
							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(actSet.touchPosition.x - iOSSet.resOrigin.x, actSet.touchPosition.y - iOSSet.resOrigin.y);
							
							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);
							
							#endif

						}
					}
				}
				#endif
				
				#if UNITY_ANDROID
				if(gacSettings.androidSavedSlots.Count > gacSettings.inGameResolutionIndex){
					GAC_SavedTouchArea androidSet = gacSettings.androidSavedSlots[gacSettings.inGameResolutionIndex];
					
					if(androidSet.saved){
						for (int actIndex = 0; actIndex < androidSet.actSlots.Count; actIndex++) {
							GAC_ActivatorSetup actSet = androidSet.actSlots[actIndex];
							
							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * (actSet.touchPosition.x - 
	                                                              androidSet.resOrigin.x), gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * 
							                                      (actSet.touchPosition.y - androidSet.resOrigin.y));
							
							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.x, 
							                                   gacSettings.resolutionScaleFactor[gacSettings.inGameResolutionIndex] * actSet.touchDimensions.y);
							
							#if UNITY_EDITOR
							//Get the relative position from the resolutions between Scene and Game view
							actSet.relativePosition = new Vector2(actSet.touchPosition.x - androidSet.resOrigin.x, actSet.touchPosition.y - androidSet.resOrigin.y);
							
							//Get the relative scale from the resolutions between Scene and Game view
							actSet.relativeScale = new Vector2(actSet.touchDimensions.x, actSet.touchDimensions.y);

							#endif

						}
					}
				}
				#endif
			}
		}
	}
}

