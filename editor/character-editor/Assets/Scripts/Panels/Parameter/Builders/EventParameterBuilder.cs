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
        new BuildConsumeInput(),                    // 0: consumeInput(B)
		new BuildSetAnimation(),					// 1: 'walk'
        new BuildSetDeltaPosition(),                // 2: position(2.1, 4.2, 5.3)
		new BuildSetVelocity(),			            // 3: vel(2.3, 1.5, 0.0)
		new BuildSetMaxInputVelocity(),				// 4: inputVel(2.3, 1.5)
		new BuildFlip(),							// 5: flip
		new BuildAutoFlip(),						// 6: autoFlip(false)
		new BuildPausePhysics(),				    // 7: pause(3)
		new BuildSetVariable(),			    		// 8: ++combo
		new BuildGrab(),							// 9: grab(hitten, 2)
		new BuildReleaseGrab(),						// 10: release(all)
		new BuildGetHurt(),							// 11: getHurt(10%)
        new BuildOwnEntity(),                       // 12: own(grab[3])
        new BuildReleaseOwnership(),                // 13: releaseOwnership
        new BuildSpawnEntity(),                     // 14: spawn('bat')
		new BuildSpawnEffect(),						// 15: spawnFX(sparks)
        new BuildDestroy()                          // 16: destroy(self)
	};


	// Types of entity references
	private static string[] entityReferenceType = {"anchored", "parent", "colliding", "hitten", "hitter"};

	private static string[] inputButtonOptions = {"A", "B", "C", "D", "E", "F", "G"};

	private static string[] hurtFacingOptions = {"inherit hit", "location", "inverse location", "orientation", "inverse orientation", "none"};

	private static string[] spawnLocation = {"self", "anchor", "hit intersection", "hurt intersection"};


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
            return operationName + "(" + SafeToString(inputButtonOptions, parameter.SafeInt(1), "entityRef") + ")";
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
        public BuildSetDeltaPosition():base("Instant move"){}
        public override string ToString(GenericParameter parameter){
            return "move(" + parameter.SafeFloatToString(0)
                + ", " + parameter.SafeFloatToString(1)
                + ", " + parameter.SafeFloatToString(2)
                + ")"
                ;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            FloatInputFieldParam.Instantiate(parent, parameter, 0, "delta pos X:");
            FloatInputFieldParam.Instantiate(parent, parameter, 1, "delta pos Y:");
            FloatInputFieldParam.Instantiate(parent, parameter, 2, "delta pos Z:");
        }
    }


    // vel(2.1, 4.2, 5.3)
    private class BuildSetVelocity: InternEventBuilder{
        public BuildSetVelocity():base("Set velocity"){}
		public override string ToString(GenericParameter parameter){
			return "vel(" + parameter.SafeFloatToString(0)
				+ ", " + parameter.SafeFloatToString(1)
				+ ", " + parameter.SafeFloatToString(2)
				+ ")"
			;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Velocity X:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Velocity Y:");
			FloatInputFieldParam.Instantiate(parent, parameter, 2, "Velocity Z:");
		}
	}


    // inputVel(2.1, 4.2, 5.3)
	private class BuildSetMaxInputVelocity: InternEventBuilder{
		public BuildSetMaxInputVelocity():base("Set max input velocity"){}
		public override string ToString(GenericParameter parameter){
			return "inputVel(" + parameter.SafeFloatToString(0)
				+ ", " + parameter.SafeFloatToString(1)
				+ ")"
			;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Max input vel X:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Max input vel Z:");
		}
	}


    // flip
	private class BuildFlip: InternEventBuilder{
		public BuildFlip():base("Flip"){}
		public override string ToString(GenericParameter parameter){
			return "flip";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			// Nothing
		}
	}


    // autoflip(true)
	private class BuildAutoFlip: InternEventBuilder{
		public BuildAutoFlip():base("Automatic flip"){}
		public override string ToString(GenericParameter parameter){
			return "autoFlip(" + parameter.SafeBoolToString(0) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			BoolToggleParam.Instantiate(parent, parameter, 0, "Automatic flip:");
		}
	}


    // pause(2.3)
    private class BuildPausePhysics: InternEventBuilder{
        public BuildPausePhysics():base("Pause physics"){}
		public override string ToString(GenericParameter parameter){
			return "pause(" + parameter.SafeInt(0) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			IntInputFieldParam.Instantiate(parent, parameter, 0, "Delay:");
		}
	}


    // 'combo'++
    private class BuildSetVariable: InternEventBuilder{
        public BuildSetVariable():base("Variable"){}
		public override string ToString(GenericParameter parameter){
			return "++combo";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			IntInputFieldParam.Instantiate(parent, parameter, 0, "Frames to reset:");
		}
	}


	// grab(hitten, 2)
	private class BuildGrab: InternEventBuilder{
        public BuildGrab():base("Grab"){}
		public override string ToString(GenericParameter parameter){
			return "grab("
				+ SafeToString(entityReferenceType, parameter.SafeInt(0), "entityRef")
				+ ", " + parameter.SafeInt(2)
				+ ")"
				;
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			IntDropdownParam.Instantiate(parent, parameter, 0, "Entity Reference:", entityReferenceType);
			// TODO: dynamic content based on the dropdown!..
			IntInputFieldParam.Instantiate(parent, parameter, 1, "Ref param (**):");
			IntDropdownParam.Instantiate(parent, parameter, 2, "Grabbing Anchor:", GetAnchorsNames());
		}
	}


	// release(all)
	private class BuildReleaseGrab: InternEventBuilder{
        public BuildReleaseGrab():base("Release all grabs"){}
		public override string ToString(GenericParameter parameter){
			return "release(all)";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			// Nothing
		}
	}


	// getHurt(10%)
	private class BuildGetHurt: InternEventBuilder{
		public BuildGetHurt():base("Get Hurt"){}
		public override string ToString(GenericParameter parameter){
			int value = parameter.SafeInt(0);
			if (value == 100) return "getHurt";
			return "getHurt(" + value + "%)";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			IntInputFieldParam.Instantiate(parent, parameter, 0, "Damage percentage");
			IntDropdownParam.Instantiate(parent, parameter, 1, "Facing options", hurtFacingOptions);
		}
	}


    // own(grabbed[2])
    private class BuildOwnEntity: InternEventBuilder{
        public BuildOwnEntity():base("Own entity"){}
        public override string ToString(GenericParameter parameter){
            return "own("
                + SafeToString(entityReferenceType, parameter.SafeInt(0), "entityRef")
                + ")"
                ;
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            IntDropdownParam.Instantiate(parent, parameter, 0, "Entity Reference:", entityReferenceType);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Grabbing Anchor:", GetAnchorsNames());
        }
    }


    // releaseOwnership
    private class BuildReleaseOwnership: InternEventBuilder{
        public BuildReleaseOwnership():base("Release Ownership"){}
        public override string ToString(GenericParameter parameter){
            return "releaseOwnership";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            // No parameters
        }
    }

   
    // spawn('bat')
    private class BuildSpawnEntity: InternEventBuilder{
        public BuildSpawnEntity():base("Spawn Entity"){}
        public override string ToString(GenericParameter parameter){
            return "spawn";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
        }
    }


	// spawnFX(sparks)
	private class BuildSpawnEffect: InternEventBuilder{
		public BuildSpawnEffect():base("Spawn Effect"){}
		public override string ToString(GenericParameter parameter){
			return "spawn(" + parameter.SafeString(0) + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
			StringInputFieldParam.Instantiate(parent, parameter, 0, "Effect");
			IntDropdownParam.Instantiate(parent, parameter, 0, "Location", spawnLocation);
			IntInputFieldParam.Instantiate(parent, parameter, 1, "Lifetime");
			BoolToggleParam.Instantiate(parent, parameter, 0, "Local space");
			FloatInputFieldParam.Instantiate(parent, parameter, 0, "Offset X:");
			FloatInputFieldParam.Instantiate(parent, parameter, 1, "Offset Y:");
			FloatInputFieldParam.Instantiate(parent, parameter, 2, "Offset Z:");
		}
	}


    // destroy
    private class BuildDestroy: InternEventBuilder{
        public BuildDestroy():base("Destroy"){}
        public override string ToString(GenericParameter parameter){
            return "destroy";
        }
        public override void Build(GameObject parent, GenericParameter parameter){
        }
    }


#endregion


}

}
