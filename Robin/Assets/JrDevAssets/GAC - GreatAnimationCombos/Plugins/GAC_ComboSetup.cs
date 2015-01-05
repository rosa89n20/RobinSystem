using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.3
//GAC_ComboSetup.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_ComboSetup{

	public bool showCombo; //Use for checking if the drop down is open
	public bool nameSet = true;
	public string comboName = "";

	public List<string> animNames = new List<string>();	//List of animation names
	public List<int> animSpot = new List<int>();	//Animation spot list to use as a popup index
	public List<int> activatorIndex = new List<int>();//List of indexes for activators to use in a popup
	public List<int> comboSequence = new List<int>();	//The sequence number for the animation in the combo
	public List<bool> setAnim = new List<bool>(); //Checks if the animation slot has been set
	public List<bool> buttonShown = new List<bool>(); //Check if buttons have been shown for this slot
	public List<bool> delayedAnim = new List<bool>(); //Check if this is a delayed animation spot

	public List<GAC_AnimationReference> animationReference = new List<GAC_AnimationReference>(); //Keep reference for use in the GAC animation callback system
	public List<GAC_AnimationReference> keepReference = new List<GAC_AnimationReference>();//Keep reference to readd them to the combo lists

	public List<bool> conflicted = new List<bool>(); //Has this animation been conflicted/cause problems with other animations from another combo?

	public List<string> theCombos = new List<string>();	//List of animations in combo including the starter
	public List<string> referenceCombos = new List<string>();	//List of animations reference kept for the combo
	
	public int linkAmount; //Keep track of the amount of links in a combo

	public Rect lastDim; //Keep reference of the dimensions of each GUI 
}
