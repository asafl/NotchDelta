using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Utils {
	// Two vectors MUST BE IN LOCAL COORDINATES (for angle between them!) - distance in microns
	public static float calcArcDistance(Vector3 from, Vector3 to, float radius) {
		float angle = Vector3.Angle(from, to);
		
		return (radius * angle * Mathf.Deg2Rad);		
	}
	
	// VECTORS MUST BE IN GLOBAL COORDINATES TO MAKE SURE SCALE IS TAKEN INTO ACCOUNT.
	public static float calcEuclideanDistance (Vector3 from, Vector3 to) {
		return Vector3.Distance(from, to);
	}
	
	public static float phi (Vector3 vec) {
		return positiveAngle(Mathf.Rad2Deg * Mathf.Atan2(vec[2],vec[1]));		
	}

    // From: http://answers.unity3d.com/questions/24983/how-to-calculate-the-angle-between-two-vectors.html
    // returns the angle relative to x axis as it is projected on the plane where normalToPlane is its normal.
    public static float phiInPlane (Vector3 vecOnPlane, Vector3 normalToPlane) {
        // project the x axis into the plane
        Vector3 xOnPlane = Vector3.ProjectOnPlane(new Vector3(1,0,0), normalToPlane);
        Vector3 yOnPlane = Vector3.ProjectOnPlane(new Vector3(0,1,0), normalToPlane);

        float angle = Vector3.Angle(vecOnPlane, xOnPlane);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left
        float sign = Mathf.Sign(Vector3.Dot(vecOnPlane, yOnPlane));

        // measure angle from x on plane
        return positiveAngle(sign * angle);
    }
		
	public static float positiveAngle (float ang) {
		if (ang < 0) 
			return (ang + 360);
		else 
			return ang;
	}
	
	public static float[] createVector (float from, float by, float to) {
		float eps = 0.00001f;
		
		List<float> arr = new List<float>();
		for (float r = from; r <= (to+eps); r += by) {
			arr.Add(r);
		}
		
		return arr.ToArray();
	}
	
	public static float[] createVectorByLogJump (float from, float to, float numOfEntries) {
		// translate values to log
		float logFrom = Mathf.Log10(from);
		float logTo = Mathf.Log10(to);
		
		float diff = (logTo - logFrom)/((int)numOfEntries-1);
		float[] logVec = createVector(logFrom, diff, logTo);
		float[] regVec = new float[logVec.Length];
		
		// translate back from log
		for (int i = 0; i < logVec.Length; i++) {
			regVec[i] = Mathf.Pow(10, logVec[i]);
		}
		
		return regVec;
	}
	
	public static float[] createVectorByDLSJump (float fromSpeed, float toSpeed, float numOfEntries, float endoRate, float timeStepsInSeconds) {
		// translate values to DLS
		float fromDLS = getDiffusionLengthScale(fromSpeed, endoRate, timeStepsInSeconds);
		float toDLS = getDiffusionLengthScale(toSpeed, endoRate, timeStepsInSeconds);
		
		float diff = (toDLS - fromDLS)/((int)numOfEntries-1);
		float[] DLSVec = createVector(fromDLS, diff, toDLS);
		float[] regVec = new float[DLSVec.Length];
		
		// translate back from DLS
		for (int i = 0; i < DLSVec.Length; i++) {
			regVec[i] = getLinearSpeedFromDiffusion(getDiffusionFromDLS(DLSVec[i], endoRate, timeStepsInSeconds));
		}
		
		return regVec;
	}
	
	public static float[] createVectorByDLSLogJump (float fromSpeed, float toSpeed, float numOfEntries, float endoRate, float timeStepsInSeconds) {
		// translate values to DLS
		float fromDLS = getDiffusionLengthScale(fromSpeed, endoRate, timeStepsInSeconds);
		float toDLS = getDiffusionLengthScale(toSpeed, endoRate, timeStepsInSeconds);

		float[] DLSVec = createVectorByLogJump(fromDLS, toDLS, numOfEntries);
		float[] regVec = new float[DLSVec.Length];
		
		// translate back from DLS
		for (int i = 0; i < DLSVec.Length; i++) {
			regVec[i] = getLinearSpeedFromDiffusion(getDiffusionFromDLS(DLSVec[i], endoRate, timeStepsInSeconds));
		}
		
		return regVec;
	}


	// Uses DIFFUSION instead of linear speeds.
	public static float[] createVectorByDLSLogJump (float fromDiffusion, float toDiffusion, float numOfEntries, float endoRate) {
		// translate values to DLS
		float fromDLS = calcDiffusionLengthScale(fromDiffusion, endoRate);
		float toDLS = calcDiffusionLengthScale(toDiffusion, endoRate);
		
		float[] DLSVec = createVectorByLogJump(fromDLS, toDLS, numOfEntries);
		
		float[] diffVec = new float[DLSVec.Length];
		
		// translate back from DLS
		for (int i = 0; i < DLSVec.Length; i++) {
			diffVec[i] = getDiffusionFromDLS(DLSVec[i], endoRate);
        }
        
		return diffVec;
	}
	
			
	public static string[][] listToStringArray (List<float> lst) {
		string[][] stArr = new string[lst.Count][];
		string[] singleCell = new string[1];
		int i=0;
		foreach (float mem in lst) {
			singleCell[0] = (string)mem.ToString();
			stArr[i++] = (string[])singleCell.Clone();
		}
		
		return stArr;
	}
	
	private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3, float scaleQuotient) {		
		float v321 = p3.x * p2.y * p1.z * scaleQuotient;
		float v231 = p2.x * p3.y * p1.z * scaleQuotient;
		float v312 = p3.x * p1.y * p2.z * scaleQuotient;
		float v132 = p1.x * p3.y * p2.z * scaleQuotient;
		float v213 = p2.x * p1.y * p3.z * scaleQuotient;
		float v123 = p1.x * p2.y * p3.z * scaleQuotient;
		return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
	}
	

	public static float volumeOfMesh(Mesh mesh, Vector3 scale) {
		float volume = 0;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		
		for (int i = 0; i < mesh.triangles.Length; i += 3) {
			Vector3 p1 = vertices[triangles[i + 0]];
			Vector3 p2 = vertices[triangles[i + 1]];
			Vector3 p3 = vertices[triangles[i + 2]];
			volume += SignedVolumeOfTriangle(p1, p2, p3, scale.x * scale.y * scale.z);
		}
		
		return Mathf.Abs(volume);
	}
	
	public static float surfaceAreaOfMesh(Transform t, Mesh mesh) {
		// run over all triangles, calculate area of each, add all
		float area = 0;
		float edgeA,edgeB,edgeC,p,triangleArea = 0;
		Vector3 p1,p2,p3;
		
		for (int i = 0; i < mesh.triangles.Length; i=i+3) {
			// get the edge lengths: 
			p1 = t.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
			p2 = t.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
			p3 = t.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);
			edgeA = Vector3.Distance(p1,p2);
			edgeB = Vector3.Distance(p1,p3);
			edgeC = Vector3.Distance(p2,p3);
			
			p = (edgeA + edgeB + edgeC)/2;
			triangleArea = Mathf.Sqrt(p*(p-edgeA)*(p-edgeB)*(p-edgeC));
			area += triangleArea;
		}
		
		return area;
	}
	
	public static float getDiffusionFromLinearSpeed (float linearSpeed) {
		//return ((0.008044342f + linearSpeed * 0.221301283f)/4);
		return (0.006456689f + linearSpeed * 0.04192019f);
	}
	
	public static float getLinearSpeedFromDiffusion (float diffusion) {
		return ((diffusion - 0.006456689f) / 0.04192019f);
	}

	public static float getDiffusionLengthScale (float speed, float endocytosis_rate, float timeStepsInSeconds) { // ENDO RATE IS PER BIOTICK -> RETURNS NOT PER BIOTICK (PER SEC)
		return Mathf.Sqrt(getDiffusionFromLinearSpeed(speed)/(endocytosis_rate/timeStepsInSeconds)); // convert endocytosis rate to seconds.
	}
	
	public static float calcDiffusionLengthScale (float diffusion, float endocytosis_rate) {
		return Mathf.Sqrt(diffusion/endocytosis_rate); // convert endocytosis rate to seconds.
	}
	
	public static float getDiffusionFromDLS (float diffusionLengthScale, float endocytosis_rate, float timeStepsInSeconds) {// ENDO RATE IS PER BIOTICK -> RETURNS DIFFUSION PER SEC
		return Mathf.Pow(diffusionLengthScale, 2) * (endocytosis_rate/timeStepsInSeconds);
	}
	
	public static float getDiffusionFromDLS (float diffusionLengthScale, float endocytosis_rate) { // IF ENDO RATE ISN'T PER BIOTICK
		return Mathf.Pow(diffusionLengthScale, 2) * (endocytosis_rate);
    }
    
	
	public static float areaOfRing(float innerRadius, float outerRadius) {
		return (Mathf.PI * (Mathf.Pow(outerRadius,2) - Mathf.Pow(innerRadius,2)));
	}
	
	public static float outerRadiusFromSurfaceAreaAndInnerRadius(float innerRadius, float surfaceArea) {
		return Mathf.Sqrt(surfaceArea/Mathf.PI + Mathf.Pow(innerRadius,2));
	}
	
}
