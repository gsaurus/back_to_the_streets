using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;


public class DebugWorldView:View<WorldModel>{
	
	// The views
	GameObject[] skierViews;

	// Common material used by all views
	static Material meshesMaterial;

	const float lerpTimeFactor = 5.0f; 

	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static DebugWorldView(){
		// setup imutable stuff
		meshesMaterial = new Material(Shader.Find("Sprites/Default"));
	}


	public DebugWorldView(WorldModel world){
		// Allocate memory for views arrays
		skierViews = new GameObject[WorldModel.MaxPlayers];
	}




	protected override void Update(WorldModel model, float deltaTime){

		UpdateSkiers(model, deltaTime);

	}


	private void UpdateSkiers(WorldModel model, float deltaTime){
		for (int i = 0 ; i < model.skiers.Length ; ++i){
			// update creation or destruction
			SkierModel skierModel = model.skiers[i];
			if (skierModel != null && skierViews[i] == null) {
				skierViews[i] = CreateSkier(NetworkCenter.Instance.GetPlayerNumber() == i);
			}else if (skierModel == null && skierViews[i] != null){
				GameObject.Destroy(skierViews[i]);
				skierViews[i] = null;
			}

			// update position
			if (skierViews[i] != null){
				Vector3 targetPos = new Vector3((float)skierModel.x, (float)skierModel.y, -0.2f);
				UpdatePosition(skierViews[i], targetPos);

				//tankViews[i].transform.RotateAround(new Vector3(0.5f, 0.5f, 0.0f), Vector3.forward, 0.4f);
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
	

	private GameObject CreateSkier(bool own){
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
