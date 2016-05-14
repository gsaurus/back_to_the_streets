// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using UnityEngine;


public class WorldObject
{
	public int type;

	public FixedFloat y;
	public FixedFloat x1;
	public FixedFloat x2;
	public bool isRight;

	// Keep track of visual object
	public GameObject view;

	static GameObject[] rocksPool;
	static GameObject[] tree1Pool;
	static GameObject[] tree2Pool;
	static int rockPoolId;
	static int tree1PoolId;
	static int tree2PoolId;

	static GameObject rockPrefab;
	static GameObject tree1Prefab;
	static GameObject tree2Prefab;
	static GameObject flagPrefab;
	public static GameObject goalPrefab;


	static WorldObject(){
		rockPrefab = Resources.Load("rock") as GameObject;
		tree1Prefab = Resources.Load("tree1") as GameObject;
		tree2Prefab = Resources.Load("tree2") as GameObject;
		flagPrefab = Resources.Load("flag") as GameObject;
		goalPrefab = Resources.Load("goal") as GameObject;

		// Instantiate objects pools
		rocksPool = new GameObject[1000];
		tree1Pool = new GameObject[4000];
		tree2Pool = new GameObject[4000];

		for (int i = 0 ; i < rocksPool.Length ; ++i) {
			rocksPool[i] = GameObject.Instantiate(rockPrefab);
			rocksPool[i].SetActive(false);
			rocksPool[i].transform.position = new Vector3(-999, -999, -999);
		}
		for (int i = 0 ; i < tree1Pool.Length ; ++i) {
			tree1Pool[i] = GameObject.Instantiate(tree1Prefab);
			tree1Pool[i].SetActive(false);
			tree1Pool[i].transform.position = new Vector3(-999, -999, -999);
		}
		for (int i = 0 ; i < tree2Pool.Length ; ++i) {
			tree2Pool[i] = GameObject.Instantiate(tree2Prefab);
			tree2Pool[i].SetActive(false);
			tree2Pool[i].transform.position = new Vector3(-999, -999, -999);
		}
	}


	public void ApplyColorToFlagArros(GameObject arrowsObj)
	{
		SpriteRenderer[] sprites = arrowsObj.GetComponentsInChildren<SpriteRenderer>();
		Color flagColor = type == -2 ? new Color(0.9f, 0.3f, 0.3f) : new Color(0.3f, 0.3f, 0.9f);
		foreach (SpriteRenderer sprite in sprites){
			sprite.material.color = flagColor;
		}
		Transform transform = arrowsObj.transform.Find("quad");
		if (transform != null) {
			GameObject collisionPanel = transform.gameObject;
			MeshRenderer quadRenderer = collisionPanel.GetComponent<MeshRenderer> ();
			quadRenderer.material.color = new Color (flagColor.r, flagColor.g, flagColor.b, 0);
		}
		transform = arrowsObj.transform.Find("signaller");
		if (transform != null) {
			GameObject signaller = transform.gameObject;
			MeshRenderer quadRenderer = signaller.GetComponent<MeshRenderer> ();
			Color signallerColor = type == -2 ? Color.red : Color.blue;
			signallerColor.a = 0.025f;
			quadRenderer.material.color = signallerColor;
		}
		transform = arrowsObj.transform.Find("model");
		if (transform != null) {
			MeshRenderer[] renderers = transform.gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer renderer in renderers) {
				renderer.material.color = type == -2 ? Color.red : Color.blue;
			}
		}
	}
	

	public WorldObject(int type, FixedFloat x, FixedFloat y, bool isRight) {
		this.type = type;
		this.y = y;
		this.x1 = x -1.1f;
		this.x2 = x +1.1f;
		this.isRight = isRight;

		float yPos;
		if (type < 0){
			// Flag
			view = GameObject.Instantiate(flagPrefab);
			if (type == -2){
				view.transform.localEulerAngles = new Vector3(0, 180, 0);
//				Transform modelTransform = view.transform.FindChild("model");
//				if (modelTransform != null){
//					// still need the flag faced to the camera
//					modelTransform.localEulerAngles = new Vector3(0, 180, 0);
//				}
			}
			view.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

			ApplyColorToFlagArros(view);

			yPos = -0.1f;
		}else {

			// Rocks & Trees
			if (type == 0) {
				view = rocksPool[rockPoolId++];
				if (rockPoolId >= rocksPool.Length) rockPoolId = 0;
			}else if (type % 2 == 0){
				view = tree1Pool[tree1PoolId++];
				if (tree1PoolId >= tree1Pool.Length) tree1PoolId = 0;
			}else {
				view = tree2Pool[tree2PoolId++];
				if (tree2PoolId >= tree2Pool.Length) tree2PoolId = 0;
			}
			view.SetActive(true);
			view.transform.localEulerAngles = new Vector3(310 + UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(0, 360));
			float radius = type == 0 ? UnityEngine.Random.Range(1.0f, 1.3f) : UnityEngine.Random.Range(1.5f, 2.0f);
			float scale = type == 0 ? 1 : UnityEngine.Random.Range(0.5f, 1.0f);
			view.transform.localScale = new Vector3(radius, UnityEngine.Random.Range(1.75f, 2.5f), radius);
			view.transform.localScale *= scale;
			yPos = 0.25f; //-0.25f;
			if (type != 0) {
				view.GetComponent<MeshRenderer>().material.color =
					new Color(
						UnityEngine.Random.Range(0.2f, 0.6f),
						UnityEngine.Random.Range(0.2f, 0.7f),
						UnityEngine.Random.Range(0.2f, 0.6f)
					)
				;
			}
		}
		view.transform.localPosition  = new Vector3((float)x, yPos, (float)y);

	}


	public void OnDestroy(){
		// nothing to do
		view.SetActive(false);
	}

}


public class WorldObjects{

	// 
	static SimpleRandomGenerator rnd = null;

	public static uint collisionFallenTime = 45;
	public static uint frozenTime = 150;

	static FixedFloat maxHorizontalDistance = 20;
	static FixedFloat maxDistanceBehind = 10;
	static FixedFloat minDistanceAhead = 80;
	// every 50 units, resort the list
	static FixedFloat controlRange = 1.0f;
	static FixedFloat maxDifficultyDistance = 600;
	static FixedFloat initialCleanupDistance = 675;
	public static FixedFloat finalGoalDistance = 700;

	static List<int> yList = new List<int>(100);
	static Dictionary<int, List<WorldObject>> objectsByY = new Dictionary<int, List<WorldObject>>(100);

	static FixedFloat lastTrackX;
	static FixedFloat lastTrackY;
	static FixedFloat nextTrackX;
	static FixedFloat nextTrackY;
	static bool nextFlagIsRight;
	static FixedFloat nextFlagDistance;


	static List<WorldObject> flags = new List<WorldObject>();

	static GameObject goalObj;


	public static void Reset(){
		rnd = null;
		foreach (List<WorldObject> objects in objectsByY.Values){
			foreach (WorldObject obj in objects) {
				obj.OnDestroy();
			}
		}
		yList = new List<int>(100);
		objectsByY = new Dictionary<int, List<WorldObject>>(100);

		lastTrackX = 0;
		lastTrackY = 0;
		nextTrackX = 0;
		nextTrackY = 0;
		nextFlagIsRight = false;
		nextFlagDistance = 0;
	}


	public static WorldObject GetNextFlagForSkier(SkierModel skierModel){
		WorldObject flag = null;
		int removeCount = 0;
		for (int i = 0 ; i < flags.Count ; ++i) {
			flag = flags[i];
			if (flag.y < skierModel.y) {
				break;
			}else {
				++removeCount;
			}
		}
		if (removeCount > 0) {
			flags.RemoveRange(0, removeCount);
		}
		return flag;
	}



	public static WorldObject GetCollisionObject(FixedFloat x, FixedFloat y){
		List<WorldObject> objects;
		if (objectsByY.TryGetValue((int)y, out objects)){
			foreach (WorldObject obj in objects){
				if (obj.type >= 0){
					if (x > obj.x1 && x < obj.x2){
						return obj;
					}
				}else {
					if (obj.type == -2 && x < obj.x1){
						return obj;
					}else if (obj.type == -1 && x > obj.x2){
						return obj;
					}
				}
			}
		}
		return null;
	}


	public static void HandleCollisionWithWorld(WorldModel world, SkierModel skier){

		if (skier.fallenTimer > 0 || skier.frozenTimer > 0){
			return; // ignore if already collided
		}
		WorldObject obj = GetCollisionObject(skier.x, skier.y);
		if (obj != null){
			if (obj.type >= 0){
				// obstacle
				skier.fallenTimer = collisionFallenTime;
				skier.velX = obj.isRight ? -1.4 : 1.4;
				skier.velY = 0.6f;
				skier.targetVelX = 0;
				skier.targetVelY = 0;
			}else {
				// flag
				FixedFloat target = StateManager.state.Random.NextFloat(obj.x1 + 0.01f, obj.x2 - 0.01f);
				skier.y = obj.y + StateManager.state.Random.NextFloat(1,1.5f);
				skier.frozenTimer =  (uint)(FixedFloat.Abs(skier.x-target) / maxHorizontalDistance) * frozenTime;
				if (skier.frozenTimer > 2*frozenTime) skier.frozenTimer = 2*frozenTime;
				if (skier.frozenTimer < collisionFallenTime) skier.frozenTimer = collisionFallenTime;
				skier.velX = 0;
				skier.velY = 0;
				skier.targetVelX = 0;
				skier.targetVelY = 0;
				skier.x = target;

				// Animate flag collision panel
				AnimateFlagOnCollision(obj.view);
			}
		}
	}


	private static void AnimateFlagOnCollision(GameObject flagObj){
		if (flagObj == null) return;
		GameObject collisionPanel = flagObj.transform.Find("quad").gameObject;
		FlagCollisionEffect effect = collisionPanel.GetComponent<FlagCollisionEffect>();
		if (effect != null) {
			effect.enabled = true;
		}
	}



	public static void HandleCollisionWithOtherSkiers(WorldModel world, SkierModel skier){

		if (skier.fallenTimer > 0 || skier.frozenTimer > 0){
			return; // ignore if already collided
		}

		SkierModel otherSkier;

		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			otherSkier = world.skiers[skierId];
		
			if (otherSkier != null && otherSkier != skier && otherSkier.frozenTimer == 0){
				if ((int)skier.y == (int)otherSkier.y){
					if (FixedFloat.Abs(skier.x - otherSkier.x) < 0.25f){
						skier.fallenTimer = collisionFallenTime;
						skier.velX = skier.x < otherSkier.x ? -1.2 : 1.2;
						skier.velY = 0.4f;
						skier.targetVelX = 0;
						skier.targetVelY = 0;
					}
				}
			}
		}
	}


	static FixedFloat GetNextFlagY(){
		FixedFloat nextFlagY = nextFlagDistance - rnd.NextFloat(20, 40);
		if (nextFlagY < -finalGoalDistance + 40 && nextFlagY > -finalGoalDistance) {
			nextFlagY = -finalGoalDistance;
		}
		return nextFlagY;
	}



	static FixedFloat GetDifficultySetting(FixedFloat nextY){
		if (nextY > -maxDifficultyDistance) {
			return -nextY / maxDifficultyDistance;
		} else if (nextY < -initialCleanupDistance && nextY > -finalGoalDistance) {
			return (-nextY - initialCleanupDistance) / (finalGoalDistance - initialCleanupDistance);
		} else if (nextY < -finalGoalDistance) {
			return FixedFloat.Zero;
		}
		return FixedFloat.One;
	}

	public static void UpdateTrack(WorldModel world, FixedFloat minY, FixedFloat maxY) {

		if (rnd == null){
			uint seed = StateManager.state.Random.NextUnsignedInt();
			rnd = new SimpleRandomGenerator(seed);
			nextFlagIsRight = rnd.NextUnsignedInt() % 2 == 0;
			nextFlagDistance = GetNextFlagY();
		}

		if (yList.Count > 0) {

			bool removedSome;
			int index;
			// remove old objects
			if (yList.Count > 0){
				FixedFloat minKnownY = yList[0];
				if (minY < minKnownY - controlRange) {
					removedSome = false;
					for (index = 0 ; index < yList.Count && yList[index] > minY + maxDistanceBehind ; ++index){
						List<WorldObject> objects;
						if (objectsByY.TryGetValue(yList[index], out objects)){
							foreach (WorldObject obj in objects) {
								obj.OnDestroy();
							}
						}
						objectsByY.Remove(yList[index]);
						removedSome = true;
					}
					if (removedSome) {
						yList.RemoveRange(0, index+1);
					}
				}
			}
		}

		// create more track
		FixedFloat maxKnownY = yList.Count == 0 ? -5 : yList[yList.Count-1];
		FixedFloat distanceAhead;
		if (StateManager.state.Keyframe > 180){
			distanceAhead = minDistanceAhead - controlRange;
		}else {
			distanceAhead = (FixedFloat.Create(StateManager.state.Keyframe) / 180.0f) * (minDistanceAhead - controlRange);
		}
		FixedFloat nextTargetY = maxY - distanceAhead;

		FixedFloat nextY;
		int latestIntY = (int)maxKnownY;
		bool nextObjectIsFlag;
		FixedFloat minYForGeneration = 0.5f;
		FixedFloat difficultySetting = GetDifficultySetting(maxKnownY);
		FixedFloat limitForObjects = minYForGeneration + (1-difficultySetting) * 3.0f;
		for (nextY = maxKnownY - 0.0001f ; nextY > nextTargetY ; ){
			nextY -= rnd.NextFloat(0.0001f, limitForObjects);
			nextObjectIsFlag = nextY < nextFlagDistance && nextFlagDistance >= -finalGoalDistance;
			while (nextY < nextTrackY){
				lastTrackY = nextY;
				lastTrackX = nextTrackX;
				nextTrackX = nextTrackX + rnd.NextFloat(-30, 30);
				nextTrackY = nextY - rnd.NextFloat(20, 60);
			}

			int nextIntY = (int)nextY;
			if (nextIntY != latestIntY){
				yList.Add(nextIntY);
				latestIntY = nextIntY;
			}
			List<WorldObject> newObjects;
			if (!objectsByY.TryGetValue(nextIntY, out newObjects)) {
				newObjects = new List<WorldObject>();
				objectsByY.Add(nextIntY,newObjects);
			}
			FixedFloat centerX;
			FixedFloat randomX;

			centerX = GetCenterXForY(nextY, lastTrackY, nextTrackY, lastTrackX, nextTrackX);
			if (nextObjectIsFlag){
				randomX = rnd.NextFloat(maxHorizontalDistance* 0.1f, maxHorizontalDistance * 0.6f);
				randomX = centerX + (nextFlagIsRight ? randomX : -randomX);
				WorldObject flagObj = new WorldObject(nextFlagIsRight ? -2 : -1, randomX, nextY, nextFlagIsRight);
				newObjects.Add(flagObj);
				flags.Add(flagObj);
				nextFlagIsRight = !nextFlagIsRight;
				if (nextFlagDistance == -finalGoalDistance) {
					// it's the goal, add an extra flag and spawn goal view
					randomX = nextFlagIsRight ? randomX - 8 : randomX + 8;
					flagObj = new WorldObject(nextFlagIsRight ? -2 : -1, randomX, nextY, nextFlagIsRight);
					newObjects.Add(flagObj);
					flags.Add(flagObj);
					if (goalObj == null) {
						goalObj = GameObject.Instantiate(WorldObject.goalPrefab);
					}
					goalObj.transform.position = new Vector3((float)(nextFlagIsRight ? randomX + 4 : randomX - 4), 0, (float)-finalGoalDistance);
				}
				nextFlagDistance = GetNextFlagY();
				flags.Add(flagObj);
			}else {
				randomX = GetRandomXAroundCenter(centerX, nextY);
				newObjects.Add(new WorldObject(rnd.NextInt(0, 4), randomX, nextY, randomX > centerX));
			}


		}

	}



	
	static FixedFloat GetCenterXForY(FixedFloat y, FixedFloat lastY, FixedFloat newY, FixedFloat lastX, FixedFloat newX){
		FixedFloat t = (y-lastY) / (newY - lastY);
		return lastX + t * (newX - lastX);
	}

	static FixedFloat GetRandomXAroundCenter(FixedFloat center, FixedFloat nextY) {
		FixedFloat randomT = rnd.NextFloat(0, 1);

		FixedFloat difficultySetting = GetDifficultySetting(nextY);
		if (difficultySetting < 0.5) {
			randomT *= randomT;
		}
		if (rnd.NextFloat(0, 1) < difficultySetting + 0.15f) {
			randomT *= randomT;
		}

		randomT = 1.05f - randomT;
		if (rnd.NextInt(0,1) == 0) {
			randomT *= -1;
		}
		return center + (randomT * (maxHorizontalDistance*(1+(1-difficultySetting)*0.5f)));
	}

}

