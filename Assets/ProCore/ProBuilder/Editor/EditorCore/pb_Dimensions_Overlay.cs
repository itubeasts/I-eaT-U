﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;

public class pb_Dimensions_Overlay : pb_ISceneEditor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Object Info/Hide Dimensions Overlay", true, 3)]
	public static bool HideVerify()
	{
		return pb_Dimensions_Overlay.instance != null;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Object Info/Hide Dimensions Overlay", false, 3)]
	public static void Hide()
	{
		pb_Dimensions_Overlay.instance.Close();
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Object Info/Show Dimensions Overlay", true, 2)]
	public static bool InitVerify()
	{
		return pb_Dimensions_Overlay.instance == null;
	}


	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Object Info/Show Dimensions Overlay", false, 2)]
	public static void Init()
	{
		pb_ISceneEditor.Create<pb_Dimensions_Overlay>();
	}

	public override void OnInitialize()
	{
		mesh = new Mesh();
		material = new Material(Shader.Find("ProBuilder/UnlitVertexColor"));
		mesh.hideFlags = HideFlags.DontSave;
		material.hideFlags = HideFlags.DontSave;
	}

	public override void OnDestroy()
	{
		GameObject.DestroyImmediate(mesh);
		GameObject.DestroyImmediate(material);
	}

	public override void OnSceneGUI(SceneView scnview)
	{
		if( Selection.activeTransform != null && Selection.activeTransform.GetComponent<MeshFilter>() != null)
			RenderBounds(Selection.activeTransform.GetComponent<MeshFilter>());
	}

	Mesh mesh;
	Material material;
	
	// readonly Color wirecolor = new Color(.9f, .9f, .9f, .6f);
	readonly Color background = new Color(.3f, .3f, .3f, .6f);
	readonly Color LightWhite = new Color(.6f, .6f, .6f, .5f);

	void RenderBounds(MeshFilter mf)
	{
		if(!mesh) return;

		// show labels
		Bounds wb = mf.transform.GetComponent<MeshRenderer>().bounds;

		DrawHeight(wb.center, wb.extents);
		DrawWidth(wb.center, wb.extents);
		DrawDepth(wb.center, wb.extents);
		
	}

	const float DISTANCE_LINE_OFFSET = .2f;
	
	float LineDistance()
	{
		return HandleUtility.GetHandleSize(Selection.activeTransform.position) * DISTANCE_LINE_OFFSET;
	}

	Transform cam { get { return SceneView.lastActiveSceneView.camera.transform; } }

	void DrawHeight(Vector3 cen, Vector3 ext)
	{
		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// front left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),

			// front right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// back left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),

			// back right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.right * 20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.x < pos.x )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		Vector3 left = Vector3.Cross(cam.forward, Vector3.up).normalized * LineDistance();

		Handles.color = LightWhite;
		Handles.DrawLine(a + left * .1f, a + left);
		Handles.DrawLine(b + left * .1f, b + left);

		a += left;
		b += left;
		
		Handles.color = Color.green;
		Handles.DrawLine(a, b);

		Handles.BeginGUI();
		gc.text = Vector3.Distance(a,b).ToString("F2");
		pos.x -= EditorStyles.label.CalcSize(gc).x * 2f;
		DrawSceneLabel(gc, pos);

		Handles.EndGUI();
	}

	void DrawDepth(Vector3 cen, Vector3 ext)
	{
		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// bottom right
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

			// top right
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// bottom left
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),

			// top left
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.up * -20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.y > pos.y )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		float dot = Vector3.Dot(cam.transform.forward, Vector3.right);
		float sign = dot < 0f ? -1f : 1f;
		Vector3 offset = -(Vector3.up + (Vector3.right * sign)).normalized * LineDistance();

		Handles.color = LightWhite;
		Handles.DrawLine(a + offset * .1f, a + offset);
		Handles.DrawLine(b + offset * .1f, b + offset);

		a += offset;
		b += offset;

		Handles.color = Color.blue;
		Handles.DrawLine(a, b);

		Handles.BeginGUI();
		gc.text = Vector3.Distance(a,b).ToString("F2");
		// pos.x += EditorStyles.label.CalcSize(gc).x;
		pos.y += EditorStyles.label.CalcHeight(gc, 20000);
		DrawSceneLabel(gc, pos);

		Handles.EndGUI();
	}

	
	void DrawWidth(Vector3 cen, Vector3 extents)
	{
		Vector3 ext = extents;// + extents.normalized * .2f;

		// positibilities
		Vector3[] edges = new Vector3[8]
		{
			// bottom front
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z - ext.z),

			// bottom back
			new Vector3(cen.x - ext.x, cen.y - ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y - ext.y, cen.z + ext.z),
			
			// top front
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z - ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z - ext.z),

			// top back
			new Vector3(cen.x - ext.x, cen.y + ext.y, cen.z + ext.z),
			new Vector3(cen.x + ext.x, cen.y + ext.y, cen.z + ext.z)
		};

		// figure leftmost height boundary
		Vector2 pos = Vector2.up * -20000f;
		Vector3 a = Vector3.zero, b = Vector3.zero;

		for(int i = 0; i < edges.Length; i += 2)
		{
			Vector2 screen = HandleUtility.WorldToGUIPoint( (edges[i] + edges[i+1]) * .5f );

			if( screen.y > pos.y )
			{
				pos = screen;
				a = edges[i+0];
				b = edges[i+1];
			}
		}

		// Vector3 offset = -Vector3.up;
		// offset = -Vector3.Cross(Vector3.Cross(cam.forward, Vector3.up), cam.forward).normalized * LineDistance();

		float dot = Vector3.Dot(cam.transform.forward, Vector3.forward);
		float sign = dot < 0f ? -1f : 1f;
		Vector3 offset = -(Vector3.up + (Vector3.forward * sign)).normalized * LineDistance();


		Handles.color = LightWhite;
		Handles.DrawLine(a + offset * .1f, a + offset);
		Handles.DrawLine(b + offset * .1f, b + offset);

		a += offset;
		b += offset;

		Handles.color = Color.red;
		Handles.DrawLine(a, b);


		Handles.BeginGUI();
		DrawSceneLabel(Vector3.Distance(a,b).ToString("F2"), HandleUtility.WorldToGUIPoint((a + b) * .5f));
		Handles.EndGUI();
	}

	GUIContent gc = new GUIContent("", "");
	void DrawSceneLabel(string content, Vector2 position)
	{
		gc.text = content;
		DrawSceneLabel(gc, position);
	}

	void DrawSceneLabel(GUIContent content, Vector2 position)
	{
		float width = EditorStyles.label.CalcSize(content).x;
		float height = EditorStyles.label.CalcHeight(content, width) + 4;

		pb_GUI_Utility.DrawSolidColor( new Rect(position.x-1, position.y, width+2, height-2), background);
		GUI.Label( new Rect(position.x, position.y, width, height), content, EditorStyles.label );
	}
}
