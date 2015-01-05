using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//Version 1.3
//GAC_AnimationSetup.cs
/////////////////////////////////////////////////////////////////////////////////////////

[Serializable] 
public class GAC_AnimationSetup{

	public string theAnim; //The name of the animation
	public bool useAsStarter; //Should animation be used to start a combo

	public List<string> animNames = new List<string>(); //Animation names list
	public List<int> activators = new List<int>();//Number used to trigger the animation

	public int activatorAmounts;//Reference of the amount of activators available

	public bool appliedAnim;
	public int currentAnim; //The current animation selected for the slot
	public LayerMask affectLayer; //The layer to animation can affect
	public float affectDistance; //The distance the animation can affect
	public float affectAngle; //The angle the animation can affect
	public float animTime; //The current time of the animation clip
	public float animEndLength; //Set the time the animation clip should end
	public float prevLength;
	public Color gizmoColor = new Color(0, 0, 0, 1); //The gizmo color box

	public List<GameObject> layerObjects = new List<GameObject>(); //Keeps all the objects to affect with this animation in the specific layers

	public float moveAmountX; //The amount to move the gameobject in X axis
	public float moveAmountY; //The amount to move the gameobject in X axis
	public float moveAmountZ; //The amount to move the gameobject in X axis
	
	//These are used to check within ranges to trigger something
	public float delayBegin;
	public bool delayToggle;
	public float delayEnd;
	public float hitBegin;
	public bool hitToggle;
	public float hitEnd;
	public float hitKnockBackX;
	public float hitKnockBackY;
	public float hitKnockBackZ;
	public bool heightToggle;
	public float linkBegin;
	public float linkEnd;	
	public float moveBegin;
	public bool moveToggle;
	public float moveEnd;
	public bool showMoves = true;
	public bool showKnockBacks = true;
	
	public List<float> moveValues = new List<float>();

	public bool delayTiming; //Used to start the delay count down
	public bool delayMode; //Used to trigger that delay animations can be used in current combo
	public float delayCountDown; //The current delay time counting down

	//Used for checking the context value modification
	public bool isDraggingDistance;
	public bool isDraggingAngle;
	public bool isDraggingMoveX;
	public bool isDraggingMoveY;
	public bool isDraggingMoveZ;
	public bool isDraggingKnockBackX;
	public bool isDraggingKnockBackY;
	public bool isDraggingKnockBackZ;
	public bool isDraggingHeight;

	public float angleHeight;
	public Vector2 anglePosition;
	public bool showAnim = true; //The animation slot fold out
	public bool gizmoFocus; //Show the gizmo for this animation slot
	public bool isPlaying; //Trigger to check if animation clip is playing



	[HideInInspector]
	public bool tweakInput;//To trigger when input time should be tweaked

	public PlayMode playMode;
	public float blendTime;

	public enum PlayMode{
		Normal,
		CrossFade
	}
}