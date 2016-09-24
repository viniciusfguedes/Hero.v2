using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(FlowBehaviour))]
public class FlowBehaviourEditor : Editor
{

	private static FlowBehaviour behaviour;

	//GUI
	private GUISkin interface_skin;
	private Texture interface_button_flowmesh;
	private Texture interface_disable_flowmesh;
	private Texture interface_button_paintmode;
	private Texture interface_disable_paintmode;
	private Texture interface_active_paintmode;

	private Texture interface_tool_direction;
	private Texture interface_tool_speed;
	private Texture interface_tool_blending;
	private Texture interface_label_tools;
	private Texture interface_label_magnitude;
	private Texture interface_label_opacity;
	private Texture interface_label_size;

	void OnEnable()
	{
		interface_skin = EditorGUIUtility.Load("Lava/Interface.guiskin") as GUISkin;
		interface_button_flowmesh = EditorGUIUtility.Load ("Lava/button_flowmesh_normal.png") as Texture;
		interface_disable_flowmesh = EditorGUIUtility.Load ("Lava/button_flowmesh_disable.png") as Texture;
		interface_button_paintmode = EditorGUIUtility.Load ("Lava/button_paintmode_normal.png") as Texture;
		interface_disable_paintmode = EditorGUIUtility.Load ("Lava/button_paintmode_disable.png") as Texture;
		interface_active_paintmode = EditorGUIUtility.Load ("Lava/button_paintmode_active.png") as Texture;

		interface_tool_direction = EditorGUIUtility.Load ("Lava/paint_tool_direction.png") as Texture;
		interface_tool_speed = EditorGUIUtility.Load ("Lava/paint_tool_speed.png") as Texture;
		interface_tool_blending = EditorGUIUtility.Load ("Lava/paint_tool_blend.png") as Texture;
		interface_label_tools = EditorGUIUtility.Load ("Lava/label_tools.png") as Texture;
		interface_label_opacity = EditorGUIUtility.Load ("Lava/label_opacity.png") as Texture;
		interface_label_magnitude = EditorGUIUtility.Load ("Lava/label_magnitude.png") as Texture;
		interface_label_size = EditorGUIUtility.Load ("Lava/label_size.png") as Texture;

		behaviour = (FlowBehaviour)target;
		//Initialize
		behaviour.IsBroken = !Init();
	}

	public bool Init()
	{
		behaviour.Filter = behaviour.GetComponent<MeshFilter>();
		if(behaviour.Filter == null)
		{
			return false;
		}
		if(behaviour.Filter.sharedMesh == null)
		{
			return false;
		}
		if(behaviour.procedural == null)
		{
			return false;
		}

		behaviour.Filter.sharedMesh = behaviour.procedural;

		//Rebuild FluidArray
		if(behaviour.Fluids == null || behaviour.Fluids.Length != behaviour.Filter.sharedMesh.vertices.Length)
		{
			behaviour.Fluids = new FluidVertice[behaviour.Filter.sharedMesh.vertices.Length];
		}
        Color32 color32;
        for (int i = 0; i < behaviour.Fluids.Length; i++)
        {
            behaviour.Fluids[i].world = behaviour.transform.TransformPoint(behaviour.Filter.sharedMesh.vertices[i]);
            color32 = behaviour.Filter.sharedMesh.colors32[i];
            behaviour.Fluids[i].fluidVector = new Vector3(color32.r/255f - 0.5f, 0, color32.g/255f - 0.5f) * 2;
            behaviour.Fluids[i].magnitude = color32.b/255f;
            behaviour.Fluids[i].blending = color32.a/255f;
        }
        return true;
	}

	void OnDisable(){
		if(behaviour.IsPaintEditing)
		{
			behaviour.PainterUsing = false;
			behaviour.PainterDirection = Vector3.zero;
			behaviour.PainterLast = Vector3.zero;
			FinishVectors();
		}
	}

	private static Vector2 _scroll1 = Vector2.zero;
	private static Vector2 _scroll2 = Vector2.zero;
	private static Rect it_flowmesh = new Rect (0, 0, 128, 58);
	private static Rect it_paintmode = new Rect (it_flowmesh.xMax + 20, 0, 128, 58);

	private static Rect lb_tools = new Rect(0,5, 128, 60);
	private static Rect it_tools = new Rect(lb_tools.xMax,lb_tools.yMin-5,200,70);
	private static Rect it_tools_button_1 = new Rect(it_tools.xMin+25,it_tools.yMin+15, 50, 35);
	private static Rect it_tools_button_2 = new Rect(it_tools_button_1.xMax,it_tools.yMin+15, 50, 35);
	private static Rect it_tools_button_3 = new Rect(it_tools_button_2.xMax,it_tools.yMin+15, 50, 35);

	private static Rect lb_size = new Rect(0,it_tools.yMax,96,45);
	private static Rect sl_size = new Rect(lb_size.xMax,lb_size.yMin+15,200,20);
    private static Rect fl_size = new Rect(sl_size.xMax + 10, sl_size.yMin - 2, 60, 20);

	private static Rect lb_magnitude = new Rect(0,lb_size.yMax,96,45);
	private static Rect sl_magnitude = new Rect(lb_magnitude.xMax,lb_magnitude.yMin+15,200,20);
    private static Rect fl_magnitude = new Rect(sl_magnitude.xMax + 10, sl_magnitude.yMin - 2, 60, 20);

    private static Rect lb_opacity = new Rect(0,lb_magnitude.yMax,96,45);
	private static Rect sl_opacity = new Rect(lb_opacity.xMax,lb_opacity.yMin+5,200,20);
    private static Rect fl_opacity = new Rect(sl_opacity.xMax + 10, sl_opacity.yMin - 2, 60, 20);
    private static Rect sl_opacitycurve = new Rect(sl_opacity.xMin, sl_opacity.yMin+20, 200, 20);

    private static Rect lb_preview = new Rect(35, lb_opacity.yMax+10, 300, 36);

    public override void OnInspectorGUI()
	{
		GUI.skin = interface_skin;
		base.OnInspectorGUI ();
		if(behaviour.IsBroken)
		{
			GUILayout.BeginScrollView(_scroll1,GUILayout.Height(it_flowmesh.yMax));
			if(GUI.Button(it_flowmesh,interface_button_flowmesh))
			{
				if(behaviour.Filter != null)
				{
					if(behaviour.Filter.sharedMesh != null)
					{
						if(AssetDatabase.LoadAssetAtPath<Mesh>("Assets/" + behaviour.Filter.sharedMesh.name+"_Flow"+".asset") == null)
						{
							Mesh procedural = new Mesh();
							procedural.name = behaviour.Filter.sharedMesh.name + "_Flow";
							procedural.vertices = behaviour.Filter.sharedMesh.vertices;
							procedural.triangles = behaviour.Filter.sharedMesh.triangles;
                            procedural.normals = behaviour.Filter.sharedMesh.normals;
                            procedural.tangents = behaviour.Filter.sharedMesh.tangents;
                            procedural.uv = behaviour.Filter.sharedMesh.uv;
							procedural.uv2 = behaviour.Filter.sharedMesh.uv2;
                            Color32[] colors32 = new Color32[procedural.vertices.Length];
                            for(int i = 0; i < colors32.Length; i++) { colors32[i] = new Color32(127, 127, 255, 255); }
                            procedural.colors32 = colors32;
                            MeshUtility.SetMeshCompression(procedural, ModelImporterMeshCompression.Off);
                            MeshUtility.Optimize(procedural);

							AssetDatabase.CreateAsset(procedural,"Assets/" + behaviour.Filter.sharedMesh.name+"_Flow"+".asset");
							AssetDatabase.SaveAssets();
							AssetDatabase.Refresh();

							behaviour.procedural = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/" + behaviour.Filter.sharedMesh.name + "_Flow" + ".asset");
                            //behaviour.procedural = FlowIO.ExportObject(behaviour.gameObject);

                            OnEnable();
						}
					}
				}
			}
			GUI.Box (it_paintmode, interface_disable_paintmode);
			GUILayout.EndScrollView ();
		}
		if(!behaviour.IsBroken)
		{
			if(!behaviour.IsPaintEditing)
			{
				GUILayout.BeginScrollView(_scroll1,GUILayout.Height(it_flowmesh.yMax));
				GUI.Box(it_flowmesh,interface_disable_flowmesh);
				if(GUI.Button(it_paintmode, interface_button_paintmode))
				{
					behaviour.IsPaintEditing = true;
					OnEnablePaintEditing();
				}
				GUILayout.EndScrollView();
			}
			else
			{
				GUILayout.BeginScrollView(_scroll1,GUILayout.Height(it_flowmesh.yMax));
				GUI.Box(it_flowmesh,interface_disable_flowmesh);
				if(GUI.Button(it_paintmode, interface_active_paintmode))
				{
					behaviour.IsPaintEditing = false;
					OnDisablePaintEditing();
				}
				GUILayout.EndScrollView();
				GUITools();
			}

		}
	}

	public void GUITools()
	{
		GUILayout.BeginScrollView(_scroll2,GUILayout.Height(lb_preview.yMax));

		GUI.Box(lb_tools, interface_label_tools);

		if(behaviour.PaintTool == PaintTool.DIRECTION){ GUI.Box(it_tools,interface_tool_direction); }
		else if(behaviour.PaintTool == PaintTool.SPEED){ GUI.Box(it_tools,interface_tool_speed); }
		else if(behaviour.PaintTool == PaintTool.BLEND){ GUI.Box(it_tools,interface_tool_blending); }

		if(GUI.Button(it_tools_button_1,"",GUI.skin.label)){ behaviour.PaintTool = PaintTool.DIRECTION; }
		else if(GUI.Button(it_tools_button_2,"",GUI.skin.label)){ behaviour.PaintTool = PaintTool.SPEED; }
		else if(GUI.Button(it_tools_button_3,"",GUI.skin.label)){ behaviour.PaintTool = PaintTool.BLEND; }

		GUI.Box(lb_size, interface_label_size);
        behaviour.PainterSize = EditorGUI.FloatField(fl_size, behaviour.PainterSize);
        behaviour.PainterSize = GUI.HorizontalSlider (sl_size, behaviour.PainterSize, 0.5f, 100f);

		GUI.Box(lb_magnitude, interface_label_magnitude);
        behaviour.PainterMagnitude = EditorGUI.FloatField(fl_magnitude, behaviour.PainterMagnitude);
        behaviour.PainterMagnitude = GUI.HorizontalSlider (sl_magnitude, behaviour.PainterMagnitude, 0.01f, 1);

		GUI.Box(lb_opacity, interface_label_opacity);
        behaviour.PainterOpacity = EditorGUI.FloatField(fl_opacity, behaviour.PainterOpacity);
        behaviour.PainterOpacity = GUI.HorizontalSlider (sl_opacity, behaviour.PainterOpacity, 0, 1);
        behaviour.PainterCurve = EditorGUI.CurveField(sl_opacitycurve, behaviour.PainterCurve);

        behaviour.IsPreviewing = GUI.Toggle(lb_preview, behaviour.IsPreviewing, new GUIContent("Preview"));
        GUILayout.EndScrollView ();
	}

	public void FinishVectors()
	{
        Undo.RecordObject(behaviour.procedural,"Paint Mesh");
        Color32[] colors32 = new Color32[behaviour.Fluids.Length];
		for(int i=0;i<behaviour.Fluids.Length;i++)
		{
			colors32[i] = new Color32(
				(byte)(127.5f + behaviour.Fluids[i].fluidVector.x * 127.5f),
				(byte)(127.5f + behaviour.Fluids[i].fluidVector.z * 127.5f),
				(byte)(255 * behaviour.Fluids[i].magnitude),
				(byte)(255 * behaviour.Fluids[i].blending)
			);
		}
		behaviour.procedural.colors32 = colors32;
		EditorUtility.SetDirty(behaviour.procedural);
		AssetDatabase.SaveAssets();
	}

	private static Tool previousTool;

	void OnEnablePaintEditing()
	{
		SceneView.onSceneGUIDelegate = OnPaintEditing;
		behaviour.IsPaintEditing = true;
        behaviour.IsPreviewing = false;
        behaviour.PaintCollider = behaviour.gameObject.AddComponent<MeshCollider>();
		behaviour.PaintCollider.sharedMesh = behaviour.Filter.sharedMesh;
		previousTool = Tools.current;

        Tools.current = Tool.View;
    }

	void OnDisablePaintEditing()
	{
		SceneView.onSceneGUIDelegate = null;
		behaviour.IsPaintEditing = false;
		if(behaviour.PaintCollider != null){
			DestroyImmediate (behaviour.PaintCollider);
			behaviour.PaintCollider = null;
		}
		FinishVectors();
		Tools.current = previousTool;
        GUIUtility.ExitGUI();
	}

	void OnPaintEditing(SceneView sceneView)
	{
		if(sceneView.camera == null) return;

		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.RightBracket)
		{
			behaviour.PainterSize += 1f;
			if(behaviour.PainterSize >= 100)
			{
				behaviour.PainterSize = 100;
			}
			Event.current.Use();
		}
		if(Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.LeftBracket){
			behaviour.PainterSize -= 1f;
			if(behaviour.PainterSize < 0.5f)
			{
				behaviour.PainterSize = 0.5f;
			}
			Event.current.Use();
		}

		if(Event.current.type == EventType.MouseUp && Event.current.button == 0 && behaviour.PainterUsing)
		{
			behaviour.PainterUsing = false;
			behaviour.PainterDirection = Vector3.zero;
			behaviour.PainterLast = Vector3.zero;
			FinishVectors();
			Event.current.Use();
            GUIUtility.ExitGUI();
            return;
        }

		RaycastHit[] hits = Physics.RaycastAll(sceneView.camera.ScreenPointToRay(new Vector3 (Event.current.mousePosition.x, -Event.current.mousePosition.y + sceneView.camera.pixelHeight)));
		for(int i=0;i<hits.Length;i++)
		{
			if(hits[i].collider == behaviour.PaintCollider)
			{
				if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					behaviour.PainterDirection = Vector3.zero;
					behaviour.PainterLast = hits [i].point;
					behaviour.AvgDirection.Clear();
					Event.current.Use();
				}
				if(Event.current.type == EventType.MouseDrag && Event.current.button == 0)
				{
					behaviour.PainterDirection = hits[i].point - behaviour.PainterLast;
					behaviour.PainterDirection.y = 0;
					if(behaviour.PainterDirection.sqrMagnitude > 0.0004f) //o.o2 * o.o2
					{
						behaviour.AvgDirection.Enqueue(behaviour.PainterDirection.normalized);
						behaviour.PainterDirection = Vector3.zero;
						if(behaviour.AvgDirection.Count > 5 || behaviour.PaintTool != PaintTool.DIRECTION)
						{
							foreach(var vec in behaviour.AvgDirection)
							{
								behaviour.PainterDirection += vec;
							}
							behaviour.PainterDirection.Normalize();
							behaviour.AvgDirection.Dequeue();
							behaviour.PainterUsing = true;
						}
						behaviour.PainterLast = hits[i].point;
					}
					else
					{
						behaviour.PainterUsing = false;
						behaviour.PainterDirection = Vector3.zero;
					}
					Event.current.Use();
				}
				DrawTool(hits[i].point,hits[i].normal, behaviour.PainterSize, behaviour.PainterUsing, behaviour.PainterDirection, behaviour.PainterMagnitude, behaviour.PainterOpacity);
				sceneView.Repaint();
				return;
			}
		}
        //Missing Collider?
        if(behaviour.PainterUsing)
		{
            behaviour.PainterUsing = false;
            behaviour.PainterDirection = Vector3.zero;
            behaviour.PainterLast = Vector3.zero;
            FinishVectors();
            Event.current.Use();
            GUIUtility.ExitGUI();
            return;
        }
        DrawDefaultMesh();
	}

	void DrawTool(Vector3 position, Vector3 normal, float size, bool paiting, Vector3 direction, float magnitude, float opacity)
	{
        //UpdateMesh
		if(paiting)
		{
            float falloff = 0;
			for (int i = 0; i < behaviour.Fluids.Length; i++)
			{
                falloff =  1 - Vector3.Distance(behaviour.Fluids[i].world, position) / size;
                if (falloff > 0)
				{
					if(behaviour.PaintTool == PaintTool.DIRECTION)
					{
						behaviour.Fluids[i].fluidVector = Vector3.Lerp(behaviour.Fluids[i].fluidVector,direction,opacity * falloff);
					}
					else if(behaviour.PaintTool == PaintTool.SPEED)
					{
						behaviour.Fluids[i].magnitude = Mathf.Lerp (behaviour.Fluids[i].magnitude, magnitude, opacity * falloff);
					}
					else if(behaviour.PaintTool == PaintTool.BLEND)
					{
						behaviour.Fluids[i].blending = Mathf.Lerp (behaviour.Fluids[i].blending, magnitude, opacity * falloff);
					}
					Handles.color = Color.green;
                    if (behaviour.IsPreviewing)
                    {
                        Handles.ArrowCap(0, behaviour.Fluids[i].world + Vector3.up * 0.1f, Quaternion.LookRotation(behaviour.Fluids[i].fluidVector), behaviour.PainterSize / 2);
                    }
                }
				else
				{
					Handles.color = Color.blue;
				}
            }
		}
        else if (behaviour.IsPreviewing)
        {
            Handles.color = Color.blue;
            //DrawMesh
            for(int i=0;i< behaviour.Fluids.Length;i++)
			{
                Handles.DrawLine(behaviour.Fluids[i].world + Vector3.up * 0.1f, behaviour.Fluids[i].world + Vector3.up * 0.1f + behaviour.Fluids[i].fluidVector);
                //Handles.ArrowCap(0, behaviour.Fluids[i].world + Vector3.up * 0.1f, Quaternion.LookRotation(behaviour.Fluids[i].fluidVector), behaviour.PainterSize);
            }
        }
		//Draw Tool
		Handles.color = Color.yellow * 0.9f;
		Handles.DrawWireDisc(position, normal, size);
	}

	void DrawDefaultMesh()
	{
		/*for(int i=0;i<_behaviour._fluids.Length;i++)
		{
			Handles.color = color_default;
				//color_default * 
				//new Color(_behaviour._fluids [i].magnitude,_behaviour._fluids [i].magnitude,_behaviour._fluids [i].magnitude,1);
			Handles.ArrowCap(0,_behaviour._fluids[i].world,Quaternion.LookRotation(_behaviour._fluids[i].fluidVector),0.5f);
			//Gizmos.DrawSphere (_behaviour._fluids [i].world, 1);
		}*/
	}
}
