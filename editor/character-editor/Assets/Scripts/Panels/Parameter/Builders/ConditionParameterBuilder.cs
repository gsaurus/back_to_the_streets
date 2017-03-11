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

	private static string[] directionOptions2D = {"horizontal", "vertical", "any"};
    private static string[] directionOptions3D = {"horizontal", "vertical", "any", "along z-axis"};
	private static string[] directionOptionsShort = {"H", "V", "", "Z"};

	private static string[] inputButtonOptions = {"A", "B", "C", "D", "E", "F", "G"};
	private static string[] inputButtonStateOptions = {"press", "hold", "release"};
	
    private static string[] listType = {"any of", "all but"};


	// Condition builders indexed by type directly on array
	private static InternConditionBuilder[] builders = {
		new BuildFrame(),				            // 0: frame >= 4
		new BuildInputVelocity(),                   // 1: moveH >= 3
		new BuildInputButton(),						// 2: press D
		new BuildGrounded(),				        // 3: grounded
		new BuildFacingRight(),				        // 4: facing right
		new BuildCollisionImpact(),	                // 5: collide_H >= 4.3
		new BuildVariable(),    					// 6: combo >= 3
        new BuildGlobalVariable(),                  // 7: p1_lives >= 3
        new BuildPhysicsVelocity(),					// 8: velocityV <= 2
        new BuildExists(),                          // 9: exists(owner)
        new BuildName(),                            // 10: name(grabbed, ["bat", "sword"])
        new BuildAnimationName()                    // 11: anim_name(parent, ["walk, run"])
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


    private static string ParseOptionsList(string options, bool isAnyOf){
        if (options != null && options.Length > 0){
            if (isAnyOf) return "[" + options + "]";
            else return "all/[" + options + "]";
        } else{
            if (isAnyOf) return "none";
            else return "all";
        }
    }


#endregion



		
#region Builder Classes


	// frame >= 4
    private class BuildFrame: InternConditionBuilder{
        public BuildFrame():base("Frame"){}
		public override string ToString(GenericParameter parameter){
            string operationName = "frame" + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 2, 0, parameter.SafeInt(3) + "");
            if (numeratorString.Equals("-1")){
                numeratorString = "last";
            }else if (numeratorString.Equals("0")){
                numeratorString = "first";
            }
            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
                + " " + numeratorString
            ;
        }
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			InstantiateArithmeticField(parent, parameter, 1);
            InstantiateNumeratorVar(parent, parameter, 2, 0);
			IntInputFieldParam.Instantiate(parent, parameter, 3, "Or compare with frame:");
		}
	}


	// |move_H| >= 5.2
    private class BuildInputVelocity: InternConditionBuilder{
        public BuildInputVelocity():base("Input Axis"){}
		public override string ToString(GenericParameter parameter){
            string orientationString = directionOptionsShort[parameter.SafeInt(1)];
            string operationName = "axis" + orientationString + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 3, 0, parameter.SafeFloat(0) + "");
            if (parameter.SafeBool(1)){
                operationName = "|" + operationName + "|";
            }
            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(2), "operator")
                + " " + numeratorString;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Orientation:", directionOptions2D);
			InstantiateArithmeticField(parent, parameter, 2);
            InstantiateNumeratorVar(parent, parameter, 3, 0);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Or compare with value:");
            BoolToggleParam.Instantiate(parent, parameter, 1, "Use absolute value");
		}
	}


	// press D
	private class BuildInputButton: InternConditionBuilder{
		public BuildInputButton():base("Input Button"){}
		public override string ToString(GenericParameter parameter){
            string operationName = "button" + SubjectString(parameter, 0);
            return FilterNegationString(parameter, operationName + SafeToString(inputButtonStateOptions, parameter.SafeInt(0), "button state") + " " + SafeToString(inputButtonOptions, parameter.SafeInt(1), "button"));
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			IntDropdownParam.Instantiate(parent, parameter, 1, "Button:", inputButtonOptions);
			IntDropdownParam.Instantiate(parent, parameter, 2, "State:", inputButtonStateOptions);
            InstantiateNegation(parent, parameter);
		}
	}


	// grounded
    private class BuildGrounded: InternConditionBuilder{
        public BuildGrounded():base("Grounded"){}
		public override string ToString(GenericParameter parameter){
            return FilterNegationString(parameter, "grounded" + SubjectString(parameter, 0));
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			InstantiateNegation(parent, parameter);
		}
	}


	// facing right
	private class BuildFacingRight: InternConditionBuilder{
        public BuildFacingRight():base("Facing right"){}
		public override string ToString(GenericParameter parameter){
            return "facing" + SubjectString(parameter, 0) + " " + (parameter.SafeBool(0) ? "←" : "→");
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			InstantiateSubject(parent, parameter, 0);
			InstantiateNegation(parent, parameter);
		}
	}
        

    // |collideH| >= 3.2
	private class BuildCollisionImpact: InternConditionBuilder{
        public BuildCollisionImpact():base("Collision Impact"){}
        public override string ToString(GenericParameter parameter){
            string orientationString = directionOptionsShort[parameter.SafeInt(1)];
            string operationName = "collide" + orientationString + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 3, 0, parameter.SafeFloat(0) + "");
            if (parameter.SafeBool(1)){
                operationName = "|" + operationName + "|";
            }
            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(2), "operator")
                + " " + numeratorString;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Orientation:", directionOptions3D);
            InstantiateArithmeticField(parent, parameter, 2);
            InstantiateNumeratorVar(parent, parameter, 3, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Or compare with value:");
            BoolToggleParam.Instantiate(parent, parameter, 1, "Use absolute value");
        }
	}
       


    // var(energy)
	private class BuildVariable: InternConditionBuilder{
        public BuildVariable():base("Variable"){}
        public override string ToString(GenericParameter parameter){
            string varName = parameter.SafeString(0);
            string varString = "var";
            string operationName = varString + "(" + varName + ")" + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 2, 1, parameter.SafeInt(3) + "");

            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
                + " " + numeratorString;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Variable name:");
            InstantiateArithmeticField(parent, parameter, 1);
            InstantiateNumeratorVar(parent, parameter, 2, 1);
            IntInputFieldParam.Instantiate(parent, parameter, 3, "Or compare with value:");
        }
	}


    // pl1_lives >= 4
    private class BuildGlobalVariable: InternConditionBuilder{
        public BuildGlobalVariable():base("Global Variable"){}
        public override string ToString(GenericParameter parameter){
            string varName = parameter.SafeString(0);
            string varString = "globalVar";
            string operationName = varString + "(" + varName + ")" + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 2, 1, parameter.SafeInt(3) + "");

            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
                + " " + numeratorString;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Global variable name:");
            InstantiateArithmeticField(parent, parameter, 1);
            InstantiateNumeratorVar(parent, parameter, 2, 1);
            IntInputFieldParam.Instantiate(parent, parameter, 3, "Or compare with value:");
        }
    }


    // physics_vel_H >= 4
    private class BuildPhysicsVelocity: InternConditionBuilder{
        public BuildPhysicsVelocity():base("Physics Velocity"){}
        public override string ToString(GenericParameter parameter){
            string orientationString = directionOptionsShort[parameter.SafeInt(1)];
            string operationName = "axis" + orientationString + SubjectString(parameter, 0);
            string numeratorString = NumeratorString(parameter, 3, 0, parameter.SafeFloat(0) + "");
            if (parameter.SafeBool(1)){
                operationName = "|" + operationName + "|";
            }
            return operationName
                + " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(2), "operator")
                + " " + numeratorString;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Orientation:", directionOptions3D);
            InstantiateArithmeticField(parent, parameter, 2);
            InstantiateNumeratorVar(parent, parameter, 3, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Or compare with value:");
            BoolToggleParam.Instantiate(parent, parameter, 1, "Use absolute value");
        }
	}


    // exists(parent)
    private class BuildExists: InternConditionBuilder{
        public BuildExists():base("Exists"){}
        public override string ToString(GenericParameter parameter){
			return FilterNegationString(parameter, "exists" + SubjectString(parameter, 0));
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			InstantiateNegation(parent, parameter);
        }
    }
			


    // name(parent, ["bat", "sword"])
    private class BuildName: InternConditionBuilder{
        public BuildName():base("Name"){}
        public override string ToString(GenericParameter parameter){
			string namesList = ParseOptionsList(parameter.SafeStringsListToString(0), parameter.SafeInt(1) == 0);
			return "name" + SubjectString(parameter, 0) + " in " + namesList;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			IntDropdownParam.Instantiate(parent, parameter, 1, "Is:", listType);
			StringListInputFieldParam.Instantiate(parent, parameter, 0, "Names list");
        }
    }


    // anim_name(parent, all\["walk"])
    private class BuildAnimationName: InternConditionBuilder{
			public BuildAnimationName():base("Animation Name"){}
			public override string ToString(GenericParameter parameter){
				string namesList = ParseOptionsList(parameter.SafeStringsListToString(0), parameter.SafeInt(1) == 0);
				return "animName" + SubjectString(parameter, 0) + " in " + namesList;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				InstantiateSubject(parent, parameter, 0);
				IntDropdownParam.Instantiate(parent, parameter, 1, "Is:", listType);
				StringListInputFieldParam.Instantiate(parent, parameter, 0, "Animations list");
			}
    }


#endregion


}

}
