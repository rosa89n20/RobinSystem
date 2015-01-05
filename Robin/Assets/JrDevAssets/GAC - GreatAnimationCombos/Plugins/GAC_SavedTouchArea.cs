using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.6
//GAC_SavedTouchArea.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_SavedTouchArea{

	public string resolutionName;
	public List<GAC_ActivatorSetup> actSlots = new List<GAC_ActivatorSetup>();//The list of all animations
	public List<int> actReference = new List<int>();//The list of all animations
	public Vector2 resOrigin;
	public bool saved;
}
