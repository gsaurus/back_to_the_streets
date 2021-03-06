﻿using UnityEngine;
using System.Collections;

namespace RetroBread{


	public static class GenericParameterExtensions{

		public static int SafeInt(this Editor.GenericParameter parameter, int index){
			parameter.EnsureIntItem(index);
			return parameter.intsList[index]; 
		}

		public static FixedFloat SafeFloat(this Editor.GenericParameter parameter, int index){
			parameter.EnsureFloatItem(index);
			return parameter.floatsList[index];
		}

		public static string SafeString(this Editor.GenericParameter parameter, int index){
			parameter.EnsureStringItem(index);
			return parameter.stringsList[index];
		}

		public static bool SafeBool(this Editor.GenericParameter parameter, int index){
			parameter.EnsureBoolItem(index);
			return parameter.boolsList[index];
		}

		public static string SafeFloatToString(this Editor.GenericParameter parameter, int index){
			parameter.EnsureFloatItem(index);
			return ((float)parameter.floatsList[index]).ToString("0.###");
		}

		public static string SafeBoolToString(this Editor.GenericParameter parameter, int index){
			parameter.EnsureBoolItem(index);
			return parameter.boolsList[index] ? "true" : "false";
		}

	}


}
