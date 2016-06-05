using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using System.Linq;


public class DebugWorldView:View<WorldModel>{
	
	// The views
	public GameObject[] skierViews;
	bool[] skierCollisionSoundActivated;

	// leaderboard times
	int[] finishedSkiers;
	string[] skiersNames;

	int[] scrambledMaterialIds;

	// Common material used by all views
	static Material meshesMaterial;

	const float lerpFrozenTimeFactor = 0.05f;
	const float lerpTimeFactor = 0.2f; 
	const float lerpAngleFactor = 0.1f;
	
	static GameObject skierPrefab;
	static int rideAnimHash;
	static int fallAnimHash;
	static int crashAnimHash;

	static AudioClip[] painClips;
	static AudioClip failClip;

	static GameObject arrowsToPointNextFlag;

	static Color originalFogColor;
	static Color finalFogColor = new Color(0.2265625f, 0.37890625f, 0.55078125f);


	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static DebugWorldView(){
		// setup imutable stuff
		meshesMaterial = new Material(Shader.Find("Sprites/Default"));

		skierPrefab = Resources.Load("skier") as GameObject;
		rideAnimHash = Animator.StringToHash("Ride");
		fallAnimHash = Animator.StringToHash("Fall");
		crashAnimHash = Animator.StringToHash("Crash");

		arrowsToPointNextFlag = GameObject.Find("nextFlagPointingArrows");
		arrowsToPointNextFlag.SetActive(false);

		painClips = new AudioClip[10];
		for (int i = 0 ; i < 10 ; ++i){
			painClips[i] = Resources.Load("sound/WS_pain_" + (i+1)) as AudioClip;
		}

		failClip = Resources.Load("sound/WS_penalty") as AudioClip;

		originalFogColor = RenderSettings.fogColor;
	}


	public DebugWorldView(WorldModel world){
		// Allocate memory for views arrays
		skierViews = new GameObject[WorldModel.MaxPlayers];
		skierCollisionSoundActivated = new bool[WorldModel.MaxPlayers];
		finishedSkiers = new int[WorldModel.MaxPlayers];
		skiersNames = new string[WorldModel.MaxPlayers];
		scrambledMaterialIds = new int[WorldModel.MaxPlayers];
		for (int i = 0; i < scrambledMaterialIds.Count(); ++i) {
			scrambledMaterialIds[i] = i+1;
		}
		scrambledMaterialIds = scrambledMaterialIds.OrderBy(x => UnityEngine.Random.Range(0, 1)).ToArray<int>();
	}



	protected override void Update(State state, WorldModel model, float deltaTime){

		if (state == StateManager.mainState && GuiMenus.Instance.IsDemoPlaying()) {
			return;
		}

		UpdateSkiers(model, deltaTime);
	
		UpdateLeaderboard(state, model);

		// check if next flag is outside screen
		if (!GuiMenus.Instance.IsDemoPlaying ()) {
			int playerId = 0;
			if (StateManager.Instance.IsNetworked) {
				playerId = NetworkCenter.Instance.GetPlayerNumber ();
				if (playerId < 0 || playerId >= model.skiers.Length)
					return;
			}

			SkierModel mySkier = model.skiers [playerId];

			WorldObject flag = WorldObjects.GetNextFlagForSkier (mySkier);
			if (flag == null)
				return;
			Vector3 flagScreenPos = new Vector3 ((float)(flag.isRight ? flag.x1 : flag.x2), 0.0f, (float)flag.y);
			flagScreenPos = Camera.main.WorldToScreenPoint (flagScreenPos);
			flagScreenPos = new Vector3 (flagScreenPos.x / Camera.main.pixelWidth, flagScreenPos.y / Camera.main.pixelHeight, 2.0f);
			bool setActive = false;
			float distanceAway = 0.0f;
			if (flagScreenPos.x < 0 && flag.isRight) {
				distanceAway = -flagScreenPos.x;
				arrowsToPointNextFlag.transform.localEulerAngles = new Vector3 (0, 180, 0);
				Vector3 arrowsPosition = Camera.main.ScreenToWorldPoint (new Vector3 (0.00f * Camera.main.pixelWidth, Mathf.Clamp (flagScreenPos.y, 0.1f, 0.95f) * Camera.main.pixelHeight, flagScreenPos.z));
				arrowsToPointNextFlag.transform.position = arrowsPosition;
				setActive = true;
			} else if (flagScreenPos.x > 1 && !flag.isRight) {
				distanceAway = flagScreenPos.x - 1;
				arrowsToPointNextFlag.transform.localEulerAngles = new Vector3 (0, 0, 0);
				Vector3 arrowsPosition = Camera.main.ScreenToWorldPoint (new Vector3 (1.0f * Camera.main.pixelWidth, Mathf.Clamp (flagScreenPos.y, 0.1f, 0.95f) * Camera.main.pixelHeight, flagScreenPos.z));
				arrowsToPointNextFlag.transform.position = arrowsPosition;
				setActive = true;
			}
			if (setActive) {
				arrowsToPointNextFlag.SetActive (true);
				flag.ApplyColorToFlagArros (arrowsToPointNextFlag);
				float scale = 0.1f - 0.08f * (1 - Mathf.Min (distanceAway, 3) / 3) + Mathf.Max (0.8f - flagScreenPos.y, 0) * 0.25f;
				scale *= 2.5f;
				arrowsToPointNextFlag.transform.localScale = new Vector3 (scale, scale, scale);
			}
			arrowsToPointNextFlag.SetActive (setActive);
		}

	}


	private string GetSkierLeaderboardName(int skierId){
		if (skiersNames[skierId] == null) {
			if (StateManager.Instance.IsNetworked) {
				NetworkPlayerData data;
				data = NetworkCenter.Instance.GetPlayerData((uint) skierId);
				if (data == null) return null;
				skiersNames[skierId] = data.playerName;
			} else {
				if (skierId == 0) {
					skiersNames[skierId] = GuiMenus.Instance.nickname;
				}else {
					int rnd = UnityEngine.Random.Range (0, 15);
					if (rnd == 2) {
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 999999);
					} else if (rnd < 2) {
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 99999999);
					} else if (rnd < 6){
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 129999999);
					}else {
						skiersNames[skierId] = DebugNames.randomNames[UnityEngine.Random.Range(0, DebugNames.randomNames.Length)];
					}
				}
			}
		}
		return skiersNames[skierId];
	}


	private void UpdateLeaderboard(State state, WorldModel model){
		GameObject leaderboard = GuiMenus.Instance.leaderboardObject;
		KeyValuePair<int, float>[] skiersYs = new KeyValuePair<int, float>[model.skiers.Count()];
		float order;
		for (int i = 0 ; i < skiersYs.Count() ; ++i){
			if (finishedSkiers[i] != 0) order = -999990 + finishedSkiers[i]*10 + i;
			else if(model.skiers[i] == null) order = 999;
			else order = (float)model.skiers[i].y;
			skiersYs[i] = new KeyValuePair<int, float>(i, order);
		}
		skiersYs = skiersYs.OrderBy(x=>x.Value).ToArray<KeyValuePair<int, float>>();
	
		Transform leadTransf = leaderboard.transform;
		UnityEngine.UI.Text textItem;
		string entryString;
		int ownPlayerNumber = StateManager.Instance.IsNetworked ? NetworkCenter.Instance.GetPlayerNumber() : 0;
		int orderedId;
		int position = 1;
		int winnerFrameNum = -1;
		for (int i = 0 ; i < WorldModel.MaxPlayers ; ++i){
			textItem = leadTransf.GetChild(i).gameObject.GetComponent<UnityEngine.UI.Text>();
			textItem.enabled = i < skiersYs.Count() && skiersYs[i].Value != 999;
			if (textItem.enabled) {
				orderedId = skiersYs[i].Key;
				entryString = GetSkierLeaderboardName(orderedId);
				textItem.enabled = entryString != null;
				if (textItem.enabled){
					if (finishedSkiers[orderedId] == 0 && skiersYs[i].Value <= -WorldObjects.finalGoalDistance) {
						int frameNum = (int) Mathf.Max((int)state.Keyframe - (int)WorldController.framesToStart, 0.0f);
						finishedSkiers[orderedId] = frameNum;
					}
					if (finishedSkiers[orderedId] != 0) {
						int frameNum = finishedSkiers[orderedId];
						float currentTime;
						if (winnerFrameNum > 0) {
							currentTime = StateManager.Instance.UpdateRate * (frameNum - winnerFrameNum);
							entryString = "+" + ClockCounter.FloatToTime(currentTime, "#0:00") + " - " + skiersNames [orderedId];
						} else {
							currentTime = StateManager.Instance.UpdateRate * frameNum;
							entryString = ClockCounter.FloatToTime (currentTime, "#0:00.00") + " - " + skiersNames [orderedId];
							winnerFrameNum = frameNum;
						}
					} else {
						entryString = position + ". " + entryString;
					}
					textItem.text = entryString;
					textItem.color = orderedId == ownPlayerNumber ? Color.yellow : Color.white;

					if (orderedId == ownPlayerNumber) {
						GuiMenus.Instance.playerPosition = position;
					}

					++position;
				}
			}
		}
		leaderboard.SetActive(position > 1);

		string winnerName = skiersNames[skiersYs[0].Key];
		if (winnerName != null) {
			GuiMenus.Instance.loserObj.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = winnerName + " won";
		}

	}


	private void UpdateSkiers(WorldModel model, float deltaTime){
		for (int i = 0 ; i < model.skiers.Length ; ++i){

			bool own = false;
			if (StateManager.Instance.IsNetworked) {
				own = NetworkCenter.Instance.GetPlayerNumber () == i;
			} else {
				own = i == 0;
			}

			// update creation or destruction
			SkierModel skierModel = model.skiers[i];
			if (skierModel != null && skierViews[i] == null) {
				
				if (own && scrambledMaterialIds[i] != 1) {
					scrambledMaterialIds[Array.FindIndex(scrambledMaterialIds, item => item == 1)] = scrambledMaterialIds[i];
					scrambledMaterialIds[i] = 1;
				}
				skierViews[i] = CreateSkier(i, own);

				float targetAngle = skierModel.targetVelY == 0 ? -Mathf.PI * 0.5f : (float)Mathf.Atan2((float)skierModel.targetVelX, (float)skierModel.targetVelY);
				targetAngle = targetAngle * Mathf.Rad2Deg;
				skierViews[i].transform.localEulerAngles = new Vector3(0, targetAngle, 0);

			}else if (skierModel == null && skierViews[i] != null){
				GameObject.Destroy(skierViews[i]);
				skierViews[i] = null;
			}
			GameObject skierView = skierViews[i];

			// update position
			if (skierView != null){

				// update background sprite
				if (own) {
					Transform bgTransform = GuiMenus.Instance.inGameBackground.transform;
					float yFactor = (float)((-skierModel.y) / WorldObjects.finalGoalDistance);
					Vector3 camRotation = Camera.main.transform.localRotation.eulerAngles;
					float xFacgtor = (180-camRotation.y)*0.02f;
					bgTransform.localPosition = new Vector3 (1*xFacgtor, yFactor*1.7f , bgTransform.localPosition.z);
					RenderSettings.fogColor = Color.Lerp(originalFogColor, finalFogColor, yFactor);
					Transform mist = bgTransform.parent.FindChild("Mist");
					if (mist != null) {
						mist.gameObject.GetComponent<SpriteRenderer>().color = RenderSettings.fogColor;
					}
				}

				// update position
				float yPos = (skierModel.fallenTimer > 0 || skierModel.frozenTimer > 0) ? 0.32f : -0.3f;
				Vector3 targetPos = new Vector3((float)skierModel.x, yPos, (float)skierModel.y);
				UpdatePosition(skierModel, skierView, targetPos);
				skierView.transform.position = new Vector3 (skierView.transform.position.x, yPos, skierView.transform.position.z);

				// update angle
				float targetAngle = skierModel.targetVelY == 0 ? -Mathf.PI * 0.5f : (float)Mathf.Atan2((float)skierModel.targetVelX, (float)skierModel.targetVelY);
				targetAngle = targetAngle * Mathf.Rad2Deg;
				float originalAngle = skierView.transform.localEulerAngles.y;
				normalizeAnglesDifference(ref targetAngle, originalAngle);
				targetAngle = Mathf.Lerp(originalAngle, targetAngle, lerpAngleFactor);
				skierView.transform.localEulerAngles = new Vector3(0, targetAngle, 0);

				// update animation
				Animator animator = skierView.GetComponent<Animator>();
				if (animator != null) {
					// update moving animation

					if (skierModel.fallenTimer == 0 && skierModel.frozenTimer == 0){

						animator.Play(rideAnimHash);
						float frictionRate = Mathf.Abs((float)skierModel.friction);
						float blendFactor = (float)(skierModel.friction) * 2.0f;
						Mathf.Clamp (blendFactor, -1, 1);
						blendFactor = blendFactor * 0.5f + 0.5f;
						animator.SetFloat ("Blend", blendFactor);
						animator.speed = new Vector2 ((float)skierModel.velX, (float)skierModel.velY).magnitude;

						ParticleSystem particles = skierView.GetComponentInChildren<ParticleSystem>();
						if (particles != null) {
							particles.enableEmission = true;
							particles.emissionRate = 1 + animator.speed * 20 + frictionRate * 329;
							particles.transform.localEulerAngles = new Vector3(30 + (1 - frictionRate) * 60, 90, 0);
							particles.transform.localPosition = new Vector3(-0.95f, -0.75f, -0.26f);
						}

						// Audio
						AudioSource[] audio = skierView.GetComponents<AudioSource>();
						if (audio != null){
							if(!audio[0].isPlaying){
								audio[0].Play();
							}
							if(!audio[1].isPlaying){
								audio[1].Play();
							}
							audio[0].volume = animator.speed * animator.speed * 0.3f;
							audio[1].volume = frictionRate * 0.4f;
							skierCollisionSoundActivated[i] = false;
						}

					}else {
						if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != crashAnimHash && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != fallAnimHash){
							bool pickCrash = skierModel.frozenTimer != 0 || UnityEngine.Random.Range(0,4) == 1;
							animator.Play(pickCrash ? crashAnimHash : fallAnimHash);
							skierView.transform.localEulerAngles = new Vector3(0, 180, 0);
							animator.speed = pickCrash ? 1.5f : 1.0f;//(pickCrash && skierModel.frozenTimer != 0) ? 1.25f : 1.0f;
						}

						ParticleSystem particles = skierView.GetComponentInChildren<ParticleSystem>();
						if (particles != null) {
							particles.enableEmission = false;
							//particles.transform.localPosition = new Vector3(particles.transform.localPosition.x, -999, particles.transform.localPosition.z);
							particles.transform.position = new Vector3(99999, -99999, 99999);
						}

						// Audio
						AudioSource[] audio = skierView.GetComponents<AudioSource>();
						if (audio != null) {
							if (!skierCollisionSoundActivated[i]){
								audio[0].Stop();
								audio[1].Stop();
								audio[2].Stop();
								if (skierModel.frozenTimer == 0) {

									audio[2].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
									audio[2].Play();
//									audio[0].clip = painClips[painIndex];
//									audio[0].Play();
//									AudioSource.PlayClipAtPoint(painClips[painIndex], skierView.transform.position);
								}else {
									if (NetworkCenter.Instance.GetPlayerNumber() == i){
										audio[2].PlayOneShot(failClip);
									}
								}

								int painIndex = UnityEngine.Random.Range(0,9);
								audio[1].volume = 1;
								audio[1].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
								audio[1].PlayOneShot(painClips[painIndex]);

								skierCollisionSoundActivated[i] = true;
							}

							if (skierModel.fallenTimer == 0 && skierModel.frozenTimer > 20 && UnityEngine.Random.Range(0, 100) < 5){
								int painIndex = UnityEngine.Random.Range(0,9);
								audio[1].volume = 0.5f;
								audio[1].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
								audio[1].PlayOneShot(painClips[painIndex]);
							}

						}
					}

				}

			}
		}
	}


	private void UpdatePosition(SkierModel skierModel, GameObject obj, Vector3 targetPos){
		float dist = Vector3.Distance(obj.transform.position, targetPos);
		if (skierModel.frozenTimer == 0){
			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpTimeFactor);
		}else {
			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpFrozenTimeFactor);
		}
	}
	
	

	public void SwitchPlayers(int oldPlayerId, int newPlayerId = 0){
		// game object
		GameObject tmpView = skierViews[oldPlayerId];
		skierViews[oldPlayerId] = skierViews[newPlayerId];
		skierViews[newPlayerId] = tmpView;
		// leaderboard
		string tmpName = skiersNames[oldPlayerId];
		skiersNames[oldPlayerId] = skiersNames[newPlayerId];
		skiersNames[newPlayerId] = tmpName;
		int tmpFinished = finishedSkiers[oldPlayerId];
		finishedSkiers[oldPlayerId] = finishedSkiers[newPlayerId];
		finishedSkiers[newPlayerId] = tmpFinished;
	}

	
	
	#region Views Creation
	

	private GameObject CreateSkier(int skierId, bool own){
		
		GameObject mainObj = GameObject.Instantiate(skierPrefab, new Vector3(0,0,10), Quaternion.identity) as GameObject;

		if (own) {
			mainObj.AddComponent<CameraTracker> ();
		} else {
			// change material
			SkinnedMeshRenderer[] renderers = mainObj.GetComponentsInChildren<SkinnedMeshRenderer> ();
			foreach (SkinnedMeshRenderer renderer in renderers) {
				renderer.material = Resources.Load<Material>("CharacterTex" + scrambledMaterialIds[skierId]);
			}
		}

//		Color color;
//		if (own){
//			color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
//		}else {
//			color = new Color(UnityEngine.Random.Range(0.2f, 1.0f), UnityEngine.Random.Range(0.2f, 1.0f), UnityEngine.Random.Range(0.25f, 1.0f), 1.0f);
//		}
//		SkinnedMeshRenderer[] renderers = mainObj.GetComponentsInChildren<SkinnedMeshRenderer>();
//		foreach(SkinnedMeshRenderer renderer in renderers) {
//			renderer.material.color = color;
//		}

		return mainObj;
	}


	private GameObject CreateView(float width, float height, Color color){
		//New mesh and game object
		GameObject obj = new GameObject();
		obj.name = "plane";
		Mesh mesh = new Mesh();
		
		//Components
		MeshFilter meshFilter= obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer= obj.AddComponent<MeshRenderer>();
		
		//Create a square mesh
		Vector3[] vertexes = new Vector3[4];
		vertexes[0] = new Vector3(0, 0);
		vertexes[1] = new Vector3(0,  height);
		vertexes[2] = new Vector3(width,  height);
		vertexes[3] = new Vector3(width, 0);
		mesh = CreateMesh(vertexes, color);
		
		//Assign materials
		meshRenderer.sharedMaterial = meshesMaterial;
		
		//Assign mesh to game object
		meshFilter.mesh = mesh;

		return obj;
	}
	
	
	private Mesh CreateMesh(Vector3[] vertexes, Color color){

		int i; // Counter used in for cycles
		
		// Create a new mesh
		Mesh mesh = new Mesh();

		// UVs
		Vector2[] uvs= new Vector2[vertexes.Length];
		
		for(i = 0; i < vertexes.Length; ++i){
			uvs[i] = new Vector2(0,0);
		}
		
		// Create triangles
		int[] tris= new int[3 * (vertexes.Length - 2)];    //3 verts per triangle * num triangles
		int C1;
		int C2;
		int C3;

		C1 = 0;
		C2 = 1;
		C3 = 2;
		
		for(i = 0 ; i < tris.Length ; i += 3){
			tris[i] = C1;
			tris[i+1] = C2;
			tris[i+2] = C3;
			
			C2++;
			C3++;
		}

		// Setup vertexes colors (here using all with the same color)
		Color[] colors = new Color[vertexes.Length];
		for (i = 0 ; i < vertexes.Length ; ++i){
			colors[i] = color;
		}
		
		//Assign data to mesh
		mesh.vertices = vertexes;
		mesh.uv = uvs;
		mesh.triangles = tris;
		mesh.colors = colors;
		
		//Recalculations
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();  
		mesh.Optimize();
		
		//Name the mesh
		mesh.name = "mesh";
		
		//Return the mesh
		return mesh;
	}


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
		if (skierViews == null) return;
		foreach (GameObject skierView in skierViews) {
			if (skierView != null){
				GameObject.Destroy(skierView);
			}
		}

		GameObject leaderboard = GuiMenus.Instance.leaderboardObject;
		leaderboard.SetActive(false);
	}


	
}

