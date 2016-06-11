using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using System.Linq;


public class DebugWorldView:View<WorldModel>{
	
	// The views
//	public GameObject[] skierViews;


	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static DebugWorldView(){
//		// setup imutable stuff
//		meshesMaterial = new Material(Shader.Find("Sprites/Default"));
//
//		skierPrefab = Resources.Load("skier") as GameObject;
//		rideAnimHash = Animator.StringToHash("Ride");
//		fallAnimHash = Animator.StringToHash("Fall");
//		crashAnimHash = Animator.StringToHash("Crash");
//
//		arrowsToPointNextFlag = GameObject.Find("nextFlagPointingArrows");
//		arrowsToPointNextFlag.SetActive(false);
//
//		painClips = new AudioClip[10];
//		for (int i = 0 ; i < 10 ; ++i){
//			painClips[i] = Resources.Load("sound/WS_pain_" + (i+1)) as AudioClip;
//		}
//
//		failClip = Resources.Load("sound/SSS_SFX_hit_penaltyWall") as AudioClip;
//
//        originalFogColor = RenderSettings.fogColor;
	}


	public DebugWorldView(WorldModel world){
		// Allocate memory for views arrays
//		skierViews = new GameObject[WorldModel.MaxPlayers];
//		skierCollisionSoundActivated = new bool[WorldModel.MaxPlayers];
//		finishedSkiers = new int[WorldModel.MaxPlayers];
//		skiersNames = new string[WorldModel.MaxPlayers];
//		scrambledMaterialIds = new int[WorldModel.MaxPlayers];
//		for (int i = 0; i < scrambledMaterialIds.Count(); ++i) {
//			scrambledMaterialIds[i] = i+1;
//		}
//		scrambledMaterialIds = scrambledMaterialIds.OrderBy(x => UnityEngine.Random.Range(0, 1)).ToArray<int>();
	}



	protected override void Update(State state, WorldModel model, float deltaTime){


	}




//	private void UpdatePosition(SkierModel skierModel, GameObject obj, Vector3 targetPos){
//		float dist = Vector3.Distance(obj.transform.position, targetPos);
//		if (skierModel.frozenTimer == 0){
//			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpTimeFactor);
//		}else {
//			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpFrozenTimeFactor);
//		}
//	}

	
	
#region Views Creation
	


	private void normalizeAnglesDifference(ref float a1, float a2){
		while (a1 - a2 > 180){
			a1 -= 360;
		}
		while (a1 - a2 < -180){
			a1 += 360;
		}
	}

#endregion


	public override void OnDestroy(State state, WorldModel model){
//		if (skierViews == null) return;
//		foreach (GameObject skierView in skierViews) {
//			if (skierView != null){
//				GameObject.Destroy(skierView);
//			}
//		}
	}


	
}

