using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.5
//GAC_SequenceSetup.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_SequenceSetup{
	
	public bool isSet;//Trigger for when a sequence slot is set
	
	//Triggers to check when different input methods are chosen
	public bool useKey;
	public bool useMouse;
	public bool useButton;
	public bool useSync;
	public bool useDFGUI;

	public float timeSet; //Set the time between button presses

	public int syncSlotIndex; //Index of current sync slot selected
	public List<string> syncSlotNames = new List<string>(); //List of all the names of the sync slots inputs
	public int setSyncReferenceIndex;

	public KeyCode keyInput = KeyCode.None; //The keycode list
	
	public int mouseIndex; //The index of the selection for mouse dropdown
	public int[] mouseInput = {0,1,2}; //Mouse input options
	public string[] mouseInputNames = {"Left","Right","Middle"};//The strings for mouse input options

	public int stateIndex; //The index of the selection for event state (eg.GetKey,GetKeyDown,GetKeyUp)
	public int[] stateInput = {0,1,2};//State input options
	public string[] stateInputNames = {"Default","Down","Up"};//The strings for state input options

	public string inputText = "The Input"; //Keeps reference to the input name to use from Input Manager
	public string inputTextY = "Input Y"; //Keeps reference to the input name to use from Input Manager
	public int inputIndex; //The index of the selection for Unity Input
	public string[] inputTypeNames = {"Sticks","Buttons"};//The strings for Unity type input options
	
	public int directionIndex; //Index of current direction for analog and sticks
	
	//The strings for directional options
	public string[] directionNames = 
	{	 
		"Up",
		"Up Left",
		"Up Right",
		"Left",
		"Right",
		"Down",
		"Down Left",
		"Down Right"
		
	};

	//FOR DFGUI USE//
	public GameObject inputObject;
	public int eventIndex;//List of indexes for activators to use in a popup
	public int memberIndex; //The index of the selection for DFGUI events
	public string[] sourceComponentMembers;//String list of all the DFGUI events
	//END FOR DFGUI USE//

	//FOR INCONTROL USE
	
	//Triggers to check when different InControl input methods are chosen
	public bool useSticks;
	public bool useDPad;
	public bool useActions;
	public bool useTriggers;
	public bool useBumpers;
	public bool useUnsupportedButtons;
	public bool useUnsupportedAnalogs;
	public bool useInCtrlSyncs;
	
	public int addedWidth; //Extra width to add when control sticks are not selected as inputs
	public int addedVert; //Extra width to add when control sticks are not selected as inputs

	public int inCtrlStickIndex;
	
	//The strings for InControl directional options
	public string[] inCtrlStickNames = 
	{	
		"Left Stick",
		"Right Stick",
		"Left Stick Button",
		"Right Stick Button"
		
	};
	
	public int inCtrlActionIndex;
	
	//The strings for InControl directional options
	public string[] inCtrlActionNames = 
	{	
		"Action 1",
		"Action 2",
		"Action 3",
		"Action 4"
		
	};
	
	public int inCtrlTriggerIndex;
	
	//The strings for InControl directional options
	public string[] inCtrlTriggerNames = 
	{	
		"Left Trigger",
		"Right Trigger"
		
	};
	
	public int inCtrlBumperIndex;
	
	//The strings for InControl directional options
	public string[] inCtrlBumperNames = 
	{	
		"Left Bumper",
		"Right Bumper"
		
	};
	
	public int inCtrlUnsupportedAnalogsIndex1; //Index of current inContrl unsupported input
	public int inCtrlUnsupportedAnalogsIndex2; //Index of current inContrl unsupported input
	
	//The strings for InControl input options
	public string[] inCtrlUnsupportedAnalogsNames = 
	{	
		"Analog0 ",
		"Analog1 ",
		"Analog2 ",
		"Analog3 ",
		"Analog4 ",
		"Analog5 ",
		"Analog6 ",
		"Analog7 ",
		"Analog8 ",
		"Analog9 ",
		"Analog10 ",
		"Analog11 ",
		"Analog12 ",
		"Analog13 ",
		"Analog14 ",
		"Analog15 ",
		"Analog16 ",
		"Analog17 ",
		"Analog18 ",
		"Analog19 "
		
	};

	public int inCtrlUnsupportedButtonsIndex; //Index of current inContrl unsupported input
	
	//The strings for InControl input options
	public string[] inCtrlUnsupportedButtonsNames = 
	{	
		"Button0",
		"Button1",
		"Button2",
		"Button3",
		"Button4",
		"Button5",
		"Button6",
		"Button7",
		"Button8",
		"Button9",
		"Button10",
		"Button11",
		"Button12",
		"Button13",
		"Button14",
		"Button15",
		"Button16",
		"Button17",
		"Button18",
		"Button19"
		
	};

	
	//END INCONTROL USE
}
