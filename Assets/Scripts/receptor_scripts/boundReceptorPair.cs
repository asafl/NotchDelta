using UnityEngine;
using System.Collections;

public class boundReceptorPair {
	public Receptor3 recA;
	public Receptor3 recB;
	
	public int bindingTime = 0;
	public bool signalReleased = false;
	
	LineRenderer lineObject;
	
	// construtor
	public boundReceptorPair (Receptor3 irecA, Receptor3 irecB) {
		recA = irecA;
		recB = irecB;
		
		// binding receptors
		recA.bound = true;
		recB.bound = true;

		// Report binding to cell output values
		/*recA.cell.outputValues.reportBinding(recA.receptorId, recA.recPosition - recA.cell.transform.position);
		recB.cell.outputValues.reportBinding(recB.receptorId, recB.recPosition - recB.cell.transform.position);        */
						
		// if both cells visible, display a line between them.
		if (bothRecsVisible()) {
			lineObject = GameObject.Instantiate(Resources.Load("LineBetweenReceptors", typeof(LineRenderer))) as LineRenderer;

			// Draw the line itself
			lineObject.SetPosition(0, recA.recPosition);
			lineObject.SetPosition(1, recB.recPosition);
			lineObject.SetWidth(0.01f, 0.01f);
			
			//Debug.Log ("//// BOUND visual receptors: cell: " + recA.cell.gameObject.name + " rec id: " + recA.receptorId + " position: " + recA.recPosition + " , cell: " + recB.cell.gameObject.name + " rec id: " + recB.receptorId + " position: " + recB.recPosition);
		}
			
		/*			haloA = (Behaviour)recA.GetComponent("Halo");			
		haloB = (Behaviour)recB.GetComponent("Halo");
		haloA.enabled = true;
		haloB.enabled = true;
		
		particleA = recA.GetComponent<ParticleSystem>();
		particleB = recB.GetComponent<ParticleSystem>();*/
		 
	}

	bool bothRecsVisible () {
		return (recA.visible && recB.visible);
	}	
			
	void incBindingTime () {
			bindingTime += 1;
	}
	
	void unbind () {
		//Debug.Log("Unbound");
		recA.bound = false;
		recB.bound = false;
		
		if (bothRecsVisible()) {
			GameObject.Destroy(lineObject.gameObject);
		}		
	}
	
	void endocytoseReceptors() {
		for (int i=0; i < recA.cell.receptors.Count; i++) {
			if (recA.receptorId ==  recA.cell.receptors[i].receptorId) {
				recA.cell.receptors.RemoveAt(i);
				break;
			}
			
		}

		for (int i=0; i < recB.cell.receptors.Count; i++)  {
			if (recB.receptorId ==  recB.cell.receptors[i].receptorId) {
				recB.cell.receptors.RemoveAt(i);
				break;
			}
		}
	}
	
	//void signal (simulationManager2 mngr) {
	//	signalReleased = true;
	//	mngr.totalSignal++;
	//	//Debug.Log("Signal! Biotick: " + mngr.biotick + " Sum of signal: " + mngr.totalSignal);		
		
	//	// Report signal to cell output values
	//	/*recA.cell.outputValues.reportSignal(recA.receptorId, recA.recPosition - recA.cell.transform.position);
	//	recB.cell.outputValues.reportSignal(recB.receptorId, recB.recPosition - recB.cell.transform.position);*/
	//	//recA.cell.outputValues.aggregateBiotickSignal();
	//	//recB.cell.outputValues.aggregateBiotickSignal();
		
	//	if (bothRecsVisible())
	//		lineObject.SetColors(Color.red, Color.red);
			
	//		/*			particleA.Emit(1);
	//	particleB.Emit(1);*/
		
	//		// TRIGGER LOG?
	//}
	
	// returns true if we should remove the pair from the list of bound receptors, false otherwise. 
	//public bool runReceptorsForBiotick (simulationManager2 mngr, System.Random rand) {
	//	//Debug.DrawLine(recA.recPosition, recB.recPosition, Color.blue);

	//	// Should they unbind
	//	if (signalReleased) {
	//		if (rand.NextDouble () <= mngr.unbindingProbabilityAfterSignal) {
	//			unbind ();
				
	//			// Both receptors should endocytose (be removed from the array) after unbinding (if they released a signal)
	//			endocytoseReceptors();                
                
	//			return true;
	
	//		}
	//	} else {
	//		if (rand.NextDouble () <= mngr.unbindingProbability) {
	//			unbind ();
	//			return true;
	//		}							
	//	}

	//	// Should they release signal?
	//	if (rand.NextDouble () <= mngr.signalGenerationProbability) {
	//		signal (mngr);
	//		incBindingTime();
	//		return false;
	//	}

	//	incBindingTime ();
	//	return false;
	//}
	
	// SAME FUNCTION FOR USE WITH SimulationManager3
	// returns true if we should remove the pair from the list of bound receptors, false otherwise. 
	public bool runReceptorsForBiotick (simulationManager3 mngr, System.Random rand) {
		//Debug.DrawLine(recA.recPosition, recB.recPosition, Color.blue);
		
		// Should they unbind
		if (signalReleased) {
			if (rand.NextDouble () <= mngr.unbindingProbabilityAfterSignal) {
				unbind ();
				
				// Both receptors should endocytose (be removed from the array) after unbinding (if they released a signal)
				endocytoseReceptors();                
				
				return true;
				
			}
		} else {
			if (rand.NextDouble () <= mngr.unbindingProbability) {
				unbind ();
				return true;
			}							
		}
		
		// Should they release signal?
		if (rand.NextDouble () <= mngr.signalGenerationProbability) {
			signal (mngr);
			incBindingTime();
			return false;
		}
		
		incBindingTime ();
		return false;
	}
	
	// SAME FUNCTION FOR USE WITH SimulationManager3...
	void signal (simulationManager3 mngr) {
		signalReleased = true;
		mngr.totalSignal++;
		//Debug.Log("Signal! Biotick: " + mngr.biotick + " Sum of signal: " + mngr.totalSignal);		
		
		// Report signal to cell output values
		/*recA.cell.outputValues.reportSignal(recA.receptorId, recA.recPosition - recA.cell.transform.position);
		recB.cell.outputValues.reportSignal(recB.receptorId, recB.recPosition - recB.cell.transform.position);*/
		//recA.cell.outputValues.aggregateBiotickSignal();
		//recB.cell.outputValues.aggregateBiotickSignal();
		
		if (bothRecsVisible())
			lineObject.SetColors(Color.red, Color.red);
		
		/*			particleA.Emit(1);
		particleB.Emit(1);*/
		
		// TRIGGER LOG?
	}
	
}

