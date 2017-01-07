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

	private static string[] directionOptions = {"horizontal", "vertical", "any"};
	private static string[] directionOptionsShort = {"H", "V", ""};

	private static string[] inputButtonOptions = {"A", "B", "C", "D", "E", "F", "G"};
	private static string[] inputButtonStateOptions = {"press", "hold", "release"};

    private static string[] collisionDirection = {"horizontal", "along z-axis", "vertical"};
	private static string[] collisionDirectionShort = {"H", "Z", "V"};

	private static string[] teamType = {"any of", "all but", "same as self"};
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
        new BuildTeam(),                            // 10: team([3, 4])
        new BuildName(),                            // 11: name(grabbed, ["bat", "sword"])
        new BuildAnimationName()                    // 12: anim_name(parent, ["walk, run"])
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
        public BuildInputVelocity():base("Input velocity"){}
		public override string ToString(GenericParameter parameter){
            string orientationString = "";
            switch (parameter.SafeInt(1)){
                case 0: orientationString = "H"; break;
                case 1: orientationString = "V"; break;
                default: orientationString = ""; break;
            }
            string operationName = "inputVel" + orientationString + SubjectString(parameter, 0);
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
            IntDropdownParam.Instantiate(parent, parameter, 1, "Orientation:", directionOptions);
			InstantiateArithmeticField(parent, parameter, 2);
            InstantiateNumeratorVar(parent, parameter, 3, 0);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Or compare with value:");
            BoolToggleParam.Instantiate(parent, parameter, 1, "Use absolute value");
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
    private class BuildGrounded: InternConditionBuilder{
        public BuildGrounded():base("Grounded"){}
		public override string ToString(GenericParameter parameter){
			return FilterNegationString(parameter, "grounded");
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			InstantiateNegation(parent, parameter);
		}
	}


	// facing right
	private class BuildFacingRight: InternConditionBuilder{
        public BuildFacingRight():base("Facing right"){}
		public override string ToString(GenericParameter parameter){
			return "facing " + (parameter.SafeBool(0) ? "←" : "→");
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			InstantiateNegation(parent, parameter);
		}
	}
        

	// collideH >= 4.3
	private class BuildCollisionImpact: InternConditionBuilder{
		public BuildCollisionImpact():base("Collision impact"){}
		public override string ToString(GenericParameter parameter){
			return "collide_" + SafeToString(collisionDirectionShort, parameter.SafeInt(0), "impact orientation")
				+ " " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator")
				+ " " + parameter.SafeFloatToString(0)
			;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			IntDropdownParam.Instantiate(parent, parameter, 0, "Orientation:", collisionDirection);
			InstantiateArithmeticField(parent, parameter, 1);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "impact velocity:");
		}
	}
       


	// combo >= 4
	private class BuildVariable: InternConditionBuilder{
        public BuildVariable():base("Variable"){}
		public override string ToString(GenericParameter parameter){
			return "combo " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator") + " " + parameter.SafeInt(0);
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			InstantiateArithmeticField(parent, parameter, 1);
			IntInputFieldParam.Instantiate(parent, parameter, 0, "Combo value:", 0);
		}
	}


    // pl1_lives >= 4
    private class BuildGlobalVariable: InternConditionBuilder{
        public BuildGlobalVariable():base("Global Variable"){}
        public override string ToString(GenericParameter parameter){
            return "combo " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(1), "operator") + " " + parameter.SafeInt(0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateArithmeticField(parent, parameter, 1);
            IntInputFieldParam.Instantiate(parent, parameter, 0, "Combo value:", 0);
        }
    }


    // physics_vel_H >= 4
    private class BuildPhysicsVelocity: InternConditionBuilder{
        public BuildPhysicsVelocity():base("Physics Velocity"){}
		public override string ToString(GenericParameter parameter){
			return "impulseV " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(0), "operator") + " " + parameter.SafeFloat(0);
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			InstantiateArithmeticField(parent, parameter, 0);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Compare with impulse:", 0);
		}
	}


    // exists(parent)
    private class BuildExists: InternConditionBuilder{
        public BuildExists():base("Exists"){}
        public override string ToString(GenericParameter parameter){
            return "impulseV " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(0), "operator") + " " + parameter.SafeFloat(0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateArithmeticField(parent, parameter, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Compare with impulse:", 0);
        }
    }


    // team(parent, same_as_self)
    private class BuildTeam: InternConditionBuilder{
        public BuildTeam():base("Team"){}
        public override string ToString(GenericParameter parameter){
            return "impulseV " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(0), "operator") + " " + parameter.SafeFloat(0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateArithmeticField(parent, parameter, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Compare with impulse:", 0);
        }
    }


    // name(parent, ["bat", "sword"])
    private class BuildName: InternConditionBuilder{
        public BuildName():base("Name"){}
        public override string ToString(GenericParameter parameter){
            return "impulseV " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(0), "operator") + " " + parameter.SafeFloat(0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateArithmeticField(parent, parameter, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Compare with impulse:", 0);
        }
    }


    // anim_name(parent, all\["walk"])
    private class BuildAnimationName: InternConditionBuilder{
        public BuildAnimationName():base("Animation Name"){}
        public override string ToString(GenericParameter parameter){
            return "impulseV " + SafeToString(arithmeticOptionsShort, parameter.SafeInt(0), "operator") + " " + parameter.SafeFloat(0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateArithmeticField(parent, parameter, 0);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Compare with impulse:", 0);
        }
    }


#endregion


}

}
