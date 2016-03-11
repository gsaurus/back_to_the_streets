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
			new BuildKeyFrame(),						// 0: frame = 4
			new BuildFrameArithmetics(),				// 1: frame >= 4
			new BuildInputAxisMoving(),					// 2: moving
			new BuildInputAxisMovingDominance(),        // 3: move left
			new BuildInputAxisComponentArithmetics(),   // 4: speed_H >= 5.2
			new BuildInputButton(),						// 5: press D
			new BuildEntityIsGrounded(),				// 6: grounded
			new BuildEntityIsFacingRight(),				// 7: facing right
			new BuildEntityHittingWall(),				// 8: collide left wall
			new BuildEntityCollisionForceArithmetics()	// 9: collide_H >= 4.3
			// TODO: everything else, including custom values List<int>, List<FixedFloat>, List<int> timers for combo counter etc
		};



		public string[] TypesList(){
			string[] types = new string[builders.Length];
			for (int i = 0 ; i < builders.Length ; ++i) {
				types[i] = builders[i].typeName;
			}
			return types;
		}

		public string ToString(GenericParameter parameter){
			if (parameter.type >= 0 && parameter.type < builders.Length) {
				builders[parameter.type].ToString(parameter);
			}
			return "Unknown condition";
		}


		public void Build(GameObject parent, GenericParameter parameter){
			if (parameter.type >= 0 && parameter.type < builders.Length) {
				builders[parameter.type].Build(parent, parameter);
			}
		}


//		private void BuildTest1(GameObject parent, GenericParameter parameter){
//			Character character = CharacterEditor.Instance.character;
//			List<string> animNames = new List<string>();
//			if (character != null) {
//				foreach (CharacterAnimation anim in character.animations) {
//					animNames.Add(anim.name);
//				}
//			}
//			IntDropdownParam.Instantiate(parent, parameter, 0, "Animation:", animNames.ToArray());
//			IntInputFieldParam.Instantiate(parent, parameter, 1, "Frame: ", 0, character != null ? CharacterEditor.Instance.CurrentAnimation().numFrames : -1);
//		}
//
//		private void BuildTest2(GameObject parent, GenericParameter parameter){
//			StringDropdownParam.Instantiate(parent, parameter, 0, "Got hit at:", new string[]{"head", "body", "feet"});
//			FloatInputFieldParam.Instantiate(parent, parameter, 0, "X Offset:");
//			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Y Offset:");
//			BoolToggleParam.Instantiate(parent, parameter, 0, "Is blocking");
//		}


		private static void InstantiateNegation(GameObject parent, GenericParameter parameter){
			BoolToggleParam.Instantiate(parent, parameter, 0, "Negate");
		}

		private static void InstantiateArithmeticField(GameObject parent, GenericParameter parameter, int paramId){
			IntDropdownParam.Instantiate(parent, parameter, paramId, "Operator:", arithmeticOptions);
		}

		private static string SafeToString(string[] stringsArray, int type, string kind) {
			if (type > 0 && type < stringsArray.Length) {
				return stringsArray[type];
			}
			return "<invalid " + kind + ">";
		}

		private static string FilterNegationString(GenericParameter parameter, string conditionText){
			return parameter.SafeBool(0) ? "!(" + conditionText + ")" : conditionText;
		}



			
#region Builder Classes


		// frame = 4
		private class BuildKeyFrame: InternConditionBuilder{
			public BuildKeyFrame():base("Frame #"){}
			public override string ToString(GenericParameter parameter){
				return "frame " + parameter.SafeInt(0);
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntInputFieldParam.Instantiate(parent, parameter, 0, "At frame:", 0);
			}
		}


		// frame >= 4
		private class BuildFrameArithmetics: InternConditionBuilder{
			public BuildFrameArithmetics():base("Frame comparison"){}
			public override string ToString(GenericParameter parameter){
				return "frame " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator") + " " + parameter.SafeInt(0);
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateArithmeticField(parent, parameter, 1);
				IntInputFieldParam.Instantiate(parent, parameter, 0, "Compare with frame:", 0);
			}
		}


		// moving
		private class BuildInputAxisMoving: InternConditionBuilder{
			public BuildInputAxisMoving():base("Input have movement"){}
			public override string ToString(GenericParameter parameter){
				return FilterNegationString(parameter, "move");
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
			}
		}


		// move left
		private class BuildInputAxisMovingDominance: InternConditionBuilder{
			public BuildInputAxisMovingDominance():base("Input direction"){}
			public override string ToString(GenericParameter parameter){
				return FilterNegationString(parameter, "move " + SafeToString(directionOptionsShort, parameter.SafeInt(0), "direction"));
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
				IntDropdownParam.Instantiate(parent, parameter, 0, "Direction:", directionOptions);
			}
		}


		// move_H >= 5.2
		private class BuildInputAxisComponentArithmetics: InternConditionBuilder{
			public BuildInputAxisComponentArithmetics():base("Input velocity"){}
			public override string ToString(GenericParameter parameter){
				return "move_" + SafeToString(inputOrientationOptionsShort, parameter.SafeInt(0), "orientation")
					+ " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
					+ " " + parameter.SafeFloat(0)
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Orientation:", inputOrientationOptions);
				InstantiateArithmeticField(parent, parameter, 1);
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "Velocity:");
			}
		}


		// press D
		private class BuildInputButton: InternConditionBuilder{
			public BuildInputButton():base("Input button"){}
			public override string ToString(GenericParameter parameter){
				return FilterNegationString(parameter, SafeToString(inputButtonStateOptions, parameter.SafeInt(0), "button state") + " " + SafeToString(inputButtonOptions, parameter.SafeInt(1), "button"));
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
				IntDropdownParam.Instantiate(parent, parameter, 1, "Button:", inputButtonOptions);
				IntDropdownParam.Instantiate(parent, parameter, 0, "State:", inputButtonStateOptions);
			}
		}


		// grounded
		private class BuildEntityIsGrounded: InternConditionBuilder{
			public BuildEntityIsGrounded():base("Grounded"){}
			public override string ToString(GenericParameter parameter){
				return FilterNegationString(parameter, "grounded");
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
			}
		}


		// facing right
		private class BuildEntityIsFacingRight: InternConditionBuilder{
			public BuildEntityIsFacingRight():base("Facing right"){}
			public override string ToString(GenericParameter parameter){
				return "facing " + (parameter.SafeBool(0) ? "←" : "→");
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
			}
		}


		// collide left wall
		private class BuildEntityHittingWall: InternConditionBuilder{
			public BuildEntityHittingWall():base("Collision against wall"){}
			public override string ToString(GenericParameter parameter){
				return FilterNegationString(parameter, "wall " + SafeToString(collisionWallDirectionShort, parameter.SafeInt(0), "wall"));
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateNegation(parent, parameter);
				IntDropdownParam.Instantiate(parent, parameter, 0, "Wall:", collisionWallDirection);
			}
		}


		// collideH >= 4.3
		private class BuildEntityCollisionForceArithmetics: InternConditionBuilder{
			public BuildEntityCollisionForceArithmetics():base("Collision impact"){}
			public override string ToString(GenericParameter parameter){
				return "collide_" + SafeToString(collisionDirectionShort, parameter.SafeInt(0), "impact orientation")
					+ " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
					+ " " + parameter.SafeFloat(0)
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Orientation:", collisionDirection);
				InstantiateArithmeticField(parent, parameter, 1);
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "impact velocity:");
			}
		}


#endregion


	}

}
