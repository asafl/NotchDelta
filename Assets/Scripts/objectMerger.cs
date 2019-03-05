using UnityEngine;
using System.Collections;

//using Parabox.CSG;
using ConstructiveSolidGeometry;

public class objectMerger : MonoBehaviour {

	public GameObject go1;
	public GameObject go2;
	public Material material;

	// Use this for initialization
	void Start () {
		Debug.Log ("Mesh go1: " + go1.GetComponent<MeshFilter>().mesh.vertices.Length);
		Debug.Log ("Mesh go2: " + go2.GetComponent<MeshFilter>().mesh.vertices.Length);
	
		//Debug.Log (ObjExporter.MeshToString(go2.GetComponent<MeshFilter>()));
		//ObjExporter.MeshToString(go2.GetComponent<MeshFilter>());
	
		/*Mesh m = CSG.Union(go1, go2);
	
		// Create a gameObject to render the result
		GameObject composite = new GameObject();
		composite.transform.position = new Vector3(8,8,8);
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = material;*/

		CSG A = CSG.fromMesh(go1.GetComponent<MeshFilter>().mesh, go1.transform);
		CSG B = CSG.fromMesh(go2.GetComponent<MeshFilter>().mesh, go2.transform);
		
		CSG result = null;
		result = A.union(B);

		GameObject composite = new GameObject();
		if (result != null) composite.AddComponent<MeshFilter>().mesh = result.toMesh();
		composite.transform.position = new Vector3(20,20,20);
		composite.AddComponent<MeshRenderer>().sharedMaterial = material;
        
        //saveMeshAsAsset sa = composite.AddComponent<saveMeshAsAsset>() as saveMeshAsAsset;		
        
        
        
    }
    
}
