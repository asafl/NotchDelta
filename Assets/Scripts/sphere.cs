using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sphere {

	// read initializaton variables for sphere // THIS WILL BE A STATIC CLASS - DOESN'T NEED TO STORE ANYTHING. 
/*	public sphere (GameObject cellIn, int detailLevelIn, Material materialIn) {
		cell = cellIn;
		detailLevel = detailLevelIn;
		material = materialIn;
	}*/
	
	// this function will create the MESH for a sphere for the inserted game object. 	
	public static void initializeSphere (GameObject cell, int detailLevel, Material material/*, out List<List<int>> neighborList*/) {
		// Creating Mesh variables
		List<Vector3> vertices = new List<Vector3>();
		List<int> tris = new List<int>();
		
		// Creating an Icosahedron (20 equally sized faces) shape
		geometryProvider.Icosahedron(vertices, tris);
		
		// Subdividing them up to the assigned detail level
		for (var i = 0; i < detailLevel; i++)
			geometryProvider.Subdivide(vertices, tris, true);
		
		int multiplier = 10;
		
		/// normalizing vectors to "inflate" the icosahedron into a sphere.
		// Also: Go over each vertex and get its neighbors, to do on sunday - think about doing it as part of the previous processes and not as a standalone procedure.
		for (var i = 0; i < vertices.Count; i++) {
			List<int> vertexTris;
			//neighborList.Insert(i, geometryProvider.getVertexNeighbors(tris, vertexTris, i));
			//vertices[i] = Vector3.Normalize(vertices[i]);
			vertices[i] = vertices[i].normalized;
			vertices[i] = vertices[i] * multiplier;
			//vertices[i] += cell.transform.position;
		}
		
		// Generating the mesh
		MeshFilter meshFilter = cell.AddComponent(typeof(MeshFilter)) as MeshFilter;		
		
		// Creating the mesh itself
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = tris.ToArray();

		//Creating the UVs - IS IT NEEDED?
		Vector2[] uvs = new Vector2[mesh.vertices.Length]; 
		for (int i=0; i<uvs.Length; i++) {
			uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
		}
		mesh.uv = uvs;
		
		// Assigning the mesh and the renderer
		meshFilter.mesh = mesh; 
		MeshRenderer meshRenderer = cell.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
		meshRenderer.material = material;
		
		Debug.Log ("Number of vertices: " + mesh.vertices.Length);
	}
	
}
