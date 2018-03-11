using UnityEngine;

public class VertexColorToonLight : MonoBehaviour {


	public Transform lig;
	public Renderer ren;
	public float distanceMultiplier = 3f;
	Material mat;

	// Use this for initialization
	void Start () {
		mat = ren.material;

	}

	// Update is called once per frame
	void Update () {
		mat.SetFloat("_LightTweakX", (lig.transform.position.x - transform.position.x) * distanceMultiplier);
		mat.SetFloat("_LightTweakY", (lig.transform.position.y - transform.position.y) * distanceMultiplier);
		mat.SetFloat("_LightTweakZ", (lig.transform.position.z - transform.position.z) * distanceMultiplier);
	}
}
