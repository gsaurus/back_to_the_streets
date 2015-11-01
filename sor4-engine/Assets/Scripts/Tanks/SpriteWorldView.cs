using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class SpriteWorldView:View<WorldModel>{
	
	// The views
	GameObject[] mapViews;
	GameObject[] tankViews;
	GameObject[] bulletViews;
	GameObject backgroundView;

	// Common material used by old views
	static Material meshesMaterial;
	static GameObject tankPrefab;
	static GameObject bulletPrefab;
	static GameObject groundPrefab;
	static GameObject concretePrefab;
	static GameObject treesPrefab;

	static GameObject brick0Prefab;
	static GameObject brick1Prefab;
	static GameObject brick2Prefab;
	static GameObject brick3Prefab;

	static int forwardAnimHash;
	static int backwardAnimHash;

	const float playgroundZ = -0.01f;


	const float lerpTimeFactor = 5.0f; 

	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static SpriteWorldView(){
		// setup imutable stuff
		meshesMaterial = new Material(Shader.Find("Sprites/Default"));
		tankPrefab 		= Resources.Load("tank") 		as GameObject;
		bulletPrefab 	= Resources.Load("bullet") 		as GameObject;
		groundPrefab 	= Resources.Load("ground")		as GameObject;
		concretePrefab 	= Resources.Load("concrete")	as GameObject;
		treesPrefab 	= Resources.Load("trees") 		as GameObject;

		brick0Prefab 	= Resources.Load("brick0") 		as GameObject;
		brick1Prefab 	= Resources.Load("brick1") 		as GameObject;
		brick2Prefab 	= Resources.Load("brick2") 		as GameObject;
		brick3Prefab 	= Resources.Load("brick3") 		as GameObject;

		forwardAnimHash = Animator.StringToHash("tank_forward");
		backwardAnimHash = Animator.StringToHash("tank_backward");
	}


	public SpriteWorldView(){
		// Allocate memory for views arrays
		mapViews = new GameObject[WorldModel.MaxWidth * WorldModel.MaxHeight];
		tankViews = new GameObject[WorldModel.MaxPlayers];
		bulletViews = new GameObject[WorldModel.MaxPlayers * WorldModel.MaxBulletsPerPlayer];
		lastKnownMap = new int[mapViews.Length];
		for (int i = 0 ; i < mapViews.Length ; ++i){
			lastKnownMap[i] = -1;
		}

		// Setup background, immutable
		backgroundView = CreateBackground();
	}



#region Updates

	protected override void Update(WorldModel model, float deltaTime){

		UpdateMap(model);

		UpdateTanks(model, deltaTime);

		UpdateBullets(model, deltaTime);

	}

	private void UpdateMap(WorldModel model){
		for (int i = 0 ; i < model.map.Length ; ++i){
			if (model.map[i] != lastKnownMap[i]) {
				// map change, recreate block
				if (mapViews[i] != null){
					GameObject.Destroy(mapViews[i]);
				}
				mapViews[i] = CreateBlock(model.map[i], i % WorldModel.MaxWidth, i / WorldModel.MaxHeight);
				lastKnownMap[i] = model.map[i];
			}
		}
	}

	private void UpdateTanks(WorldModel model, float deltaTime){
		for (int i = 0 ; i < model.tanks.Length ; ++i){
			// update creation or destruction
			TankModel tankModel = model.tanks[i];
			bool own = NetworkCenter.Instance.GetPlayerNumber() == i;
			if (tankModel != null && tankViews[i] == null) {
				tankViews[i] = CreateTank(own);
			}else if (tankModel == null && tankViews[i] != null){
				GameObject.Destroy(tankViews[i]);
				tankViews[i] = null;
			}

			if (tankViews[i] != null){

				GameObject tank = tankViews[i].transform.GetChild(0).gameObject;

				if (tankModel.timeToRespawn > 0){
					SetTankColor(tank, Color.black);
				}else {
					Color color;
					if (own){
						color = new Color(0.3f, 0.5f, 1.0f, 1.0f);
					}else {
						color = new Color(1.0f, 0.3f, 0.3f, 1.0f);
					}
					SetTankColor(tank, color);		
				}

				Vector3 targetPos = new Vector3((float)tankModel.position.X, (float)tankModel.position.Y, playgroundZ);

				// update rotation
				
				GameObject turret = tank.transform.GetChild(0).gameObject;
				tank.transform.localEulerAngles = new Vector3(0, 0, (float)tankModel.orientationAngle * Mathf.Rad2Deg - 90);
				turret.transform.localEulerAngles = new Vector3(0, 0, (float)tankModel.turretAngle * Mathf.Rad2Deg);

				// update moving animation
				Animator animator = tank.GetComponent<Animator>();
				if (tankModel.movingBackwards && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != backwardAnimHash){
					animator.Play(backwardAnimHash);
				}else if (!tankModel.movingBackwards && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != forwardAnimHash){
					animator.Play(forwardAnimHash);
				}
				float vel = Vector3.Distance(tankViews[i].transform.position, targetPos); 
				animator.speed = vel / (float)WorldController.maxTankVelocity;

				// update position
				UpdatePosition(tankViews[i], targetPos);
				
				//tankViews[i].transform.RotateAround(new Vector3(0.5f, 0.5f, 0.0f), Vector3.forward, 0.4f);
			}
		}
	}


	private void UpdateBullets(WorldModel model, float deltaTime){
		for (int i = 0 ; i < model.bullets.Length ; ++i){
			// update creation or destruction
			BulletModel bulletModel = model.bullets[i];
			if (bulletModel != null && bulletViews[i] == null) {
				bulletViews[i] = CreateBullet(NetworkCenter.Instance.GetPlayerNumber() == i / WorldModel.MaxBulletsPerPlayer);
			}else if (model.bullets[i] == null && bulletViews[i] != null){
				GameObject.Destroy(bulletViews[i]);
				bulletViews[i] = null;
			}
			
			// update position
			if (bulletViews[i] != null){
				Vector3 targetPos = new Vector3((float)bulletModel.position.X, (float)bulletModel.position.Y, playgroundZ);
				UpdatePosition(bulletViews[i], targetPos);
				bulletViews[i].transform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2((float)bulletModel.velocity.Y, (float)bulletModel.velocity.X) * Mathf.Rad2Deg - 90);
			}
		}
	}

	private void UpdatePosition(GameObject obj, Vector3 targetPos){
		float dist = Vector3.Distance(obj.transform.position, targetPos);
		if (dist < 0.25f || dist > 2) {
			obj.transform.position = targetPos;
		}else{
			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpTimeFactor);
		}
	}
	
	
	
#endregion


	
#region Views Creation


	private GameObject CreateBackground(){
		return null;
	}


	private void SetTankColor(GameObject tankObj, Color color){
		SpriteRenderer renderer = tankObj.GetComponent<SpriteRenderer>();
		renderer.color = color;
		GameObject turret = tankObj.transform.GetChild(0).gameObject;
		renderer = turret.GetComponent<SpriteRenderer>();
		renderer.color = color;
	}

	private GameObject CreateTank(bool own){
		GameObject mainObj = new GameObject("tank_instance");
		GameObject tankObj = GameObject.Instantiate(tankPrefab);

		tankObj.transform.parent = mainObj.transform;
		tankObj.transform.position = new Vector3(0.5f, 0.5f, playgroundZ);

		return mainObj;
	}

	private GameObject CreateBullet(bool own){
		GameObject bulletObj = GameObject.Instantiate(bulletPrefab);
		return bulletObj;
	}

	private GameObject CreateBlock(int type, int x, int y){

		GameObject obj;
		SpriteRenderer renderer;
		switch (type){
			case 1: // concrete
				obj = GameObject.Instantiate(concretePrefab);
			break;
			case 2: // water
				obj = GameObject.Instantiate(concretePrefab);
				renderer = obj.GetComponent<SpriteRenderer>();
				renderer.color = new Color(0.2f, 0.2f, 0.8f, 1.0f);
			break;
			case 3: // florest
				obj = GameObject.Instantiate(treesPrefab);
			break;
			case 4: // weakest brick
				obj = GameObject.Instantiate(brick3Prefab);
				break;
			case 5:
			case 6: // weak brick
				obj = GameObject.Instantiate(brick2Prefab);
				break;
			case 7:
			case 8: // slightly weak brick
				obj = GameObject.Instantiate(brick1Prefab);
				break;
			case 9: // strong brick
				obj = GameObject.Instantiate(brick0Prefab);
				break;
			default:
				// ground block
				obj = GameObject.Instantiate(groundPrefab);
			break;
		}
		obj.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
		return obj;
	}
	

#endregion

	
}

