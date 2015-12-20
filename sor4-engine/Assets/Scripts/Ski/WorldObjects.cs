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
	GameObject view;

	static GameObject[] rocksPool;
	static GameObject[] tree1Pool;
	static GameObject[] tree2Pool;
	static int rockPoolId;
	static int tree1PoolId;
	static int tree2PoolId;

	static GameObject rockPrefab;
	static GameObject tree1Prefab;
	static GameObject tree2Prefab;


	static WorldObject(){
		rockPrefab = Resources.Load("rock") as GameObject;
		tree1Prefab = Resources.Load("tree1") as GameObject;
		tree2Prefab = Resources.Load("tree2") as GameObject;

		// Instantiate objects pools
		rocksPool = new GameObject[500];
		tree1Pool = new GameObject[1000];
		tree2Pool = new GameObject[1000];

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


	public WorldObject(int type, FixedFloat x, FixedFloat y, bool isRight) {
		this.type = type;
		this.y = y;
		this.x1 = x - 1.0f;
		this.x2 = x + 1.0f;
		this.isRight = isRight;

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
		view.transform.eulerAngles = new Vector3(310 + UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(0, 360));
		float radius = type == 0 ? UnityEngine.Random.Range(1.0f, 1.3f) : UnityEngine.Random.Range(0.75f, 1.25f);
		view.transform.localScale = new Vector3(radius, UnityEngine.Random.Range(0.75f, 1.75f), radius);
		view.transform.localPosition  = new Vector3((float)x, -0.25f, (float)y);
	}


	public void OnDestroy(){
		// nothing to do
		view.SetActive(false);
	}

}


public class WorldObjects{

	// 
	static SimpleRandomGenerator rnd = new SimpleRandomGenerator();

	static uint collisionFallenTime = 45;

	static FixedFloat maxHorizontalDistance = 20;
	static FixedFloat maxDistanceBehind = 10;
	static FixedFloat minDistanceAhead = 70;
	// every 50 units, resort the list
	static FixedFloat controlRange = 1.0f;

	static List<int> yList = new List<int>(100);
	static Dictionary<int, List<WorldObject>> objectsByY = new Dictionary<int, List<WorldObject>>(100);


	static FixedFloat maxYKnown;
	static FixedFloat lastTrackX;
	static FixedFloat lastTrackY;
	static FixedFloat nextTrackX;
	static FixedFloat nextTrackY;



	public static WorldObject GetCollisionObject(FixedFloat x, FixedFloat y){
		List<WorldObject> objects;
		if (objectsByY.TryGetValue((int)y, out objects)){
			foreach (WorldObject obj in objects){
				if (x > obj.x1 && x < obj.x2){
					return obj;
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
			skier.fallenTimer = collisionFallenTime;
			skier.velX = obj.isRight ? -1.4 : 1.4;
			skier.velY = 0.6f;
			skier.targetVelX = 0;
			skier.targetVelY = 0;
		}
	}



	public static void HandleCollisionWithOtherSkiers(WorldModel world, SkierModel skier){

		if (skier.fallenTimer > 0 || skier.frozenTimer > 0){
			return; // ignore if already collided
		}

		SkierModel otherSkier;

		for (uint skierId = 0 ; skierId < world.skiers.Length ; ++skierId){
			otherSkier = world.skiers[skierId];
		
			if (otherSkier != null && otherSkier != skier){
				if ((int)skier.y == (int)otherSkier.y){
					if (FixedFloat.Abs(skier.x - otherSkier.x) < 0.8f){
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


	public static void UpdateTrack(WorldModel world, FixedFloat minY, FixedFloat maxY) {

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
		FixedFloat maxKnownY = yList.Count == 0 ? 0 : yList[yList.Count-1];
		FixedFloat nextTargetY = maxY - minDistanceAhead - controlRange;

		FixedFloat nextY;
		int latestIntY = (int)maxKnownY;
		for (nextY = maxKnownY - 0.001f ; nextY > nextTargetY ; nextY -= rnd.NextFloat(0.0001f, 1.5f)){

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
			randomX = GetRandomXAroundCenter(centerX);
			newObjects.Add(new WorldObject(rnd.NextInt(0, 5), randomX, nextY, randomX > centerX));


		}

		maxYKnown = maxKnownY = nextY;

	}



	
	static FixedFloat GetCenterXForY(FixedFloat y, FixedFloat lastY, FixedFloat newY, FixedFloat lastX, FixedFloat newX){
		FixedFloat t = (y-lastY) / (newY - lastY);
		return lastX + t * (newX - lastX);
	}
	
	static FixedFloat GetRandomXAroundCenter(FixedFloat center) {
		FixedFloat randomT = rnd.NextFloat(0, 1);
		randomT = randomT * randomT * randomT;
		randomT = 1.05f - randomT;
		if (rnd.NextInt(0,1) == 0) {
			randomT *= -1;
		}
		return center + (randomT * maxHorizontalDistance);
	}

}

