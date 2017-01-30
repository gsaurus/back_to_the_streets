using UnityEngine;
using System.Collections.Generic;
using RetroBread.Editor;

namespace RetroBread{

public class EventParameterBuilder: ParameterBuilder {
	private static ParameterBuilder instance;
	public static ParameterBuilder Instance {
		get{
			if (instance == null) {
				instance = new EventParameterBuilder();
			}
			return instance;
		}
	}

	private abstract class InternEventBuilder{

		public string typeName { get; private set; }

		public abstract string ToString(GenericParameter parameter);
		public abstract void Build(GameObject parent, GenericParameter parameter);

		public InternEventBuilder(string typeName){
			this.typeName = typeName;
		}
	}


	// Condition builders indexed by type directly on array
	private static InternEventBuilder[] builders = {
        new BuildConsumeInput(),
		new BuildSetAnimation(),
        new BuildSetDeltaPosition(),
		new BuildSetVelocity(),
		new BuildSetMaxInputVelocity(),
		new BuildFlip(),
		new BuildAutoFlip(),
		new BuildPausePhysics(),
		new BuildSetVariable(),
        new BuildSetGlobalVariable(),
		new BuildGrab(),
		new BuildReleaseGrab(),
		new BuildGetHurt(),
        new BuildOwnEntity(),
        new BuildReleaseOwnership(),
        new BuildSpawnEntity(),
		new BuildSpawnEffect(),
        new BuildDestroy()
	};


	// Types of entity references
	private static string[] entityReferenceType = {"anchored", "parent", "colliding", "hitten", "hitter"};

	private static string[] inputButtonOptions = {"A", "B", "C", "D", "E", "F", "G"};

	private static string[] hurtFacingOptions = {"inherit hit", "location", "inverse location", "orientation", "inverse orientation", "none"};

	private static string[] spawnLocation = {"origin", "anchor", "hit intersection", "hurt intersection"};

    private static string[] maskOptions = { "X", "Y", "Z", "XY", "XZ", "YZ", "XYZ" };

    private static string[] setOrAddOptions = { "set", "add" };


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
		return "Unknown event";
	}


	public override void Build(GameObject parent, GenericParameter parameter){
		if (parameter.type >= 0 && parameter.type < builders.Length) {
			builders[parameter.type].Build(parent, parameter);
		}
	}



#region Builders

    // consumeInput(B)
    private class BuildConsumeInput: InternEventBuilder{
        public BuildConsumeInput():base("Consume Input"){}
        public override string ToString(GenericParameter parameter){
            string operationName = "consumeInput" + SubjectString(parameter, 0);
            return operationName + "(" + SafeToString(inputButtonOptions, parameter.SafeInt(1), "Button ID") + ")";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Button:", inputButtonOptions);
            BoolToggleParam.Instantiate(parent, parameter, 0, "Consume Release Event");
        }
    }


    // 'walk'
	private class BuildSetAnimation: InternEventBuilder{
		public BuildSetAnimation():base("Set Animation"){}
		public override string ToString(GenericParameter parameter){
			return "'" + parameter.SafeString(0) + "'" + SubjectString(parameter, 0);
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Animation:");
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Transition time:", 0);
		}
	}


    // pos(2.1, 4.2, 5.3)
    private class BuildSetDeltaPosition: InternEventBuilder{
        public BuildSetDeltaPosition():base("Set Position"){}
        public override string ToString(GenericParameter parameter){
            int mask = parameter.SafeInt(2);
            bool hasX = mask == 0 || mask == 3 || mask == 4 || mask == 6;
            bool hasY = mask == 1 || mask == 3 || mask == 5 || mask == 6;
            bool hasZ = mask == 2 || mask == 4 || mask == 5 || mask == 6;
            string paramsString;
            switch (mask){
                case 0:
                    paramsString = parameter.SafeFloat(0) + "";
                    break;
                case 1:
                    paramsString = parameter.SafeFloat(1) + "";
                    break;
                case 2:
                    paramsString = parameter.SafeFloat(2) + "";
                    break;
                case 3:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(1);
                    break;
                case 4:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(2);
                    break;
                case 5:
                    paramsString = parameter.SafeFloat(1) + ", " + parameter.SafeFloat(2);
                    break;
                default:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(1) + ", " + parameter.SafeFloat(2);
                    break;
            }
            return "pos" + SafeToString(maskOptions, mask, "Mask")
                + SubjectString(parameter, 0) + SubjectString(parameter, 1)
                + "(" + paramsString + ")"
            ;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Relative to:");
            IntDropdownParam.Instantiate(parent, parameter, 2, "Mask:", maskOptions);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "delta pos X:");
            FloatInputFieldParam.Instantiate(parent, parameter, 1, "delta pos Y:");
            FloatInputFieldParam.Instantiate(parent, parameter, 2, "delta pos Z:");
        }
    }


    // vel(2.1, 4.2, 5.3)
    private class BuildSetVelocity: InternEventBuilder{
        public BuildSetVelocity():base("Set Velocity"){}
		public override string ToString(GenericParameter parameter){
            int mask = parameter.SafeInt(1);
            bool hasX = mask == 0 || mask == 3 || mask == 4 || mask == 6;
            bool hasY = mask == 1 || mask == 3 || mask == 5 || mask == 6;
            bool hasZ = mask == 2 || mask == 4 || mask == 5 || mask == 6;
            string paramsString;
            switch (mask){
                case 0:
                    paramsString = parameter.SafeFloat(0) + "";
                    break;
                case 1:
                    paramsString = parameter.SafeFloat(1) + "";
                    break;
                case 2:
                    paramsString = parameter.SafeFloat(2) + "";
                    break;
                case 3:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(1);
                    break;
                case 4:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(2);
                    break;
                case 5:
                    paramsString = parameter.SafeFloat(1) + ", " + parameter.SafeFloat(2);
                    break;
                default:
                    paramsString = parameter.SafeFloat(0) + ", " + parameter.SafeFloat(1) + ", " + parameter.SafeFloat(2);
                    break;
            }
            return "vel" + SafeToString(maskOptions, mask, "Mask")
                + SubjectString(parameter, 0)
                + "(" + paramsString + ")"
                ;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Mask:", maskOptions);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Velocity X:");
            FloatInputFieldParam.Instantiate(parent, parameter, 1, "Velocity Y:");
            FloatInputFieldParam.Instantiate(parent, parameter, 2, "Velocity Z:");
		}
	}


    // inputVel(2.1, 4.2, 5.3)
	private class BuildSetMaxInputVelocity: InternEventBuilder{
		public BuildSetMaxInputVelocity():base("Set max input velocity"){}
		public override string ToString(GenericParameter parameter){
            return "inputVel" + SubjectString(parameter, 0)
                + "(" + parameter.SafeFloatToString(0)
				+ ", " + parameter.SafeFloatToString(1)
				+ ")"
			;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Max input vel X:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Max input vel Z:");
		}
	}


    // flip
	private class BuildFlip: InternEventBuilder{
		public BuildFlip():base("Flip"){}
		public override string ToString(GenericParameter parameter){
            return "flip" + SubjectString(parameter, 0);
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
		}
	}


    // autoflip(true)
	private class BuildAutoFlip: InternEventBuilder{
		public BuildAutoFlip():base("Automatic flip"){}
		public override string ToString(GenericParameter parameter){
            return "autoFlip" + SubjectString(parameter, 0) + "(" + parameter.SafeBoolToString(0) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			BoolToggleParam.Instantiate(parent, parameter, 0, "Automatic flip:");
		}
	}


    // pause(2.3)
    private class BuildPausePhysics: InternEventBuilder{
        public BuildPausePhysics():base("Pause physics"){}
		public override string ToString(GenericParameter parameter){
            return "pause" + SubjectString(parameter, 0)
                + "(" + NumeratorString(parameter, 1, 0, parameter.SafeInt(2) + "") + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateNumeratorVar(parent, parameter, 1, 0);
            IntInputFieldParam.Instantiate(parent, parameter, 2, "Or use value:");
		}
	}


    // combo+=1
    private class BuildSetVariable: InternEventBuilder{
        public BuildSetVariable():base("Variable"){}
		public override string ToString(GenericParameter parameter){
            string varName = parameter.SafeString(0);
            string operationString;
            switch (parameter.SafeInt(1)){
                case 0: operationString = "="; break;
                default: operationString = "+="; break;
            }
            return varName + SubjectString(parameter, 0)
                + operationString
                + NumeratorString(parameter, 2, 1, parameter.SafeInt(3) + "")
            ;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Variable:");
            IntDropdownParam.Instantiate(parent, parameter, 1, "Mode:", setOrAddOptions);
            InstantiateNumeratorVar(parent, parameter, 2, 1);
            IntInputFieldParam.Instantiate(parent, parameter, 3, "Or select value:");
		}
	}


    // lives+=1
    private class BuildSetGlobalVariable: InternEventBuilder{
        public BuildSetGlobalVariable():base("Global Variable"){}
        public override string ToString(GenericParameter parameter){
            string varName = parameter.SafeString(0);
            string operationString;
            switch (parameter.SafeInt(0)){
                case 0: operationString = "="; break;
                default: operationString = "+="; break;
            }
            return varName + "[global]"
                + operationString
                + NumeratorString(parameter, 1, 1, parameter.SafeInt(2) + "")
                ;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Global variable:");
            IntDropdownParam.Instantiate(parent, parameter, 0, "Mode:", setOrAddOptions);
            InstantiateNumeratorVar(parent, parameter, 1, 1);
            IntInputFieldParam.Instantiate(parent, parameter, 2, "Or select value:");
        }
    }


	// grab
	private class BuildGrab: InternEventBuilder{
        public BuildGrab():base("Grab"){}
		public override string ToString(GenericParameter parameter){
            return "grab" + SubjectString(parameter, 0)
                + "(" + SubjectString(parameter, 1) + ", " +parameter.SafeInt(2) + ")"
				;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Subject to grab:");
            IntInputFieldParam.Instantiate(parent, parameter, 2, "Anchor ID:");
		}
	}


	// releaseGrab
	private class BuildReleaseGrab: InternEventBuilder{
        public BuildReleaseGrab():base("Release Grab"){}
		public override string ToString(GenericParameter parameter){
            return "releaseGrab" + SubjectString(parameter, 0) + "(" + SubjectString(parameter, 1) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Subject to release:");
		}
	}


	// getHurt(10%)
	private class BuildGetHurt: InternEventBuilder{
		public BuildGetHurt():base("Get Hurt"){}
		public override string ToString(GenericParameter parameter){
			int value = parameter.SafeInt(3);
            string valueString = "";
            if (value != 100){
                valueString = ", " + value + "%";
            }
            return "getHurt" + SubjectString(parameter, 0)
                + "(" + SubjectString(parameter, 1) + valueString + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Hitters:");
            InstantiateNumeratorVar(parent, parameter, 2, 0);
			IntInputFieldParam.Instantiate(parent, parameter, 3, "Damage percentage:");
			IntDropdownParam.Instantiate(parent, parameter, 4, "Facing options:", hurtFacingOptions);
		}
	}


    // own(grabbed[2])
    private class BuildOwnEntity: InternEventBuilder{
        public BuildOwnEntity():base("Own entity"){}
        public override string ToString(GenericParameter parameter){
            return "own" + SubjectString(parameter, 0) + "(" + SubjectString(parameter, 1) + ")";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Subject to be Owned:");
        }
    }


    // releaseOwnership
    private class BuildReleaseOwnership: InternEventBuilder{
        public BuildReleaseOwnership():base("Release Ownership"){}
        public override string ToString(GenericParameter parameter){
            return "releaseOwnership" + SubjectString(parameter, 0) + "(" + SubjectString(parameter, 1) + ")";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            InstantiateSubject(parent, parameter, 1, "Subject to be release:");
        }
    }

   
    // spawn('bat')
    private class BuildSpawnEntity: InternEventBuilder{
        public BuildSpawnEntity():base("Spawn Entity"){}
        public override string ToString(GenericParameter parameter){
            return "spawn" + SubjectString(parameter, 0) + "(" + parameter.SafeString(0) + ")";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            StringInputFieldParam.Instantiate(parent, parameter, 0, "Entity to spawn:");
            IntDropdownParam.Instantiate(parent, parameter, 1, "Localtion:", spawnLocation);
            IntInputFieldParam.Instantiate(parent, parameter, 2, "Location anchor ID:");
            StringInputFieldParam.Instantiate(parent, parameter, 1, "Initial animation:");
            IntInputFieldParam.Instantiate(parent, parameter, 3, "Team ID (-1 for same team):");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Own:");
            StringListInputFieldParam.Instantiate(parent, parameter, 0, "Variable keys:");
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Variable values:");
            IntDropdownParam.Instantiate(parent, parameter, 4, "Facing options:", hurtFacingOptions);
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "Offset X:");
            FloatInputFieldParam.Instantiate(parent, parameter, 1, "Offset Y:");
            FloatInputFieldParam.Instantiate(parent, parameter, 2, "Offset Z:");
        }
    }


	// spawnFX(sparks)
	private class BuildSpawnEffect: InternEventBuilder{
		public BuildSpawnEffect():base("Spawn Effect"){}
		public override string ToString(GenericParameter parameter){
            return "spawnFX" + SubjectString(parameter, 0) + "(" + parameter.SafeString(0) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
			StringInputFieldParam.Instantiate(parent, parameter, 0, "Effect");
            IntDropdownParam.Instantiate(parent, parameter, 1, "Localtion:", spawnLocation);
            IntInputFieldParam.Instantiate(parent, parameter, 2, "Location anchor ID:");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Local Space:");
            IntDropdownParam.Instantiate(parent, parameter, 3, "Facing options:", hurtFacingOptions);
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Offset X:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Offset Y:");
			FloatInputFieldParam.Instantiate(parent, parameter, 2, "Offset Z:");
		}
	}


    // destroy
    private class BuildDestroy: InternEventBuilder{
        public BuildDestroy():base("Destroy"){}
        public override string ToString(GenericParameter parameter){
            return "destroy" + SubjectString(parameter, 0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
        }
    }


#endregion


}

}
