using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class geometryProvider {

	// This function will return the vertices array with a new midpoint between the vertices at indexes i0 and i1, and the new midpoint's index in that array
	private static int GetMidpointIndex(Dictionary<string, int> midpointIndices, List<Vector3> vertices, int i0, int i1)
	{
		// Fomatting the edgekey for the dictionary entry search
		var edgeKey = string.Format("{0}_{1}", Mathf.Min(i0, i1), Mathf.Max(i0, i1));
		
		var midpointIndex = -1;
		
		// if the value isn't in the array already, find it
		if (!midpointIndices.TryGetValue(edgeKey, out midpointIndex))
		{
			var v0 = vertices[i0];
			var v1 = vertices[i1];
		
			// finding the midpoint itself - the center between two vertices. 	
			var midpoint = (v0 + v1) / 2f;
			
			// if the vertices array already contains that exact point (meaning it was already found), set its index (saved in the tris array) to be the one that already exists.
			if (vertices.Contains(midpoint))
				midpointIndex = vertices.IndexOf(midpoint);
			else
			{
				midpointIndex = vertices.Count; // it's index is at the end of the vertices array. 
				vertices.Add(midpoint); // add the newfound midpoint to the vertices array
			}
		}
		
		return midpointIndex;
		
	}
	
	// This function will subdivide each edge of the triangle into two, creating four faces from one.
	///      i0
	///     /  \
	///    m02-m01
	///   /  \ /  \
	/// i2---m12---i1
	public static void Subdivide(List<Vector3> vertices, List<int> tris, bool removeSourceTriangles)
	{
		// Setting the midpoint indexes Dictionart - the string will be the start and end vertex of the midpoint, such as: 
		var midpointIndices = new Dictionary<string, int>();
		
		// Setting the tris List. 
		var newTris = new List<int>(tris.Count * 4);

		// If you DON'T want to remove source tris (removeSourceTriangles = false) - add them to the new indexes list. (but we probably do)
		if (!removeSourceTriangles)
			newTris.AddRange(tris);
		
		// for each face (three tri entries), find the midpoints.
		for (var i = 0; i < tris.Count - 2; i += 3)
		{
			// getting the indexes (in the vertices array) of the tri
			var i0 = tris[i];
			var i1 = tris[i + 1];
			var i2 = tris[i + 2];
			
			// get the midpoints between all the vertices of the current tri. notice that the vertices array is also updated. 
			var m01 = GetMidpointIndex(midpointIndices, vertices, i0, i1);
			var m12 = GetMidpointIndex(midpointIndices, vertices, i1, i2);
			var m02 = GetMidpointIndex(midpointIndices, vertices, i2, i0);
			
			// add the new midpoints as indexes to the newTris array (thus creating four new faces from one)
			newTris.AddRange(
				new[] {
				i0,m01,m02, 
				i1,m12,m01, 
				i2,m02,m12,
				m02,m01,m12
			}
			);
			
		}
		
		// update the tris array with the new faces.
		tris.Clear();
		tris.AddRange(newTris);
	}
	
	// This function will script creation of an Icosahedron	
	public static void Icosahedron(List<Vector3> vertices, List<int> tris)
	{
		tris.AddRange(new int[] 
		{1,4,0,
		4,9,0,
		4,5,9,
		8,5,4,
		1,8,4,
		1,10,8,
		10,3,8,
		8,3,5,
		3,2,5,
		3,7,2,
		3,10,7,
		10,6,7,
		6,11,7,
		6,0,11,
		6,1,0,
		10,1,6,
		11,0,9,
		2,11,9,
		5,2,9,
		11,2,7});
		
		float X = 0.525731112119133606f;
		float Z = 0.850650808352039932f;
				
		vertices.AddRange(new[] {
			new Vector3(-X, 0f, Z),
			new Vector3(X, 0f, Z),
			new Vector3(-X, 0f, -Z),
			new Vector3(X, 0f, -Z),
			new Vector3(0f, Z, X),
			new Vector3(0f, Z, -X),
			new Vector3(0f, -Z, X),
			new Vector3(0f, -Z, -X),
			new Vector3(Z, X, 0f),
			new Vector3(-Z, X, 0f),
			new Vector3(Z, -X, 0f),
			new Vector3(-Z, -X, 0f) 
		});
		
	}
	
	// This function will return the indexes of the neighboring vertices for the selected vertex.
	public static List<int> getVertexNeighbors (List<int> tris, out List<int> vertexTris, int selectedVertIndex)
	{
		vertexTris = new List<int>();
		List<int> indexOfVertInTris = new List<int>();
		List<int> indexOfVertNeighbors = new List<int>();
		int searchFromIndex = 0;
		int indexFound = 0;

		indexFound = Array.IndexOf(tris.ToArray(), selectedVertIndex, searchFromIndex);
		
		//find the occurances of the selected vertex in the tris array - SHOULD IMPROVE IT SINCE MOVING TO LIST FOR TRIS INSTEAD OF ARRAY
		while (indexFound != -1) {
			// insert found index into array and increase it by one to continue searching
			indexOfVertInTris.Add(indexFound);
			searchFromIndex = indexFound+1;
			indexFound = Array.IndexOf(tris.ToArray(), selectedVertIndex, searchFromIndex);
		}

		List<int> neighborsOfIndex = new List<int>();

		// for all the indices of the selected vertex in the tris array, return the vertices that complete that triangle, 
		// meaning its neighbors. This will also remove the original found triangle from the array, since we are subdividing it and don't need it.
		foreach(int index in indexOfVertInTris) {
			switch (index % 3)
			{
				case 0: // it's the first index in a tri
				neighborsOfIndex.AddRange(new int[] {tris[index], tris[index+1], tris[index+2]});
				break;
				
				case 1: // it's the second index in a tri
				neighborsOfIndex.AddRange(new int[] {tris[index-1], tris[index], tris[index+1]});
				break;
				
				case 2: // it's the last index in a tri
				neighborsOfIndex.AddRange(new int[] {tris[index-2], tris[index-1],tris[index]});
				break;
			}
		}

		// This array will contain all the tris themselves (including the searched index) that the index is found in.
		vertexTris.AddRange(neighborsOfIndex);
						
		// cleaning the duplicate values using Linq - there are duplicates because each pair of tris here share a vertex
		List<int> neighborsOfIndexNoDupes = neighborsOfIndex.Distinct().ToList();
		
		return neighborsOfIndexNoDupes;
	
	}
	
	public static float gaussian (float t, float r, float sigma) {
		return Mathf.Exp(-Mathf.Pow(r, 2)/(2*Mathf.Pow(sigma, 2)*(1/t)))/Mathf.Sqrt(2* Mathf.Pow(sigma, 2)*(1/t));
	}

	public static float lorentzian (float t, float r, float sigma) {
		return t*Mathf.Pow(sigma, 2)/(Mathf.Pow(sigma, 2) + Mathf.Pow(r, 2));
	}
	
			
}