using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JrDevAssets;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.5
//GAC_SetEvent.cs
/////////////////////////////////////////////////////////////////////////////////////////
/// 
public class GAC_SetEvent : MonoBehaviour {
	[HideInInspector]
	public GameObject target;//Reference to this gameobject

	[HideInInspector]
	public string animName;//The animation name to call

	[HideInInspector]
	public int activator;//The activator to call


	public void PlayAnimation(){
		GAC.PlayTheAnimation(gameObject, animName, activator);
	}

}
