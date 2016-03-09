using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class ImportedData{

		public FixedVector3 position;
		public FixedVector3 scale;
		// TODO: do we need/want rotation too?

		public ImportedData(Transform transform){
			position = transform.position.AsFixedVetor3();
			scale = transform.lossyScale.AsFixedVetor3();
		}


	}

	public static class AutoImporter {
		

		// NOTE: decided to keep this feature out
		// NOTE: this method is complete, in case I need it again
//		// for each frame --> for each kind --> for each item in that kind
//		public static List<List<List<ImportedData>>> ImportFromAnimationClip(GameObject characterModel, AnimationClip clip, List<List<Transform>> transformsToLookAt){
//
//			if (clip == null) return null;
//
//			List<List<List<ImportedData>>> importedData = new List<List<List<ImportedData>>>();
//			float clipLen = clip.averageDuration;
//			Transform itemTransform;
//
//			// for each frame
//			for (float time = 0; time < clipLen; time += Time.fixedDeltaTime) {
//
//				// sample animation at that frame
//				List<List<ImportedData>> frameImportedData = new List<List<ImportedData>>();
//				importedData.Add(frameImportedData);
//				clip.SampleAnimation(characterModel, time);
//
//				// for each kind of items
//				for (int i = 0; i < transformsToLookAt.Count; ++i) {
//					
//					List<ImportedData> itemsImportedDataForFrame = new List<ImportedData>();
//					frameImportedData.Add(itemsImportedDataForFrame);
//
//					// for each item
//					foreach (Transform item in transformsToLookAt[i]) {
//						if (item != null && item.gameObject.activeInHierarchy) {
//							itemsImportedDataForFrame.Add(new ImportedData(item));
//						} else {
//							itemsImportedDataForFrame.Add(null);
//						}
//					} // each item
//
//				} // each kind
//
//			} // each frame
//
//			return importedData;
//		}

	}


}
