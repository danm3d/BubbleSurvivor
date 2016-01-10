using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(MazeCreator))]
public class MazeCreator_Editor : Editor
{
    MazeCreator mazeCreate
    {
        get{ return (MazeCreator)target;}
    }

    Event e
    {
        get{ return Event.current;}
    }

    public RawImage LayoutImage
    {
        get
        {
            return mazeCreate.layoutImage;
        }
        set
        {
            mazeCreate.layoutImage = value;
        }
    }

    private bool mazeSettings, uvSettings;

    public override void OnInspectorGUI()
    {
        LayoutImage = GameObject.Find("Maze Layout Image").GetComponent<RawImage>();

        if (mazeSettings = EditorGUILayout.Foldout(mazeSettings, "Maze Settings"))
        {
            mazeCreate.mazeSize.x = Mathf.Floor(EditorGUILayout.Slider("Maze Width:", mazeCreate.mazeSize.x, 1, 20));
            mazeCreate.mazeSize.y = Mathf.Floor(EditorGUILayout.Slider("Maze Height:", mazeCreate.mazeSize.y, 1, 20));

            mazeCreate.blockSize = EditorGUILayout.Slider("Block Size:", mazeCreate.blockSize, 0.1f, 5f);
        }

        if (uvSettings = EditorGUILayout.Foldout(uvSettings, "UV Settings"))
        {
            var uvPosition = EditorGUILayout.Vector2Field("Position", mazeCreate.uvPosition);
            var uvScale = EditorGUILayout.FloatField("Scale", mazeCreate.uvScale);
            var uvRotation = EditorGUILayout.Slider("Rotation", mazeCreate.uvRotation, -180, 180) % 360;
            if (uvRotation < -180)
                uvRotation += 360;
            if (GUI.changed)
            {
                mazeCreate.uvPosition = uvPosition;
                mazeCreate.uvScale = uvScale;
                mazeCreate.uvRotation = (int)uvRotation;
                mazeCreate.RemakeUV();
            }
            if (GUILayout.Button("Reset UVs"))
            {
                mazeCreate.uvPosition = Vector3.zero;
                mazeCreate.uvScale = 1;
                mazeCreate.uvRotation = 0;
                mazeCreate.RemakeUV();
            }
        }

        if (GUILayout.Button("Make New Maze"))
        {
            MakeNewMaze();
        }
        if (GUILayout.Button("Delete Maze"))
        {
            DeleteMaze();
        }
    }

    private void DeleteMaze()
    {
        DestroyImmediate(mazeCreate.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh);
        mazeCreate.gameObject.GetComponent<PolygonCollider2D>().points = null;
    }

    private void MakeNewMaze()
    {
        MazeCreator.MazeSection[,] maze = mazeCreate.CreateMaze((int)mazeCreate.mazeSize.x, (int)mazeCreate.mazeSize.y);
        //DestroyImmediate(mazeCreate.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh);
        //mazeCreate.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh = mazeCreate.MakeMazeMesh(maze);
        mazeCreate.SetMazePoints();
    }

}
