using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

//Copyright(c) 2014 Eric Turgott
//Licensed under the Unity Asset Package Product License (the "License");
//JrDevArts_Utilities.cs


public static class JrDevArts_Utilities {

    public static LayerMask LayerMaskField(string label, LayerMask selected, GUIStyle style)
    {
        return LayerMaskField(label, selected, style, true);
    }

	public static LayerMask LayerMaskField(Rect rect, LayerMask selected, GUIStyle style)
	{
		return LayerMaskField(rect, selected, style, true);
	}
	/// <summary>
    /// Add a LayerMash field. This hasn't been made available by Unity even though
    /// it exists in the automated version of the inspector (when no custom editor).
    /// Contains code from: 
    ///     http://answers.unity3d.com/questions/60959/mask-field-in-the-editor.html
    /// </summary>
    /// <param name="label">The string to display to the left of the control</param>
    /// <param name="selected">The LayerMask variable</param>
    /// <param name="showSpecial">True to display "Nothing" & "Everything" options</param>
    /// <returns>A new LayerMask value</returns>
    public static LayerMask LayerMaskField(string label, LayerMask selected, GUIStyle style, bool showSpecial)
    {
		#if UNITY_EDITOR
        string selectedLayers = "";
        for (int i = 0; i < 32; i++){
            string layerName = LayerMask.LayerToName(i);
            if (layerName == "") continue;  // Skip empty layers

            if (selected == (selected | (1 << i)))
            {
                if (selectedLayers == "")
                {
                    selectedLayers = layerName;
                }
                else
                {
                    selectedLayers = "Mixed";
                    break;
                }
            }
        }

        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();
        if (Event.current.type != EventType.MouseDown && Event.current.type != EventType.ExecuteCommand){
            if (selected.value == 0){
                layers.Add("Nothing");
			}else if (selected.value == -1){
                layers.Add("Everything");
			}else{
                layers.Add(selectedLayers);
			}
            layerNumbers.Add(-1);
        }

        string check = "[x] ";
        string noCheck = "     ";
       
		if (showSpecial){
            layers.Add((selected.value == 0 ? check : noCheck) + "Nothing");
            layerNumbers.Add(-2);

            layers.Add((selected.value == -1 ? check : noCheck) + "Everything");
            layerNumbers.Add(-3);
        }

        // A LayerMask is based on a 32bit field, so there are 32 'slots' max available
        for (int i = 0; i < 32; i++){
        
            string layerName = LayerMask.LayerToName(i);
            if (layerName != ""){
            
                // Add a check box to the left of any selected layer's names
				if (selected == (selected | (1 << i))){
                    layers.Add(check + layerName);
				}else{
                    layers.Add(noCheck + layerName);
				}
                layerNumbers.Add(i);
            }
        }

        bool preChange = GUI.changed;
        GUI.changed = false;

        int newSelected = 0;
        if (Event.current.type == EventType.MouseDown) newSelected = -1;

        newSelected = UnityEditor.EditorGUILayout.Popup(label,
                                            newSelected,
                                            layers.ToArray(),
                                            style);

        if (GUI.changed && newSelected >= 0)
        {
            if (showSpecial && newSelected == 0)
                selected = 0;
            else if (showSpecial && newSelected == 1)
                selected = -1;
            else
            {
                if (selected == (selected | (1 << layerNumbers[newSelected])))
                    selected &= ~(1 << layerNumbers[newSelected]);
                else
                    selected = selected | (1 << layerNumbers[newSelected]);
            }
        }
        else
        {
            GUI.changed = preChange;
        }
#endif
        return selected;
    }
    
	public static LayerMask LayerMaskField(Rect rect, LayerMask selected, GUIStyle style, bool showSpecial){
		#if UNITY_EDITOR
		string selectedLayers = "";
		for (int i = 0; i < 32; i++)
		{
			string layerName = LayerMask.LayerToName(i);
			if (layerName == "") continue;  // Skip empty layers
			
			if (selected == (selected | (1 << i))){
			
				if (selectedLayers == ""){
				
					selectedLayers = layerName;
				
				}else{
				
					selectedLayers = "Mixed";
					break;
				}
			}
		}
		
		List<string> layers = new List<string>();
		List<int> layerNumbers = new List<int>();
		if (Event.current.type != EventType.MouseDown &&
		    Event.current.type != EventType.ExecuteCommand)
		{
			if (selected.value == 0)
				layers.Add("Nothing");
			else if (selected.value == -1)
				layers.Add("Everything");
			else
				layers.Add(selectedLayers);
			
			layerNumbers.Add(-1);
		}
		
		string check = "[x] ";
		string noCheck = "     ";
		if (showSpecial)
		{
			layers.Add((selected.value == 0 ? check : noCheck) + "Nothing");
			layerNumbers.Add(-2);
			
			layers.Add((selected.value == -1 ? check : noCheck) + "Everything");
			layerNumbers.Add(-3);
		}
		
		// A LayerMask is based on a 32bit field, so there are 32 'slots' max available
		for (int i = 0; i < 32; i++)
		{
			string layerName = LayerMask.LayerToName(i);
			if (layerName != "")
			{
				// Add a check box to the left of any selected layer's names
				if (selected == (selected | (1 << i)))
					layers.Add(check + layerName);
				else
					layers.Add(noCheck + layerName);
				
				layerNumbers.Add(i);
			}
		}
		
		bool preChange = GUI.changed;
		GUI.changed = false;
		
		int newSelected = 0;
		if (Event.current.type == EventType.MouseDown) newSelected = -1;
		
		newSelected = UnityEditor.EditorGUI.Popup(rect,
		                                    newSelected,
		                                    layers.ToArray(),
		                                    style);
		
		if (GUI.changed && newSelected >= 0)
		{
			if (showSpecial && newSelected == 0)
				selected = 0;
			else if (showSpecial && newSelected == 1)
				selected = -1;
			else
			{
				if (selected == (selected | (1 << layerNumbers[newSelected])))
					selected &= ~(1 << layerNumbers[newSelected]);
				else
					selected = selected | (1 << layerNumbers[newSelected]);
			}
		}
		else
		{
			GUI.changed = preChange;
		}

#endif
		return selected;
	}

	//Sends a dialog box and stops play mode if there are any errors in the GAC Editor
	public static bool PlayModeWarning(){
		#if UNITY_EDITOR
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode){
			UnityEditor.EditorUtility.DisplayDialog("There are GAC-Errors in Scene!", "Please fix all GAC Errors before entering Play Mode!", "OK", "");
			UnityEditor.EditorApplication.isPlaying = false;

		}
		#endif

		return true;
	}

	public static Texture2D DrawSquareTexture(int width, int height, Color col){
		Color[] pixels = new Color[width * height];

		for( int i = 0; i < pixels.Length; ++i){
			pixels[i] = col;
		}

		Texture2D result = new Texture2D(width, height);

		if(width > 0 && height > 0){
			result.SetPixels(pixels);
			result.Apply();
		}

		return result;
	}


	public static void ShowTexture(Texture texture) {
		if (texture != null) {
			
			Rect rect = GUILayoutUtility.GetRect(0f, 0f);
			rect.width = texture.width;
			rect.height = texture.height;
			GUILayout.Space(rect.height);
			GUI.DrawTexture(rect, texture);
		}
	}
	
	/// <summary>
	/// Get string value between [first] a and [last] b.
	/// </summary>
	public static string Between(this string value, string a, string b){
		int posA = value.IndexOf(a);
		int posB = value.LastIndexOf(b);
		if (posA == -1)
		{
			return "";
		}
		if (posB == -1)
		{
			return "";
		}
		int adjustedPosA = posA + a.Length;
		if (adjustedPosA >= posB)
		{
			return "";
		}
		return value.Substring(adjustedPosA, posB - adjustedPosA);
	}

	/// <summary>
	/// Get string value after [first] a.
	/// </summary>
	public static string Before(this string value, string a)
	{
		int posA = value.IndexOf(a);
		if (posA == -1)
		{
			return "";
		}
		return value.Substring(0, posA);
	}

	/// <summary>
	/// Get string value after [last] a.
	/// </summary>
	public static string After(this string value, string a){
		int posA = value.LastIndexOf(a);
		if (posA == -1)
		{
			return "";
		}
		int adjustedPosA = posA + a.Length;
		if (adjustedPosA >= value.Length)
		{
			return "";
		}
		return value.Substring(adjustedPosA);
	}

	public static string LastIndexOfSecond(string theString, string toFind){
		
		int last = theString.LastIndexOf(toFind);
		string result = "";
		
		if (last == -1){
			return "";
		}else{
			
			// Find the "next" occurrence by starting just past the first
			int index = theString.LastIndexOf(toFind, last - 1);
			
			if (index == -1){
				return "";
			}else{ 
				result = theString.Substring(index);
				
				if(result.Contains(toFind)){
					result = result.Replace(toFind, "");
				}
				
				return result;
			}
		}	
	}

	public static int FindIndex( string[] list, string value){
		
		for( int i = 0; i < list.Length; i++){
			if( list[ i ] == value ){
				return i;
			}
		}
		
		return -1;
		
	}

	static public string RemoveDuplicateWords(string v)
	{
		// 1
		// Keep track of words found in this Dictionary.
		var d = new Dictionary<string, bool>();
		
		// 2
		// Build up string into this StringBuilder.
		StringBuilder b = new StringBuilder();
		
		// 3
		// Split the input and handle spaces and punctuation.
		string[] a = v.Split(new char[] { ' ', ',', ';', '.' },
		StringSplitOptions.RemoveEmptyEntries);
		
		// 4
		// Loop over each word
		foreach (string current in a)
		{
			// 5
			// Lowercase each word
			string lower = current.ToLower();
			
			// 6
			// If we haven't already encountered the word,
			// append it to the result.
			if (!d.ContainsKey(lower))
			{
				b.Append(current).Append(' ');
				d.Add(lower, true);
			}
		}
		// 7
		// Return the duplicate words removed
		return b.ToString().Trim();
	}

	public static float GetMinNumber(float[] numbers){
		float minNumber;

		minNumber = numbers.Min();
		return minNumber;
	}

	public static float NANCheck(float value){

		if(System.Single.IsNaN(value)){
			value = 0;
		}

		return value;
	}
	
	#if UNITY_EDITOR
	static bool gacEditor_isDraggingX;
	static bool gacEditor_isDraggingZ;

	public static Vector3 Vector2Field(Rect rect, Vector2 value, string firstAxis, string secondAxis, float increment, params GUILayoutOption[] options) {
	
		Vector2 newValue = value;

		GUILayout.Space(rect.x);
		UnityEditor.EditorGUILayout.BeginHorizontal();

		//Reference the Rect of X Slide Arrow area
		Rect xRect = new Rect (rect.x, rect.y, 20, 15);

		UnityEditor.EditorGUI.LabelField(new Rect (rect.x, rect.y, 20, 15), firstAxis);
		value.x = UnityEditor.EditorGUI.FloatField(new Rect (rect.x + 13, rect.y, rect.width/3, rect.height), "", value.x);

		//Reference the Rect of X Slide Arrow area
		Rect zRect = new Rect (rect.x + (rect.width/3) + 15, rect.y, 20, 15);

		UnityEditor.EditorGUI.LabelField(new Rect (rect.x + (rect.width/3) + 15, rect.y, 20, 15), secondAxis);
		value.y = UnityEditor.EditorGUI.FloatField(new Rect (rect.x + (rect.width/3) + 28, rect.y, rect.width/3, rect.height), "", value.y);

		
		//Show the slide arrow over the X position
		UnityEditor.EditorGUIUtility.AddCursorRect (new Rect (rect.x, rect.y, 20, 15), UnityEditor.MouseCursor.SlideArrow);
		
		//Show the slide arrow over the Z position
		UnityEditor.EditorGUIUtility.AddCursorRect (new Rect (rect.x + (rect.width/3) + 15, rect.y, 20, 15), UnityEditor.MouseCursor.SlideArrow);

		UnityEditor.EditorGUILayout.EndHorizontal();

		if(Event.current.type == EventType.MouseDown){
			if(xRect.Contains(Event.current.mousePosition)){
				gacEditor_isDraggingX = true;
			}

			if(zRect.Contains(Event.current.mousePosition)){
				gacEditor_isDraggingZ = true;
			}
		}else if(Event.current.type == EventType.MouseUp){
			gacEditor_isDraggingX = false;
			gacEditor_isDraggingZ = false;
		}

		if(Event.current.type == EventType.MouseDrag){

			if(gacEditor_isDraggingX){
				if(Event.current.delta.x < 0){
					
					value.x = value.x - increment * Math.Abs (Event.current.delta.x);
					
				}else if(Event.current.delta.x > 0){
					
					value.x = value.x + increment * Math.Abs(Event.current.delta.x);
					
				}
			}

			if(gacEditor_isDraggingZ){
				if(Event.current.delta.x < 0){
					
					value.y = value.y - increment * Math.Abs (Event.current.delta.x);

				}else if(Event.current.delta.x > 0){
					
					value.y = value.y + increment * Math.Abs(Event.current.delta.x);
					
				}
			}
		}

		//Round value to 2 decimal points
		value.x = (float)System.Math.Round(value.x, 2);
		value.y = (float)System.Math.Round(value.y, 2);

		//If there is any change in value, mark gui as changed
		if(newValue != value){
			GUI.changed = true;
		}
		return value;
	}

	public static KeyValuePair<float, bool> ContextFloatField(Rect rect, float value, float increment, float minValue, float maxValue, bool isDraggingField, params GUILayoutOption[] options) {

		float newValue = value;

		GUILayout.Space(rect.x);
		UnityEditor.EditorGUILayout.BeginHorizontal();
		
		//Reference the Rect of Left of Slide Arrow area
		Rect leftSideRect = new Rect (rect.x, rect.y, 20, 15);

		//Reference the Rect of Right Slide Arrow area
		Rect rightSideRect = new Rect (rect.x + rect.width + 10, rect.y, 20, 15);

		//Decrease the amount if button is pressed
		if (GUI.Button(new Rect (rect.x - 28, rect.y + 1, 26, 20),new GUIContent("-", "Decrease the amount"),UnityEditor.EditorStyles.toolbarButton)){
			
			value = value - 0.01f;
		}

		value = UnityEditor.EditorGUI.FloatField(new Rect (rect.x + 13, rect.y, rect.width, rect.height), "", value);

		//Increase the amount if button is pressed
		if (GUI.Button(new Rect (rect.x + rect.width + 28, rect.y + 1, 26, 20),new GUIContent("+", "Increase the amount"),UnityEditor.EditorStyles.toolbarButton)){
			
			value = value + 0.01f;
		}

		
		//Prevent from decreasing below minimum or increasing above maximum move amounts
		if (value > maxValue){
			value = maxValue;
		}else if (value < minValue){
			value = minValue;
		}
		UnityEditor.EditorGUILayout.EndHorizontal();

		if(GUI.enabled){
			//Show the slide arrow over the X position
			UnityEditor.EditorGUIUtility.AddCursorRect (new Rect (rect.x, rect.y, 20, 15), UnityEditor.MouseCursor.SlideArrow);
			
			//Show the slide arrow over the Z position
			UnityEditor.EditorGUIUtility.AddCursorRect (new Rect (rect.x + rect.width + 10, rect.y, 20, 15), UnityEditor.MouseCursor.SlideArrow);
			

			
			if(Event.current.type == EventType.MouseDown){
				if(leftSideRect.Contains(Event.current.mousePosition)){
					isDraggingField = true;
				}else if(rightSideRect.Contains(Event.current.mousePosition)){
					isDraggingField = true;
				}

			}else if(Event.current.type == EventType.MouseUp){
				isDraggingField = false;
			}

			if(Event.current.type == EventType.MouseDrag){
				
				if(isDraggingField){
					if(Event.current.delta.x < 0){
						
						value = value - increment * Math.Abs (Event.current.delta.x);
						
					}else if(Event.current.delta.x > 0){
						
						value = value + increment * Math.Abs(Event.current.delta.x);
						
					}
				}

			}
		}
		//Round value to 2 decimal points
		value = (float)System.Math.Round(value, 2);

		//If there is any change in value, mark gui as changed
		if(newValue != value){
			GUI.changed = true;
		}

		return new KeyValuePair<float, bool>(value, isDraggingField);
	}
	#endif

}
