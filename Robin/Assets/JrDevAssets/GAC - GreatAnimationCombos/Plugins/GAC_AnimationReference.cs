using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.3
//GAC_AnimationReference.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_AnimationReference{

	public string starterName; //The name of the starter animation for the combo	
	public int activator; //The activator for this animation
	public string animName; //The animation in the combo
	public int sequence; //The sequence for this  animation in the combo
	public bool delayed;

}
