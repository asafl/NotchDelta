using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class twoCellInteraction {
	public List<Receptor3> cellA_receptorsInInteractionRadius = new List<Receptor3>();
	public List<Receptor3> cellB_receptorsInInteractionRadius = new List<Receptor3>();	

	public int cellA_receptorsInInteractionRadius_listLength = 0;
	public int cellB_receptorsInInteractionRadius_listLength = 0;

	// saving the cells for class use
	Cell2 cellA;
	Cell2 cellB;
	
	
	public twoCellInteraction(Cell2 icellA, Cell2 icellB) {
		cellA = icellA;
		cellB = icellB;
		
		// Assigning the closest point on each cell surface to the cells themselves. DONT FORGET TO MOVE IT HERE! 
		// Find the closest point between the two cells on each cell's surface, and calculate the radius around which a receptor interaction might happen. LOCAL SPACE!
		Vector3 fromCellAToCellB = cellB.gameObject.transform.position - cellA.gameObject.transform.position;
		
		//Vector3 closestPointIfSphere = cellA.localToGlobal(cellA.radius * fromCellAToCellB.normalized);// WHY USE THIS?!
		// to find actual closest point on surface, we find the closest vertex to the point on a spherical cell
		//cellA.closestPointToInteractingCellOnCellSurface = cellA.findClosestVertexOnSurface(closestPointIfSphere); // KANAL - no need to find closest point on sphere first, just find the closest point to the other cell.	
		cellA.closestPointToInteractingCellOnCellSurface = cellA.findClosestVertexOnSurface(cellB.transform.position);
		// Setting the origin vertex for filopodia
		//cellA.endocytosisCenterVert = cellA.findFarthestVertexIndexOnSurface(cellB.transform.position);
		
		// cell 2
		Vector3 fromCellBToCellA = cellA.gameObject.transform.position - cellB.gameObject.transform.position;
		
		//closestPointIfSphere = cellB.localToGlobal(cellB.radius * fromCellBToCellA.normalized);	
		// to find actual closest point on surface, we find the closest vertex to the point on a spherical cell
		//cellB.closestPointToInteractingCellOnCellSurface = cellB.findClosestVertexOnSurface(closestPointIfSphere);
		cellB.closestPointToInteractingCellOnCellSurface = cellB.findClosestVertexOnSurface(cellA.transform.position);
		// Setting the origin vertex for filopodia
		//cellB.endocytosisCenterVert = cellB.findFarthestVertexIndexOnSurface(cellA.transform.position);
			
		// Set values for output
		/*cellA.outputValues.interactionPointOnCell = cellA.closestPointToInteractingCellOnCellSurface;
		cellB.outputValues.interactionPointOnCell = cellB.closestPointToInteractingCellOnCellSurface;*/
		
		// Create a line between the cells 
		/*LineRenderer lineObject = GameObject.Instantiate(Resources.Load("LineBetweenReceptors", typeof(LineRenderer))) as LineRenderer;
		lineObject.gameObject.name = "LineBetweenCells";
		// Draw the line itself
		lineObject.SetPosition(0, cellA.closestPointToInteractingCellOnCellSurface);//cellA.transform.position
		lineObject.SetPosition(1, cellB.closestPointToInteractingCellOnCellSurface); //cellB.transform.position
		lineObject.SetColors(new Color(0f, 0f, 1f, 0.3f), new Color(0f, 0f, 1f, 0.3f));*/
		
		Debug.Log ("Distance between cells: " + Utils.calcEuclideanDistance(cellA.closestPointToInteractingCellOnCellSurface, cellB.closestPointToInteractingCellOnCellSurface));
		
	}

	
	private int overwriteReceptorsInInteractionRadiusList (Cell2 cell, List<Receptor3> receptorsInInteractionRadius) {			
		int innerCounter = 0;
		
		// going over all receptors of cell to check if they're inside the possible interaction radius.
		foreach(Receptor3 rec in cell.receptors) {		
			if (rec.inInteractionRadius()) { // receptor IN interaction radius
				try { // TRYING TO OVERWRITE
					// Insert it into beginning of array. We're overwriting elements intentionally, to not search the array everytime (to add delete elements) and to not define new arrays in every iteration.
					receptorsInInteractionRadius[innerCounter++] = rec;						
				} catch { // NO PLACE ALLOCATED YET - ADDING.
					receptorsInInteractionRadius.Add(rec);
					// We're NOT incrementing innerCounter, because apparently it is done even if the above code fails and we reach here. Interesting. 
				}
			}
		}
		
		// returning length of array:
		return innerCounter;
	}
	
	// The per biotick update function - updates the member lists.
	public void updateReceptorsInInteractionRadius () {
		// update the two lists and their lengths.
		cellA_receptorsInInteractionRadius_listLength = overwriteReceptorsInInteractionRadiusList(cellA, cellA_receptorsInInteractionRadius);
		//Debug.Log("UPDATED cell A list length: " + cellA_receptorsInInteractionRadius_listLength + " ACTUAL LIST COUNT: " + cellA_receptorsInInteractionRadius.Count);
		cellB_receptorsInInteractionRadius_listLength = overwriteReceptorsInInteractionRadiusList(cellB, cellB_receptorsInInteractionRadius);
		//Debug.Log("UPDATED cell B list length: " + cellB_receptorsInInteractionRadius_listLength + " ACTUAL LIST COUNT: " + cellB_receptorsInInteractionRadius.Count);
	}
	
}



/*

// Get a list of the receptors of the cell that are inside its interaction radius.
private List<Receptor3> initialReceptorsWithinInteractionRadius (Cell2 cell) {
	List<Receptor3> receptorsInRadius = new List<Receptor3>();
	
	// going over all receptors to check if their inside the possible interaction radius.
	foreach(Receptor3 rec in cell.receptors) {				
		// Is the receptor in the interaction radius?
		if (rec.inInteractionRadius()) {
			// if so - add it to list - all receptor pairs from both lists should be checked for proximity(?)
			receptorsInRadius.Add(rec);					
		}				
	}
	
	return receptorsInRadius;
}

private int findFirstReceptorInList (int recIdToFind, List<Receptor3> recList) {
	for (int i=0; i<recList.Count; i++) 
		if (recList[i].receptorId == recIdToFind)
			return i;
	
	return -1; 
}

private void findAndRemoveReceptorFromList (int recIdToRemove, List<Receptor3> recList) {
	int indOfRec = findFirstReceptorInList(recIdToRemove, recList);
	
	if (indOfRec >= 0)
		recList.RemoveAt(indOfRec);
}

private void refreshReceptorsInInteractionRadiusList (Cell2 cell, List<Receptor3> receptorsInInteractionRadius) {			
	// going over all receptors of cell to check if they're inside the possible interaction radius.
	foreach(Receptor3 rec in cell.receptors) {
		if (rec.inInteractionRadius()) { // receptor IN interaction radius
			// Search for receptor in list - if the receptor isn't in the list yet - add it. 
			if (findFirstReceptorInList(rec.receptorId, receptorsInInteractionRadius) < 0) // it it's smaller than 0 - it means that it wasn't found.
				receptorsInInteractionRadius.Add(rec);						
			
		} else { // it's NOT in the radius
			// if it's already in the list - remove it! 
			findAndRemoveReceptorFromList(rec.receptorId, receptorsInInteractionRadius);
		}
	}			
}

// The per biotick update function - updates the member lists.
public void updateReceptorsInInteractionRadius () {
	// update the two lists:
	refreshReceptorsInInteractionRadiusList(cellA, cellA_receptorsInInteractionRadius);
	
	refreshReceptorsInInteractionRadiusList(cellB, cellB_receptorsInInteractionRadius);
}*/