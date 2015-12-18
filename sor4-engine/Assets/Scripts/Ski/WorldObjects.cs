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

	public FixedFloat x1;
	public FixedFloat x2;

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
		rocksPool = new GameObject[50];
		tree1Pool = new GameObject[100];
		tree2Pool = new GameObject[100];

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


	public WorldObject(int type, FixedFloat x, FixedFloat y) {
		this.type = type;
		this.x1 = x - 0.5f;
		this.x2 = x + 0.5f;

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
		float radius = UnityEngine.Random.Range(0.75f, 1.25f);
		view.transform.localScale = new Vector3(radius, UnityEngine.Random.Range(0.75f, 1.5f), radius);
		view.transform.localPosition  = new Vector3((float)x, -0.25f, (float)y);
	}


	public void OnDestroy(){
		// nothing to do
		view.SetActive(false);
	}

}


public class WorldObjects{

	static FixedFloat maxHorizontalDistance = 30;
	static FixedFloat maxDistanceBehind = 10;
	static FixedFloat minDistanceAhead = 70;
	// every 50 units, resort the list
	static FixedFloat controlRange = 0.1f;

	static List<FixedFloat> yList = new List<FixedFloat>(100);
	static Dictionary<FixedFloat, List<WorldObject>> objectsByY = new Dictionary<FixedFloat, List<WorldObject>>(100);



	public static WorldObject getCollisionObject(FixedFloat x, FixedFloat lastY, FixedFloat newY){
		return null;
	}


	public static void UpdateTrack(WorldModel world, FixedFloat minY, FixedFloat maxY) {

		if (yList.Count > 0) {

			// remove future objects...
			int index;
			bool removedSome = false;
			for (index = yList.Count-1 ; index > 0 && yList[index] <  world.maxYKnown; --index){
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
				yList.RemoveRange(index, yList.Count - index);
			}

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
		FixedFloat nextTargetY = maxKnownY + minDistanceAhead + controlRange;
		while (maxY < nextTargetY) {

			FixedFloat nextY;
			for (nextY = maxKnownY - 0.001f ; nextY > nextTargetY ; nextY -= StateManager.state.Random.NextFloat(0.0001f, 2.0f)){

				if (nextY < world.nextTrackY){
					world.nextTrackX = world.lastTrackX + StateManager.state.Random.NextFloat(-30, 30);
					world.nextTrackY = nextY - StateManager.state.Random.NextFloat(20, 60);
				}

				yList.Add(nextY);
				List<WorldObject> newObjects = new List<WorldObject>();
				FixedFloat centerX;
				FixedFloat randomX;
				do {
					centerX = GetCenterXForY(nextY, maxKnownY, world.nextTrackY, world.lastTrackX, world.nextTrackX);
					randomX = GetRandomXAroundCenter(centerX);
					newObjects.Add(new WorldObject(StateManager.state.Random.NextInt(0, 5), randomX, nextY));
				}while (StateManager.state.Random.NextInt(0, 1) != 0);
				UnityEngine.Debug.Log("next: " + nextY);
				objectsByY.Add(nextY,newObjects);
			}

			world.lastTrackX = world.nextTrackX;
			world.maxYKnown = maxKnownY = nextY;

		}
	}



	
	static FixedFloat GetCenterXForY(FixedFloat y, FixedFloat lastY, FixedFloat newY, FixedFloat lastX, FixedFloat newX){
		FixedFloat t = (y-lastY) / (newY - lastY);
		return lastX + t * (newX - lastX);
	}
	
	static FixedFloat GetRandomXAroundCenter(FixedFloat center) {
		FixedFloat randomT = StateManager.state.Random.NextFloat(0, 1);
		randomT = randomT * randomT * randomT;
		randomT = 1 - randomT;
		if (StateManager.state.Random.NextInt(0,1) == 0) {
			randomT *= -1;
		}
		return center + (randomT * maxHorizontalDistance);
	}

}

