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
			new BuildDelay()							// 8: pause(3)
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
				return builders[parameter.type].ToString(parameter);
			}
			return "Unknown event";
		}


		public void Build(GameObject parent, GenericParameter parameter){
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
				Character character = CharacterEditor.Instance.character;
				List<string> animNames = new List<string>();
				if (character != null) {
					foreach (CharacterAnimation anim in character.animations) {
						animNames.Add(anim.name);
					}
				}
				StringDropdownParam.Instantiate(parent, parameter, 0, "Animation:", animNames.ToArray());
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


#endregion


	}

}
