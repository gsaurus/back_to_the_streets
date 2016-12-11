using UnityEngine;
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
            return stringsArray[type].ToLower();
		}
		return "<invalid " + kind + ">";
	}

	protected static string[] GetAnimationsNames(){
		Character character = CharacterEditor.Instance.character;
		List<string> animNames = new List<string>();
		if (character != null) {
			foreach (CharacterAnimation anim in character.animations) {
				animNames.Add(anim.name);
			}
		}
		return animNames.ToArray();
	}


	protected static string[] GetAnchorsNames(){
		Character character = CharacterEditor.Instance.character;
		return character.viewAnchors.ToArray();
	}


    #region helper methods


    protected static void InstantiateSubject(GameObject parent, GenericParameter parameter, int paramId, string description = "Subject:"){
        IntDropdownParam.Instantiate(parent, parameter, paramId, description, CharacterEditor.Instance.AvailableSubjects());
    }

    protected static void InstantiateNumeratorVar(GameObject parent, GenericParameter parameter, int numeratorParamId, int varParamId){
        string[] subjects = CharacterEditor.Instance.AvailableSubjects();
        string[] subjectsPlusNone = new string[subjects.Length + 2];
        subjectsPlusNone[0] = "none";
        subjectsPlusNone[1] = "global variable";
        subjects.CopyTo(subjectsPlusNone, 2);
        IntDropdownParam.Instantiate(parent, parameter, numeratorParamId, "Numerator Subject:", subjectsPlusNone);
        StringInputFieldParam.Instantiate(parent, parameter, varParamId, "Numerator Variable:");
    }
        

    protected static string SubjectString(GenericParameter parameter, int paramId){
        int subjectId = parameter.SafeInt(paramId);
        if (subjectId == 0) return "";
        else return "[" + SafeToString(CharacterEditor.Instance.AvailableSubjects(), subjectId, "Subject") + "]";
    }

    protected static string NumeratorString(GenericParameter parameter, int numeratorParamId, int varParamId, string noneStringReplacement){
        int numeratorId = parameter.SafeInt(numeratorParamId);
        if (numeratorId == 0){
            return noneStringReplacement;
        } else{
            string numeratorString;
            if (numeratorId == 1){
                numeratorString = "global";
            } else{
                numeratorString = SafeToString(CharacterEditor.Instance.AvailableSubjects(), numeratorId - 2, "Numerator Subject");
            }
            return parameter.SafeString(varParamId) + "[" + numeratorString + "]";
        }
    }


    #endregion

}


}