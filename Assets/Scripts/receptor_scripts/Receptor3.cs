using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Receptor3 {

	public Cell2 cell;
	//public simulationManager2 smngr;
	public simulationManager3 smngr;
				
	/*public Vector3 initialPosition; // UNCOMMENT FOR DIFFUTION CALCULATIONS
	public Vector3 initialPositionRelativeToSphereCenter;*/
	
/*	// CELL VERTICES IN GLOBAL COORDINATES
	public Vector3[] cellVerts;
*/
	public int currentV;
	public int nextV;
	
	public int totalMinisteps;
	public int currentMiniStep;

	public bool isMoving = false;
	public bool bound = false;
	public bool visible = false; 
	
	// Position of receptor IN GLOBAL COORDINATES.
	public Vector3 recPosition;	
	
	public int firstIndexInVertsArr;
	public int firstIndexInTrisArr;
	
	int lengthOfReceptorVertsArray = 4;
	int lengthOfReceptorTrisArray = 12;
	
	List<Vector3> tempVertices;
	List<Vector3> globalVertices;
	List<int> tempTris; 
	List<int> globalTris; 
	
	float pushOutsideMultiplier = 1.005f;

	public int receptorId;


    // Variables for moving visible receptors
    int indInVerticesArrayForMoving;
	int firstIndexInVertsArrayForMoving;
	
	int currentPlaceInTempVerticesArray = 0;
	int maxSizeTempVerticesArray = 1000000;
	

	
	Vector3 hardCodedCellCenter = new Vector3(-51.17f, 21.22f, 7.54f); // :_( It's the only way to find the cell body (without the filopodia) center. 
	
	// Constructor
	public Receptor3 (Cell2 icell, /*simulationManager2*/ simulationManager3 ismngr, System.Random rand, bool ivisible) { //GameObject receptorMesh, 
		// saving the cell and the sim manager objets
		cell = icell;
		smngr = ismngr;		
		receptorId = smngr.generateUniqueId();
		
		// Initializing variables.
		nextV = -1;		
		totalMinisteps = 0;
		currentMiniStep = 0;        
		
		// Assign position if set to random
		//if (icell.randomStartingPosition) {
			currentV = rand.Next(0, cell.globalMeshVerts.Length);
			
			// COMMENTING OUT TO AVOID RUNNING THIS. PROBABLY NOT RELEVANT ANYWAY.
			/*if (icell.exocytoseOutsideInteractionRadius) { // we have to check if the random position is outside the interactions radius
				while (isVertexInInteractionRadius(currentV)) { // run until you find a vertex that's outside the interaction radius
					currentV = rand.Next(0, cell.globalMeshVerts.Length);
				}
			}*/
			
			/*if (icell.noExocytosisInFilopodia) { // we have to check if the random position is on the main body
				while (!isVertexInMainBody(currentV)) { // run until you find a vertex that's on the main body itself (not on filopodia)
					currentV = rand.Next(0, cell.globalMeshVerts.Length);
				}
			}*/

            if (icell.exocytosisMainlyInFilopodia) { // we have to check if the random position is on the filopodia
                float chanceOfExocytosingInFilopodia = (float) rand.NextDouble();

                if (chanceOfExocytosingInFilopodia <= icell.ratioOfExocytosingReceptorsInFilopodia) {
                    // exocytose in filopodia
                    while (isVertexInMainBody(currentV)) { // run until you find a vertex that's on the filopodia itself (not on the cell body)
                        currentV = rand.Next(0, cell.globalMeshVerts.Length);
                    }
                } else {
                    // exocytose in cell body
                    while (!isVertexInMainBody(currentV)) { // run until you find a vertex that's on the cell body.
                        currentV = rand.Next(0, cell.globalMeshVerts.Length);
                    }
                }

            }

		
		recPosition = cell.globalMeshVerts[currentV];		
		
		// Create receptor mesh if visible
		if (ivisible) {
			visible = true; 			
		}
				
	}

	public void initializeReceptor () {	// unused
        // Initializing variables.
        nextV = -1;		
        totalMinisteps = 0;
        currentMiniStep = 0;
        
        bound = false;
        isMoving = false;
        
        // Assign position if set to random
        //if (!cell.randomStartingPosition) 
        //    currentV = cell.originVert;
            
		recPosition = cell.globalMeshVerts[currentV];
    }


	int getFirstVertexIndexByReceptorIndex(int recIndex) {
		return recIndex * lengthOfReceptorVertsArray;
	}

	int getFirstTrisIndexByReceptorIndex(int recIndex) {
		return recIndex * lengthOfReceptorTrisArray;
	}
	
 


/*	public void removeReceptorFromGlobalMesh (int recIndex) {
		//Debug.Log("tris array size: " + cell.receptorsMeshFilter.sharedMesh.triangles.Length + " relative to vertices: " + firstIndexInTrisArr);
		// save the tris as a list. 
		//tempTris = cell.receptorsMeshFilter.sharedMesh.triangles.ToList();
		
		// Debug.Log("REMOVING REC: length of tris array: " + tempTris.Count() + " first index in tris array: " + firstIndexInTrisArr);
		
		// remove the range from the list 
		cell.receptorMeshTris.RemoveRange(getFirstTrisIndexByReceptorIndex(recIndex), lengthOfReceptorTrisArray);
		
		// if you remove a group of vertices from the middle of the array, all the tris that have pointed above the starting index from which we took out the vertex are now pointing to the wrong vertex, AND SOME OF THEM ARE POINTING AT INDICES THAT
		// ARE OUTSIDE OF THE ARRAY AND NO LONGER EXIST. Fixing it by readjusting the poiners. We know that the first tri to start from is the one that pointed to the first vertex (because that's the position from which the first vertex was taken...?)
		for (int i = getFirstTrisIndexByReceptorIndex(recIndex); i < cell.receptorMeshTris.Count; i++) {
			if (cell.receptorMeshTris[i] >= getFirstVertexIndexByReceptorIndex(recIndex) + lengthOfReceptorVertsArray) 
				cell.receptorMeshTris[i] -= lengthOfReceptorVertsArray;
		}
		
		// save the tris as an arrays back in the mesh
		//cell.receptorsMeshFilter.mesh.triangles = tempTris.ToArray();
		//Debug.Log("tris array size after removing: " + cell.receptorsMeshFilter.sharedMesh.triangles.Length);
		
		// save the vertices as a list
		//tempVertices = cell.receptorsMeshFilter.sharedMesh.vertices.ToList();
		// remove the range of the vertices from the list
		cell.receptorMeshVertices.RemoveRange(getFirstVertexIndexByReceptorIndex(recIndex), lengthOfReceptorVertsArray);
		// save the vertices as an array back to the mesh.
		//cell.receptorsMeshFilter.mesh.vertices = tempVertices.ToArray();			
		
	}*/
	

	public void removeReceptorFromGlobalMesh (int recIndex) { // VERSION WITHOUT ALL THE COMMENTED, IRRELEVANT STUFF
		// remove the range from the list 
		cell.receptorMeshTris.RemoveRange(getFirstTrisIndexByReceptorIndex(recIndex), lengthOfReceptorTrisArray);
		
		// if you remove a group of vertices from the middle of the array, all the tris that have pointed above the starting index from which we took out the vertex are now pointing to the wrong vertex, AND SOME OF THEM ARE POINTING AT INDICES THAT
		// ARE OUTSIDE OF THE ARRAY AND NO LONGER EXIST. Fixing it by readjusting the poiners. We know that the first tri to start from is the one that pointed to the first vertex (because that's the position from which the first vertex was taken...?)
		for (int i = getFirstTrisIndexByReceptorIndex(recIndex); i < cell.receptorMeshTris.Count; i++) {
			if (cell.receptorMeshTris[i] >= getFirstVertexIndexByReceptorIndex(recIndex) + lengthOfReceptorVertsArray) 
				cell.receptorMeshTris[i] -= lengthOfReceptorVertsArray;
		}
		
		// remove the range of the vertices from the list
		cell.receptorMeshVertices.RemoveRange(getFirstVertexIndexByReceptorIndex(recIndex), lengthOfReceptorVertsArray);
	}
	

	
	void createReceptorMeshAndAddToGlobal () {
		// VERTICES
		// prepare vertices - they're in the local coordinate system of the mesh filter object
		globalVertices = new List<Vector3>();
		foreach (Vector3 v in cell.geom3.tetradedronVertices) { // O(4)
			// subtract the position of the cell to make the receptor position vector local to 0, which is what the vertices should be because they're local relative to the mesh GO.			
			globalVertices.Add(receptorVertexPosition(v));
		}
		
		// save the index of the first vertex in the vertices mesh array
		firstIndexInVertsArr = cell.receptorMeshVertices.Count;
		
		// add the range of vertices
		cell.receptorMeshVertices.AddRange(globalVertices);
		
		// TRIANGLES		
		// prepare tris array with relative positions in array:			
		globalTris = new List<int>();
		foreach (int ind in cell.geom3.tetradedronTris) {
			globalTris.Add(ind + firstIndexInVertsArr);
		}
		
		// find the first tri's position
		//firstIndexInTrisArr = cell.receptorsMeshFilter.sharedMesh.triangles.Length;
		
		// add tris to receptors mesh
		//tempTris = cell.receptorsMeshFilter.sharedMesh.triangles.ToList();
		cell.receptorMeshTris.AddRange(globalTris);
		//cell.receptorsMeshFilter.mesh.triangles = tempTris.ToArray(); // SAVING DONE IN THE END!
	}
	
	
	Vector3 receptorVertexPosition (Vector3 vert) {
		//return (recPosition - cell.transform.position) + (vert - geometryProvider2.tetrahedronCenter);
		return (cell.transform.InverseTransformPoint(recPosition)) + (vert - cell.geom3.tetrahedronCenter);
	}
	

	// Must appear after recalculation of rec position.
	void moveReceptorInMesh(int recIndex) {
		if (visible) {			
			/*tempVerticesForMoving.Clear();
			tempVerticesForMoving = cell.receptorsMeshFilter.mesh.vertices.ToList();	*/
	
			indInVerticesArrayForMoving = 0;
			
			firstIndexInVertsArrayForMoving = getFirstVertexIndexByReceptorIndex(recIndex);

			// update receptors mesh for movement
			for (int i = firstIndexInVertsArrayForMoving; i < firstIndexInVertsArrayForMoving + lengthOfReceptorVertsArray; i++) {
				cell.receptorMeshVertices[i] = receptorVertexPosition(cell.geom3.tetradedronVertices[indInVerticesArrayForMoving++]);
			}
									
			//cell.receptorsMeshFilter.mesh.vertices = tempVerticesForMoving.ToArray(); 
		}
	}
	
	// Must appear after recalculation of rec position.
	void moveReceptorInMeshByRedraw() {
		if (visible) {			
			firstIndexInVertsArr = cell.receptorMeshVertices.Count;
			
			// add vertices
			foreach (Vector3 vert in cell.geom3.tetradedronVertices) {
				cell.receptorMeshVertices.Add(receptorVertexPosition(vert));
			}
			
			// add tris
			foreach (int tri in cell.geom3.tetradedronTris) {
				cell.receptorMeshTris.Add(firstIndexInVertsArr + tri);
			}
		}
		
		// update output values for receptor positions (good for diffusion calculation):
		//cell.outputValues.reportReceptorPosition(receptorId, recPosition - cell.transform.position, bound);              
		
	}
	

	
	
	// Calculates the position of a receptor on the edge map - returns GLOBAL position!
	Vector3 calcPosition() {
		if (totalMinisteps == 0) return cell.globalMeshVerts[nextV];
		else return (cell.globalMeshVerts[currentV] + ((float)currentMiniStep / (float)totalMinisteps) * (cell.globalMeshVerts[nextV] - cell.globalMeshVerts[currentV]));
	}
	
	
	// calculates distance on sphere of two vectors, assuming vectors are in global space.
/*	float calcArcDistOnSphereFromInitialPosition(Vector3 from) { // UNCOMMENT FOR DIFFUSION CALCULATIONS
		//Vector3 v1 = x1 - cellPosition;
		Vector3 v2 = from - cell.transform.position;
		
		return calcArcDistance(initialPositionRelativeToSphereCenter, v2);
	}*/
	
	
	public bool inInteractionRadius() {
		/*// calculate the receptor position relative to the cell's center
		Vector3 receptorPositionRelativeToCellCenter = cell.globalToLocal(recPosition);
		
		// if the euclidean distance between the receptor and the center of the interaction area is small enough - we're in the interaction radius.
		float distanceBetweenReceptorAndInteractionCenter = Utils.calcEuclideanDistance(cell.closestPointToInteractingCellOnCellSurface, receptorPositionRelativeToCellCenter);*/
		
		float distanceBetweenReceptorAndInteractionCenter = Utils.calcEuclideanDistance(cell.closestPointToInteractingCellOnCellSurface, recPosition);
		
		//Debug.Log("Receptor position: " + receptorPositionRelativeToCellCenter + " Closest point between cells on cell: " + cell.closestPointToInteractingCellOnCellSurface + " Distance: " + distanceBetweenReceptorAndInteractionCenter);
		if (distanceBetweenReceptorAndInteractionCenter <= smngr.interactionRadius) return true;
		else return false; 
	}
	

	public bool isVertexInInteractionRadius(int vertexIndex) {
		/*Vector3 vertexPosition = cell.globalMeshVerts[vertexIndex];		
		
		// calculate the receptor position relative to the cell's center
		Vector3 receptorPositionRelativeToCellCenter = vertexPosition - cell.transform.position;
		
		
		// if the arc distance between the receptor and the center of the interaction area is small enough - we're in the interaction radius. 
		float distanceBetweenReceptorAndInteractionCenter = Utils.calcEuclideanDistance(cell.closestPointToInteractingCellOnCellSurface, receptorPositionRelativeToCellCenter);*/
		
		float distanceBetweenReceptorAndInteractionCenter = Utils.calcEuclideanDistance(cell.closestPointToInteractingCellOnCellSurface, cell.globalMeshVerts[vertexIndex]);
		
		//Debug.Log("Receptor position: " + receptorPositionRelativeToCellCenter + " Closest point between cells on cell: " + cell.closestPointToInteractingCellOnCellSurface + " Distance: " + distanceBetweenReceptorAndInteractionCenter);
		if (distanceBetweenReceptorAndInteractionCenter <= smngr.interactionRadius) return true;
		else return false; 
	}
	
	// Checking if the vertex is inside the radius of the main body, or inside the filopodia
	public bool isVertexInMainBody (int vertexIndex) {
		// adding a delta for non-spherical cells // HORRIBLE HORRIBLE HARD CODING
		if (Utils.calcEuclideanDistance(hardCodedCellCenter, cell.globalMeshVerts[vertexIndex]) <= 10.5) return true;
		else return false; 
	}
	
			
	//** moves receptor, returns its distances from its origin
	public void moveReceptor (System.Random rand, int recIndex) {
		if (!bound) {
			if (isMoving) { // moving from a vertex
				if (currentMiniStep + 1 == totalMinisteps) { // the next step number on an edge is the same as the number of steps on the edge: REACHED THE NEXT VERTEX
					// got to the end, move and re-calc all values
					isMoving = false;
					currentV = nextV;
					recPosition = cell.globalMeshVerts[currentV];

					//moveReceptorInMesh(recIndex);
					moveReceptorInMeshByRedraw();
					
					return;
				} else { // actual movement on an edge: STILL ON THE MOVE ON THE EDGE
					currentMiniStep++;
					recPosition = calcPosition();				

					//moveReceptorInMesh(recIndex);
					moveReceptorInMeshByRedraw();
										
					return;
				}
			} else { // ON A VERTEX - either stay in place or start moving to next
				float th = (float) rand.NextDouble();
				
				if (th <= smngr.stayProb) { // STAY ON VERTEX				
					moveReceptorInMeshByRedraw();
					return;
				}

                // DON'T STAY - START THE JOURNEY				
                // Slimmer code: (without defining new variables)
                // FOR DIFFUSION WITH DIRECTIONAL PREFERENCE
                int randomPositionInList = rand.Next(0, cell.neighbors.detailsForVertex[currentV].neighborIndicesByProbability.Count);
                nextV = (int)cell.neighbors.detailsForVertex[currentV].neighborIndicesByProbability[randomPositionInList];

                // RANDOM DIFFUSION
                //int randomPositionInList = rand.Next(0, cell.neighbors.detailsForVertex[currentV].neighborIndicesList.Count);
                //nextV = (int)cell.neighbors.detailsForVertex[currentV].neighborIndicesList[randomPositionInList];

                // calculate number of ministeps for edge
                float len = Vector3.Distance(cell.globalMeshVerts[currentV], cell.globalMeshVerts[nextV]);

				// The total number of steps on an edge, per time steps. Len is in micrometer, timeStepsInSeconds is in second / timestemps, which means the speed is in micrometers per seconds. 
				//totalMinisteps = (int)(len / (smngr.speed_in_micrometers_per_second * smngr.timeStepsInSeconds));
				//totalMinisteps = (int)(len / (cell.localSpeedInMicrometersPerSecond * smngr.timeStepsInSeconds));
				totalMinisteps = (int)Mathf.Round((Mathf.Pow(len,2) / (4f * cell.localDiffusionValue * smngr.timeStepsInSeconds)));
				
				if (totalMinisteps < 2) {
					Debug.Log("ONE step edge"); //Section too small, consider reducing the speed
					// If the number of steps is less than 2, it's either 1 or 0. Either way we're already on the next vertex - is moving is false.
					isMoving = false;					
				} else {
					isMoving = true;
				}

				// make first step on this edge! 
				currentMiniStep = 1;
				// calculate new position of receptor
				recPosition = calcPosition();								
				//moveReceptorInMesh(recIndex);			
				moveReceptorInMeshByRedraw();
								
				return;
			}			
		} else {
			//return calcArcDistOnSphereFromInitialPosition(recPosition);
			
            moveReceptorInMeshByRedraw();
            
			return;
		}
		
	}

	
}

// DANNY'S MOVEMENT
// choose next vertex, calc ministeps, and move first ministep.
/*Hashtable keysHash = ((Hashtable) cell.verticesHash[currentV]); 
				int amountOfNeighbors = keysHash.Count; // how many vertices are in this vertex's hash table
				int chosen = rand.Next (0,amountOfNeighbors); // which vertex to go to
				
				// a must do for access to chosen
				ArrayList al = new ArrayList(keysHash.Keys); // creates an arraylist containing the keys of the hashtable - the neighbors' indices in the vertex array
				nextV = (int) al[chosen];*/


// OLD FUNCTIONS BEFORE USING VISIBLE INDEX
/*	// After a receptor has been removed, we have to go over all other receptors and make sure they still point to the right index. 
	public void updateFirstIndexInArrays (int firstIndexOfVertRemoved, int firstIndexOfTriRemoved) {
		if (visible) {
			// If the removed index was before ours, we have to reduce our index by a length of a receptor verts array. 
			if (firstIndexOfVertRemoved < firstIndexInVertsArr) {
				firstIndexInVertsArr -= lengthOfReceptorVertsArray;
			}
			
			// If the removed index was before ours, we have to reduce our index by a length of a receptor tris array. 
			if (firstIndexOfTriRemoved < firstIndexInTrisArr) {
				firstIndexInTrisArr -= lengthOfReceptorTrisArray;
			}			
		}
	}

	public void updateFirstIndexInArrays (List<int[]> endocytosedReceptorsIndicesInVertsAndTrisArrays) {
		numOfReceptorsBeforeOurVert = 0;
		numOfReceptorsBeforeOurTri = 0;			
		
		// find all endocytosed receptor indices that are before our receptor's vert and tri and count(!) them			
		foreach (int[] vertAndTri in endocytosedReceptorsIndicesInVertsAndTrisArrays) {
			// If the removed index was before ours, we have to reduce our index by a length of a receptor verts array. 
			if (vertAndTri[0] < firstIndexInVertsArr) {
				numOfReceptorsBeforeOurVert++;
			}
			
			// If the removed index was before ours, we have to reduce our index by a length of a receptor tris array. 
			if (vertAndTri[1] < firstIndexInTrisArr) {
				numOfReceptorsBeforeOurTri++;
			}							
		}
		
		// After finding how many of the endocytosed receptor's indices in the vertices and tris array are found before ours, bring back our index to the right one.
		//Debug.Log("First vertex before: " + firstIndexInVertsArr);
		firstIndexInVertsArr -= numOfReceptorsBeforeOurVert * lengthOfReceptorVertsArray;
		firstIndexInTrisArr -= numOfReceptorsBeforeOurTri * lengthOfReceptorTrisArray;
		//Debug.Log("First vertex after: " + firstIndexInVertsArr)
	}



	public void removeReceptorFromGlobalMesh () {
		//Debug.Log("tris array size: " + cell.receptorsMeshFilter.sharedMesh.triangles.Length + " relative to vertices: " + firstIndexInTrisArr);
		// save the tris as a list. 
		var tempTris = cell.receptorsMeshFilter.sharedMesh.triangles.ToList();
		
		// Debug.Log("REMOVING REC: length of tris array: " + tempTris.Count() + " first index in tris array: " + firstIndexInTrisArr);
		
		// remove the range from the list 
		tempTris.RemoveRange(firstIndexInTrisArr, lengthOfReceptorTrisArray);
		
		// if you remove a group of vertices from the middle of the array, all the tris that have pointed above the starting index from which we took out the vertex are now pointing to the wrong vertex, AND SOME OF THEM ARE POINTING AT INDICES THAT
		// ARE OUTSIDE OF THE ARRAY AND NO LONGER EXIST. Fixing it by readjusting the poiners. 
		for (int i=0; i<tempTris.Count; i++) {
			if (tempTris[i] >= firstIndexInVertsArr + lengthOfReceptorVertsArray) 
				tempTris[i] -= lengthOfReceptorVertsArray;
		}
		
		// save the tris as an arrays back in the mesh
		cell.receptorsMeshFilter.mesh.triangles = tempTris.ToArray();
		//Debug.Log("tris array size after removing: " + cell.receptorsMeshFilter.sharedMesh.triangles.Length);

		// save the vertices as a list
		var tempVerts = cell.receptorsMeshFilter.sharedMesh.vertices.ToList();
		// remove the range of the vertices from the list
		tempVerts.RemoveRange(firstIndexInVertsArr, lengthOfReceptorVertsArray);
		// save the vertices as an array back to the mesh.
		cell.receptorsMeshFilter.mesh.vertices = tempVerts.ToArray();			
		
	}*/


/*	// Must appear after recalculation of rec position.
	void moveReceptorInMesh() {
		if (visible) {			
			//tempVertices.Clear();
			tempVertices = cell.receptorsMeshFilter.mesh.vertices.ToList();	
						
			int indInVerticesArray = 0;

			//			Debug.Log("MOVING REC: length of vertices array: " + tempVertices.Count() + " receptor first index in vertices array: " + firstIndexInVertsArr);
					
			// update receptors mesh for movement
			for (int i = firstIndexInVertsArr; i < firstIndexInVertsArr + lengthOfReceptorVertsArray; i++) {
				tempVertices[i] = recPosition - cell.transform.position + geometryProvider2.tetradedronVertices[indInVerticesArray++];
			}
						
			cell.receptorsMeshFilter.mesh.vertices = tempVertices.ToArray(); 
		}
	}
*/


/*//** moves receptor, returns its distances from its origin
public void moveReceptor (System.Random rand) {
	if (!bound) {
		if (isMoving) { // moving from a vertex
			if (currentMiniStep + 1 == totalMinisteps) { // the next step number on an edge is the same as the number of steps on the edge: REACHED THE NEXT VERTEX
				// got to the end, move and re-calc all values
				isMoving = false;
				currentV = nextV;
				recPosition = cell.globalMeshVerts[currentV];
				
				moveReceptorInMesh();
				//return calcArcDistOnSphereFromInitialPosition(recPosition);
				return;
			} else { // actual movement on an edge: STILL ON THE MOVE ON THE EDGE
				currentMiniStep++;
				recPosition = calcPosition();				
				
				moveReceptorInMesh();
				
				//return calcArcDistOnSphereFromInitialPosition(recPosition);
				return;
			}
		} else { // ON A VERTEX - either stay in place or start moving to next
			float th = (float) rand.NextDouble();
			if (th <= smngr.stayProb) // STAY ON VERTEX				
				//return calcArcDistOnSphereFromInitialPosition(cell.globalMeshVerts[currentV]);
				return;
			
			// choose next vertex, calc ministeps, and move first ministep.
			Hashtable keysHash = ((Hashtable) cell.verticesHash[currentV]); // how many vertices are in this vertex's hash table
			int amount = keysHash.Count; 
			int chosen = rand.Next (0,amount); // which vertex to go to
			
			// a must do for access to chosen
			ArrayList al = new ArrayList(keysHash.Keys);
			nextV = (int) al[chosen];
			
			// calculate number of ministeps for edge
			float len = Vector3.Distance(cell.globalMeshVerts[currentV], cell.globalMeshVerts[nextV]);
			//Debug.Log("Danny's Distance: " + len);
			// The total number of steps on an edge, per time steps. Len is in micrometer, timeStepsInSeconds is in second / timestemps, which means the speed is in micrometers per seconds. 
			totalMinisteps = (int)(len / (smngr.speed_in_micrometers_per_second * smngr.timeStepsInSeconds));
			
			if (totalMinisteps < 2) {
				Debug.Log("Section too small, consider reducing the speed");
			}
			
			// make first step on this edge! 
			currentMiniStep = 1;
			// calculate new position of receptor
			recPosition = calcPosition();				
			
			moveReceptorInMesh();			
			
			isMoving = true;
			//return calcArcDistOnSphereFromInitialPosition(recPosition);
			return;
		}			
	} else {
		//return calcArcDistOnSphereFromInitialPosition(recPosition);
		return;
	}
	
}*/



// static constructor 
/*
	public static Receptor3 CreateReceptor (Cell2 icell, simulationManager2 ismngr, System.Random rand) { //GameObject receptorMesh, 
		Receptor3 rec = new Receptor3();
	
		// saving the cell and the sim manager objets
		rec.cell = icell;
		rec.smngr = ismngr;
		
		rec.receptorId = rec.smngr.generateUniqueId();
		
		// Saving the mesh vertices (in GLOBAL space)(because we're telling the receptors where to go in global 3D space).
		rec.cellVerts = icell.gameObject.GetComponent<MeshFilter>().mesh.vertices;
		for (var i=0; i<rec.cellVerts.Length; i++) rec.cellVerts[i] += icell.transform.position;
		
		// Add cell vertices and tris to mesh.
		
		
		// Initializing variables.
		rec.cellPosition = rec.cell.transform.position;
		
		rec.nextV = -1;
		
		rec.totalMinisteps=0;
		rec.currentMiniStep=0;
		
		// Assign position if set to random
		if (icell.randomStartingPosition) 
			rec.currentV = rand.Next(0, rec.cellVerts.Length);
		else
			rec.currentV = icell.originVert;
			
			
		rec.initialPosition = rec.cellVerts[rec.currentV];
		rec.recPosition = rec.initialPosition;
		
		rec.initialPositionRelativeToSphereCenter = rec.initialPosition - rec.cellPosition;
		
		rec.isMoving = false;	
		
				
		return rec;	
	}
*/

