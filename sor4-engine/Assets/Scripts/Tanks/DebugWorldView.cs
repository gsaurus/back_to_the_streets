using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class DebugWorldView:View<WorldModel>{
	
	// The views
	GameObject[] mapViews;
	GameObject[] tankViews;
	GameObject[] bulletViews;
	GameObject backgroundView;

	// Common material used by all views
	static Material meshesMaterial;

	const float lerpTimeFactor = 5.0f; 

	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static DebugWorldView(){
		// setup imutable stuff
		meshesMaterial = new Material(Shader.Find("Sprites/Default"));
	}


	public DebugWorldView(){
		// Allocate memory for views arrays
		mapViews = new GameObject[WorldModel.MaxWidth * WorldModel.MaxHeight];
		tankViews = new GameObject[WorldModel.MaxPlayers];
		bulletViews = new GameObject[WorldModel.MaxPlayers * WorldModel.MaxBulletsPerPlayer];
		lastKnownMap = new int[mapViews.Length];

		// Setup background, immutable
		backgroundView = CreateBackground();
	}




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
			if (tankModel != null && tankViews[i] == null) {
				tankViews[i] = CreateTank(NetworkCenter.Instance.GetPlayerNumber() == i);
			}else if (tankModel == null && tankViews[i] != null){
				GameObject.Destroy(tankViews[i]);
				tankViews[i] = null;
			}

			// update position
			if (tankViews[i] != null){
				Vector3 targetPos = new Vector3((float)tankModel.position.X, (float)tankModel.position.Y, -0.2f);
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
				Vector3 targetPos = new Vector3((float)bulletModel.position.X, (float)bulletModel.position.Y, -0.2f);
				UpdatePosition(bulletViews[i], targetPos);
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
	
	
	
	
	
	#region Views Creation


	private GameObject CreateBackground(){
		return CreateView(WorldModel.MaxWidth, WorldModel.MaxHeight, new Color(0.75f, 0.75f, 0.8f, 1.0f));
	}

	private GameObject CreateTank(bool own){
		Color color;
		if (own){
			color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
		}else {
			color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		}
		GameObject mainObj = CreateView(1.0f, 1.0f, color);
		GameObject aimObj = CreateView(0.2f, 0.8f, new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f));
		aimObj.transform.position = new Vector3(0.4f, 0.3f, -0.005f);
		aimObj.transform.parent = mainObj.transform;
		return mainObj;
	}

	private GameObject CreateBullet(bool own){
		Color color;
		if (own){
			color = new Color(0.5f, 0.5f, 1.0f, 1.0f);
		}else {
			color = new Color(1.0f, 0.5f, 0.5f, 1.0f);
		}
		return CreateView(0.1f, 0.1f, color);
	}

	private GameObject CreateBlock(int type, int x, int y){

		// empty block, nothing to create
		if (type == 0)
			return null;

		Color color;
		float z;
		switch (type){
			case 1: // concrete
				color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
				z = -0.1f;
			break;
			case 2: // water
				color = new Color(0.0f, 0.2f, 0.4f, 1.0f);
				z = -0.1f;
			break;
			case 3: // florest
				color = new Color(0.3f, 0.6f, 0.0f, 1.0f);
				z = -0.3f;
			break;
			default:
				// ??? unknown type of block
				color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
				z = -1.0f;
			break;
		}
		GameObject obj = CreateView(1, 1, color);
		obj.transform.position = new Vector3(x, y, z);
		return obj;
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

#endregion

	
}

