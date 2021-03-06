﻿using UnityEngine;
using System.Collections.Generic;
using RetroBread.Editor;

namespace RetroBread{


	public abstract class ParameterBuilder{
		
		// Get a string array containing description of each available parameter type
		public abstract string[] TypesList();

		// Build the components of the parameter panel (parent), for the given type
		public abstract void Build(GameObject parent, GenericParameter parameter);

		// String representation accordingly to the builder interpretation
		public abstract string ToString(GenericParameter parameter);


		protected static string SafeToString(string[] stringsArray, int type, string kind) {
			if (type >= 0 && type < stringsArray.Length) {
				return stringsArray[type];
			}
			return "<invalid " + kind + ">";
		}


	}


}