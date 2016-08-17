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

		private abstract class InternConditionBuilder{
			
			public string typeName { get; private set; }

			public abstract string ToString(GenericParameter parameter);
			public abstract void Build(GameObject parent, GenericParameter parameter);

			public InternConditionBuilder(string typeName){
				this.typeName = typeName;
			}
		}

		private static string[] arithmeticOptions = { "equal", "notEqual", "less", "less or equal", "greater", "greater or equal" };
		private static string[] arithmeticOptionsShort = { "=", "!=", "<", "<=", ">", ">=" };	

		private static string[] directionOptions = {"up", "down", "left", "right"};
		private static string[] directionOptionsShort = {"↑", "↓", "←", "→"};

		private static string[] inputOrientationOptions = {"horizontal", "vertical"};
		private static string[] inputOrientationOptionsShort = {"H", "V"};

		private static string[] inputButtonOptions = {"A", "B", "C", "D", "E", "F", "G"};
		private static string[] inputButtonStateOptions = {"press", "hold", "release"};

		private static string[] collisionWallDirection = {"far", "near", "left", "right"};
		private static string[] collisionWallDirectionShort = {"↑", "↓", "←", "→"};

		private static string[] collisionDirection = {"horizontal", "vertical", "along z-axis"};
		private static string[] collisionDirectionShort = {"H", "V", "Z"};


		// Condition builders indexed by type directly on array
		private static InternConditionBuilder[] builders = {
			new BuildOnEnable(),			// 0: enable / disable
			new BuildOnVariableChange(),	// 1: changed(energy)
			new BuildOnVariableValue()		// 2: energy >= 4
		};



		public override string[] TypesList(){
			string[] types = new string[builders.Length];
			for (int i = 0 ; i < builders.Length ; ++i) {
				types[i] = builders[i].typeName;
			}
			return types;
		}

		public override string ToString(GenericParameter parameter){
			if (parameter.type >= 0 && parameter.type < builders.Length) {
				return builders[parameter.type].ToString(parameter);
			}
			return "Unknown condition";
		}


		public override void Build(GameObject parent, GenericParameter parameter){
			if (parameter.type >= 0 && parameter.type < builders.Length) {
				builders[parameter.type].Build(parent, parameter);
			}
		}

#region helper methods


		private static void InstantiateNegation(GameObject parent, GenericParameter parameter){
			BoolToggleParam.Instantiate(parent, parameter, 0, "Negate");
		}

		private static void InstantiateArithmeticField(GameObject parent, GenericParameter parameter, int paramId){
			IntDropdownParam.Instantiate(parent, parameter, paramId, "Operator:", arithmeticOptions);
		}
			

		private static string FilterNegationString(GenericParameter parameter, string conditionText){
			return parameter.SafeBool(0) ? "!(" + conditionText + ")" : conditionText;
		}


#endregion



			
#region Builder Classes



		// enable / disable
		private class BuildOnEnable: InternConditionBuilder{
			public BuildOnEnable():base("On Enable"){}
			public override string ToString(GenericParameter parameter){
				return parameter.SafeBool(0) ? "disable" : "enable";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
			}
		}


		// change(energy)
		private class BuildOnVariableChange: InternConditionBuilder{
			public BuildOnVariableChange():base("Variable Change"){}
			public override string ToString(GenericParameter parameter){
				return "change(" + parameter.SafeString(0) + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Variable:");
			}
		}

		// energy >= 4
		private class BuildOnVariableValue: InternConditionBuilder{
			public BuildOnVariableValue():base("Variable comparison"){}
			public override string ToString(GenericParameter parameter){
				return parameter.SafeString(0) + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator") + parameter.SafeInt(0);
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Variable:");
				InstantiateArithmeticField(parent, parameter, 1);
				IntInputFieldParam.Instantiate(parent, parameter, 0, "Compare with value:", 0);
			}
		}



#endregion


	}

}
