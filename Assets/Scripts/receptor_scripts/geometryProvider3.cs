﻿using UnityEngine;
using System.Collections;

public class geometryProvider3  {
	
	public float magnitude = 0.06f;
	
	public Vector3[] tetradedronVertices = {
		new Vector3(0, 0, 0), 
		new Vector3(1, 0, 0), 
		new Vector3(0.5f, 0, Mathf.Sqrt(0.75f)), 
		new Vector3(0.5f, Mathf.Sqrt(0.75f), Mathf.Sqrt(0.75f)/3)};
	
	public int[] tetradedronTris = {
		0,1,2,
		0,2,3,
		2,1,3,
		0,3,1
	};
	
	public Vector3 tetrahedronCenter = Vector3.zero;
	
	public geometryProvider3(float receptorSizeMultiplier) {
		for(int i=0; i<tetradedronVertices.Length; i++) {
			//tetradedronVertices[i] = tetradedronVertices[i].normalized;
			tetradedronVertices[i] = receptorSizeMultiplier * tetradedronVertices[i];
			tetrahedronCenter.x += tetradedronVertices[i].x;
			tetrahedronCenter.y += tetradedronVertices[i].y;
			tetrahedronCenter.z += tetradedronVertices[i].z;
		}
		
		tetrahedronCenter.x /= tetradedronVertices.Length;
		tetrahedronCenter.y /= tetradedronVertices.Length;
		tetrahedronCenter.z /= tetradedronVertices.Length;
	}
}
