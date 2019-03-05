using UnityEngine;
using System.Collections;
using System;

public class exoEndoRates : MonoBehaviour {

	// THIS OBJECT WAS CREATED TO BE THE FIRST IN EXECUTION ORDER, WITHOUT DISURBING THE REST OF THE BALANCE.

	double[] endocytosisRatesPerSecond;
	public double[] endocytosisRates;
	public double[] exocytosisRatesNotch;
	public double[] exocytosisRatesDelta;
	public float[] bindingProbs;
	public float[] unbindingProbs; 
	public float[] signalProbs;
	
	float notchSS = 125663.7f;
	float deltaSS = 12566.37f;
	
	double exoBoost = 1;
	
	// PER BIOTICK FOR REFERENCE!!!!!!!!
	float refEndoRate = 0.002f;
	float refBindingProb = 0.0167f;
	float refUnbindingProb = 0.0034f;
	float refSignalGenProb = 0.034f;
	
	//public simulationManager2 mngr;
	
	float adjustParameterToReference(double currEndo, float parValue) {
		return parValue * (refEndoRate/(float)currEndo);
	}
	
	double[] createArrayWithOne (double[] oldArray, int fromPosition) {
		double[] newArray = new double[1];
        Array.Copy (oldArray, fromPosition, newArray, 0, 1);
        return newArray;
	}
		
	// Use this for initialization
	void Start () {
		int positionToCopy = 15;
		
		// ALL VALUES ARE PER BIOTICKS!!!!!!!!!
		//endocytosisRates = new double[] {2e-06,2.93895335634114e-06,4.31872341537442e-06,6.34626333836186e-06,9.32568596925164e-06,1.37038780397578e-05,2.01375291799179e-05,2.95916294858686e-05,4.34842093985485e-05,6.38990315798525e-05,9.3898136664278e-05,0.000137981121951829,0.000202760040736022,0.000297951151126499,0.000437832267814466,0.000643384306503888,0.000945438233508408,0.00138929943479139,0.00204154311842149,0.003};
		
		//endocytosisRates = new double[] {2e-05,6.99927102316117e-05,0.000244948974278318,0.00085723212890964,0.003}; // LOG SPACED
		//endocytosisRates = new double[] {2e-05,0.000765,0.00151,0.002255,0.003}; // LINEARLY SPACED
		//endocytosisRates = new double[] {0.003,0.000206465206498276,6.83780564328642e-05,3.36963957984374e-05,2e-05}; // 1/SQRT SPACED
		
		//endocytosisRates = new double[] {2e-05, 0.000244948974278318, 0.003} ;
		//endocytosisRates = new double[] {2e-07, 2e-06, 2e-05, 2e-04, 2e-03, 0.02, 0.2}; // VERY HIGH AND VERY LOW
		endocytosisRatesPerSecond = new double[] {0.02};
		
		// CONVERT ENDO RATES TO PER SECOND
/*		endocytosisRates = new double[endocytosisRatesPerSecond.Length];
		for (int i = 0; i < endocytosisRatesPerSecond.Length; i++) {
			endocytosisRates[i] = (double)mngr.toPerBiotick((float)endocytosisRatesPerSecond[i]);
		}
		
		// FIX EXO RATES TO FIX ENDO RATES
		exocytosisRatesNotch = new double[endocytosisRates.Length];
		for (int i=0; i<endocytosisRates.Length;i++) {
			exocytosisRatesNotch[i] = notchSS * endocytosisRates[i] * exoBoost;
			//exocytosisRatesNotch[i] = 0;
		}
		
		exocytosisRatesDelta = new double[endocytosisRates.Length];
		for (int i=0; i<endocytosisRates.Length;i++) {
			exocytosisRatesDelta[i] = deltaSS * endocytosisRates[i] * exoBoost;
			//exocytosisRatesDelta[i] = 0;
		}

		// BINDING PROBABILITY		
		bindingProbs = new float[endocytosisRates.Length];
		for (int i=0; i<endocytosisRates.Length;i++) {
			bindingProbs[i] = adjustParameterToReference(endocytosisRates[i], refBindingProb);
		}
		
		// UNBINDING PROBABILITY		
		unbindingProbs = new float[endocytosisRates.Length];
		for (int i=0; i<endocytosisRates.Length;i++) {
			unbindingProbs[i] = adjustParameterToReference(endocytosisRates[i], refUnbindingProb);
		}
		
		// SIGNAL GENERATION PROBABILITY		
		signalProbs = new float[endocytosisRates.Length];
		for (int i=0; i<endocytosisRates.Length;i++) {
			signalProbs[i] = adjustParameterToReference(endocytosisRates[i], refSignalGenProb);
		}*/
	}
	
	
	
}
