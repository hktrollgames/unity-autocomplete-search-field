//using System;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;
//using System.Linq;

//public class EnumSelector : EditorWindow
//{
//	[MenuItem("Window/test")]
//	static void Init()
//	{
//		GetWindow<EnumSelector>("Enum selector").Show();
//	}

//	//static Type targetEnum;
//	static List<UIButtonType> list = new List<UIButtonType>();
//	static UIEventListDrawerDrawer caller;
//	static public void ShowUI(UIEventListDrawerDrawer _caller)
//	{
//		caller = _caller;
//		list =(from UIButtonType item in Enum.GetValues(typeof(UIButtonType))
//			   select item).ToList();
//		GetWindow<EnumSelector>("Enum selector").Show();
//	}

//	void OnGUI()
//	{
//		foreach(var item in list)
//		{
//			if (GUILayout.Button(item.ToString()))
//			{
//				Debug.Log($"{item} Click");
//				caller.Select(item);
//			}
//		}
//	}
//}