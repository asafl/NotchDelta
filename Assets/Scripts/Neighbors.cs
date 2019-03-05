using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct vertexDetails {
	public float vertexDensity; // average distance from all neighbors
	public float vertexExocytosisProbability;
	public List<int> neighborIndicesList; // list of indices of neighbors
	public List<int> neighborIndicesByProbability; 
	public List<float> distancePerNeighbor;
	public List<float> probabilityPerNeighbor; // by position in indices list
	//public List<float> probabilityDensityPerNeighbor; // summed probability for random with probability... (rec to decide where to turn)	
}

public class Neighbors {

	//public Dictionary<int,List<int>> neighborsForVertex = new Dictionary<int, List<int>>();
	// Example: neighborsForVertex[vertex index in vertices array] = List(number of neighbors)[index of first neighbor, index of second, ...]
	public Dictionary<int, vertexDetails> detailsForVertex = new Dictionary<int, vertexDetails>();
	public float averageDensity; 
	
	Mesh objectMesh;
	Cell2 cell;
	float dist;
		
	void addNeighborDetails (int vertexIndex, int indOfVertToAdd) {
		// if that index is not already in the neighbors list, add it.
		if (!detailsForVertex[vertexIndex].neighborIndicesList.Contains(indOfVertToAdd)) {
			dist = Vector3.Distance(cell.globalMeshVerts[vertexIndex], cell.globalMeshVerts[indOfVertToAdd]);
			if (dist == 0) { // if the distance is 0 - it's a fuck in the mesh. - do NOT treat this as a neighbor.
				return;
			}
			
			// add new index to list
			detailsForVertex[vertexIndex].neighborIndicesList.Add(indOfVertToAdd);
			// measure distance to that neighbor, insert in corresponding place in array.
			detailsForVertex[vertexIndex].distancePerNeighbor.Add(dist);
		}
		
	}
	
	void addAllVerticesToEachOthersLists (int ind1, int ind2, int ind3) {
		// go to the index of vertex 1 position in the array, look in its list - and try to add both neighbors if they're not already there.
		addNeighborDetails(ind1, ind2);
		addNeighborDetails(ind1, ind3);
		
		addNeighborDetails(ind2, ind1);
		addNeighborDetails(ind2, ind3);
		
		addNeighborDetails(ind3, ind1);
		addNeighborDetails(ind3, ind2);
		
	}
	
	// calculate the chances, per each vertex, to go to each of the neighbors, by the distance to that neighbor. SEEMS TO BE WRONG.
	void calculateStandardProbability () {
		float sumOfDensities = 0;
		float sumOfDistances;
		float sumOfInverseSquaredDistances;		
		float prevDist; 
		
		// After finishing the neighbors list, calculate density and probability for each neighbor.
		for (int j = 0; j < detailsForVertex.Count; j++) {
			sumOfDistances = 0;
			sumOfInverseSquaredDistances = 0;
			
			vertexDetails tempDets = new vertexDetails();
			tempDets = detailsForVertex[j];
			
			// calculate density and probability for each neighbor
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				sumOfDistances += detailsForVertex[j].distancePerNeighbor[i];
				sumOfInverseSquaredDistances += 1/Mathf.Pow(detailsForVertex[j].distancePerNeighbor[i],2);
			}
			
			tempDets.vertexDensity = (float)sumOfDistances / (float)detailsForVertex[j].neighborIndicesList.Count;
			sumOfDensities += tempDets.vertexDensity;
			
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				tempDets.probabilityPerNeighbor.Add(1/Mathf.Pow(detailsForVertex[j].distancePerNeighbor[i],2) / sumOfInverseSquaredDistances);
				/*prevDist = (i == 0) ? 0 : tempDets.probabilityDensityPerNeighbor[i-1];				
				tempDets.probabilityDensityPerNeighbor.Add (prevDist + tempDets.probabilityPerNeighbor[i]);*/
				// now inserting the correlative number of neighbors to the probability - so that running a random on this list will effectively create a random with probability.
				int numOfNeighborsForProbability = Mathf.RoundToInt(tempDets.probabilityPerNeighbor[i] * 100);
				for (int k = 0; k < numOfNeighborsForProbability; k++) {
					tempDets.neighborIndicesByProbability.Add(detailsForVertex[j].neighborIndicesList[i]);
				}				
			}
			
			detailsForVertex[j] = tempDets;
		}
		
		averageDensity = (float)sumOfDensities / (float)detailsForVertex.Count;
	}
	
	void calculateDensityForExocytosis () {
		// Setting exocytosis probabilities to all vertices. 
		for (int j = 0; j < detailsForVertex.Count; j++) {
			vertexDetails tempDets = new vertexDetails();
			tempDets = detailsForVertex[j];
			
			// The "denser" the vertex is, the smaller the average distance from surrounding vertices. 
			// Exocytosis probability should be in direct correlation to density - so that the higher the density value (the less dense the vertex), the higher the chances to exocytose there. 
			tempDets.vertexExocytosisProbability = tempDets.vertexDensity / averageDensity;
			
			detailsForVertex[j] = tempDets;
		}		
	}
	
	// builds a map of the neighbors of each vector
	public void buildNeighborsDetails(Cell2 icell, Mesh mesh) { 
		objectMesh = mesh;
		cell = icell;
		
		int[] t = mesh.triangles;
		//verticesHash = new ArrayList(mesh.vertices.Length);
		
		/*for (int i = 0; i < mesh.vertices.Length; i++) { 
			neighborsForVertex.Add(i, new List<int>());
		}*/
		//vertexDetails tempDets;
		
		// Initialize vertexDetails array to each vertex.
		for (int i = 0; i < mesh.vertices.Length; i++) {
			vertexDetails tempDets = new vertexDetails();
			tempDets.neighborIndicesList = new List<int>();
			tempDets.distancePerNeighbor = new List<float>();
			tempDets.probabilityPerNeighbor = new List<float>();
			//tempDets.probabilityDensityPerNeighbor = new List<float>();
			tempDets.neighborIndicesByProbability = new List<int>();
			
			detailsForVertex.Add(i, tempDets);
		}
		
		// For each trio of indices of vectors that form a triangle, register that they are neighbors of each other...
		for (int i = 0; i < t.Length; i+=3) {
			// read the indices of the vertices that make up the tri
			int indexOfVert1 = t[i];
			int indexOfVert2 = t[i+1];
			int indexOfVert3 = t[i+2];
			
			addAllVerticesToEachOthersLists(indexOfVert1, indexOfVert2, indexOfVert3);
		}
		
		calculateStandardProbability();
		
		calculateDensityForExocytosis();		
	}
	
}
