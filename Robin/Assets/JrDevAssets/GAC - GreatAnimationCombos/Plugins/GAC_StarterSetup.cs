using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.3
//GAC_StarterSetup.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_StarterSetup{

	public string starterName;//Name of this starter animation
	public bool showStarter;
	public bool showActivators; //Trigger to show activators used for this starter
	public bool firstActivatorSet; //Trigger to know when the first activators has already been added to the activatorForStarters list

	public List<GAC_ComboSetup> starterCombos = new List<GAC_ComboSetup>();	//The starter animations list
	public List<Conflicts> conflicts = new List<Conflicts>();	//The starter animations list

	public List<int> normalConflicts = new List<int>();
	public List<int> delayedConflicts = new List<int>();

	public int comboAmount = 0; //Keeps track of the amount of combo attributes that are setup

	public Rect lastDim; //Keep reference of the dimensions of each GUI 

	public List<GAC_ComboSetup> theReferences = new List<GAC_ComboSetup>();
}

//This keeps a reference string for animations that were set in the combo's attributes (activator, name, sequence)
public class Conflicts {
	public List<string> animationsUsed = new List<string>();
	public List<string> normalIndexes = new List<string>();
	public List<string> delayIndexes = new List<string>();
	
}