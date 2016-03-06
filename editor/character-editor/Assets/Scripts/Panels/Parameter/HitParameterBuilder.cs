using UnityEngine;
using System.Collections;
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
			
		private string[] typesList = { "test 1", "test 2" };

		public string[] TypesList(){
			return typesList;
		}


		public void Build(GameObject parent, GenericParameter parameter, int type){
			switch (type) {
				case 0:
					BuildTest1(parent, parameter);
					break;
				case 1:
					BuildTest2(parent, parameter);
					break;
			}
		}


		private void BuildTest1(GameObject parent, GenericParameter parameter){
			
		}

		private void BuildTest2(GameObject parent, GenericParameter parameter){

		}

	}

}
