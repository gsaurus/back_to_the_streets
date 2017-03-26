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
		private bool isInToString = false;
		public string ToString(GenericParameter parameter){
			if (isInToString) {
				return "ERROR - RECURSIVE SUBJECT";
			}
				isInToString = true;
				string theToString = InternalToString(parameter);
				isInToString = false;
				return theToString;
		}

		public abstract string InternalToString(GenericParameter parameter);
		public abstract void Build(GameObject parent, GenericParameter parameter);

		public InternSubjectBuilder(string typeName){
			this.typeName = typeName;
		}
	}
	
	private static string[] listingOptions = {"any of", "all but"};
	private static string[] orientationOptions = {"any", "from back", "from front"};

    public static string[] predefinedSubjectsList = {
        "self"             // 0
    };


	// Subject builders indexed by type directly on array
	private static InternSubjectBuilder[] builders = {
        new BuildOwnerSubject(),                // 0: owner
        new BuildOwnerOrSelfSubject(),          // 1: ownerOrSelf
        new BuildParentSubject(),               // 2: parent
		new BuildGrabbedSubject(),				// 3: grabbed
		new BuildHitterSubject(),				// 4: hitter
		new BuildHittenSubject(),				// 5: hitten
		new BuildCollidingSubject(),			// 6: colliding
		new AllSubject()						// 7: all
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


    // 0: owner
    private class BuildOwnerSubject: InternSubjectBuilder{
        public BuildOwnerSubject():base("Owner"){}
		public override string InternalToString(GenericParameter parameter){
            return "Owner" + SubjectString(parameter, 0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
        }
    }

    // 1: owner_or_self
    private class BuildOwnerOrSelfSubject: InternSubjectBuilder{
        public BuildOwnerOrSelfSubject():base("Owner or Self"){}
		public override string InternalToString(GenericParameter parameter){
            return "OwnerOrSelf" + SubjectString(parameter, 0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
        }
    }

    // 2: parent
    private class BuildParentSubject: InternSubjectBuilder{
        public BuildParentSubject():base("Parent"){}
		public override string InternalToString(GenericParameter parameter){
            return "Parent" + SubjectString(parameter, 0);
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
        }
    }


	// 3: grabbed
	private class BuildGrabbedSubject: InternSubjectBuilder{
		public BuildGrabbedSubject():base("Anchored"){}
		public override string InternalToString(GenericParameter parameter){
            string options = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(1) == 0);
            return "Anchored" + SubjectString(parameter, 0) + "(" + options + ")";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Anchor Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Anchor IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}


	// 4: hitter
	private class BuildHitterSubject: InternSubjectBuilder{
		public BuildHitterSubject():base("Hitter"){}
		public override string InternalToString(GenericParameter parameter){
            string types = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(2) == 0);
            string boxes = ParseOptionsList(parameter.SafeIntsListToString(1), parameter.SafeInt(3) == 0);
            return "Hitter" + SubjectString(parameter, 0) + "(types:" + types + ", boxes:" + boxes + ")"; 
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Orientation Options", orientationOptions);
            IntDropdownParam.Instantiate(parent, parameter, 2, "Types Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Hit Types");
            IntDropdownParam.Instantiate(parent, parameter, 3, "Box IDs Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 1, "Box IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}


	// 5: hitten
	private class BuildHittenSubject: InternSubjectBuilder{
		public BuildHittenSubject():base("Hitten"){}
		public override string InternalToString(GenericParameter parameter){
            string types = ParseOptionsList(parameter.SafeIntsListToString(0), parameter.SafeInt(1) == 0);
            string boxes = ParseOptionsList(parameter.SafeIntsListToString(1), parameter.SafeInt(2) == 0);
            return "Hitten" + SubjectString(parameter, 0) + "(types:" + types + ", boxes:" + boxes + ")"; 
        }
        public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            IntDropdownParam.Instantiate(parent, parameter, 1, "Types Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 0, "Hit Types");
            IntDropdownParam.Instantiate(parent, parameter, 2, "Box IDs Options", listingOptions);
            IntListInputFieldParam.Instantiate(parent, parameter, 1, "Box IDs");
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
        }
	}


	// 6: colliding
	private class BuildCollidingSubject: InternSubjectBuilder{
		public BuildCollidingSubject():base("Colliding"){}
		public override string InternalToString(GenericParameter parameter){
            return "Colliding" + SubjectString(parameter, 0);
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            InstantiateSubject(parent, parameter, 0);
            // No more parameters, currently only one entity colliding at any collision box is supported
		}
	}

	// 7: all subjects
	private class AllSubject: InternSubjectBuilder{
		public AllSubject():base("All"){}
		public override string InternalToString(GenericParameter parameter){
			return "All";
		}
		public override void Build(GameObject parent, GenericParameter parameter){
            BoolToggleParam.Instantiate(parent, parameter, 0, "Single Subject");
		}
	}

#endregion


}

}
