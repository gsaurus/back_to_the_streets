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
			new BuildSetAnimation(),					// 0: 'walk'
			new BuildZeroAnimationVelocity(),			// 1: vel(zero)
			new BuildSetAnimationVelocity(),			// 2: vel(2.3, 1.5, 0.0)
			new BuildZeroMaxInputVelocity(),			// 3: inputVel(zero)
			new BuildSetMaxInputVelocity(),				// 4: inputVel(2.3, 1.5)
			new BuildAddAnimationVerticalImpulse(),		// 5: impulseV(1.5)
			new BuildFlip(),							// 6: flip
			new BuildAutoFlip(),						// 7: autoFlip(false)
			new BuildDelay(),							// 8: pause(3)
			new BuildComboIncrement(),					// 9: ++combo
			new BuildComboReset(),						// 10: reset(combo)
			new BuildInstantMove(),						// 11: instantMove(2.1, 4.2, 5.3)
			new BuildAnchorWithMove(),					// 12: grab(hitten, 2, (2.1, 4.2, 5.3))
			new BuildAnchor(),							// 13: grab(hitten, 2)
			new BuildReleaseAll(),						// 14: release(all)
			new BuildRelease(),							// 15: release(2)
			new BuildSetAnchoredPos(),					// 16: grabbedPos(2, (2.1, 4.2, 5.3))
			new BuildSetAnchoredAnim(),					// 17: grabbedAnim(2, jump)
			new BuildRefImpulse(),						// 18: impulse(hitten, (2.1, 4.2, 5.3))
			new BuildResetImpulse(),					// 19: reset(impulse)
			new BuildConsumeInput(),					// 20: consumeInput(B)
			new BuildGetHurt(),							// 21: getHurt(10%)
			new BuildSpawnEffect(),						// 22: spawnFX(sparks)
			new BuildOwnEntity(),						// 23: own(anchored, 2)
			new BuildReleaseOwnership()					// 24: releaseOwnership
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


		private class BuildSetAnimation: InternEventBuilder{
			public BuildSetAnimation():base("Jump to Animation"){}
			public override string ToString(GenericParameter parameter){
				return "'" + parameter.SafeString(0) + "'";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				StringDropdownParam.Instantiate(parent, parameter, 0, "Animation:", GetAnimationsNames());
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "Transition time:", 0);
			}
		}


		private class BuildZeroAnimationVelocity: InternEventBuilder{
			public BuildZeroAnimationVelocity():base("Stop velocity"){}
			public override string ToString(GenericParameter parameter){
				return "vel(zero)";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// No parameters
			}
		}


		private class BuildSetAnimationVelocity: InternEventBuilder{
			public BuildSetAnimationVelocity():base("Set velocity"){}
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


		private class BuildZeroMaxInputVelocity: InternEventBuilder{
			public BuildZeroMaxInputVelocity():base("Stop input velocity"){}
			public override string ToString(GenericParameter parameter){
				return "inputVel(zero)";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// Nothing
			}
		}


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


		private class BuildAddAnimationVerticalImpulse: InternEventBuilder{
			public BuildAddAnimationVerticalImpulse():base("Add vertical impulse"){}
			public override string ToString(GenericParameter parameter){
				return "impulseV(" + parameter.SafeFloatToString(0) + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "Vertical Impulse:");
			}
		}


		private class BuildFlip: InternEventBuilder{
			public BuildFlip():base("Flip"){}
			public override string ToString(GenericParameter parameter){
				return "flip";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// Nothing
			}
		}


		private class BuildAutoFlip: InternEventBuilder{
			public BuildAutoFlip():base("Automatic flip"){}
			public override string ToString(GenericParameter parameter){
				return "autoFlip(" + parameter.SafeBoolToString(0) + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				BoolToggleParam.Instantiate(parent, parameter, 0, "Automatic flip:");
			}
		}


		private class BuildDelay: InternEventBuilder{
			public BuildDelay():base("Pause physics"){}
			public override string ToString(GenericParameter parameter){
				return "pause(" + parameter.SafeInt(0) + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntInputFieldParam.Instantiate(parent, parameter, 0, "Delay:");
			}
		}


		private class BuildComboIncrement: InternEventBuilder{
			public BuildComboIncrement():base("Combo increment"){}
			public override string ToString(GenericParameter parameter){
				return "++combo";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntInputFieldParam.Instantiate(parent, parameter, 0, "Frames to reset:");
			}
		}


		private class BuildComboReset: InternEventBuilder{
			public BuildComboReset():base("Combo reset"){}
			public override string ToString(GenericParameter parameter){
				return "reset(combo)";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// Nothing
			}
		}


	#region Anchoring



		// instantMove(2.1, 4.2, 5.3)
		private class BuildInstantMove: InternEventBuilder{
			public BuildInstantMove():base("Instant move"){}
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


		// grab(hitten, 2, (2.1, 4.2, 5.3)), position relative to entity being grabbed
		private class BuildAnchorWithMove: InternEventBuilder{
			public BuildAnchorWithMove():base("Grab & adjust position"){}
			public override string ToString(GenericParameter parameter){
				return "grab("
					+ SafeToString(entityReferenceType, parameter.SafeInt(0), "entityRef")
					+ ", " + parameter.SafeInt(2)
					+ ", ("+ parameter.SafeFloatToString(0)
					+ ", " + parameter.SafeFloatToString(1)
					+ ", " + parameter.SafeFloatToString(2)
					+ "))"
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Entity Reference:", entityReferenceType);
				// TODO: dynamic content based on the dropdown!..
				IntInputFieldParam.Instantiate(parent, parameter, 1, "Ref param (**):");
				IntDropdownParam.Instantiate(parent, parameter, 2, "Grabbing Anchor:", GetAnchorsNames());
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "delta pos X:");
				FloatInputFieldParam.Instantiate(parent, parameter, 1, "delta pos Y:");
				FloatInputFieldParam.Instantiate(parent, parameter, 2, "delta pos Z:");
			}
		}


		// grab(hitten, 2)
		private class BuildAnchor: InternEventBuilder{
			public BuildAnchor():base("Grab"){}
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
		private class BuildReleaseAll: InternEventBuilder{
			public BuildReleaseAll():base("Release all grabs"){}
			public override string ToString(GenericParameter parameter){
				return "release(all)";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// Nothing
			}
		}


		// release(2)
		private class BuildRelease: InternEventBuilder{
			public BuildRelease():base("Release specific grab"){}
			public override string ToString(GenericParameter parameter){
				return "release("
					+ parameter.SafeInt(0)
					+ ")"
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Grabbing Anchor:", GetAnchorsNames());
			}
		}


		// grabbedPos(2, (2.1, 4.2, 5.3)), relative to main entity
		private class BuildSetAnchoredPos: InternEventBuilder{
			public BuildSetAnchoredPos():base("Set grabbed position"){}
			public override string ToString(GenericParameter parameter){
				return "grabbedPos("
					+ parameter.SafeInt(0)
					+ ", ("+ parameter.SafeFloatToString(0)
					+ ", " + parameter.SafeFloatToString(1)
					+ ", " + parameter.SafeFloatToString(2)
					+ "))"
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Grabbing Anchor:", GetAnchorsNames());
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "delta pos X:");
				FloatInputFieldParam.Instantiate(parent, parameter, 1, "delta pos Y:");
				FloatInputFieldParam.Instantiate(parent, parameter, 2, "delta pos Z:");
			}
		}


		// grabbedAnim(2, jump)
		private class BuildSetAnchoredAnim: InternEventBuilder{
			public BuildSetAnchoredAnim():base("Set grabbed animation"){}
			public override string ToString(GenericParameter parameter){
				return "grabbedAnim(" +
					+ parameter.SafeInt(0)
					+ ", " + parameter.SafeString(0)
					+ ")"
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Grabbing Anchor:", GetAnchorsNames());
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Animation:");
			}
		}


	#endregion


		// impulse(hitten, (2.1, 4.2, 5.3))
		private class BuildRefImpulse: InternEventBuilder{
			public BuildRefImpulse():base("Impulse Other"){}
			public override string ToString(GenericParameter parameter){
				return "impulse("
					+ SafeToString(entityReferenceType, parameter.SafeInt(0), "entityRef")
					+ ", ("+ parameter.SafeFloatToString(0)
					+ ", " + parameter.SafeFloatToString(1)
					+ ", " + parameter.SafeFloatToString(2)
					+ "))"
				;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Entity Reference:", entityReferenceType);
				// TODO: dynamic content based on the dropdown!..
				IntInputFieldParam.Instantiate(parent, parameter, 1, "Ref param (**):");
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "delta pos X:");
				FloatInputFieldParam.Instantiate(parent, parameter, 1, "delta pos Y:");
				FloatInputFieldParam.Instantiate(parent, parameter, 2, "delta pos Z:");
			}
		}


		// reset(impulse)
		private class BuildResetImpulse: InternEventBuilder{
			public BuildResetImpulse():base("Reset XZ Impulse"){}
			public override string ToString(GenericParameter parameter){
				return "reset(impulse)";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// Nothing
			}
		}


		// consumeInput(B)
		private class BuildConsumeInput: InternEventBuilder{
			public BuildConsumeInput():base("Consume Input"){}
			public override string ToString(GenericParameter parameter){
				return "consumeInput(" + SafeToString(inputButtonOptions, parameter.SafeInt(0), "entityRef")  + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "Button:", inputButtonOptions);
				BoolToggleParam.Instantiate(parent, parameter, 0, "Consume Release Event");
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

		// 23: own(anchored, 2) (take ownership of an entity)
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

	
		// 24: releaseOwnership (the item itself gets free from his owner)
		private class BuildReleaseOwnership: InternEventBuilder{
			public BuildReleaseOwnership():base("Release Ownership"){}
			public override string ToString(GenericParameter parameter){
				return "releaseOwnership";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				// No parameters
			}
		}

#endregion


	}

}
