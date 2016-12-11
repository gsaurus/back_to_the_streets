using UnityEngine;
using System.Collections;

namespace RetroBread{


public static class GenericParameterExtensions{

	public static int SafeInt(this Storage.GenericParameter parameter, int index){
		return parameter.intsList != null && index >= 0 && index < parameter.intsList.Length ? parameter.intsList[index] : 0; 
	}

	public static FixedFloat SafeFloat(this Storage.GenericParameter parameter, int index){
		return parameter.floatsList != null && index >= 0 && index < parameter.floatsList.Length ? parameter.floatsList[index] : 0; 
	}

	public static string SafeString(this Storage.GenericParameter parameter, int index){
		return parameter.stringsList != null && index >= 0 && index < parameter.stringsList.Length ? parameter.stringsList[index] : null; 
	}

	public static bool SafeBool(this Storage.GenericParameter parameter, int index){
		return parameter.boolsList != null && index >= 0 && index < parameter.boolsList.Length ? parameter.boolsList[index] : false; 
	}

	public static int[] SafeIntsList(this Storage.GenericParameter parameter, int index){
		return parameter.intsListList != null && index >= 0 && index < parameter.intsListList.Length ? parameter.intsListList[index] : null;
	}

	public static string[] SafeStringsList(this Storage.GenericParameter parameter, int index){
		return parameter.stringsListList != null && index >= 0 && index < parameter.stringsListList.Length ? parameter.stringsListList[index] : null;
	}

}


}
