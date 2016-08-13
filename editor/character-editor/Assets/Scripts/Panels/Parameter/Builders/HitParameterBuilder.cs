using UnityEngine;
using System.Collections.Generic;
using RetroBread.Editor;

namespace RetroBread{
	
	public class HitParameterBuilder: ParameterBuilder {
		private static ParameterBuilder instance;
		public static ParameterBuilder Instance {
			get{
				if (instance == null) {
					instance = new HitParameterBuilder();
				}
				return instance;
			}
		}
			
		private static string[] typesList = { "Standard" };

		public static string[] facingOptions = {"location", "inverse location", "orientation", "inverse orientation", "none"};
		public static string[] hitTypeOptions = {"contact", "K.O.", "grab", "electrocution", "burn", "freeze"};

		public override string[] TypesList(){
			return typesList;
		}

		public override string ToString(GenericParameter parameter){
			return typesList[parameter.type];
		}


		public override void Build(GameObject parent, GenericParameter parameter){
			switch (parameter.type) {
				case 0:
					BuildStandard(parent, parameter);
					break;
			}
		}


		private void BuildStandard(GameObject parent, GenericParameter parameter){
			IntDropdownParam.Instantiate(parent, parameter, 0, "Type:", hitTypeOptions);
			IntInputFieldParam.Instantiate(parent, parameter, 1, "Damage");
			IntDropdownParam.Instantiate(parent, parameter, 2, "Hitten Facing:", facingOptions);
		}

	}

}
