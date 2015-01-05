using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.7
//GAC_ActivatorSetup.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_ActivatorSetup{

	public GAC_SetEvent evt; //Reference to the GAC event script
	public string name;
	public bool activatorTriggered; //Trigger for when activator is being used
	public int indexBeforeMove;

	public int activatorIndex;//List of indexes for activators to use in a popup
	public int animationIndex;//List of indexes for activators to use in a popup
	public int eventIndex;//List of indexes for activators to use in a popup

	public string inputInitials; //Keeps a string intial of the type of input being used

	public bool activatorSet; //Keeps track of when an activators slot has been set
	public KeyCode keyInput = KeyCode.None; //The keycode list

	public int mouseIndex; //The index of the selection for mouse dropdown
	public int[] mouseInput = {0,1,2}; //Mouse input options
	public string[] mouseInputNames = {"Left","Right","Middle"};//The strings for mouse input options

	public string inputText = "The Input"; //Keeps reference to the input name to use from Input Manager
	public string inputTextY = "The Input Y"; //Keeps reference to the input name to use from Input Manager
	public int inputIndex; //The index of the selection for Unity Input
	public string[] inputTypeNames = {"Sticks","Buttons"};//The strings for Unity type input options
	public int addedWidth; //Extra width to add when control sticks are not selected as inputs

	public int stateIndex; //The index of the selection for event state (eg.GetKey,GetKeyDown,GetKeyUp)
	public int[] stateInput = {0,1,2};//State input options
	public string[] stateInputNames = {"Default","Down","Up"};//The strings for state input options


	//Triggers to check when different input methods are chosen
	public bool useKey;
	public bool useMouse;
	public bool useButton;
	public bool useTouch;
	public bool useSync;
	public bool useSequence;


	//FOR TOUCH INPUT USE
	public bool showTouchArea; //Trigger to hide or show the visual touch area setup in scene
	public bool wasShowing;//Keeps track of show state when not using indexes own settings
	public bool restoreShowState;//Save/Restores the show state when the simuation is turned on/of
	public bool touchedArea;//Keeps track of it the touch area was triggered
	public string setTouchName;//The name of the touch area
	public string touchNameNotSet; //The name of this touch activator when it's not set
	public bool touchInUse; //Is this touch activator being used by another touch activator
	public int setTouchReferenceIndex; //The reference index that was set to use
	public int touchReferenceIndex; //The reference index of this activator
	public Vector2 touchPosition;//The position of the touch area
	public Vector2 touchDimensions;//The dimensions of the touch area
	public float minSwipeLength;//The length set to recognize swipe
	public Vector2 relativePosition;//The relative position to the resolution
	public Vector2 relativeScale;//The relative scale to the resolution
	public Rect areaRect; //The rect of the Touch area
	public Rect rectPos; //The rect of the Touch area for TAG window
	public Rect moveRect; //The rect of the move icon	
	public bool isDragging; //Is the touch area being dragged
	public Rect leftScale;//Scaling touch area to left rect
	public Rect rightScale;//Scaling touch area to right rect
	public Rect topScale;//Scaling touch area to top rect
	public Rect bottomScale;//Scaling touch area to bottom rect
	public bool isLeft; //Is the touch area to the left of TAG window edge?
	public bool isRight;//Is the touch area to the right of TAG window edge?
	public bool isTop;//Is the touch area to the top of TAG window edge?
	public bool isBottom;//Is the touch area to the bottom of TAG window edge?
	public bool atEdge; //Check if at the edge of the TAG window
	public Color areaColor;//Keep the color set for the Touch Area
	public Vector2 resetPosition; //position for Touch area to reset to 

	public int touchIndex; //The index of the selection for mouse dropdown
	public string[] touchAmountNames = {"One","Two","Three","Four","Five"};//The strings for mouse input options
	public int touchSlotIndex; //Index of current touch slot selected

	public KeyCode modifyKey = KeyCode.None; //The keycode list

	public Gestures gestures;
	
	public enum Gestures{
		Up,
		UpLeft,
		UpRight,
		Left,
		Right,
		Down,
		DownLeft,
		DownRight,
		Tap,
		DoubleTap,
		Hold
	}
	//FOR TOUCH INPUT USE

	//FOR SYNC OR SEQUENCE INPUT USE
	public List<GAC_SyncSetup> syncSlots = new List<GAC_SyncSetup>();//The list of all animations
	public List<GAC_SequenceSetup> sequenceSlots = new List<GAC_SequenceSetup>();//The list of all animations

	public int buttonIndex; //Keep track of the index for input triggering
	public int inputCountGoal; //Keep track of the total inputs triggered
	public int syncAmounts; //Keep track of the amount of sync slots
	public int sequenceAmounts; //Keep track of the amount of sequence slots

	public bool singleInputTriggered;//Has this input been triggered with a single input?
	public bool syncInputTriggered; //Has this input been triggered with sync inputs?
	public float timeInput; //Used for countdown for time between single input being triggered and animation being called
	public bool allInputsTriggered; //Have all inputs been triggered?
	public float timeSinceInput; //Keep track of the time since the last input was triggered

	public bool syncWarning; //Warning trigger for when no syncs available to add
	public bool dupeInputWarning; //Warning trigger for when there is already a similar input set in the sync activator
	public string syncedString;//The string of all the inputs synced
	public bool syncInUse; //Set for when this synchro activator is being used in a sequence
	public string sequencedString;//The string of all the inputs in sequence
	public string syncNameNotSet;
	public string setSyncName;
	public int sequenceStringCount;
	public int setSyncReferenceIndex;
	public int syncReferenceIndex;

	public bool showActivator;//Show the activator group
	public bool showSync;//Show the sync group
	public bool showSequence;//Show the sequence group
	public bool sequenceApplied;


	public int[] sequenceStateInput = {1,2};//State input options
	public string[] sequenceStateInputNames = {"Down","Up"};//The strings for state input options

	public List<string> sourceStrings = new List<string>();//Keep a list of the source inputs added for activators (Key,Mouse,Buton etc)
	public List<string> inputStrings = new List<string>();//Keep a list of the inputs added from activators
	public List<bool> inputTriggered = new List<bool>();//Keep a list of the inputs triggers for activators

	public GAC_ActivatorSetup interruptedActivator; //Register the activator index that has been interrupted from single input triggeres

	public SyncSource syncSource;
	
	public enum SyncSource{
		KEY,
		MOUSE,
		BUTTON
	}

	public SequenceSource sequenceSource;
	
	public enum SequenceSource{
		KEY,
		MOUSE,
		BUTTON,
		SYNC
	}
	//END FOR SYNC OR SEQUENCE INPUT USE

	
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

	//Triggers to check when different input methods are chosen
	public bool useDFGUI;
	public bool useDFSync;
	public bool useDFSequence;

	public int memberIndex; //The index of the selection for DFGUI events
	public GameObject inputObject; //The object that is being used for DFGUI
	public string[] sourceComponentMembers;//String list of all the DFGUI events
	public List<GameObject> dfInputs = new List<GameObject>();//Keep a list of the inputs added from activators
	//END FOR DFGUI USE//

	//FOR INCONTROL USE

	//Triggers to check when different input methods are chosen
	public bool useInCtrl;
	public bool useInCtrlUnSupported;
	public bool useInCtrlSync;
	public bool useInCtrlSequence;

	public string inCtrlSyncNameNotSet;

	public int inCtrlIndex; //The index of the selection for mouse dropdown

	public List<string> directionStrings = new List<string>();//Keep a list of the source inputs added for activators (Key,Mouse,Buton etc)

	//The strings for InControl input options
	public string[] inCtrlInputNames = 
	{	
		"Left Stick",
		"Right Stick",
		"Left Stick Button",
		"Right Stick Button",
		"D-Pad",
		"Action 1",
		"Action 2",
		"Action 3",
		"Action 4",
		"Left Trigger",
		"Right Trigger",
		"Left Bumper",
		"Right Bumper"
	
	};

	public int inCtrlUnsupportedIndex; //Index of current inContrl unsupported input
	
	//The strings for InControl input options
	public string[] inCtrlUnsupportedNames = 
	{	
		"Anlg",
		"Buttons"
	};

	public int inCtrlUnsupportedButtonsIndex; //Index of current inContrl unsupported input

	//The strings for InControl input options
	public string[] inCtrlUnsupportedButtonsNames = 
	{	
		"Button 0",
		"Button 1",
		"Button 2",
		"Button 3",
		"Button 4",
		"Button 5",
		"Button 6",
		"Button 7",
		"Button 8",
		"Button 9",
		"Button 10",
		"Button 11",
		"Button 12",
		"Button 13",
		"Button 14",
		"Button 15",
		"Button 16",
		"Button 17",
		"Button 18",
		"Button 19"
		
	};

	public int inCtrlUnsupportedAnalogsIndex1; //Index of current inContrl unsupported input
	public int inCtrlUnsupportedAnalogsIndex2; //Index of current inContrl unsupported input
	
	//The strings for InControl input options
	public string[] inCtrlUnsupportedAnalogsNames = 
	{	
		"Analog0",
		"Analog1",
		"Analog2",
		"Analog3",
		"Analog4",
		"Analog5",
		"Analog6",
		"Analog7",
		"Analog8",
		"Analog9",
		"Analog10",
		"Analog11",
		"Analog12",
		"Analog13",
		"Analog14",
		"Analog15",
		"Analog16",
		"Analog17",
		"Analog18",
		"Analog19"
		
	};


	public InControlSyncSource inCtrlSyncSource;
	
	public enum InControlSyncSource{
		STICKS,
		DPAD,
		ACTIONS,
		TRIGGERS,
		BUMPERS,
		ANALOGSUNSUPPORTED,
		BUTTONSUNSUPPORTED
	}
	
	public InControlSequenceSource inCtrlSequenceSource;
	
	public enum InControlSequenceSource{
		STICKS,
		DPAD,
		ACTIONS,
		TRIGGERS,
		BUMPERS,
		ANALOGSUNSUPPORTED,
		BUTTONSUNSUPPORTED,
		INCTRL_SYNCS
	}
	//END INCONTROL USE
}