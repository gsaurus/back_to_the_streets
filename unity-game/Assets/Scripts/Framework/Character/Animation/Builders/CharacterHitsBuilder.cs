using System;
using System.Collections.Generic;

namespace RetroBread{


	public static class CharacterHitsBuilder {

		// Hits builders indexed by type directly on array
		private delegate HitData BuilderAction(Storage.GenericParameter param);
		private static BuilderAction[] builderActions = {
			BuildStandardHit
		};
			

		// The public builder method
		public static HitData Build(Storage.GenericParameter param){
			int callIndex = param.type;
			if (callIndex < builderActions.Length) {
				return builderActions[callIndex](param);
			}
			Debug.Log("CharacterHitsBuilder: Unknown hit type: " + param.type);
			return null;
		}



#region Conditions


		// frame = 4
		private static HitData BuildStandardHit(Storage.GenericParameter parameter){
			return new HitData((HitData.HitType) parameter.SafeInt(0), parameter.SafeInt(1), (HitData.HitFacing) parameter.SafeInt(2));
		}


#endregion


	}

}
