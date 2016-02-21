using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RetroBread;


namespace RetroBread.Editor{

	public class HitBox {

		public List<bool> enabledFrames;
		public List<Box> boxesPerFrame;
		public GenericParameter param;

		// This is a temporary builder variable
		// it's populated during save, and discarded at save end 
		private Storage.HitBox storageBox;


		public HitBox(){
			// Nothing to do here
		}


		public static HitBox LoadFromStorage(Storage.HitBox storageBox, Storage.Character storageCharacter){

			HitBox newBox = new HitBox();

			// Populate boxes
			newBox.boxesPerFrame = new List<Box>(storageBox.boxIds.Length);
			newBox.enabledFrames = new List<bool>(storageBox.boxIds.Length);
			Box addedBox;
			foreach(int boxId in storageBox.boxIds){
				addedBox = Box.LoadFromStorage(storageCharacter.boxes[boxId]);
				newBox.boxesPerFrame.Add(addedBox);
				newBox.enabledFrames.Add(addedBox != null);
			}

			newBox.param = GenericParameter.LoadFromStorage( storageCharacter.genericParameters[storageBox.paramId] );

			return newBox;
		}


		public Storage.HitBox SaveToStorage(){
			Storage.HitBox ret = storageBox;
			storageBox = null;
			return ret;
		}


		public void BuildStorage(List<Box> boxes, List<GenericParameter> genericParams){

			// Build boxes
			storageBox = new Storage.HitBox();
			storageBox.boxIds = new int[boxesPerFrame.Count];
			Box searchBox;
			int boxIndex;
			for (int i = 0 ; i < boxesPerFrame.Count ; ++i){
				searchBox = boxesPerFrame[i];
				if (!enabledFrames[i] || searchBox == null) {
					storageBox.boxIds[i] = -1;
				}else{
					boxIndex = boxes.FindIndex(x => x.IsEqual(searchBox));
					if (boxIndex < 0){
						boxIndex = boxes.Count;
						boxes.Add(searchBox);
					}
					storageBox.boxIds[i] = boxIndex;
				}
			}

			// Build parameter
			int paramIndex = genericParams.FindIndex(x => x.IsEqual(param));
			if (paramIndex < 0){
				paramIndex = genericParams.Count;
				genericParams.Add(param);
			}
			storageBox.paramId = paramIndex;
		}


	}


}
