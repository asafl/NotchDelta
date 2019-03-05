using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class Cell2 : MonoBehaviour {

	public ArrayList verticesHash;
	
	Vector3[] meshVerts;
	public Vector3[] globalMeshVerts; // for the cell vertices
	public System.Random rand;
	public int randomSeedForReceptorMovement;

	public List<Receptor3> receptors = new List<Receptor3>();
	public int visibleIndex = 0;
	bool receptorVisible;
	
	string[][] csvString;
	
	//public simulationManager2 mngr;
	public simulationManager3 mngr;
	
	// Simulation parameters: 
	//public int originVert = 100;
	//public bool randomStartingPosition = false;
	//public bool exocytoseOutsideInteractionRadius = false;
	//public bool noExocytosisInFilopodia = false;
	//public bool endocytoseOnlyNearTip = false;
    public bool exocytosisMainlyInFilopodia = false;
    public float ratioOfExocytosingReceptorsInFilopodia = 1;    
	//public float endocytosisRadius = 0f;
	//public int endocytosisCenterVert = 0;
	public bool readyToRun = false;
	//public float endocytosisRate;
	public float exocytosisRate;
	public float localEndocytosisRate; 
	//public float localSpeedInMicrometersPerSecond = 0;
	public float localDiffusionValue = 0;
	float tempEndoRate;
	
	[SerializeField] exoEndoRates exoEndoRates;
	
	//public float exocytosingReceptors;
	//public int numOfReceptors;
	//[SerializeField] bool useNumOfReceptors = false;
		
	[SerializeField] float percentOfReceptorsToShow;
	
	public float radius;
	public float mainBodyRadius;
	public Vector3 closestPointToInteractingCellOnCellSurface; // IN GLOBAL COORDINATES! SET FROM OUTSIDE!
	public float volume;
	
	int prevBiotick = -1;
	
	// Receptors 
	public MeshFilter receptorsMeshFilter;	
	// Doing all the receptor mesh work with one array passed between all the vertices (and tris). These arrays are an exact copy of the real mesh vertices and tris. 
	public List<Vector3> receptorMeshVertices;
	public List<int> receptorMeshTris;
	
	public float receptorSizeMultiplier = 1f;
	public geometryProvider3 geom3;
	
	// Output values
	//public cellOutputValues outputValues; 
	
	public int rayOfLightVert;
	
	public Neighbors2 neighbors;
	
	[SerializeField] bool fixMesh = false;
	[SerializeField] float weldThreshold;
	[SerializeField] float weldBucket;
	
	
	public Vector3 localToGlobal (Vector3 localPosition) {
		//return (localPosition + transform.position);
		return transform.TransformPoint(localPosition);
        
	}
	
	public Vector3 localToGlobal (int vertexIndex) {
		return globalMeshVerts[vertexIndex];		
	}
	
	public Vector3 globalToLocal (Vector3 globalPosition) {
		//return (globalPosition - transform.position);
		return transform.InverseTransformPoint(globalPosition);
	}
	
	// REQUIRES VECTOR IN GLOBAL SPACE -> RETURNS VECTOR IN GLOBAL COORDINATES
	public Vector3 findClosestVertexOnSurface (Vector3 point) {
		float minDist = float.MaxValue;
		Vector3 foundVector = Vector3.zero;		
		
		foreach (Vector3 vert in globalMeshVerts) {
			if (Utils.calcEuclideanDistance(vert, point) < minDist) {
				minDist = Utils.calcEuclideanDistance(vert, point);
				foundVector = vert;
			}
		}
		
		return foundVector;
	}
	
	// REQUIRES VECTOR IN GLOBAL SPACE -> RETURNS VECTOR IN GLOBAL COORDINATES
	public int findFarthestVertexIndexOnSurface (Vector3 point) {
		float maxDist = 0;
		int foundVectorIndex = -1;		
		
		//foreach (Vector3 vert in globalMeshVerts) {
		for(int i = 0; i < globalMeshVerts.Length; i++) {
			if (Utils.calcEuclideanDistance(globalMeshVerts[i], point) > maxDist) {
				maxDist = Utils.calcEuclideanDistance(globalMeshVerts[i], point);
				foundVectorIndex = i;
			}
		}
		
		return foundVectorIndex;
	}

	void createReceptor() {
		if (rand.NextDouble() <= percentOfReceptorsToShow) { 
			// Create receptor and show it
			receptors.Add(new Receptor3(this, mngr, rand, true));			
			// Add to visible receptor list 
			// visibleReceptorsPosition.Add (newRec.receptorId);
		} else { 
			// Create receptor and don't show it
			receptors.Add(new Receptor3(this, mngr, rand, false));			
		}
		
		return;
		//Receptor3 rec = new Receptor3(this, mngr, rand);
		//receptorGameobject.AddComponent<Receptor3>();
	}
	
	// Adds a receptor in a random place
	public void addReceptors(float endocytosisRateForCreation) { 
	
		float initialNumOfRecs = 0;
		
		//if (useNumOfReceptors) {
		//	initialNumOfRecs = numOfReceptors;
		//} else {
			initialNumOfRecs = exocytosisRate/endocytosisRateForCreation;
		//}
	
		for (int i = 0; i < initialNumOfRecs; i++) {
			createReceptor();			
		}
		
		saveLocalReceptorArraysToReceptorMeshVertsFirst();
	}
	
	
	void saveLocalReceptorArraysToReceptorMeshVertsFirst() { // SAVES VERTICES FIRST!		
		//Debug.Log ("/// CELL: " + gameObject.name + "; Verts length: " + receptorMeshVertices.Count() + " tris length: " + receptorMeshTris.Count() + " last tris:");
		// Saving the meshes after adding / changing the receptors
		receptorsMeshFilter.mesh.triangles = new int[0]; // Doing this to avoid the error when inserting a smaller array than before into the vertices array, and it screams that there are some tris pointing nowhere...
		receptorsMeshFilter.mesh.vertices  = receptorMeshVertices.ToArray();
		receptorsMeshFilter.mesh.triangles = receptorMeshTris.ToArray();					
		
		receptorsMeshFilter.mesh.RecalculateBounds();
		receptorsMeshFilter.mesh.RecalculateNormals();					
	}
	
	void saveLocalReceptorArraysToReceptorMeshTrisFirst() { // SAVES TRIS FIRST!
		receptorsMeshFilter.mesh.triangles = receptorMeshTris.ToArray();		
		receptorsMeshFilter.mesh.vertices  = receptorMeshVertices.ToArray();
		
		receptorsMeshFilter.mesh.RecalculateBounds();
		receptorsMeshFilter.mesh.RecalculateNormals();		
	}
	
	void clearLocalReceptorMeshArrays () {
		receptorMeshTris.Clear ();
		receptorMeshVertices.Clear ();
	}
	
	void createReceptorsMesh() {
		receptorsMeshFilter.gameObject.transform.position = this.transform.position;
		receptorsMeshFilter.gameObject.transform.rotation = this.transform.rotation;
		Mesh newMesh = new Mesh();
		receptorsMeshFilter.mesh = newMesh;
		
		// Initializing the multi - purpose, centralized receptor mesh vertices and tris arrays. All actions will be done on them and saved at the end.
		receptorMeshVertices = new List<Vector3>();
		receptorMeshTris = new List<int>();
	}
	
	
	public delegate void MyVoidDel();
	
	// Use this for initialization
	void Start () {
	
		//buildHash();
		// Fix mesh - automatically weld vertices: 
		Mesh mesh = (Mesh)gameObject.GetComponent<MeshFilter>().mesh;
		
		if (fixMesh) {
			Debug.Log (this.name + " Num of vertices before mesh fix: " + mesh.vertices.Length);
			// The mesh is now updated.
			MeshFixer.AutoWeld(ref mesh, weldThreshold, weldBucket); // GOOD FOR FILOPODIA: 0.15 0.2 // FOR regular sphere: .04 .04
			Debug.Log (this.name + " Num of vertices AFTER mesh fix: " + mesh.vertices.Length);			
		}
		
		meshVerts = mesh.vertices;
		
		// Creating the global array - moving from local to global mesh coordinates.
		var tempGlobalMeshVerts = new List<Vector3>();
		for (int i=0; i<meshVerts.Length; i++) 
			tempGlobalMeshVerts.Add(gameObject.transform.TransformPoint(meshVerts[i])); 
			//tempGlobalMeshVerts.Add (meshVerts[i] + gameObject.transform.position);
			
		globalMeshVerts = tempGlobalMeshVerts.ToArray();

        neighbors = new Neighbors2();
        neighbors.buildNeighborsDetails(this, mesh);		
		
		// Variables for analysis
		radius = meshVerts[10].magnitude;
		mainBodyRadius = meshVerts[309].magnitude;
		print(gameObject.name + " radius: " + radius + " Main body radius size: " + mainBodyRadius);						
		print ("Number of vertices: " + meshVerts.Length);
		
		// Variables for movement		
		/*Hashtable keysHash = ((Hashtable) verticesHash[0]);
		ArrayList al = new ArrayList(keysHash.Keys);		
		int nextV = (int) al[1];*/
		
		// Calculating edge size
		//float edgeSize = (meshVerts[nextV] - meshVerts[0]).magnitude;
		float edgeSize = (meshVerts[mesh.triangles[1]] - meshVerts[mesh.triangles[0]]).magnitude;
		print (gameObject.name + " Edge size: " + edgeSize);
		
		// Creating the SUPER IMPORTANT rand variable
		rand = new System.Random(randomSeedForReceptorMovement);		
		
		// prepare the cell output values class
		//outputValues = new cellOutputValues(this.gameObject.name, radius, mngr);		
		//outputValues = new cellOutputValues(this);
		
		// initialize the exocytosis value:
		//if (mngr.runningEndocytosis) {
		//	if (gameObject.name == "cell1Delta") {
		//		exocytosisRate = (float)exoEndoRates.exocytosisRatesDelta[0];
		//	} else {
		//		exocytosisRate = (float)exoEndoRates.exocytosisRatesNotch[0];
		//	}
		//} else {
		//	if (mngr.perCellParameters) {			
		//		localEndocytosisRate = mngr.toPerBiotick(localEndocytosisRate); // COMMENT TO USE GLOBAL ENDO RATE				
		//	} else {
				localEndocytosisRate = mngr.toPerBiotick(mngr.endocytosisRate); // COMMENT THIS TO USE LOCAL ENDO RATE			
			//}
			
			
			exocytosisRate = mngr.toPerBiotick(exocytosisRate);
		//}
				
		// Create the mesh
		createReceptorsMesh();
		
		// Add the receptors randomly - first time: use the original array.
		//addReceptors((float)exoEndoRates.endocytosisRates[0]);	 // MOVED TO SIM MANAGER TO BE RUN AFTER twoCellInteraction RUNS!
		
		//print ("Cell " + name + " volume: " + Utils.volumeOfMesh(gameObject.GetComponent<MeshFilter>().sharedMesh, transform.localScale));		
		//print ("Surface area of cell " + name + ": " + Utils.surfaceAreaOfMesh(gameObject.transform, gameObject.GetComponent<MeshFilter>().sharedMesh));
		
		// initialize geometry provider 3 for rec size
		geom3 = new geometryProvider3(receptorSizeMultiplier);
		
		readyToRun = true;
		
		//csvString = new string[mngr.steps+1][];		
		
	}
	
	// UNUSED! receptors are anyway redrawn every frame. no need to remove from mesh.
	/*void endocytoseReceptor (ref int visibleIndex, bool visible, Receptor3 receptor, int indInArray) {				
		if (visible) {
			Debug.Log ("//// ENDOCYTOSIS! Cell: " + gameObject.name + " endocytosed receptor: " + receptor.receptorId + " from position: " + receptor.recPosition);  //" (VISIBLE: " + visible + ")"
			//Debug.DrawLine(this.gameObject.transform.position, receptor.recPosition, Color.green);
			
			// Remove receptor from mesh - vertices and tris
			receptor.removeReceptorFromGlobalMesh(visibleIndex);
			
			// We've just removed a visible receptor by removing its vertices and tris from the mesh. This means that the visible index (which correlates to the position of the receptor's vertices and tris in the mesh) must be taken one step back.
			visibleIndex--;		
		}
		
		// Remove receptor from list		
		receptors.RemoveAt(indInArray);
	}*/
	
	
	void endocytoseReceptor (int indInArray) {
		//if (endocytoseOnlyNearTip) {
		//	if (receptorInEndocytosisArea(indInArray)) {
		//		// Remove receptor from list		
		//		receptors.RemoveAt(indInArray);				
		//	}
		//} else {
			// Remove receptor from list		
			receptors.RemoveAt(indInArray);							
		//}
	}
	
	//bool receptorInEndocytosisArea (int indInArray) { 
	//	if (Utils.calcEuclideanDistance(receptors[indInArray].recPosition, globalMeshVerts[endocytosisCenterVert]) <= endocytosisRadius) return true; 
	//	else return false;
	//}
	
	/*void updateVertAndTriIndicesForReceptors () {	
		// Go over all OTHER receptors to update their indices in the receptor mesh vertices and tris. (Because the arrays changed after endocytosis, they may no longer point to the right index)			
		foreach (Receptor3 rec in receptors) 
			if (rec.visible) 
				rec.updateFirstIndexInArrays(endocytosedReceptorsIndicesInVertsAndTrisArrays);					
	}*/
	
	void FixedUpdate() {
        //Debug.DrawLine(gameObject.transform.position, globalMeshVerts[rayOfLightVert], Color.white);	
        
        debugMesh();

        if (prevBiotick != mngr.biotick && mngr.biotick < mngr.steps) {
			// Setting the biotick for all future updates.
			//outputValues.updateBiotick(mngr.biotick);

			//Profiler.BeginSample("clear local rec mesh arrays");
									
			clearLocalReceptorMeshArrays();
			
			/*Profiler.EndSample();
			
			Profiler.BeginSample("exocytosis");*/
			
			// Exocytosis - There's no "chance" of exocytosis. Every biotick X receptors are exocytosed for sure.
			if (exocytosisRate < 1) {
				if (rand.NextDouble() <= exocytosisRate) {
					createReceptor();
				}	
			} else {
				// calculating the chances for number of receptors
				int full = Mathf.FloorToInt(exocytosisRate);
				double decs = exocytosisRate - Math.Floor(exocytosisRate);
				int rest = 0;				
				if (rand.NextDouble() <= decs) rest = 1;
				
				for (int j=0; j < (full+rest); j++) createReceptor();
			}
			
			/*Profiler.EndSample();
			
			Profiler.BeginSample("move or endocytose receptors");*/
			
			// Go over all receptors and perform actions:
			for (int i=0; i < receptors.Count; i++) {				
				//if (mngr.runningEndocytosis) tempEndoRate = mngr.endocytosisRate; else tempEndoRate = localEndocytosisRate;
                //tempEndoRate = mngr.endocytosisRate;
                tempEndoRate = localEndocytosisRate;

                // Endocytosis? 
                if ((rand.NextDouble() <= tempEndoRate) && (!receptors[i].bound)) {
					endocytoseReceptor(i);
					--i; // after taking out a receptor out of the receptors array, i points to a new receptor. We have to decrement it so it would still point to it after the loop incrementation
				} else { // if the receptor wasn't endocytosed, move it (and draw if needed)		
					// if we're NOT using local parameters, assign the global parameter before moving receptors. 
					localDiffusionValue = mngr.diffusion_in_micrometers_squared_per_second;
					
					receptors[i].moveReceptor(rand, visibleIndex);											
				}				
			}
			
			/*Profiler.EndSample();
			
			Profiler.BeginSample("save local receptor arrays...");*/
			
			//saveLocalReceptorArraysToReceptorMeshTrisFirst();
			saveLocalReceptorArraysToReceptorMeshVertsFirst();		
			
			//Profiler.EndSample();
			
			prevBiotick = mngr.biotick;			
		}	
	}
	
	
	void debugMesh () {
        //if (fixMesh) {
        //	foreach (int indexOfNeighb in neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList) {
        //		Debug.Log("index: " + indexOfNeighb + " vector: " + globalMeshVerts[indexOfNeighb]);
        //		Debug.DrawLine(gameObject.transform.position, globalMeshVerts[indexOfNeighb], Color.blue);
        //	}
        //	/*Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[0] + " blue");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[0]], Color.blue);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[1] + " black " + globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[1]]);
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[1]], Color.black);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[2] + " green");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[2]], Color.green);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[3] + " gray");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[3]], Color.gray);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[4] + " magenta");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[4]], Color.magenta);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[5] + " red");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[5]], Color.red);

        //	Debug.Log("index: " + neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[6] + " yellow");
        //	Debug.DrawLine(gameObject.transform.position, globalMeshVerts[neighbors.detailsForVertex[rayOfLightVert].neighborIndicesList[6]], Color.yellow);*/

        //      }
        //// vector sum
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(0.1588668f, -0.03328738f, -0.3756406f), Color.red);

        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(0.1317822f, 0.001924688f, 0.05556307f), Color.blue);
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(-0.0002627485f, 0.07349062f, -0.00662349f), Color.blue);
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(-0.1290342f, 0.07340661f, -0.06107637f), Color.blue);
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(-0.1335224f, -8.709919E-05f, -0.05646189f), Color.blue);
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(-0.0002474943f, -0.07537429f, 0.00657462f), Color.blue);
        //Debug.DrawLine(globalMeshVerts[rayOfLightVert], globalMeshVerts[rayOfLightVert] + new Vector3(0.1354112f, -0.07339299f, 0.06377213f), Color.blue);

    }
    /*void addVertices(Hashtable h, int k2, int k3) {
		if (h.ContainsKey(k2)) { // does the hashtable (FOR K1) contain a key k2? 
			int count_2 = (int)h[k2]; // 
			h[k2] = count_2++;
		} else {
			h[k2] = 1; 
		}
		
		if (h.ContainsKey(k3)) {
			int count_3 = (int)h[k3];
			h[k3] = count_3++;
		} else {
			h[k3] = 1;
		}
	}
	
	
	// builds a map of the neighbors of each vector
	void buildHash() { 
		Mesh goMesh = gameObject.GetComponent<MeshFilter>().mesh;
		goMesh.RecalculateBounds();
		goMesh.RecalculateNormals();
		
		Vector3[] v = goMesh.vertices;
		int[] t = goMesh.triangles;
		verticesHash = new ArrayList(v.Length);
		for (int i = 0; i < v.Length; i++) { 
			verticesHash.Add(new Hashtable());
		}
		
		// For each trio of indices of vectors that form a triangle...
		for (int i=0;i<t.Length;i+=3) {
			int k1 = t[i];
			int k2 = t[i+1];
			int k3 = t[i+2];
			
			// Add the three neighbours to the verticesHash array. Each entry contains a hashtable with the index's neighbours.	
			// Example: 
			addVertices((Hashtable) verticesHash[k1],k2,k3); // passes the hashtable at the position of the index of first vertex of the tri. Adds the other indices of the vertices of the tri, if not already in the hashtable. 
			addVertices((Hashtable) verticesHash[k2],k1,k3);
			addVertices((Hashtable) verticesHash[k3],k1,k2);
		}
		
		// Try to unite mesh and object	
	}*/


    /*	IEnumerator delayedRun(int seconds, params MyVoidDel[] functionsToRunAfterDelay) {
		//print(Time.time);
		yield return new WaitForSeconds(seconds);
		
		foreach(MyVoidDel fn in functionsToRunAfterDelay) {
			fn();
		}
		
		meshVerts = gameObject.GetComponent<MeshFilter>().mesh.vertices;
		radius = meshVerts[10].magnitude;
		print("Radius size: " + radius);				
		
		print ("Number of vertices: " + meshVerts.Length);
		
		// calculating edge size:		
		Hashtable keysHash = ((Hashtable) verticesHash[0]);
		ArrayList al = new ArrayList(keysHash.Keys);
		int nextV = (int) al[1];
		float edgeSize = (meshVerts[nextV] - meshVerts[0]).magnitude;
		print ("Edge size: " + edgeSize);
	}*/





    /*	void FixedUpdate() {
            if (prevBiotick != mngr.biotick) {
                Debug.Log("/////// NEW BIOTICK FOR CELL " + gameObject.name + " " + mngr.biotick);
                //string[] allReceptorDistPerT = new string[receptors.Count];

                // Setting the biotick for all future updates.
                outputValues.updateBiotick(mngr.biotick);

                // Extremely important! it points to the current index of the visible receptor - thus correlating to its position in the vertices and tris arrays of the receptors unified mesh. We need those indices for receptors actions (e.g. moving and removing it).
                visibleIndex = 0;

                //clearLocalReceptorMeshArrays();

                // Go over all receptors and perform actions:
                for (int i=0; i < receptors.Count; i++) {
                    //allReceptorDistPerT[i] = recComp.moveReceptor(rand).ToString();		
                    receptors[i].moveReceptor(rand, visibleIndex);							

                    // we have to assign the visible parameter before the endocytosis, because after it we might not be able to check the ith position in the array - it would point to a different (next) member because we removed the current. 
                    receptorVisible = receptors[i].visible;

                    // Endocytosis? 
                    if ((rand.NextDouble() <= endocytosisRate) && (!receptors[i].bound)) 
                        endocytoseReceptor(ref visibleIndex, receptorVisible, receptors[i], i);

                    if (receptorVisible) visibleIndex++;										

                }

                Debug.Log("/// CELL : " + gameObject.name + " num of visible receptors: " + visibleIndex + " Length of vertex list/4: " + receptorMeshVertices.Count/4);

                saveLocalReceptorArraysToReceptorMeshTrisFirst();

                // Exocytosis - There's no "chance" of exocytosis. Every biotick X receptors are exocytosed for sure.
                if (exocytosisRate < 1) {
                    if (rand.NextDouble() <= exocytosisRate) {
                        createReceptor();
                    }	
                } else {
                    // calculating the chances for number of receptors
                    int full = Mathf.FloorToInt(exocytosisRate);
                    double decs = exocytosisRate - Math.Floor(exocytosisRate);
                    int rest = 0;				
                    if (rand.NextDouble() < decs) rest = 1;

                    for (int j=0; j < (full+rest); j++) createReceptor();
                }

                saveLocalReceptorArraysToReceptorMeshVertsFirst();			


                prevBiotick = mngr.biotick;
                //csvString[prevBiotick] = allReceptorDistPerT;
            }
        }*/
}
