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
			enabledFrames = new List<bool>();
			boxesPerFrame = new List<Box>();
			param = new GenericParameter();
		}

		// method to enforce number of boxes per frame
		public void EnsureBoxExists(int boxId){
			while (boxesPerFrame.Count < boxId + 1){
				boxesPerFrame.Add(new Box());
				enabledFrames.Add(false);
			}
		}


		public static HitBox LoadFromStorage(Storage.HitBox storageBox, Storage.Character storageCharacter){

			HitBox newBox = new HitBox();

			// Populate boxes
			newBox.boxesPerFrame = new List<Box>(storageBox.boxIds.Length);
			newBox.enabledFrames = new List<bool>(storageBox.boxIds.Length);
			foreach(int boxId in storageBox.boxIds){
				if (boxId == Box.invalidBoxId) {
					newBox.enabledFrames.Add(false);
					newBox.boxesPerFrame.Add(new Box());
				}else {
					newBox.enabledFrames.Add(true);
					newBox.boxesPerFrame.Add(Box.LoadFromStorage(storageCharacter.boxes[boxId]));
				}
			}

			newBox.param = GenericParameter.LoadFromStorage( storageCharacter.genericParameters[storageBox.paramId] );

			return newBox;
		}


		public Storage.HitBox SaveToStorage(){
			Storage.HitBox ret = storageBox;
			storageBox = null;
			return ret;
		}


		public void BuildStorage(List<Box> boxes, int numFrames, List<GenericParameter> genericParams){

			// Build boxes
			storageBox = new Storage.HitBox();
			storageBox.boxIds = new int[numFrames];
			Box searchBox;
			int boxIndex;
			for (int i = 0 ; i < numFrames ; ++i){
				searchBox = i < boxesPerFrame.Count ? boxesPerFrame[i] : null;
				if (searchBox == null || !enabledFrames[i]) {
					storageBox.boxIds[i] = Box.invalidBoxId;
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
