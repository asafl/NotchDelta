using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

public class sphereManager2 : MonoBehaviour {
	
	private List<Vector3> vertices = new List<Vector3>();
	private List<int> tris = new List<int>();
	
	private Mesh sphereMesh;
	
	// Sphere variables
	public Material material;
	public int detailLevel; 
	
	// Use this for initialization
	void Start () {
		// creating the meshfilter and mesh for the sphere gameobject
		sphere.initializeSphere(gameObject, detailLevel, material);
		
		sphereMesh = GetComponent<MeshFilter>().mesh;
		// Converting the arrays to lists for easier handling.
		tris = sphereMesh.triangles.OfType<int>().ToList();
		vertices = sphereMesh.vertices.OfType<Vector3>().ToList();
	
		// updating mesh after tentacle creation
		sphereMesh.vertices = vertices.ToArray();
		sphereMesh.triangles = tris.ToArray();
		sphereMesh.RecalculateBounds();
		sphereMesh.RecalculateNormals();
		
		var savePath = "assets/Tests/sphereMesh2_rad10.asset";
		Debug.Log("Saved Mesh to:" + savePath);
		AssetDatabase.CreateAsset(sphereMesh, savePath);
		AssetDatabase.SaveAssets();
		
		gameObject.GetComponent<MeshFilter>().mesh = sphereMesh;
		PrefabUtility.CreatePrefab("assets/Tests/sphereMesh2_rad10.prefab", this.gameObject);
		
	}
}
