#if UNITY_EDITOR
#pragma warning disable 0618
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

[CustomEditor(typeof(SimpleBones))]
public class SimpleBonesEditor : Editor {
	SimpleBones t;
	bool altDown;
	bool ctrlDown;
	EditorStyles smallButtonStyle;
	SerializedProperty defaultRootName;
	SerializedProperty ignore;
	SerializedProperty rootNode;
	SerializedProperty colorNode;
	SerializedProperty colorSelected;
	SerializedProperty colorRoot;
	SerializedProperty colorLine;
	SerializedProperty colorLabel;
	SerializedProperty size;
	SerializedProperty clickSizeMultiplier;
	SerializedProperty lineWidth;
	SerializedProperty hideEnds;
	SerializedProperty label;
	SerializedProperty show;
	SerializedProperty shortcutToggle;
	SerializedProperty toggleShortcut;
	SerializedProperty onlyWhenSelected;
//	SerializedProperty targetAnimation;
	SerializedProperty excludeEndNodes;
	SerializedProperty disableWarnings;
	static Texture2D buttonTexture;
	static Texture2D buttonTextureActive;
	Texture2D texture;

	void Serialization() {
		defaultRootName = serializedObject.FindProperty("defaultRootName");
		ignore = serializedObject.FindProperty("ignore");
		rootNode = serializedObject.FindProperty("rootNode");
		colorNode = serializedObject.FindProperty("colorNode");
		colorSelected = serializedObject.FindProperty("colorSelected");
		colorRoot = serializedObject.FindProperty("colorRoot");
		colorLine = serializedObject.FindProperty("colorLine");
		colorLabel = serializedObject.FindProperty("colorLabel");
		size = serializedObject.FindProperty("size");
		clickSizeMultiplier = serializedObject.FindProperty("clickSizeMultiplier");
		lineWidth = serializedObject.FindProperty("lineWidth");
		hideEnds = serializedObject.FindProperty("hideEnds");
		label = serializedObject.FindProperty("label");
		show = serializedObject.FindProperty("show");
		shortcutToggle = serializedObject.FindProperty("shortcutToggle");
		toggleShortcut = serializedObject.FindProperty("toggleShortcut");
		onlyWhenSelected = serializedObject.FindProperty("onlyWhenSelected");
//		targetAnimation = serializedObject.FindProperty("targetAnimation");
		excludeEndNodes = serializedObject.FindProperty("excludeEndNodes");
		disableWarnings = serializedObject.FindProperty("disableWarnings");
	}

	void OnEnable() {
		CreateTextures();
		Serialization();
		t = (target as SimpleBones);
		if (t.rootNode == null) t.rootNode = t.transform.Find(t.defaultRootName);
		if (t.rootNode != null) PopulateChildren();
		t.hideFlags = HideFlags.DontSaveInBuild;
		if (t.s != null) return;
		SceneView.onSceneGUIDelegate -= t.s;
		t.s = SceneView.onSceneGUIDelegate += OnScene;
		
		
	}

	void OnScene(SceneView sceneview) {
		if (t == null) {
			t = (target as SimpleBones);
			if (t == null) return;
		}
		Event e = Event.current;
		if (e != null) {
			if (e.type == EventType.KeyUp && e.keyCode == t.toggleShortcut) {
				t.shortcutToggle = !t.shortcutToggle;
			}
			if (e.alt) {
				altDown = true;
			} else {
				altDown = false;
			}
			if (e.control || e.shift) {
				ctrlDown = true;
			} else {
				ctrlDown = false;
			}
		}
		if (t.rootNode == null) return;
		if (t.childNodes.Length == 0) PopulateChildren();
		DrawLines();
		DrawNodes();
		OnlyWhenSelected();
	}

	void OnlyWhenSelected() {
		if (!t.onlyWhenSelected) return;
		if (!t.show && Selection.activeTransform != null && Selection.activeTransform.root == t.transform) {
			t.show = true;
		} else if (Selection.activeTransform != null && Selection.activeTransform.root != t.transform || Selection.activeTransform == null) {
			t.show = false;
		}
	}

	void DrawNodes() {
		if (!t.show || !t.shortcutToggle) return;
		for (int i = 0; i < t.childNodes.Length; i++) {
			if (t.hideEnds && t.childNodes[i].childCount == 0) continue;
			if (t.childNodes[i] == t.rootNode) {
				Handles.color = t.colorRoot;
			} else {
				Vector3[] x = new Vector3[2];
				x[1] = t.childNodes[i].parent.position;
				x[0] = t.childNodes[i].position;
				Handles.color = t.colorNode;
			}
			if (BoneIsSelected(t.childNodes[i].gameObject)) {
				Handles.color = t.colorSelected;
			}
			float m = 1f;
			if (t.childNodes[i] == t.rootNode) m = 1.5f;
			if (altDown) {
				Handles.CylinderCap(i, t.childNodes[i].position, t.childNodes[i].transform.rotation, 0.0075f * t.size * m);
			} else if (Handles.Button(t.childNodes[i].position, t.childNodes[i].rotation, 0.0075f * t.size * m, 0.0075f * t.size * t.clickSizeMultiplier, Handles.CylinderCap)) {
				if (ctrlDown) {
					AddToSelection(t.childNodes[i].gameObject);
				} else {
					Selection.activeGameObject = t.childNodes[i].gameObject;
				}
			}
			if (t.label) {
				GUIStyle style = new GUIStyle();
				style.normal.textColor = t.colorLabel;
				Handles.Label(t.childNodes[i].position + t.labelPos, new GUIContent(t.childNodes[i].name), style);
			}
		}
	}

	bool BoneIsSelected(GameObject obj) {
		for (int i = 0; i < Selection.gameObjects.Length; i++) {
			if (obj == Selection.gameObjects[i]) return true;
		}
		return false;
	}

	void AddToSelection(GameObject obj) {
		List<GameObject> someList = new List<GameObject>(Selection.gameObjects);
		someList.Add(obj);
		Selection.objects = someList.ToArray();
	}

	void DrawLines() {
		if (!t.show || !t.shortcutToggle) return;
		Handles.color = t.colorLine;
		float s =  2f / t.sizeModifier  * t.size;
		for (int i = 0; i < t.childNodes.Length; i++) {
			if (t.hideEnds && t.childNodes[i].childCount == 0) continue;
			if (t.childNodes[i] == t.rootNode) {
				Handles.color = t.colorRoot;
			} else {
				Handles.color = t.colorLine;
				Vector3[] x = new Vector3[2];
				x[1] = t.childNodes[i].parent.position;
				x[0] = t.childNodes[i].position;
				Handles.DrawAAPolyLine(texture, t.lineWidth * s, x);
				Handles.color = t.colorNode;
			}
		}
	}

	void CreateTextures() {
		if (texture == null) {
			texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
			texture.hideFlags = HideFlags.HideAndDontSave;
		}
		if (buttonTexture == null) {
			buttonTexture = new Texture2D(1, 1);
			buttonTexture.SetPixel(0, 0, Color.white);
			buttonTexture.Apply();
			buttonTexture.hideFlags = HideFlags.HideAndDontSave;
		}
		if (buttonTextureActive == null) {
			buttonTextureActive = new Texture2D(1, 1);
			buttonTextureActive.SetPixel(0, 0, new Color(.5f, .5f, .5f));
			buttonTextureActive.Apply();
			buttonTextureActive.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	public void PopulateChildren() {
		t.childNodes = t.rootNode.GetComponentsInChildren<Transform>();
	}

	public void GUIColors() {
		GUI.color = new Color(.85f, .85f, .85f);
		GUIBeginBox();
		EditorGUILayout.BeginHorizontal();
		GUI.color = Color.white;
		string bText = "+ Colors";
		if (t.showColors) {
			bText = "= Colors";
		}
		if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
			t.showColors = !t.showColors;
			EditorUtility.SetDirty(t);
		}
		GUI.color = Color.white;
		EditorGUILayout.EndHorizontal();
		if (t.showColors) {
			GUIBeginBox("", true);
			GUILayout.Space(2);
			EditorGUILayout.PropertyField(colorNode, new GUIContent("Color Node", "Color of bones"));
			EditorGUILayout.PropertyField(colorSelected, new GUIContent("Color Selected", "Color of selected bone"));
			EditorGUILayout.PropertyField(colorRoot, new GUIContent("Color Root", "Color of the root bone"));
			EditorGUILayout.PropertyField(colorLine, new GUIContent("Color Line", "Color of lines"));
			EditorGUILayout.PropertyField(colorLabel, new GUIContent("Color Label", "Color of labels"));
			GUILayout.Space(4);

			GUIBeginBox("", true);
			bText = "+ Presets";
			GUI.color = Color.white;
			if (t.showPresets) bText = "= Presets";
			if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
				t.showPresets = !t.showPresets;
				EditorUtility.SetDirty(t);
			}
			if (t.showPresets) {
				float w = EditorGUIUtility.currentViewWidth / 4.5f;
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				

				if (GUIButton("Orange", false, w)) {
					t.colorNode = new Color32(255, 134, 90, 255);
					t.colorRoot = Color.cyan;
					t.colorLine = new Color32(255, 209, 60, 255);
					t.colorLabel = new Color32(255, 209, 60, 255);
					t.colorSelected = new Color32(255, 255, 0, 255);
				}
				if (GUIButton("Blue", false, w)) {
					t.colorNode = new Color32(60, 209, 90, 255);
					t.colorRoot = new Color32(255, 0, 142, 255);
					t.colorLine = new Color32(60, 209, 255, 255);
					t.colorLabel = new Color32(60, 209, 255, 255);
					t.colorSelected = new Color32(86, 101, 255, 255);
				}
				if (GUIButton("White", false, w)) {
					t.colorNode = new Color32(255, 255, 255, 255);
					t.colorRoot = new Color32(150, 150, 150, 255);
					t.colorLine = t.colorLabel = new Color32(200, 200, 200, 255);
					t.colorSelected = new Color32(255, 255, 0, 255);
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();


				if (GUIButton("Grey", false, w)) {
					t.colorNode = new Color32(150, 150, 150, 255);
					t.colorRoot = new Color32(25, 25, 25, 255);
					t.colorLine = new Color32(90, 90, 90, 255);
					t.colorSelected = new Color32(55, 55, 55, 255);
					t.colorLabel = new Color32(75, 75, 75, 255);
				}
				if (GUIButton("Green", false, w)) {
					t.colorNode = new Color32(201, 209, 60, 255);
					t.colorRoot = new Color32(0, 255, 65, 255);
					t.colorLine = new Color32(135, 255, 0, 255);
					t.colorSelected = new Color32(64, 255, 28, 255);
					t.colorLabel = new Color32(0, 255, 86, 255);
				}
				if (GUIButton("Red", false, w)) {
					t.colorNode = new Color32(255, 58, 0, 255);
					t.colorRoot = new Color32(255, 111, 0, 255);
					t.colorLine = new Color32(255, 100, 0, 255);
					t.colorSelected = new Color32(255, 142, 0, 255);
					t.colorLabel = new Color32(255, 47, 0, 255);
				}

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				//if (GUIButton("Print", false, w)) {
				//	Debug.Log(t.colorNode);
				//	Debug.Log(t.colorRoot);
				//	Debug.Log(t.colorLine);
				//	Debug.Log(t.colorSelected);
				//	Debug.Log(t.colorLabel);
				//}

				GUILayout.Space(8);
			}
			GUIEndBox();
			

			GUIEndBox();
		}
		GUIEndBox();
	}

	public void GUIBones() {
		GUIBeginBox();
		string bText = "+ Bones";
		GUI.color = Color.white;
		if (t.showBones) bText = "= Bones";
		if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
			t.showBones = !t.showBones;
			EditorUtility.SetDirty(t);
		}
		if (t.showBones) {
			GUIBeginBox("", true);
			GUILayout.Space(2);
			EditorGUILayout.PropertyField(defaultRootName, new GUIContent("Default Root Name", "Name of the root bone"));
			EditorGUILayout.PropertyField(rootNode, new GUIContent("Root Node", "Root bone of skeleton"));
			EditorGUILayout.PropertyField(size, true);
			EditorGUILayout.PropertyField(clickSizeMultiplier, true);
			EditorGUILayout.PropertyField(lineWidth, true);
			EditorGUILayout.PropertyField(hideEnds, true);
			EditorGUILayout.PropertyField(label, true);
			EditorGUILayout.PropertyField(show, true);
			EditorGUILayout.PropertyField(shortcutToggle, true);
			EditorGUILayout.PropertyField(toggleShortcut, true);
			EditorGUILayout.PropertyField(onlyWhenSelected, true);
			GUIEndBox();
		}
		GUIEndBox();
	}

	static void GUIBeginBox(string label = "", bool white = false) {
		if (white) {
			if (EditorGUIUtility.isProSkin)
				GUI.color = new Color(0f, 0f, 0f);
			else
				GUI.color = new Color(0.8f, 0.8f, 0.8f);
		} else {
			GUI.color = new Color(.85f, .85f, .85f);
		}
		GUIStyle b = new GUIStyle("Box");
		if (!EditorGUIUtility.isProSkin)
			b.normal.background = buttonTexture;

		EditorGUILayout.BeginVertical(b);


		GUI.color = Color.white;
		if (label != "") EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
	}

	static bool GUIButton(string label = "", bool white = false, float width = -1f) {
		bool r = false;
		GUIBeginBox("", white);
		GUIStyle b = new GUIStyle(EditorStyles.miniButton);
		b.alignment = TextAnchor.MiddleCenter;
		b.normal.background = null;
		b.active.background = buttonTextureActive;



		if(width > 1) {
			if (GUILayout.Button(label, b, GUILayout.Width(width))) r = true;
		} else {
			if (GUILayout.Button(label, b)) r = true;
		}

		
		EditorGUILayout.EndVertical();
		return r;
	}

	static void GUIEndBox() {
		EditorGUILayout.EndVertical();
	}

	public void GUIEditAnimations() {
		
		GUIBeginBox();
		EditorGUILayout.BeginHorizontal();
		string bText = "+ Animation";
		GUI.color = Color.white;
		if (t.showAnimations) bText = "= Animation";
		if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
			t.showAnimations = !t.showAnimations;
			EditorUtility.SetDirty(t);
		}
		if (GUILayout.Button("?", EditorStyles.boldLabel, GUILayout.Width(15), GUILayout.Height(15))) {
			PopupHelp.OpenWindow();
		}
		EditorGUILayout.EndHorizontal();
		if (t.targetAnimation == null) {
			GUILayout.Label("Please Select Clip in Animation View");
			GUIEndBox();
			return;
		}

		if (t.showAnimations) {
			GUIBeginBox("", true);
			if (GUIButton(t.targetAnimation.name, false)) EditorGUIUtility.PingObject(t.targetAnimation);
			EditorGUILayout.PropertyField(disableWarnings, true);
			GUIEndBox();

			if (t.targetAnimation == null) {
				GUI.enabled = false;
			}

			GUIBeginBox("", true);
			bText = "+ Create Animation Keys";
			GUI.color = Color.white;
			if (t.showCreateCurves) bText = "= Create Animation Keys";
			if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
				t.showCreateCurves = !t.showCreateCurves;
				EditorUtility.SetDirty(t);
			}
			if (t.showCreateCurves) {
				EditorGUILayout.PropertyField(excludeEndNodes, new GUIContent("Exclude End Nodes", "Exclude End Nodes"));
				EditorGUILayout.BeginHorizontal();
				if (GUIButton("Position", false)) AddKeys("m_LocalPosition");
				if (GUIButton("Rotation", false)) AddKeys("localEulerAnglesBaked");
				if (GUIButton("Scale", false)) AddKeys("m_LocalScale");
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(8);
			}
			GUIEndBox();

			GUIBeginBox("", true);
			bText = "+ Remove Animation Curves";
			GUI.color = Color.white;
			if (t.showRemoveCurves) bText = "= Remove Animation Curves";
			if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
				t.showRemoveCurves = !t.showRemoveCurves;
				EditorUtility.SetDirty(t);
			}
			if (t.showRemoveCurves) {
				EditorGUILayout.BeginHorizontal();
				if (GUIButton("Position", false)) KeyRemoveAll("Position");
				if (GUIButton("Rotation", false)) KeyRemoveAll("Rotation");
				if (GUIButton("Scale", false)) KeyRemoveAll("LocalScale");
				EditorGUILayout.EndHorizontal();

				GUI.color = Color.cyan;
				EditorGUILayout.PropertyField(ignore, new GUIContent("Ignore List", "  \n  Exclude Object From Curve Deletion\n\n  It's a good idea to put the root bone in this list..\n  "), true);
				GUI.color = Color.white;
				GUILayout.Space(8);
			}
			GUIEndBox();

			GUIBeginBox("", true);
			bText = "+ Remove Duplicated Keys";
			GUI.color = Color.white;
			if (t.showRemoveDuplicateKeys) bText = "= Remove Duplicated Keys";
			if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
				t.showRemoveDuplicateKeys = !t.showRemoveDuplicateKeys;
				EditorUtility.SetDirty(t);
			}
			if (t.showRemoveDuplicateKeys) {
				EditorGUILayout.BeginHorizontal();
				if (GUIButton("Position", false)) KeyRemoveDuplicate("m_LocalPosition", "Position");
				if (GUIButton("Rotation", false)) KeyRemoveDuplicate("localEulerAnglesBaked", "Rotation");
				if (GUIButton("Scale", false)) KeyRemoveDuplicate("m_LocalScale", "Scale");
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(8);
			}
			GUIEndBox();

			GUIBeginBox("", true);
			bText = "+ Simplify";
			GUI.color = Color.white;
			if (t.showSimplifyKeys) bText = "= Simplify";
			if (GUILayout.Button(bText, EditorStyles.boldLabel)) {
				t.showSimplifyKeys = !t.showSimplifyKeys;
				EditorUtility.SetDirty(t);
			}
			if (t.showSimplifyKeys) {
				EditorGUILayout.BeginHorizontal();
				t.simplifyTimeStep = (int)GUILayout.HorizontalSlider(t.simplifyTimeStep, 1, 20);
				if (GUI.changed) {
					EditorUtility.SetDirty(t);
				}
				GUILayout.Label("0" + (t.simplifyTimeStep * 0.01f).ToString(".00").Replace(".", ":"), EditorStyles.miniLabel, GUILayout.Width(35));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				if (GUIButton("Position", false)) Simplify("m_LocalPosition", t.simplifyTimeStep * 0.01f);
				if (GUIButton("Rotation", false)) Simplify("localEulerAnglesBaked", t.simplifyTimeStep * 0.01f);
				if (GUIButton("Scale", false)) Simplify("m_LocalScale", t.simplifyTimeStep * 0.01f);
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(8);
			}
			GUIEndBox();
		}
		GUIEndBox();
		GUI.enabled = true;
	}

	void AddKeys(string type) {
		if (!t.disableWarnings) if (!EditorUtility.DisplayDialog("Create Keys", "Sure you want to create keys for all bones for?\n" + t.targetAnimation.name + "?\n\nConsider backing up animation before doing this.", "Yes", "No")) return;
		GetAnimationClipFromAnimationWindow();
		Undo.RegisterCompleteObjectUndo(t.targetAnimation, "SimpleBoned: Undo Create Keys");
		KeyCreate(type, UnityAnimationWindow.GetAnimationWindowCurrentTime());
	}

	void KeyCreate(string type, float animationTime, bool createCurves = true) {
		for (int i = 0; i < t.childNodes.Length; i++) {
			if (t.excludeEndNodes && t.childNodes[i].childCount == 0) continue;
			string p = AnimationUtility.CalculateTransformPath(t.childNodes[i], t.transform);
			for (int j = 0; j < 3; j++) {
				Keyframe key;
				AnimationCurve curve;
				bool newCurveCreated = false;
				string xyz  = ".x";
				if (j == 1) {
					xyz = ".y";
				} else if (j == 2) {
					xyz = ".z";
				}
				curve = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(p, typeof(Transform), type + xyz));
				if (curve == null && createCurves) {
					newCurveCreated = true;
					curve = new AnimationCurve();
				}
				if (curve == null) continue;
				if (type == "m_LocalPosition") {
					if (!newCurveCreated) key = new Keyframe(animationTime, curve.Evaluate(animationTime));
					else {
						if (xyz == ".x") key = new Keyframe(animationTime, t.childNodes[i].localPosition.x);
						else if (xyz == ".y") key = new Keyframe(animationTime, t.childNodes[i].localPosition.y);
						else key = new Keyframe(animationTime, t.childNodes[i].localPosition.z);
					}
				} else if (type == "localEulerAnglesBaked") {
					if (!newCurveCreated) key = new Keyframe(animationTime, curve.Evaluate(animationTime));
					else {
						Vector3 fixAxis = t.childNodes[i].localEulerAngles;
						if (xyz == ".x") {
							if (fixAxis.x > 180f) fixAxis.x = (360f - fixAxis.x) * -1;
							key = new Keyframe(animationTime, fixAxis.x);
						} else if (xyz == ".y") {
							if (fixAxis.y > 180f) fixAxis.y = (360f - fixAxis.y) * -1;
							key = new Keyframe(animationTime, fixAxis.y);
						} else {
							if (fixAxis.z > 180f) fixAxis.z = (360f - fixAxis.z) * -1;
							key = new Keyframe(animationTime, fixAxis.z);
						}
					}
				} else {
					if (!newCurveCreated) key = new Keyframe(animationTime, curve.Evaluate(animationTime));
					else {
						if (xyz == ".x") key = new Keyframe(animationTime, t.childNodes[i].localScale.x);
						else if (xyz == ".y") key = new Keyframe(animationTime, t.childNodes[i].localScale.y);
						else key = new Keyframe(animationTime, t.childNodes[i].localScale.z);
					}
				}
				curve.AddKey(key);
				AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(p, typeof(Transform), type + xyz), curve);
			}
		}
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}

	void Simplify(string type, float timeStep = 0.1f) {
		timeStep = timeStep / 60 * 100;
		if (!t.disableWarnings) if (!EditorUtility.DisplayDialog("Create Keys", "Sure you want to create keys for all bones for?\n" + t.targetAnimation.name + "?\n\nConsider backing up animation before doing this.", "Yes", "No")) return;
		GetAnimationClipFromAnimationWindow();
		EditorUtility.DisplayProgressBar("Simplify", "Getting Ready", 0);
		Undo.RegisterCompleteObjectUndo(t.targetAnimation, "SimpleBoned: Undo Create Keys");

		if (timeStep < 0.01) return;
		GetAnimationClipFromAnimationWindow();
		float timeCount = 0f;

		while (timeCount < t.targetAnimation.length) {
			KeyCreate(type, timeCount, false);
			timeCount += timeStep;
			EditorUtility.DisplayProgressBar("Simplify", "Adding Keys", timeCount / t.targetAnimation.length * .5f);
		}
		KeyCreate(type, t.targetAnimation.length, false);

		AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(t.targetAnimation);
		for (int j = 0; j < curves.Length; j++) {

			EditorUtility.DisplayProgressBar("Simplify", "Removing Keys", .5f + (float)((float)j / (float)curves.Length) * .5f);

			AnimationCurve curve;

			curve = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".x"));
			curve = RemoveKeyframes(curve, timeStep);
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".x"), curve);

			curve = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".y"));
			curve = RemoveKeyframes(curve, timeStep);
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".y"), curve);

			curve = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".z"));
			curve = RemoveKeyframes(curve, timeStep);
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(curves[j].path, typeof(Transform), type + ".z"), curve);
		}
		EditorUtility.ClearProgressBar();
	}

	AnimationCurve RemoveKeyframes(AnimationCurve curve, float timeStep) {
		float rt, rb = 0f;
		if (curve == null) return null;
		for (int i = curve.keys.Length - 1; i >= 0; i--) {
			rt = curve.keys[i].time;
			rb = rt % timeStep;
			if (rb > 0.001f && rb <= timeStep - 0.001f && rt != t.targetAnimation.length) {
				curve.RemoveKey(i);
			}
		}
		return curve;
	}

	void KeyRemoveAll(string type) {
		GetAnimationClipFromAnimationWindow();
		if (!t.disableWarnings) if (!EditorUtility.DisplayDialog("Delete Curves", "Sure you want to delete all " + type + " curves in\n" + t.targetAnimation.name + "\n\nConsider backing up animation before doing this.", "Yes", "No")) return;
		EditorCurveBinding[] ac;
		ac = AnimationUtility.GetCurveBindings(t.targetAnimation);
		Undo.RegisterCompleteObjectUndo(t.targetAnimation, "SimpleBoned: Undo Remove All Curves");
		for (int i = 0; i < ac.Length; i++) {
			if (!IgnoreDelete(ac[i].path) && ac[i].propertyName.Contains(type)) {
				AnimationUtility.SetEditorCurve(t.targetAnimation, ac[i], null);
			}
		}
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}

	void KeyRemoveDuplicate(string type, string name) {
		GetAnimationClipFromAnimationWindow();
		if (!t.disableWarnings) if (!EditorUtility.DisplayDialog("Delete Keys", "Sure you want to delete all duplicate " + name + " keys in\n" + t.targetAnimation.name + "?\n\nConsider backing up animation before doing this.", "Yes", "No")) return;
		EditorCurveBinding[] ac;
		ac = AnimationUtility.GetCurveBindings(t.targetAnimation);
		Undo.RegisterCompleteObjectUndo(t.targetAnimation, "SimpleBoned: Undo Remove All Duplicate Keys");
		for (int i = 0; i < ac.Length; i++) {
			AnimationCurve curveX = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type+".x"));
			AnimationCurve curveY = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type+".y"));
			AnimationCurve curveZ = AnimationUtility.GetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type+".z"));
			if (curveX == null) continue;
			for (int j = 1; j < curveX.length - 1; j++) {
				if (curveX.keys[j].value == curveX.keys[j - 1].value && curveX.keys[j].value == curveX.keys[j + 1].value) {
					curveX.RemoveKey(j);
				}
			}
			for (int j = 1; j < curveY.length - 1; j++) {
				if (curveY.keys[j].value == curveY.keys[j - 1].value && curveY.keys[j].value == curveY.keys[j + 1].value) {
					curveY.RemoveKey(j);
				}
			}
			for (int j = 1; j < curveZ.length - 1; j++) {
				if (curveZ.keys[j].value == curveZ.keys[j - 1].value && curveZ.keys[j].value == curveZ.keys[j + 1].value) {
					curveZ.RemoveKey(j);
				}
			}
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type + ".x"), curveX);
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type + ".y"), curveY);
			AnimationUtility.SetEditorCurve(t.targetAnimation, EditorCurveBinding.FloatCurve(ac[i].path, typeof(Transform), type + ".z"), curveZ);
		}
		UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
	}

	public override void OnInspectorGUI() {
		CreateTextures();
		serializedObject.Update();
		GUIBones();
		GUIColors();
		GUIEditAnimations();
		if (t.showAnimations) {
			GetAnimationClipFromAnimationWindow();
		}
		if (GUI.changed) {
			SceneView.RepaintAll();
			serializedObject.ApplyModifiedProperties();
		}
	}

	bool IgnoreDelete(string path) {
		for (int i = 0; i < t.ignore.Length; i++) {
			string p = AnimationUtility.CalculateTransformPath(t.ignore[i], t.transform);
			if (p == path) return true;
		}
		return false;
	}

	void GetAnimationClipFromAnimationWindow() {

		AnimationClip clip = UnityAnimationWindow.GetAnimationWindowCurrentClip();
		if (clip != null) t.targetAnimation = clip;
	}
}

[AddComponentMenu("Simple Bones", 1)]
public class SimpleBones : MonoBehaviour {
	public SceneView.OnSceneFunc s;
	public string defaultRootName = "Root";
	public int simplifyTimeStep = 2;
	public bool showSimplifyKeys;
	public bool showCreateCurves;
	public bool showRemoveCurves;
	public bool showRemoveDuplicateKeys;
	public bool showColors;
	public bool showPresets;

	public bool showBones = true;
	public bool showAnimations;
	public AnimationClip targetAnimation;
	public bool disableWarnings;
	public Transform[] ignore;
	public Transform rootNode;
	public Color32 colorNode = new Color32(255, 134 , 90, 255);
	public Color32 colorSelected = new Color32(255, 255 , 0, 255);
	public Color32 colorRoot = Color.cyan;
	public Color32 colorLine = new Color32(255, 209, 60 ,255);
	public Color32 colorLabel =  new Color32(255, 209, 60 ,255);
	[Tooltip("Node size")]
	public float size = 1.5f;
	[Tooltip("Increase the clickable area of the node")]
	public float clickSizeMultiplier = 1;
	[Tooltip("Width of line between nodes")]
	public float lineWidth = 1.5f;
	[Tooltip("Hide last children of parent nodes")]
	public bool hideEnds;
	[Tooltip("Show labels")]
	public bool label;
	[Tooltip("Offset position of labels")]
	public Vector3 labelPos = new Vector3(0.01f, 0.005f,0f);
	public float sizeModifier = 1;
	public bool show = true;
	public bool shortcutToggle = true;
	public KeyCode toggleShortcut = KeyCode.Backslash;
	public bool onlyWhenSelected;
	public Transform[] childNodes;
	public bool excludeEndNodes;
}

public class PopupHelp : EditorWindow {
	static EditorWindow currentWindow;
	static PopupHelp window;

	static public void OpenWindow() {
		window = CreateInstance<PopupHelp>();
		currentWindow = EditorWindow.mouseOverWindow;
		window.position = currentWindow.position;
		window.ShowPopup();
	}

	private void OnInspectorUpdate() {
		if (window == null) return;
		window.position = currentWindow.position;
	}

	void OnGUI() {
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("Create Animation Keys.\n", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Used to create keys and curves for all the bones. For crossfade to work, all bones in the animation need to have keys on the start and end. If one animation have position keys and tweens to one without them, it will get stuck in the last position.\n\nUsing this beyond the starting keys could prove problematic. Double click the Animation Window top timeline bar for the same functionality.", EditorStyles.wordWrappedLabel);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Remove Animation Curves.\n", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Deletes all keys and curves of a given type, position, rotation or scale. Except keys and curves of gameobjects that are in the Ignore list. The root bone is a good idea to put in this list, since it usually is the only bone that uses position.", EditorStyles.wordWrappedLabel);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Remove Duplicated Keys.\n", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("If there is no use for a key, it will be removed.", EditorStyles.wordWrappedLabel);
		GUILayout.Space(20);
		EditorGUILayout.LabelField("Simplify.\n", EditorStyles.boldLabel);
		EditorGUILayout.LabelField("Creates keys in all curves on an interval, then removes keys inbetween.", EditorStyles.wordWrappedLabel);
		GUILayout.Space(20);
		if (GUILayout.Button("Close")) Close();
		GUILayout.FlexibleSpace();
	}
}

static class UnityAnimationWindow {
	static Type animationWindowType = null;

	static Type GetAnimationWindowType() {
		if (animationWindowType == null) animationWindowType = Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
		return animationWindowType;
	}

	static UnityEngine.Object GetOpenAnimationWindow() {
		UnityEngine.Object[] openAnimationWindows = Resources.FindObjectsOfTypeAll(GetAnimationWindowType());
		if (openAnimationWindows.Length > 0) return openAnimationWindows[0];
		return null;
	}

	public static float GetAnimationWindowCurrentTime() {
		UnityEngine.Object w = GetOpenAnimationWindow();
		if (w != null) {
			BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			FieldInfo animEditor = GetAnimationWindowType().GetField("m_AnimEditor", flags);

			Type animEditorType = animEditor.FieldType;
			System.Object animEditorObject = animEditor.GetValue(w);
			FieldInfo animWindowState = animEditorType.GetField("m_State", flags);
			Type windowStateType = animWindowState.FieldType;

			System.Object timeInSeconds = windowStateType.InvokeMember("get_currentTime", BindingFlags.InvokeMethod | BindingFlags.Public, null, animWindowState.GetValue(animEditorObject), null);

			return (float)timeInSeconds;
		}
		return 0f;
	}

	public static AnimationClip GetAnimationWindowCurrentClip() {
		UnityEngine.Object aw = GetOpenAnimationWindow();
		if (aw == null) return null;
		BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
		FieldInfo animEditor = GetAnimationWindowType().GetField("m_AnimEditor", flags);
		Type animEditorType = animEditor.FieldType;
		System.Object animEditorObject = animEditor.GetValue(aw);
		FieldInfo animWindowState = animEditorType.GetField("m_State", flags);
		Type windowStateType = animWindowState.FieldType;
		System.Object clip = windowStateType.InvokeMember("get_activeAnimationClip", BindingFlags.InvokeMethod | BindingFlags.Public, null, animWindowState.GetValue(animEditorObject), null);
		return (AnimationClip)clip;
	}

	public static System.Object GetDopeline() {
		System.Object aw = GetOpenAnimationWindow();
		if (aw == null) return null;
		BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
		FieldInfo animEditor = GetAnimationWindowType().GetField("m_AnimEditor", flags);
		Type animEditorType = animEditor.FieldType;
		System.Object animEditorObject = animEditor.GetValue(aw);
		FieldInfo animWindowState = animEditorType.GetField("m_State", flags);
		Type windowStateType = animWindowState.FieldType;
		System.Object clip = (System.Object)windowStateType.InvokeMember("get_dopelines", BindingFlags.InvokeMethod | BindingFlags.Public, null, animWindowState.GetValue(animEditorObject), null);

		return clip;
	}
}


#endif