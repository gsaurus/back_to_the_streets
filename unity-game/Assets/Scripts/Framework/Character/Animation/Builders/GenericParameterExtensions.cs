using UnityEngine;
using System.Collections;

namespace RetroBread{


	public static class GenericParameterExtensions{

		public static int SafeInt(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.intsList.Length ? parameter.intsList[index] : 0; 
		}

		public static FixedFloat SafeFloat(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.floatsList.Length ? parameter.floatsList[index] : 0; 
		}

		public static string SafeString(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.stringsList.Length ? parameter.stringsList[index] : null; 
		}

		public static bool SafeBool(this Storage.GenericParameter parameter, int index){
			return index >= 0 && index < parameter.boolsList.Length ? parameter.boolsList[index] : false; 
		}

	}


}
