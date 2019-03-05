using UnityEngine;
using System.Collections;
using UnityEditor;

public class saveMeshAsAsset : MonoBehaviour {

	public string nameOfMesh = "CellWithFilopodia";

	// Use this for initialization
	void Start () {	
		var savePath = "assets/Prefabs/" + nameOfMesh + ".asset";
		Debug.Log("Saved Mesh to:" + savePath);
		AssetDatabase.CreateAsset(this.gameObject.GetComponent<MeshFilter>().mesh, savePath);
		AssetDatabase.SaveAssets();	
	}
	

}
