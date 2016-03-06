using UnityEngine;
using System.Collections.Generic;
using RetroBread.Editor;

namespace RetroBread{
	
	public class ConditionParameterBuilder: ParameterBuilder {
		private static ParameterBuilder instance;
		public static ParameterBuilder Instance {
			get{
				if (instance == null) {
					instance = new ConditionParameterBuilder();
				}
				return instance;
			}
		}
			
		private string[] typesList = { "Frame is", "Got Hit" };

		public string[] TypesList(){
			return typesList;
		}

		public string ToString(GenericParameter parameter){
			switch (parameter.type) {
			case 0:
				parameter.EnsureIntItem(1);
				return typesList[parameter.type] + " " + parameter.intsList[1];
			case 1:
				parameter.EnsureFloatItem(0);
				return typesList[parameter.type] + " with damage: " + parameter.floatsList[0];
			}
			return "Unknown condition";
		}


		public void Build(GameObject parent, GenericParameter parameter){
			switch (parameter.type) {
				case 0:
					BuildTest1(parent, parameter);
					break;
				case 1:
					BuildTest2(parent, parameter);
					break;
			}
		}


		private void BuildTest1(GameObject parent, GenericParameter parameter){
			Character character = CharacterEditor.Instance.character;
			List<string> animNames = new List<string>();
			if (character != null) {
				foreach (CharacterAnimation anim in character.animations) {
					animNames.Add(anim.name);
				}
			}
			IntDropdownParam.Instantiate(parent, parameter, 0, "Animation:", animNames.ToArray());
			IntInputFieldParam.Instantiate(parent, parameter, 1, "Frame: ", 0, character != null ? CharacterEditor.Instance.CurrentAnimation().numFrames : -1);
		}

		private void BuildTest2(GameObject parent, GenericParameter parameter){
			StringDropdownParam.Instantiate(parent, parameter, 0, "Got hit at:", new string[]{"head", "body", "feet"});
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "X Offset:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Y Offset:");
			BoolToggleParam.Instantiate(parent, parameter, 0, "Is blocking");
		}
			

	}

}
