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
			new BuildPlayAnimation(),					// 0: play(win)
			new BuildSetAnimationParam(),				// 1: energy=23
			new BuildSetTexture(),						// 2: texture(axel)
			new BuildSetText(),							// 3: label(name)
			new BuildSpawnEffect()						// 4: spawnFX(sparks)
		};


		// Types of entity references
//		private static string[] entityReferenceType = {"anchored", "parent", "colliding", "hitten", "hitter"};

		private static string[] variableFromOptions = {"Variable", "Custom"};

		private static string[] textureFromOptions = {"Character Portrait", "Custom"};

		private static string[] textFromOptions = {"Character Name", "Variable", "Custom"};

		private static string[] spawnLocation = {"hud obj", "character obj", "anchor", "hit intersection", "hurt intersection"};
	

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
			

		// 'walk'
		private class BuildPlayAnimation: InternEventBuilder{
			public BuildPlayAnimation():base("Set Animation"){}
			public override string ToString(GenericParameter parameter){
				return "'" + parameter.SafeString(0) + "'"; 
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Animation:");
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "Transition time:", 0);
			}
		}


		// energy=4
		private class BuildSetAnimationParam: InternEventBuilder{
			public BuildSetAnimationParam():base("Set Animation Parameter"){}
			public override string ToString(GenericParameter parameter){
				switch (parameter.SafeInt(0)) {
					case 0: // variable
						return "'" + parameter.SafeString(0) + "'='" + parameter.SafeString(0) + "'[" + parameter.SafeInt(1) + "-" + parameter.SafeInt(2) + "]";
					default: // custom
						return "'" + parameter.SafeString(0) + "'=" + parameter.SafeFloat(0);
				}
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Parameter Name:");
				IntDropdownParam.Instantiate(parent, parameter, 0, "From:", variableFromOptions);
				StringInputFieldParam.Instantiate(parent, parameter, 1, "From variable:");
				IntInputFieldParam.Instantiate(parent, parameter, 1, "Variable minimum value:");
				IntInputFieldParam.Instantiate(parent, parameter, 2, "Variable maximum value:");
				FloatInputFieldParam.Instantiate(parent, parameter, 0, "Custom Value:");
				FloatInputFieldParam.Instantiate(parent, parameter, 1, "Delay time (seconds):");
				FloatInputFieldParam.Instantiate(parent, parameter, 2, "Interpolation time (seconds):");
			}
		}


		// texture(portrait)
		private class BuildSetTexture: InternEventBuilder{
			public BuildSetTexture():base("Set Texture"){}
			public override string ToString(GenericParameter parameter){
				string textureName;
				switch (parameter.SafeInt(0)) {
					case 0: // portrait
						textureName = "portrait";
						break;
					default: // custom
						textureName = parameter.SafeString(0);
						break;
				}
				return "texture(" + textureName + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "From:", textureFromOptions);
				StringInputFieldParam.Instantiate(parent, parameter, 0, "Custom Texture:");
			}
		}


		// text(name)
		private class BuildSetText: InternEventBuilder{
			public BuildSetText():base("Set Text"){}
			public override string ToString(GenericParameter parameter){
				string text;
				switch (parameter.SafeInt(0)) {
					case 0: // name
						text = "name";
						break;
					case 1: // variable
						text = string.Format(parameter.SafeString(1), "'" + parameter.SafeString(0) + "'");
						if (string.IsNullOrEmpty(text)) {
							text = "'" + parameter.SafeString(0) + "'";
						}
						break;
					default: // custom
						text = parameter.SafeString(1);
						break;
				}
				return "text(" + text + ")";
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				IntDropdownParam.Instantiate(parent, parameter, 0, "From:", textFromOptions);
				StringInputFieldParam.Instantiate(parent, parameter, 0, "From Variable:");
				StringInputFieldParam.Instantiate(parent, parameter, 1, "Custom Text:");
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

#endregion


	}

}
