using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct vertexDetails2 {
	public int vertexIndex; 
	//public float vertexDensity; // average distance from all neighbors
	//public float vertexExocytosisProbability;
	public List<Vector3> vectorsToNeighbors; // in LOCAL COORDINATES.
	public List<int> neighborIndicesList; // list of indices of neighbors
	public List<int> neighborIndicesByProbability;  // list of indices, where each neighbor appears relative to its probability.
	//public List<float> probabilityPerNeighbor; // by position in indices list
	//public List<float> probabilityDensityPerNeighbor; // summed probability for random with probability... (rec to decide where to turn)	
}

public class Neighbors2 {
	
	//public Dictionary<int,List<int>> neighborsForVertex = new Dictionary<int, List<int>>();
	// Example: neighborsForVertex[vertex index in vertices array] = List(number of neighbors)[index of first neighbor, index of second, ...]
	public Dictionary<int, vertexDetails2> detailsForVertex = new Dictionary<int, vertexDetails2>();
	public float averageDensity; 
	
	private class neighborDetails {
		public Vector3 projectedVector; // the vector's projection on the plane
		public float angleSpan; 
		public float angleFromPrevious;
		public float relativeAngleSpan;
		public float fullAngle; // PHI angle
		public int neighborIndex;		
		
		/*public static implicit operator neighborDetails(int neighborIndex, float fullAngle) {
			return new MyStruct() { s = value, length = value.Length };
		}*/
	}
	
	Mesh objectMesh;
	Cell2 cell;
	float dist;
	
	void addNeighborDetails (int vertexIndex, int indOfVertToAdd) {
		// if that index is not already in the neighbors list, and it is not the same vertex itself, add it.
		if (!detailsForVertex[vertexIndex].neighborIndicesList.Contains(indOfVertToAdd) && vertexIndex != indOfVertToAdd) {
			// add new index to list
			detailsForVertex[vertexIndex].neighborIndicesList.Add(indOfVertToAdd);
			detailsForVertex[vertexIndex].vectorsToNeighbors.Add(objectMesh.vertices[indOfVertToAdd] - objectMesh.vertices[vertexIndex]);
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
	
	
	private float calcAngleFromPrevious (List<neighborDetails> sortedNeighborDets, int index) {
		float angle;
		
		if (index == 0) {
            //angle = Vector3.Angle(sortedNeighborDets[0].projectedVector, sortedNeighborDets.Last().projectedVector);
            angle = sortedNeighborDets[0].fullAngle + (360 - sortedNeighborDets.Last().fullAngle);
        } else {
            angle = sortedNeighborDets[index].fullAngle - sortedNeighborDets[index - 1].fullAngle;
		}
		
		return angle;
	}
	
	private float calcAngleFromNext (List<neighborDetails> sortedNeighborDets, int index) {
		float angle;
		
		if (index == sortedNeighborDets.Count - 1) { // if it's the last vector
            //angle = Vector3.Angle(sortedNeighborDets[0].projectedVector, sortedNeighborDets.Last().projectedVector);
            angle = sortedNeighborDets[0].fullAngle + (360 - sortedNeighborDets.Last().fullAngle);
        } else {
            //angle = Vector3.Angle(sortedNeighborDets[index + 1].projectedVector, sortedNeighborDets[index].projectedVector);
            angle = sortedNeighborDets[index + 1].fullAngle - sortedNeighborDets[index].fullAngle;
        }
        
        return angle;
    }
    
	// calculate the angle span of a vector in the sorted neighbor details array.
	private float calcAngleSpan (List<neighborDetails> sortedNeighborDets, int index) {
		// angle to previous vector + angle to next vector
		return calcAngleFromPrevious(sortedNeighborDets, index)/2 + calcAngleFromNext(sortedNeighborDets, index)/2;
	}
	
	// calculate the chances, per each vertex, to go to each of the neighbors, by the angles to each neighbor. 
	void calculateAngularProbability () {
		float sumOfDensities = 0;
		float sumOfDistances;
		float sumOfInverseSquaredDistances;		
		float prevDist; 
		
		// Iterate over all vertices and calculate chances to go to each neighbor, by the angle to that neighnbor.
		for (int j = 0; j < detailsForVertex.Count; j++) {
            //if (j==900) {
            //    Debug.Log("900");
            //}
            

			vertexDetails2 tempDets = new vertexDetails2();
			tempDets = detailsForVertex[j];
			
			Vector3 vectorSum = Vector3.zero;
			// find the sum vector of all neighbors:
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				vectorSum += detailsForVertex[j].vectorsToNeighbors[i].normalized;
			}

			if (vectorSum != Vector3.zero) {
				// creating a neighborDetail struct array for all neigbors, so it'd be easier to work with. 
				List<neighborDetails> neighborDets = new List<neighborDetails>();				
				
				for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
					// find the projection of the neighbors on the plane defined by the sum vector as its normal.
					Vector3 mProjectedVector = Vector3.ProjectOnPlane(detailsForVertex[j].vectorsToNeighbors[i], vectorSum);
					neighborDets.Add(new neighborDetails() {
						projectedVector = mProjectedVector,
						neighborIndex = detailsForVertex[j].neighborIndicesList[i], 
						fullAngle = Utils.phiInPlane(mProjectedVector, vectorSum)
                    });                    
				}
				
				// sorting the vectors indices by the size of the phi angle. Now we have the vectors in their order on the plane.
				List<neighborDetails> sortedNeighborDetails = neighborDets.OrderBy(neighbor => neighbor.fullAngle).ToList();

				// calculate the angle span of each vector -- see function that does it.
				for (int i = 0; i < sortedNeighborDetails.Count; i++) {
					sortedNeighborDetails[i].angleFromPrevious = calcAngleFromPrevious(sortedNeighborDetails, i);
					sortedNeighborDetails[i].angleSpan = calcAngleSpan(sortedNeighborDetails, i);
				}
				
				float sumOfAngles = sortedNeighborDetails.Select(neighbor => neighbor.angleFromPrevious).Sum(); // I think it should be 360, but just in case.
				
				// Normalizing the array by the smallest member, so that there wouldn't be so many members in the 'by probability' array, and it would run faster. 
				sortedNeighborDetails.ForEach(neighbor => {
					neighbor.relativeAngleSpan = (float)(neighbor.angleSpan/sumOfAngles);
				});
				

				// remove extremely small angle spans - such angles mean that there's a duplicate path somewhere, which shouldn't appear
				sortedNeighborDetails = sortedNeighborDetails.Where(neighbor => neighbor.angleSpan > 1).ToList<neighborDetails>();
				
				// finding the smallest member and calculating its division from 1 - then, when multiplying all members by this multiplier, the smallest would be 1 and all the rest would be bigger. This is a way to keep the proportional array small.
				//float multiplier = 1 / sortedNeighborDetails.Select(n => n.relativeAngleSpan).Min();
								
				for (int i = 0; i < sortedNeighborDetails.Count; i++) {					
					// now inserting the correlative number of neighbors to the probability - so that running a random on this list will effectively create a random with probability.
					int numOfNeighborsForProbability = Mathf.RoundToInt(sortedNeighborDetails[i].relativeAngleSpan * 100);
					
					/*if (numOfNeighborsForProbability > 10) {
						Debug.Log("num of neighbors: " + numOfNeighborsForProbability);	
					}					*/
					
					for (int k = 0; k < numOfNeighborsForProbability; k++) {
						tempDets.neighborIndicesByProbability.Add(sortedNeighborDetails[i].neighborIndex);
					}				
				}
				
			} else { // the only case where the sum of normalized vectors is the 0 vector is if they are orthogonal, the chances are equal between them. 
				// Just copy the neighbor list
				tempDets.neighborIndicesByProbability = tempDets.neighborIndicesList;
			}

			
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
		//vertexDetails2 tempDets;
		
		// Initialize vertexDetails2 array to each vertex.
		for (int i = 0; i < mesh.vertices.Length; i++) {
			vertexDetails2 tempDets = new vertexDetails2();
			tempDets.vertexIndex = i;
			tempDets.vectorsToNeighbors = new List<Vector3>();
			tempDets.neighborIndicesList = new List<int>();
			tempDets.neighborIndicesByProbability = new List<int>();
			//tempDets.probabilityPerNeighbor = new List<float>();
			
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
		
		calculateAngularProbability();
		
		//calculateDensityForExocytosis();		
	}
	
}


// calculate the chances, per each vertex, to go to each of the neighbors, by the angles to each neighbor. 
/*void calculateAngularProbability () {
	float sumOfDensities = 0;
	float sumOfDistances;
	float sumOfInverseSquaredDistances;		
	float prevDist; 
	
	// Iterate over all vertices and calculate chances to go to each neighbor, by the angle to that neighnbor.
	for (int j = 0; j < detailsForVertex.Count; j++) {
		vertexDetails2 tempDets = new vertexDetails2();
		tempDets = detailsForVertex[j];
		
		Vector3 vectorSum = Vector3.zero;
		// find the sum vector of all neighbors:
		for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
			vectorSum += detailsForVertex[j].vectorsToNeighbors[i].normalized;
		}
		
		if (vectorSum != Vector3.zero) {
			List<Vector3> vectorProjections = new List<Vector3>();	
			// find the projection of the neighbors on the plane defined by the sum vector as its normal.
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				// sum all vector positions relative to the vertex itself
				vectorProjections.Add(Vector3.ProjectOnPlane(detailsForVertex[j].vectorsToNeighbors[i], vectorSum));
			}
			
			List<float> vectorAngles = new List<float>();
			// finding the angle of each of the projections relative to some coordinate system 
			for (int i = 0; i < vectorProjections.Count; i++) {
				vectorAngles.Add(Utils.phi(vectorProjections[i]));				
			}
			
			// creating a neighborDetail struct array for all neigbors, so it'd be easier to work with. 
			List<neighborDetails> neighborDets = new List<neighborDetails>();
			
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				neighborDets.Add(new neighborDetails() {
					neighborIndex = detailsForVertex[j].neighborIndicesList[i], fullAngle = vectorAngles[i]
				});
			}
			
			// sorting the vectors indices by the size of the angle
			List<neighborDetails> sortedNeighborDetails = neighborDets.OrderBy(neighbor => neighbor.fullAngle).ToList();
			
			// adding 360 angle at the end of the array, again with the index of the smallest vertex (it's now cyclical)
			// initialize a new last member 
			neighborDetails lastMember = new neighborDetails() {
				fullAngle = 360, neighborIndex = sortedNeighborDetails[0].neighborIndex
			};
			sortedNeighborDetails.Add(lastMember);		
			
			sortedNeighborDetails[0].angle = sortedNeighborDetails[0].fullAngle;
			for (int i = 0; i < sortedNeighborDetails.Count-1; i++) {
				sortedNeighborDetails[i+1].angle = sortedNeighborDetails[i+1].fullAngle - sortedNeighborDetails[i].fullAngle;
			}
			
			// Adjusting the array for the last / first member:
			sortedNeighborDetails[0].angle = sortedNeighborDetails[0].angle + sortedNeighborDetails.Last().angle;
			// remove the last "dummy" member.
			sortedNeighborDetails.RemoveAt(sortedNeighborDetails.Count-1);				
			
			// iterating over the sorted array - to find the angle that that neighbor SPANS, which represents the probability to go to each angle (the MIFTACH of the angle). This is equivalent to the width of area that vertex represents. 
			// Example: 
			for (int i = 0; i < sortedNeighborDetails.Count-1; i++) {
				sortedNeighborDetails[i].angleSpan = (sortedNeighborDetails[i+1].angle + sortedNeighborDetails[i].angle)/2;
			}
			// it is cyclical
			sortedNeighborDetails.Last().angleSpan = (sortedNeighborDetails.Last().angle + sortedNeighborDetails[0].angle)/2;
			
			// Now we have an array that represents the probability to go in the direction of each neighbor.				
			// The decision to which vertex to go would be achieved by drawing a number between 0 and 360, and whichever angle it is smaller than - that vertex is the direction. 				
			// Create a new array where the number of times each neighbor correlates to its probability.
			float sumOfAngles = sortedNeighborDetails.Select(neighbor => neighbor.angleSpan).Sum(); // I think it should be 360, but just in case.
			
			// Normalizing the array by the smallest member, so that there wouldn't be so many members in the 'by probability' array, and it would run faster. 
			sortedNeighborDetails.ForEach(neighbor => {
				neighbor.relativeAngleSpan = (float)(neighbor.angleSpan/sumOfAngles);
			});
			
			// remove extremely small relative angles - such angles mean that there's a duplicate path somewhere, which shouldn't appear
			sortedNeighborDetails = sortedNeighborDetails.Where(neighbor => neighbor.relativeAngleSpan > 0.001).ToList<neighborDetails>();
			
			// finding the smallest member and calculating its division from 1 - then, when multiplying all members by this multiplier, the smallest would be 1 and all the rest would be bigger. This is a way to keep the proportional array small.
			//float multiplier = 1 / sortedNeighborDetails.Select(n => n.relativeAngleSpan).Min();
			
			for (int i = 0; i < sortedNeighborDetails.Count; i++) {
				
				// now inserting the correlative number of neighbors to the probability - so that running a random on this list will effectively create a random with probability.
				int numOfNeighborsForProbability = Mathf.RoundToInt(sortedNeighborDetails[i].relativeAngleSpan * 100);
                
                for (int k = 0; k < numOfNeighborsForProbability; k++) {
                    tempDets.neighborIndicesByProbability.Add(sortedNeighborDetails[i].neighborIndex);
                }				
            }
            
        } else { // the only case where the sum of normalized vectors is the 0 vector is if they are orthogonal, the chances are equal between them. 
            // Just copy the neighbor list
            tempDets.neighborIndicesByProbability = tempDets.neighborIndicesList;
        }
        
        
        detailsForVertex[j] = tempDets;
    }
    
}
*/

/*// calculate the chances, per each vertex, to go to each of the neighbors, by the angles to each neighbor. 
void calculateAngularProbability () {
	float sumOfDensities = 0;
	float sumOfDistances;
	float sumOfInverseSquaredDistances;		
	float prevDist; 
	
	// Iterate over all vertices and calculate chances to go to each neighbor, by the angle to that neighnbor.
	for (int j = 0; j < detailsForVertex.Count; j++) {
		vertexDetails2 tempDets = new vertexDetails2();
		tempDets = detailsForVertex[j];
		
		Vector3 vectorSum = Vector3.zero;
		// find the sum vector of all neighbors:
		for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
			vectorSum += detailsForVertex[j].vectorsToNeighbors[i].normalized;
		}
		
		if (vectorSum != Vector3.zero) {
			List<Vector3> vectorProjections = new List<Vector3>();	
			// find the projection of the neighbors on the plane defined by the sum vector as its normal.
			for (int i = 0; i < detailsForVertex[j].neighborIndicesList.Count; i++) {
				// sum all vector positions relative to the vertex itself
				vectorProjections.Add(Vector3.ProjectOnPlane(detailsForVertex[j].vectorsToNeighbors[i], vectorSum));
			}
			
			List<float> vectorAngles = new List<float>();
			// finding the angle of each of the projections relative to some coordinate system 
			for (int i = 0; i < vectorProjections.Count; i++) {
				vectorAngles.Add(Utils.phi(vectorProjections[i]));				
			}
			
			//Sorting the neighbors array by the angles, so it would correspond in indices to the sorted angles array. 
			// Prepare empty neighbor indices array to be sorted: 
			List<int> sortedNeighborIndices = new List<int>();
			int indOfMin;
			for (int i = 0; i < vectorAngles.Count; i++) {
				indOfMin = vectorAngles.IndexOf(vectorAngles.Min ());
				sortedNeighborIndices.Add(detailsForVertex[j].neighborIndicesList[indOfMin]);
				vectorAngles[indOfMin] = 40000; // can't simply remove it because it will destroy the run.
			}
				
				// adding 360 angle at the end of the array, again with the index of the smallest vertex (it's now cyclical)
				//sortedSummedAngles.Add(360);
				
			List<float> sortedAngles = new List<float>();
				sortedAngles.Add(sortedSummedAngles[0]);
				for (int i = 0; i < sortedSummedAngles.Count-1; i++) {
					sortedAngles.Add(sortedSummedAngles[i+1] - sortedSummedAngles[i]);
				}
			
			
			///// STOPPED WORK HERE! BELOW THIS NOTHING'S FIXEDs
			// Adjusting the array for the last / first member:
			sortedAngles[0] = sortedAngles[0] + sortedAngles.Last();
			// remove the last "dummy" member.
			sortedAngles.RemoveAt(sortedAngles.Count-1);
			
			
			
			
			
			List<float> crossAngles = new List<float>();
			// iterating over the sorted array - instead of the angles -> that angle + cross angle (HOTSE ZAVIT), which represents the probability to go to each angle (the MIFTACH of the angle). This is equivalent to the width of area that vertex represents. 
			// Example: 30(neighbor: 2), 120 (1), 180 (3), 360 (2 again!)-> 75 (2), 150 (1), 270 (3), 360 (2). 
			for (int i = 0; i < sortedAngles.Count-1; i++) {
				crossAngles.Add((sortedAngles[i+1] + sortedAngles[i])/2);
			}
			// it is cyclical
			crossAngles.Add((sortedAngles.Last() + sortedAngles[0])/2);
			
			// Now we have an array that represents the probability to go in the direction of each neighbor.				
			// The decision to which vertex to go would be achieved by drawing a number between 0 and 360, and whichever angle it is smaller than - that vertex is the direction. 				
			// Create a new array where the number of times each neighbor correlates to its probability.
			float sumOfAngles = crossAngles.Sum(); // I think it should be 360, but just in case.
			
			// Normalizing the array by the smallest member, so that there wouldn't be so many members in the 'by probability' array, and it would run faster. 
			List<float> relativeCrossAngles = crossAngles.Select(angle => (float)(angle/sumOfAngles)).ToList<float>();
			
			// remove extremely small relative angles - such angles mean that there's a duplicate path somewhere, which shouldn't appear
			relativeCrossAngles = relativeCrossAngles.Where(angle => angle > 0.001).ToList<float>();
			
			float multiplier = 1 / relativeCrossAngles.Min();
			
			float probabilityOfAngle;
			for (int i = 0; i < relativeCrossAngles.Count; i++) {
				
				// now inserting the correlative number of neighbors to the probability - so that running a random on this list will effectively create a random with probability.
				int numOfNeighborsForProbability = Mathf.RoundToInt(relativeCrossAngles[i] * multiplier);
				
				for (int k = 0; k < numOfNeighborsForProbability; k++) {
					tempDets.neighborIndicesByProbability.Add(sortedNeighborIndices[i]);
				}				
			}
			
		} else { // the only case where the sum of normalized vectors is the 0 vector is if they are orthogonal, the chances are equal between them. 
			// Just copy the neighbor list
			tempDets.neighborIndicesByProbability = tempDets.neighborIndicesList;
		}
		
		
		detailsForVertex[j] = tempDets;
	}
	
}*/
