using System;
using System.Collections.Generic;
using UnityEngine;


namespace RetroBread{


	public class DebugPhysicPlaneView: View<PhysicPlaneModel>{

		private GameObject[] meshObjects = new GameObject[2];


		public DebugPhysicPlaneView(PhysicPlaneModel model){
			for(int x = 0; x < 2; x++){
				
				//New mesh and game object
				meshObjects[x] = new GameObject();
				meshObjects[x].name = "plane";
				Mesh mesh = new Mesh();
				
				//Components
				MeshFilter MF= meshObjects[x].AddComponent<MeshFilter>();
				MeshRenderer MR= meshObjects[x].AddComponent<MeshRenderer>();
				//myObject[x].AddComponent<>();
				
				//Create mesh
				mesh = CreateMesh(model, x);
				
				//Assign materials
				MR.material = new Material(Shader.Find("Diffuse"));
				
				//Assign mesh to game object
				MF.mesh = mesh;
			}
		}



		Mesh CreateMesh (PhysicPlaneModel model, int num){
			int x; //Counter
			
			//Create a new mesh
			Mesh mesh = new Mesh();

			List<Vector3> vertexes = new List<Vector3>(model.offsets.Count + 1);
			vertexes.Add(new Vector3());
			foreach (FixedVector3 offset in model.offsets){
				vertexes.Add(offset.AsVector3());
			}
			
			//UVs
			Vector2[] uvs= new Vector2[vertexes.Count];
			
			for(x = 0; x < vertexes.Count; x++)
			{
				if((x%2) == 0)
				{
					uvs[x] = new Vector2(0,0);
				}
				else
				{
					uvs[x] = new Vector2(1,1);
				}
			}
			
			//Triangles
			int[] tris= new int[3 * (vertexes.Count - 2)];    //3 verts per triangle * num triangles
			int C1;
			int C2;
			int C3;
			
			if(num == 0)
			{
				C1 = 0;
				C2 = 1;
				C3 = 2;
				
				for(x = 0; x < tris.Length; x+=3)
				{
					tris[x] = C1;
					tris[x+1] = C2;
					tris[x+2] = C3;
					
					C2++;
					C3++;
				}
			}
			else
			{
				C1 = 0;
				C2 = vertexes.Count - 1;
				C3 = vertexes.Count - 2;
				
				for(x = 0; x < tris.Length; x+=3)
				{
					tris[x] = C1;
					tris[x+1] = C2;
					tris[x+2] = C3;
					
					C2--;
					C3--;
				}  
			}
			
			//Assign data to mesh
			mesh.vertices = vertexes.ToArray();
			mesh.uv = uvs;
			mesh.triangles = tris;
			
			//Recalculations
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();  
			mesh.Optimize();
			
			//Name the mesh
			mesh.name = "mesh";
			
			//Return the mesh
			return mesh;
		}


		protected override void Update(PhysicPlaneModel model, float deltaTime){
			foreach (GameObject obj in meshObjects){
				obj.transform.position = model.origin.AsVector3();
			}

			List<FixedVector3> points = model.GetPointsList();
			for (int i = 1 ; i < points.Count ; ++i){
				UnityEngine.Debug.DrawLine(points[i-1].AsVector3(), points[i].AsVector3(), UnityEngine.Color.grey);
			}
			UnityEngine.Debug.DrawLine(points[points.Count-1].AsVector3(), points[0].AsVector3(), UnityEngine.Color.grey);
			UnityEngine.Debug.DrawLine(points[0].AsVector3(), (points[0] + model.normal).AsVector3(), UnityEngine.Color.red);
		}
			

	}


}

