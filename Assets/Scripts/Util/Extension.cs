using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;
public static class Extension
{
	public static void BindEvent(this GameObject go, Action<PointerEventData> action, UIEvent type = UIEvent.Click)
	{
		UI_Base.BindEvent(go, action, type);
	}
	
	public static void SetLayerRecursively(this GameObject obj, string layer)
	{
		var newLayer = LayerMask.NameToLayer(layer);
		if (!obj.CompareTag("Range"))
		{
			obj.layer = newLayer;
			foreach (Transform child in obj.transform)
			{
				child.gameObject.SetLayerRecursively(layer);
			}
		}
			
		
	}
}
