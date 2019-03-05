using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System;

public class cellOutputValues {
	
	/*public string name;
	
	public float cellRadius;
	*/
	public int currentBiotick = -1;
	
	//public simulationManager2 mngr;

	// the "interaction point" is the closest point to the cell whose receptors we're interacting with.
	//public Vector3 interactionPointOnCell = Vector3.zero;
	
	// all receptors went out of this vertex. IN LOCAL COORDINATES.
	Vector3 allReceptorsOriginPoint;
	
	Cell2 cell;
	
	// SUMMARIZING PARAMETERS
	// Amount of receptors in each biotick.
	public Dictionary<int, int> boundReceptorAmount = new Dictionary<int, int>(); // <biotick, NumberOfReceptors>
	
	public Dictionary<int, int> unboundReceptorAmount = new Dictionary<int, int>(); // <biotick, NumberOfReceptors>
	
	public Dictionary<int, int> createdSignalAmount = new Dictionary<int, int>(); // <biotick, AmountOfSignal>
	
	public Dictionary<int, int> receptorAmount = new Dictionary<int, int>(); // <biotic, numOfReceptors>
	
	// POSITION RELATED PARAMETERS, ALL POSITIONS ARE RELATIVE TO CENTER OF CELL!
	// For each unbound receptor in each biotick - its position relative to the center of the cell.
	public Dictionary<int, List<Vector3>> unboundReceptorsPositions = new Dictionary<int, List<Vector3>>(); // <biotick, <receptor_id, position_relavtive_to_center>> 

	// For each bound receptor in each biotick - its position relative to the center of the cell.
	public Dictionary<int, List<Vector3>> boundReceptorsPositions = new Dictionary<int, List<Vector3>>(); // <biotick, <receptor_id, position_relavtive_to_center>> 	

	// Binding occurances position
	public Dictionary<int, List<Vector3>> bindingOccurencePosition = new Dictionary<int, List<Vector3>>(); // <biotick, <receptor_that_is_generating_the_signal, position_relative_to_center>>

	// For each signal created in each biotick - ...
	public Dictionary<int, List<Vector3>> signalCreationPosition = new Dictionary<int, List<Vector3>>(); // <biotick, <receptor_that_is_generating_the_signal, position_relative_to_center>>	
	
	
	public List<Vector3> boundReceptorsPositionsSS = new List<Vector3>();
	public List<Vector3> unBoundReceptorsPositionsSS = new List<Vector3>();
	
	// FILE AND PARAMETERS
	public string basicPathForOutput;
	
	float distanceRingInterval = 0.1f;
	int numOfDistanceIntervals = 5000;
	
	
	// Constructor
	//public cellOutputValues (string cellName, float iradius, simulationManager2 imngr) {
	public cellOutputValues (Cell2 icell) {
		/*name = cellName;
		cellRadius = iradius;
		mngr = imngr;*/
		cell = icell;
		
		//if (!cell.randomStartingPosition) {
		//	allReceptorsOriginPoint = cell.globalToLocal(cell.globalMeshVerts[cell.originVert]);
		//}
		
		
		string expFolder = "exp_" + cell.mngr.experimentNumber;
		basicPathForOutput = Application.dataPath + "/Output/" + expFolder + "/";
		
		if (!Directory.Exists(basicPathForOutput)) {
			Directory.CreateDirectory(basicPathForOutput);
			Directory.CreateDirectory(basicPathForOutput + "distance_from_interaction_point/");
			Directory.CreateDirectory(basicPathForOutput + "diffusion_calculations/");
			Directory.CreateDirectory(basicPathForOutput + "receptor_amounts/");
			Directory.CreateDirectory(basicPathForOutput + "signal/");
		}		
	}
	
	public void resetArrays () {
		currentBiotick = -1;
		
		boundReceptorAmount.Clear();
		unboundReceptorAmount.Clear();
		createdSignalAmount.Clear();
		receptorAmount.Clear();
		
		boundReceptorsPositionsSS.Clear();
		unBoundReceptorsPositionsSS.Clear();
		
		//boundReceptorsPositions.Clear (); // FOR DIFFUSION - COMMENT THIS AND NEXT LINE WHEN FINISHED
		//unboundReceptorsPositions.Clear ();
		
		/*signalCreationPosition.Clear();
		bindingOccurencePosition.Clear();*/
	}
	
	// Initialize all arrays for biotick
	public void updateBiotick (int newBiotick) {
		// Close out previous biotick - 
/*		if (newBiotick > 0) {
            boundReceptorAmount.Add (currentBiotick, boundReceptorsPositions[currentBiotick].Count);
			unboundReceptorAmount.Add (currentBiotick, unboundReceptorsPositions[currentBiotick].Count);
			createdSignalAmount.Add(currentBiotick, signalCreationPosition[currentBiotick].Count) ;
		}
*/	
		// update to current biotick
		currentBiotick = newBiotick;
		
		// initialize arrays for this biotick
		//boundReceptorsPositions.Add(currentBiotick, new List<Vector3>()); // FOR DIFFUSION - COMMENT THIS AND NEXT LINE WHEN FINISHED
		//unboundReceptorsPositions.Add(currentBiotick, new List<Vector3>());
		
		/*signalCreationPosition.Add(currentBiotick, new List<Vector3>());
		bindingOccurencePosition.Add (currentBiotick, new List<Vector3>());*/
		
		createdSignalAmount.Add(currentBiotick, 0);
	}
	
	public void reportReceptorPosition (int recId, Vector3 recPosition, bool bound) {
		if (bound) {
			boundReceptorsPositions[currentBiotick].Add (recPosition);
		} else {
			unboundReceptorsPositions[currentBiotick].Add (recPosition);
		}
	}
	
	public void reportReceptorPositionInSS (int recId, Vector3 recPosition, bool bound) {
		if (bound) {
			boundReceptorsPositionsSS.Add (recPosition);
		} else {
			unBoundReceptorsPositionsSS.Add (recPosition);
		}
	}
	
	
	public void reportReceptorAmount(int amount) {
		receptorAmount.Add(currentBiotick, amount);
	}
	
	public void reportSignal (int recId, Vector3 recPosition) {
		signalCreationPosition[currentBiotick].Add (recPosition);
	}
	
	public void aggregateBiotickSignal() {
		createdSignalAmount[currentBiotick]++;
	}
	
	// SOULD USE OTHER ARRAY THAN createdSignalAmount! 
	/*public void reportTotalSignal (int totalSignal) {
		createdSignalAmount.Add(currentBiotick, totalSignal);
	}	*/

	public void reportBinding (int recId, Vector3 recPosition) {
		bindingOccurencePosition[currentBiotick].Add (recPosition);
	}
	
	//public void calcDistanceFromOriginOnArc
	
	
/*	public void calculateDistanceOfEachTypeFromInteractionPointAtEachBiotick () {
		foreach(KeyValuePair<int,)
	}*/

	void saveToCsv(string restOfPath, string filename, string[][] dataToSave) {
		//string filePath = Application.dataPath + "/DiffCalc/Files/" + subFolderName + "/speed=" + mngr.speed_in_micrometers_per_second.ToString() + "_random_seed=" + randomSeedForReceptorMovement + ".csv";
		string filePath = basicPathForOutput + restOfPath + filename + ".csv";
		string delimiter = ",";  		
		
		Debug.Log ("Saving to: " + filePath);
		
		int lines = dataToSave.GetLength(0);  
		
		StringBuilder sb = new StringBuilder();  
		for (int index = 0; index < lines; index++)  
			//sb.AppendLine(csvString[index]);
			sb.AppendLine(string.Join(delimiter, dataToSave[index])); 
		
		
		File.WriteAllText(filePath, sb.ToString());                 	
	}
	
	// calculate the distance of the receptors from the interaction point in "rings" - so we can count how many receptors are in the distance of "x" from interaction point
	public void saveDistanceOfTypeFromInteractionPoint (float sizeOfInteractionRadius, float diffusionLengthScale) {
		// BOUND:
		// get the amount at the last biotick, assuming it's at steady state.
		//List<Vector3> steadyStateBoundReceptors = boundReceptorsPositions[boundReceptorsPositions.Count - 1];
		List<Vector3> steadyStateBoundReceptors = boundReceptorsPositionsSS;
		string[][] boundAmount = new string[numOfDistanceIntervals][];
	
		// Create an axis of ring centers
		List<float> rings = new List<float>();
	
		// Each ring sould have the SAME AREA.
		float outerRadius = distanceRingInterval;
		float innerRadius = 0;
		float midRing = (float)(outerRadius + innerRadius)/2f;
		rings.Add(midRing);
		float surfaceArea = Utils.areaOfRing(0, outerRadius);
		
		// Iterate over "rings" of distance from the interaction point and count the amount of receptors of each type (bound, unbound)
		string[] intervalNum = new string[1];
		for (int i=0; i < numOfDistanceIntervals; i++) {
			// find number of receptors in ring
			intervalNum[0] = steadyStateBoundReceptors.Count(pos => Utils.calcEuclideanDistance(pos, cell.closestPointToInteractingCellOnCellSurface) >= innerRadius && 
			                                                 Utils.calcEuclideanDistance(pos, cell.closestPointToInteractingCellOnCellSurface) < outerRadius).ToString();
			boundAmount[i] = (string[])intervalNum.Clone();
			
			// calculate new outer radius
			innerRadius = outerRadius;
			outerRadius = Utils.outerRadiusFromSurfaceAreaAndInnerRadius(innerRadius, surfaceArea);
			midRing = (float)(outerRadius + innerRadius)/2f;
			rings.Add(midRing);
		}
		
		saveToCsv("distance_from_interaction_point/", cell.name + "_numOfBoundReceptorsAtSteadyStateVsDistanceFromInteractionPoint_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, boundAmount);
		
		saveToCsv("distance_from_interaction_point/", "profileRingsDistances", Utils.listToStringArray(rings));
		
		// UNBOUND: 
		//List<Vector3> steadyStateUnboundReceptors = unboundReceptorsPositions[unboundReceptorsPositions.Count - 1];
		List<Vector3> steadyStateUnboundReceptors = unBoundReceptorsPositionsSS;
		string[][] unboundAmount = new string[numOfDistanceIntervals][];
		
		// Each ring sould have the SAME AREA.
		outerRadius = distanceRingInterval;
		innerRadius = 0;
		surfaceArea = Utils.areaOfRing(0, outerRadius);
		
		// Iterate over "rings" of distance from the interaction point and count the amount of receptors of each type (bound, unbound)
		for (int i=0; i < numOfDistanceIntervals; i++) {
			// find number of receptors in ring
			intervalNum[0] = steadyStateUnboundReceptors.Count(pos => Utils.calcEuclideanDistance(pos, cell.closestPointToInteractingCellOnCellSurface) >= innerRadius 
			                                                   && Utils.calcEuclideanDistance(pos, cell.closestPointToInteractingCellOnCellSurface) < outerRadius).ToString();
			boundAmount[i] = (string[])intervalNum.Clone();
			
			// calculate new outer radius
			innerRadius = outerRadius;
			outerRadius = Utils.outerRadiusFromSurfaceAreaAndInnerRadius(innerRadius, surfaceArea);			
		}
		
		saveToCsv("distance_from_interaction_point/", cell.name + "_numOfUnboundReceptorsAtSteadyStateVsDistanceFromInteractionPoint_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, boundAmount);
	}
	
	public void saveSignal (float sizeOfInteractionRadius, float diffusionLengthScale) {
		// Transform Dictionary into double string array:
		string[][] stringToPrint = new string[createdSignalAmount.Count][];
		//List<string[]> stringToPrint = new List<string[]>();

		// List<string> temp = new List<string>();
		string[] temp = new string[2];
		int i = 0;		
		foreach (KeyValuePair<int, int> kv in createdSignalAmount) {
			/*temp.Clear();
			temp.Add(kv.Key.ToString());
			temp.Add(kv.Value.ToString());*/
			temp[0] = kv.Key.ToString();
			temp[1] = kv.Value.ToString();
			stringToPrint[i++] = (string[])temp.Clone();
		}
		
		saveToCsv("signal/", cell.name + "_signalAmountVsTime_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, stringToPrint);		
	}
	
	public void saveTotalSignal (float sizeOfInteractionRadius, float diffusionLengthScale, int totalSignal) {
		string[][] totalSignalToPrint = new string[1][];
		totalSignalToPrint[0] = new string[1] {totalSignal.ToString()};
		
		saveToCsv("signal/", cell.name + "_totalSignal_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, totalSignalToPrint);
	}
	
	
/*	public void saveReceptorAmounts (float sizeOfInteractionRadius, float diffusionLengthScale) {
		string[][] receptorAmountToPrint = new string[boundReceptorAmount.Count][];
		
		string[] temp = new string[2];
		int totalRecs = 0;		
		int i = 0;

		foreach (KeyValuePair<int, int> kv in boundReceptorAmount) {
			// adding the unbound and bound receptors amount
			totalRecs = kv.Value + unboundReceptorAmount[kv.Key];
			temp[0] = kv.Key.ToString();
			temp[1] = totalRecs.ToString();
			receptorAmountToPrint[i++] = (string[])temp.Clone();
		}

		saveToCsv("receptor_amounts/", name + "_numOfReceptors_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, receptorAmountToPrint);
	}
*/	

	public void saveReceptorAmounts (float sizeOfInteractionRadius, float diffusionLengthScale) {
		string[][] receptorAmountToPrint = new string[receptorAmount.Count][];
		
		string[] temp = new string[2];
		int i = 0;
		
		foreach (KeyValuePair<int, int> kv in receptorAmount) {
			// adding the unbound and bound receptors amount	
			temp[0] = kv.Key.ToString();
			temp[1] = kv.Value.ToString();
			receptorAmountToPrint[i++] = (string[])temp.Clone();
		}
		
		saveToCsv("receptor_amounts/", cell.name + "_numOfReceptors_interaction_radius=" + sizeOfInteractionRadius + "_diffusion_length_scale=" + diffusionLengthScale, receptorAmountToPrint);
	}
	
	
	// prepare the array for printing - for each biotick - the squared average distance from the origin point.
	public void saveSquaredSDForDiffusionCalculations (int randomSeed) {
		string[][] distancesFromOriginPoint = new string[unboundReceptorsPositions.Count][];
		
		string[] biotickAverageDistance = new string[2];
		int i = 0;
        foreach (KeyValuePair<int, List<Vector3>> biotickPositions in unboundReceptorsPositions) {
			biotickAverageDistance[0] = biotickPositions.Key.ToString();
			
			float sumOfDistance = 0;
			foreach(Vector3 position in biotickPositions.Value) {
				sumOfDistance += Mathf.Pow(Utils.calcArcDistance(allReceptorsOriginPoint, position, cell.radius),2);
				//sumOfDistance += Utils.calcEuclideanDistance(cell.localToGlobal(allReceptorsOriginPoint), cell.localToGlobal(position));
			}
			
			// find the mean of the squared distances
			biotickAverageDistance[1] = (sumOfDistance / biotickPositions.Value.Count()).ToString();
			
			// save the array
			distancesFromOriginPoint[i++] = (string[])biotickAverageDistance.Clone();
		}
		
		// no need for cell name, because we use both cells to get different seeds.
		saveToCsv("diffusion_calculations/", "SquaredMeanOfDistancesVsTime_diffusion=" + cell.mngr.diffusion_in_micrometers_squared_per_second + "_seed=" + randomSeed, distancesFromOriginPoint);		
	}
	
	// Calculates the vector sum (rather than squared sum of distances) of all receptors throughout experiment - which should signify how evenly distanced they are from the center
	public void saveAverageForDiffusionCalculations (int randomSeed) {
		string[][] vectorSumsFromOriginPoint = new string[unboundReceptorsPositions.Count][];
		
		string[] biotickVectorSum = new string[2];
		int i = 0;
		foreach (KeyValuePair<int, List<Vector3>> biotickPositions in unboundReceptorsPositions) {
			biotickVectorSum[0] = biotickPositions.Key.ToString();
			
			Vector3 vectorSum = Vector3.zero;
			foreach(Vector3 position in biotickPositions.Value) {
				vectorSum += (position - allReceptorsOriginPoint);
				//sumOfDistance += Utils.calcEuclideanDistance(cell.localToGlobal(allReceptorsOriginPoint), cell.localToGlobal(position));
			}
			
			// find the square mean of the distances
			biotickVectorSum[1] = vectorSum.x + "," + vectorSum.y + "," + vectorSum.z;
			
			// save the array
			vectorSumsFromOriginPoint[i++] = (string[])biotickVectorSum.Clone();
		}
		
		// no need for cell name, because we use both cells to get different seeds.
		saveToCsv("diffusion_calculations/", "vectorSumVsTime_diffusion=" + cell.mngr.diffusion_in_micrometers_squared_per_second + "_seed=" + randomSeed, vectorSumsFromOriginPoint);		        
    }
    
	
	public void saveForDiffusionCalculationsAtLastBiotick (int randomSeed, float diffusionLengthScale) {
		string[][] varDiffFromOriginPointAtLastBiotick = new string[1][];
		
		string[] biotickAverageDistance = new string[1];
		float sumOfDistance = 0;
		
		foreach (Vector3 position in unBoundReceptorsPositionsSS) {		
			sumOfDistance += Utils.calcArcDistance(allReceptorsOriginPoint, cell.globalToLocal(position), cell.radius);						
		}

		// find the square mean of the distances
		biotickAverageDistance[0] = Mathf.Pow(sumOfDistance / unBoundReceptorsPositionsSS.Count(), 2).ToString();
		
		// save the array
		varDiffFromOriginPointAtLastBiotick[0] = (string[])biotickAverageDistance.Clone();
		
		// no need for cell name, because we use both cells to get different seeds.
		saveToCsv("diffusion_calculations/", "varOfDistanceFromOrigin_seed=" + randomSeed + "_diffusion_length_scale=" + diffusionLengthScale, varDiffFromOriginPointAtLastBiotick);		
	}
	
	
    public void saveForCovarianceCalculationsAtLastBiotick (int randomSeed, float diffusionLengthScale) {
        string[][] positionOfAllReceptorsOnNormalPlane = new string[unBoundReceptorsPositionsSS.Count][];

        // allVectorsOrigin also would have worked...
        Vector3 normalToPlane = cell.transform.position - cell.globalMeshVerts[0]; // THIS IS THE WRONG INDEX, used only to make it compile. The Output values class is not used in this version.s
        Vector3 xOnThePlane = Vector3.ProjectOnPlane(new Vector3(1, 0, 0), normalToPlane);
        Vector3 yOnThePlane = Vector3.ProjectOnPlane(new Vector3(0, 1, 0), normalToPlane);
        string[] vectorPositionOnPlane = new string[1];
        Vector3 tempVectorOnPlane;
        int i = 0;
        foreach (Vector3 position in unBoundReceptorsPositionsSS) {
            tempVectorOnPlane = Vector3.ProjectOnPlane(position, normalToPlane);
            vectorPositionOnPlane[0] = tempVectorOnPlane.x + "," + tempVectorOnPlane.y + "," + tempVectorOnPlane.z;
            positionOfAllReceptorsOnNormalPlane[i++] = (string[])vectorPositionOnPlane.Clone();
        }

        // all the vectors are on the same plane... 
        saveToCsv("diffusion_calculations/", "allVectorsOnNormalPlane_seed=" + randomSeed + "_diffusion_length_scale=" + diffusionLengthScale, positionOfAllReceptorsOnNormalPlane);
    }

    public void saveDiffLengthScaleAndInteractionRadiiLists (float[] diffusionLengthScale, float[] interactionRadii) {
		string[][] LSandIR = new string[2][];
		
		string[] LStemp = new string[diffusionLengthScale.Length];
		for(int i=0; i<diffusionLengthScale.Length; i++) {
			LStemp[i] = diffusionLengthScale[i].ToString();
		}
		
		LSandIR[0] = LStemp;
		
		string[] IRtemp = new string[interactionRadii.Length];
		for(int i=0; i<interactionRadii.Length; i++) {
			IRtemp[i] = interactionRadii[i].ToString();
		}
		
		LSandIR[1] = IRtemp;
		
		saveToCsv("", "list_of_LS_and_IR", LSandIR);		
	}
	

}
