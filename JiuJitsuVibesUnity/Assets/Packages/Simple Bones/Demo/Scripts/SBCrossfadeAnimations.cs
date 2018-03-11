using UnityEngine;
using System.Collections;
public class SBCrossfadeAnimations : MonoBehaviour {
	Animation a;
	ArrayList animations;
	int current;

	void Start () {
		CreateAnimationList();
		if (animations.Count == 0) return;
		Invoke("CrossfadeRandom", 1f);
	}
	
	void CreateAnimationList() {
		animations = new ArrayList();
		a = transform.GetComponent<Animation>();

		foreach (AnimationState state in a) {
			animations.Add(state.name);
		}
	}


	void CrossfadeRandom () {
		//int r = Random.Range(0, animations.Count);
		//if (r != current) {
			a.CrossFade((string)animations[Random.Range(0, animations.Count)], .5f);
		//	current = r;
			Invoke("CrossfadeRandom", Random.Range(2, 3));
		//} else {
		//	current = r;
		//	Invoke("CrossfadeRandom", Random.Range(.1f, .2f));
		//}

	}
}
