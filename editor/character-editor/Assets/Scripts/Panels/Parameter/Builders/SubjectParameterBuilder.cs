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

    public static int numPredefinedSubjects = 4;
    public static string[] predefinedSubjectsList = {
        "self",             // 0
        "owner",            // 1
        "owner or self",    // 2
        "parent"            // 3
    };


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



#region Helper Functions


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


	// 0: grabbed
	private class BuildGrabbedSubject: InternSubjectBuilder{
		public BuildGrabbedSubject():base("Anchored"){}
		public override string ToString(GenericParameter parameter){
            string options = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(0) == 0);
            return "Anchored(" + options + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            IntDropdownParam.Instantiate(parent, parameter, 0, "Anchor Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Anchor IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}


	// 1: hitter
	private class BuildHitterSubject: InternSubjectBuilder{
		public BuildHitterSubject():base("Hitter"){}
		public override string ToString(GenericParameter parameter){
            string types = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(1) == 0);
            string boxes = ParseOptionsList(parameter.SafeIntsListToString(1), parameter.SafeInt(2) == 0);
            return "Hitter(types:" + types + ", boxes:" + boxes + ")"; 
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            IntDropdownParam.Instantiate(parent, parameter, 0, "Orientation Options", orientationOptions);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Types Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Hit Types");
            IntDropdownParam.Instantiate(parent, parameter, 2, "Box IDs Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 1, "Box IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}


	// 2: hitten
	private class BuildHittenSubject: InternSubjectBuilder{
		public BuildHittenSubject():base("Hitten"){}
        public override string ToString(GenericParameter parameter){
            string types = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(0) == 0);
            string boxes = ParseOptionsList(parameter.SafeIntsListToString(1), parameter.SafeInt(1) == 0);
            return "Hitter(types:" + types + ", boxes:" + boxes + ")"; 
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            IntDropdownParam.Instantiate(parent, parameter, 0, "Types Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Hit Types");
            IntDropdownParam.Instantiate(parent, parameter, 1, "Box IDs Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 1, "Box IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
        }
	}


	// 3: colliding
	private class BuildCollidingSubject: InternSubjectBuilder{
		public BuildCollidingSubject():base("Colliding"){}
		public override string ToString(GenericParameter parameter){
            string boxes = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(0) == 0);
            return "Colliding(" + boxes + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            IntDropdownParam.Instantiate(parent, parameter, 0, "Box IDs Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Box IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}

	// 4: all subjects
	private class AllSubject: InternSubjectBuilder{
		public AllSubject():base("All"){}
		public override string ToString(GenericParameter parameter){
			return "All";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}

#endregion


}

}
