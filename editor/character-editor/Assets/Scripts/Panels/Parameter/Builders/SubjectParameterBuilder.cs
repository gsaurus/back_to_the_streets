using UnityEngine;
using System.Collections.Generic;
using RetroBread.Editor;

namespace RetroBread{
	
	public class SubjectParameterBuilder: ParameterBuilder {
		
		private static ParameterBuilder instance;
		public static ParameterBuilder Instance {
			get{
				if (instance == null) {
					instance = new SubjectParameterBuilder();
				}
				return instance;
			}
		}

		private abstract class InternSubjectBuilder{
			
			public string typeName { get; private set; }

			public abstract string ToString(GenericParameter parameter);
			public abstract void Build(GameObject parent, GenericParameter parameter);

			public InternSubjectBuilder(string typeName){
				this.typeName = typeName;
			}
		}
		
		private static string[] listingOptions = {"any of", "all but"};
		private static string[] orientationOptions = {"any", "from back", "from front"};


		// Subject builders indexed by type directly on array
		private static InternSubjectBuilder[] builders = {
			new BuildGrabbedSubject(),				// 0: grabbed
			new BuildHitterSubject(),				// 1: hitter
			new BuildHittenSubject(),				// 2: hitten
			new BuildCollidingSubject(),			// 3: colliding
			new AllSubject()						// 4: all
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
			return "Unknown subject";
		}


		public override void Build(GameObject parent, GenericParameter parameter){
			if (parameter.type >= 0 && parameter.type < builders.Length) {
				builders[parameter.type].Build(parent, parameter);
			}
		}


			
	#region Builder Classes


		// 0: grabbed
		private class BuildGrabbedSubject: InternSubjectBuilder{
			public BuildGrabbedSubject():base("Anchored"){}
			public override string ToString(GenericParameter parameter){
//				int frameNum = parameter.SafeInt(0);
//				if (frameNum == 0) {
//					return "first frame";
//				} else if (frameNum > 0) {
//					return "frame " + frameNum;
//				}
//				return "last frame";
				return null;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
//				IntInputFieldParam.Instantiate(parent, parameter, 0, "At frame:");
			}
		}


		// 1: hitter
		private class BuildHitterSubject: InternSubjectBuilder{
			public BuildHitterSubject():base("Hitter"){}
			public override string ToString(GenericParameter parameter){
				return null;
			}
			public override void Build(GameObject parent, GenericParameter parameter){
				
			}
		}


		// 2: hitten
		private class BuildHittenSubject: InternSubjectBuilder{
			public BuildHittenSubject():base("Hitten"){}
			public override string ToString(GenericParameter parameter){
				return null;
			}
			public override void Build(GameObject parent, GenericParameter parameter){

			}
		}


		// 3: colliding
		private class BuildCollidingSubject: InternSubjectBuilder{
			public BuildCollidingSubject():base("Colliding"){}
			public override string ToString(GenericParameter parameter){
				return null;
			}
			public override void Build(GameObject parent, GenericParameter parameter){

			}
		}

		// 4: all subjects
		private class AllSubject: InternSubjectBuilder{
			public AllSubject():base("All"){}
			public override string ToString(GenericParameter parameter){
				return null;
			}
			public override void Build(GameObject parent, GenericParameter parameter){

			}
		}

	#endregion


	}

}
