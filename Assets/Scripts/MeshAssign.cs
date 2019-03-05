using UnityEngine;
using System.Collections;

public class MeshAssign : MonoBehaviour {

	[SerializeField] Mesh mesh;

	// Use this for initialization
	void Start () {
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
