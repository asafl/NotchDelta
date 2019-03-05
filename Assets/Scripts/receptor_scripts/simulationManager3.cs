using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class simulationManager3 : MonoBehaviour {

	public int experimentNumber;
	public int expLengthInSeconds;
	public int steps = 10000;
	public float stayProb = 0.68f;
	public float diffusion_in_micrometers_squared_per_second = 0;

	//[SerializeField] bool autoDiffusion;
	//[SerializeField] float[] StartEndNumOfMembersForDiffusion = new float[3];
	
	//public float[] linearSpeeds; // micrometers per second
	//public float[] diffusionValues;
	
	public float timeStepsInSeconds = 0.1f;
	float elapsedTime;
	
	public float bindingProbability;
	//public float bindingProbabilityAfterSignal;
	public float unbindingProbability;
	public float unbindingProbabilityAfterSignal;	
	public float signalGenerationProbability;
	public float bindingDistance;
	//[SerializeField] bool autoInteractionRadius;
	//[SerializeField] float[] StartEndNumOfMembersForRadius = new float[3];
	public float interactionRadius;
	//public float[] interactionRadii; // in micrometers
	List<float> diffusionLengthScales = new List<float>();
	
	//public bool runningEndocytosis = false;
	public float endocytosisRate;
	//[SerializeField] exoEndoRates exoEndoRates;
	//public bool perCellParameters = false;
	
	public int totalSignal = 0;
	
	[SerializeField] Cell2 cell1;
	[SerializeField] Cell2 cell2;
	[SerializeField] Text receptorNumbersText;
	[SerializeField] Text totalSignalText;
	[SerializeField] Text timeText;
	
	public int biotick = -1;
	bool simStarted = false;
	float latestBioTickTime;
	public System.Random rand;
	[SerializeField] int randomSeedForReceptorBindingOperations = 5;
	
	public twoCellInteraction cellInteraction;
	
	int currentUniqueId = 0;
	
	public int valueCounterLS = 0;
	public int valueCounterIR = 0;
	
	[SerializeField] bool showInteractionRadius = false; 
	Vector3 radiusVector;
	
	// Temporary variables for receptors. 
	Receptor3 recA;
	Receptor3 recB;
	
	
	// Use this for initialization
	void Start () {
		rand = new System.Random(randomSeedForReceptorBindingOperations);
		
		// JUST FOR RUNNING ENDOCYTOSIS
		//if (runningEndocytosis) {
		//	// running logarithmically equidistant endocytosis rates
		//	// assign initial value
		//	endocytosisRate = (float)exoEndoRates.endocytosisRates[valueCounterLS];
		//	/*bindingProbability = exoEndoRates.bindingProbs[valueCounterLS];
		//	unbindingProbability = exoEndoRates.unbindingProbs[valueCounterLS];
		//	signalGenerationProbability = exoEndoRates.signalProbs[valueCounterLS];*/
		//} /*else { 
		//	endocytosisRate = toPerBiotick(endocytosisRate);
		//}*/
		
		// prepare values for experiment
		//if (autoDiffusion) {
		//	// calc endo rate for vector 
		//	//linearSpeeds = Utils.createVectorByDLSJump(LSValues[0], LSValues[1], LSValues[2], endocytosisRate, timeStepsInSeconds);
		//	diffusionValues = Utils.createVectorByDLSLogJump(StartEndNumOfMembersForDiffusion[0], StartEndNumOfMembersForDiffusion[1], StartEndNumOfMembersForDiffusion[2], endocytosisRate);
		//	diffusion_in_micrometers_squared_per_second = diffusionValues[valueCounterLS];
		//}
		
		//if (autoInteractionRadius) {
		//	interactionRadii = Utils.createVectorByLogJump(StartEndNumOfMembersForRadius[0], StartEndNumOfMembersForRadius[1], StartEndNumOfMembersForRadius[2]);
		//	// initialize cell speed value by array
		//	interactionRadius = interactionRadii[valueCounterIR++];	
		//} 
		
		// Convert all parameters to per biotick
		convertAllToPerBiotick();		
	}
	
	void convertAllToPerBiotick () {
		bindingProbability = toPerBiotick(bindingProbability);
		unbindingProbability = toPerBiotick(unbindingProbability);
		signalGenerationProbability = toPerBiotick(signalGenerationProbability);
		steps = (int)Mathf.Floor(expLengthInSeconds / timeStepsInSeconds);
	}
	
	public int generateUniqueId () {
		return currentUniqueId++;
	}
	
	public List<boundReceptorPair> boundReceptors = new List<boundReceptorPair>(); // should this be a class also?
	
	
	public float toPerBiotick (float value) {
		return value * timeStepsInSeconds;
	}
	
	bool closeEnough(Vector3 a, Vector3 b) {
		return Vector3.Distance(a,b) <= bindingDistance;
	}
	
	void allCellsReady() {
		// waits for both cells to be ready, runs some code once, initializing parameters. 
		if (cell1.readyToRun && cell2.readyToRun && !simStarted) { 
			// Define the interaction class
			cellInteraction = new twoCellInteraction(cell1, cell2);
			
			//if (runningEndocytosis) {
			//	cell1.addReceptors((float)exoEndoRates.endocytosisRates[0]);
			//	cell2.addReceptors((float)exoEndoRates.endocytosisRates[0]);				
			//} else {
				cell1.addReceptors(toPerBiotick(endocytosisRate));
				cell2.addReceptors(toPerBiotick(endocytosisRate));
			//}
			
			// Preparing variables for run
			latestBioTickTime = Time.time; // - timeStepsInSeconds; // Setting like this so that the 0 biotick would still generate a fixed update run (otherwise only the "second" biotick would pass the condition)
			simStarted = true;
			biotick = 0;
			
			Debug.Log("Sim stated!");
		}
	}
	
	
	void resetExp (int LSindex, int IRindex) { 
		// Preparing output values for next round
		//cell1.outputValues.resetArrays();
		//cell2.outputValues.resetArrays();
		
		// Instead of initializing receptors, we'll just purge the receptor arrays
		cell1.receptors.Clear();
		cell2.receptors.Clear();								
		
		// assing new value for run
		//if (autoDiffusion) {
		//	diffusion_in_micrometers_squared_per_second = diffusionValues[LSindex];
		//}
		
		//if (runningEndocytosis) {
		//	endocytosisRate = (float)exoEndoRates.endocytosisRates[LSindex];
		//	// assign the complement exo rates that would keep the same SS:
		//	// delta:
		//	cell1.exocytosisRate = (float)exoEndoRates.exocytosisRatesDelta[LSindex];
		//	// notch: 
		//	cell2.exocytosisRate = (float)exoEndoRates.exocytosisRatesNotch[LSindex];
			
		//	// OTHER PARAMETERS:
		//	/*bindingProbability = exoEndoRates.bindingProbs[valueCounterLS];
		//				unbindingProbability = exoEndoRates.unbindingProbs[valueCounterLS];
		//				signalGenerationProbability = exoEndoRates.signalProbs[valueCounterLS];*/
		//}
		
		//if (autoInteractionRadius) {
		//	interactionRadius = interactionRadii[IRindex];
		//}
		
		// Now recreate the needed receptors.
		cell1.addReceptors(toPerBiotick(endocytosisRate));
		cell2.addReceptors(toPerBiotick(endocytosisRate));									
		
		// Rerun simulation
		totalSignal = 0;
		latestBioTickTime = Time.time;
		biotick = 0;
		
		print ("//////////////////////// NEW EXPERIMENT! Diffusion: " + diffusion_in_micrometers_squared_per_second + " Interaction radius: " + interactionRadius + " Diffusion length scale: " + Utils.calcDiffusionLengthScale(diffusion_in_micrometers_squared_per_second, endocytosisRate));
	}
	
	
	// Update is called once per frame
	void FixedUpdate () {		
		allCellsReady();
		
		// CANCELING THE TIME EFFECT - EXP SHOULD RUN AS FAST AS IT CAN
		//elapsedTime = Time.time - latestBioTickTime;
		// Rounding biotick time to prevent problems with running in wrong times ?
		if (Mathf.Abs(elapsedTime - timeStepsInSeconds) < 1E-06) elapsedTime = timeStepsInSeconds;		
		
		// Are we ready for the next timestep (in bioticks)
		if ((biotick > -1) && (biotick < steps) ) { //&& (elapsedTime >= timeStepsInSeconds)
			Profiler.BeginSample("update interaction radius");
			
			// Moving the cell output biotick initalization to here, for now... (because a new biotick is first run here, and we may need to add values to the output arrays, so we have to initalize them here).
			/*cell1.outputValues.updateBiotick(biotick);
			cell2.outputValues.updateBiotick(biotick);*/
			
			// update receptors within interaction radius for both interacting cells:
			cellInteraction.updateReceptorsInInteractionRadius();
			
			Profiler.EndSample();
			
			Profiler.BeginSample("binding");
			
			/*Debug.Log("num of cell in int area A " + cellInteraction.cellA_receptorsInInteractionRadius_listLength);
			Debug.Log("aera B " + cellInteraction.cellB_receptorsInInteractionRadius_listLength);*/
			
			for(int j = 0; j < cellInteraction.cellA_receptorsInInteractionRadius_listLength; j++) {
				recA = cellInteraction.cellA_receptorsInInteractionRadius[j];
				if (!recA.bound) { // receptor can bind only if not currently bound
					for(int k = 0; k < cellInteraction.cellB_receptorsInInteractionRadius_listLength; k++) { 
						recB = cellInteraction.cellB_receptorsInInteractionRadius[k];
						if (!recB.bound) { // receptor can bind only if not currently bound
							if (rand.NextDouble() <= bindingProbability) { // What's the probability for binding?
								// only perform check if we passed the probabily check, which is cheaper computationally.
								if (closeEnough(recA.recPosition, recB.recPosition)) {								
									// receptors bound!
									boundReceptors.Add(new boundReceptorPair(recA, recB));
									// After binding two receptors, we of course have to break, so that rec A wouldn't bind any other receptors (we've already passed the "is it bound" check, so it's a risk)
									break;
								}
							}
						}
					}
				}
			}
			
			Profiler.EndSample();
			
			Profiler.BeginSample("removing bound receptors");
			
			// running all receptor pairs - should they release signal, should they unbind? Doing this first so to not bind and then instantly unbind a receptor pair.
			for (int i=0; i<boundReceptors.Count; i++) {
				// if returned true, remove from bound receptors list. 
				if (boundReceptors[i].runReceptorsForBiotick(this, rand)) 
					// Using i-- after removing an element from the list, all the other elements go back by 1. Decrementing i will cause us to check the same index again (because it increments again in the for), which is actually the next memeber on the list.
					boundReceptors.RemoveAt(i--);
			}
			
			Profiler.EndSample();
			
			// reports before end of biotick
			//cell1.outputValues.reportReceptorAmount(cell1.receptors.Count);
			//cell2.outputValues.reportReceptorAmount(cell2.receptors.Count);
			
			biotick++;
			
			receptorNumbersText.text = "Delta receptors (visible): " + cell1.receptors.Count + " (" + cell1.receptorMeshVertices.Count()/4  + ")" + "\nNotch receptors (visible): " + cell2.receptors.Count + " (" + cell2.receptorMeshVertices.Count()/4 + ")";
			totalSignalText.text = "Number of bound receptors: " + boundReceptors.Count + "\nTotal signal released: " + totalSignal;
			timeText.text = "" + Mathf.Floor(biotick * timeStepsInSeconds);
			
			//latestBioTickTime = Mathf.Floor(Time.time * 10)/10 ;
			latestBioTickTime = Time.time; 
			//Debug.Log ("t: " + Time.time + " biotick: " + biotick); // + " Sim time: " + biotick/10.0
		} else if (biotick == steps) {
			// REPORTS
			
			// After sim finished, save everything
			//float diffusionLengthScale = Utils.calcDiffusionLengthScale(diffusion_in_micrometers_squared_per_second, endocytosisRate); // endocytosis rate of both cells is equal!
			
			//// Save signal per biotick
			//cell1.outputValues.saveSignal(interactionRadius, diffusionLengthScale);
			//cell2.outputValues.saveSignal(interactionRadius, diffusionLengthScale);
			
			//// Save the number of receptors per each biotick
			//cell1.outputValues.saveReceptorAmounts(interactionRadius, diffusionLengthScale);            
			//cell2.outputValues.saveReceptorAmounts(interactionRadius, diffusionLengthScale);            
			
			//// Report AND save locations of all receptors in last biotick (steady state) for receptor profiles.
			//foreach (Receptor3 rec in cell1.receptors) {
			//	cell1.outputValues.reportReceptorPositionInSS(rec.receptorId, rec.recPosition, rec.bound);
			//}
   //         //cell1.outputValues.saveDistanceOfTypeFromInteractionPoint(interactionRadius, diffusionLengthScale);
   //         //cell1.outputValues.saveForCovarianceCalculationsAtLastBiotick(cell1.randomSeedForReceptorMovement, diffusionLengthScale);
			
			//foreach (Receptor3 rec in cell2.receptors) {
			//	cell2.outputValues.reportReceptorPositionInSS(rec.receptorId, rec.recPosition, rec.bound);
			//}
   //         //cell2.outputValues.saveDistanceOfTypeFromInteractionPoint(interactionRadius, diffusionLengthScale);
   //         //cell2.outputValues.saveForCovarianceCalculationsAtLastBiotick(cell2.randomSeedForReceptorMovement, diffusionLengthScale);

   //         // Diffusion calculations - distance of all receptor in EACH BIOTICK (SUPER WASTEFUL) // DONT FORGET TO BRING BACK ALL POSITION REPORTING! / ALSO BRING BACK INIT IN cellOutputValues / COMMENT THIS WHEN NO DIFF CALCULATIONS
   //         //cell1.outputValues.saveSquaredSDForDiffusionCalculations(cell1.randomSeedForReceptorMovement);
   //         //cell1.outputValues.saveAverageForDiffusionCalculations(cell1.randomSeedForReceptorMovement);
   //         //cell2.outputValues.saveSquaredSDForDiffusionCalculations(cell2.randomSeedForReceptorMovement);
   //         //cell2.outputValues.saveAverageForDiffusionCalculations(cell2.randomSeedForReceptorMovement);		

   //         // FOR DLS calculations - Variance of distances from origin at last biotick
   //         /*cell1.outputValues.saveForDiffusionCalculationsAtLastBiotick(cell1.randomSeedForReceptorMovement, diffusionLengthScale);
			//cell2.outputValues.saveForDiffusionCalculationsAtLastBiotick(cell2.randomSeedForReceptorMovement, diffusionLengthScale);*/


   //         int outerLoopLength = 0;
			//if (autoDiffusion) {
			//	outerLoopLength = diffusionValues.Length;
			//}
			// else {
			//	outerLoopLength = exoEndoRates.endocytosisRates.Length;
			//}
			
			//// Advancing the indices
			//if (valueCounterLS < outerLoopLength) {
			//	if (valueCounterIR < interactionRadii.Length) {				
					
			//		resetExp(valueCounterLS, valueCounterIR);
			//		valueCounterIR++;
					
			//	} else {
			//		valueCounterIR = 0;
			//		valueCounterLS++;
					
			//		if (valueCounterLS < outerLoopLength) {
			//			resetExp(valueCounterLS, valueCounterIR);
			//			valueCounterIR++;						
			//		}
			//	}
			//}
			
			//Debug.Log("outer loop length: " + outerLoopLength);
			
			//// If we've gone through all the values
			//if (valueCounterLS == outerLoopLength) {
			//	// Create diffusion length scale list: 
			//	foreach(float diffusion in diffusionValues) {
			//		//foreach(float endoRate in exoEndoRates.endocytosisRates) {
			//		diffusionLengthScales.Add (Utils.calcDiffusionLengthScale(diffusion, endocytosisRate));
			//		//diffusionLengthScales.Add (Utils.getDiffusionLengthScale(speed_in_micrometers_per_second, endoRate, timeStepsInSeconds));
			//	}
				
			//	cell1.outputValues.saveDiffLengthScaleAndInteractionRadiiLists(interactionRadii, diffusionLengthScales.ToArray());
				
			//	// Move to the next biotick to stop operation
			//	biotick++;
			//}
		}
		
		/*        if (showInteractionRadius) {
			for (int i=0; i<8; i++) {
				// Setup vector
				radiusVector = Math3d.
			}
        }*/
	}
}




/*					// For diffusion calculations - reposition all receptors at original vertex: 
					foreach (Receptor3 rec in cell1.receptors) {
						rec.initializeReceptor();
					}				
					foreach (Receptor3 rec in cell2.receptors) {
						rec.initializeReceptor();
					}
*/



// LATEST BEFORE CHANGING TO OVERWRITING.
/*foreach(Receptor3 rec1 in cellInteraction.cellA_receptorsInInteractionRadius) {
	if (!rec1.bound) { // receptor can bind only if not currently bound
		foreach(Receptor3 rec2 in cellInteraction.cellB_receptorsInInteractionRadius) {
			if (!rec2.bound) { // receptor can bind only if not currently bound
				if (rand.NextDouble() <= bindingProbability) { // What's the probability for binding?
					// only perform check if we passed the probabily check, which is cheaper computationally.
					if (closeEnough(rec1.recPosition, rec2.recPosition)) {								
						// receptors bound!
						boundReceptors.Add(new boundReceptorPair(rec1, rec2));														
					}								
				}
			}
		}
	}
}
*/


/*
			// running all receptor pairs - should they release signal, should they unbind? Doing this first so to not bind and then instantly unbind a receptor pair.
			List<boundReceptorPair> receptorsToRemoveFromList = new List<boundReceptorPair>();
			foreach (boundReceptorPair recPair in boundReceptors) {
				// if returned true, add to list to remove from bound receptors list later. 
				if (recPair.runReceptorsForBiotick(this, rand)) receptorsToRemoveFromList.Add(recPair);
			}
			
			// remove the receptors that were set to be removed. This has been separated from creating the list of receptors to be removed in order to not change an array while iterating over it. 
			foreach (boundReceptorPair recToRemove in receptorsToRemoveFromList) boundReceptors.Remove(recToRemove);			
*/

/*
foreach(Receptor3 rec1 in cell1.receptorScripts) {
				if (!rec1.bound) { // receptor can bind only if not bound
					foreach(Receptor3 rec2 in cell2.receptorScripts) {
						if (!rec2.bound) { // receptor can bind only if not bound
							if (rand.NextDouble() <= bindingProbability) { // what's the probability for binding?
								// only perform check if we passed the probabily check, which is cheaper computationally.
								if (closeEnough(rec1.transform.position, rec2.transform.position)) {								
									//Debug.DrawLine(rec1.transform.position, rec2.transform.right, Color.green);
									// receptors bound!
									boundReceptors.Add(new boundReceptorPair(rec1.gameObject, rec2.gameObject));														
								}								
							}						
						}							
					}					
				}
			}
*/			