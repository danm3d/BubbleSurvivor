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

    private GameObject selected;//object currently being placed
    private bool placing = false;//currently placing an object
    private bool upsideDown = false;//object being placed upside down
    private bool newMaze = false;//do you want to create a new maze or edit the maze
    private Vector2 mazeSize = new Vector2(20, 20);//size of the new maze

    public override void OnInspectorGUI()
    {
        //LayoutImage = (RawImage)EditorGUILayout.ObjectField (LayoutImage, typeof(RawImage), true);
        LayoutImage = GameObject.Find("Maze Layout Image").GetComponent<RawImage>();

        newMaze = EditorGUILayout.Toggle("New Maze", newMaze);

        if (newMaze)
        {
            mazeSize.x = Mathf.Floor(EditorGUILayout.Slider("Maze Width:", mazeSize.x, 5, 20));
            mazeSize.y = Mathf.Floor(EditorGUILayout.Slider("Maze Height:", mazeSize.y, 5, 20));

            mazeCreate.blockSize = EditorGUILayout.Slider("Block Size:", mazeCreate.blockSize, 0.01f, 5f);

            if (GUILayout.Button("Make New Maze"))
            {
                //MakeNewMaze ();
                MazeCreator.MazeSection[,] maze = mazeCreate.CreateMaze((int)mazeSize.x, (int)mazeSize.y);
                DestroyImmediate(mazeCreate.gameObject.GetComponent<MeshFilter>().sharedMesh);
                mazeCreate.gameObject.GetComponent<MeshFilter>().sharedMesh = mazeCreate.MakeMazeMesh(maze);
                mazeCreate.gameObject.GetComponent<MeshCollider>().sharedMesh = mazeCreate.gameObject.GetComponent<MeshFilter>().sharedMesh;
            }
            if (GUILayout.Button("Delete Maze"))
            {
                DeleteMaze();
            }
        } else
        {

            mazeCreate.mazeLayer = (MazeCreator.MazeLayer)EditorGUILayout.EnumPopup("Maze Layer", mazeCreate.mazeLayer);
            mazeCreate.extras = EditorGUILayout.Toggle("Extras", mazeCreate.extras);

            if (mazeCreate.mazeLayer == MazeCreator.MazeLayer.Wall)
            {
                if (mazeCreate.extras)
                {
                    mazeCreate.wallExtras = (MazeCreator.WallExtras)EditorGUILayout.EnumPopup("Wall Extra Type", mazeCreate.wallExtras);
                    if (GUILayout.Button("Place Wall Extra"))
                    {
                        CreateWallExtra();
                        //Selection.objects = new Object[]{selected};
                    }
                } else
                {
                    mazeCreate.wallSection = (MazeCreator.WallSectionType)EditorGUILayout.EnumPopup("Wall Section Type", mazeCreate.wallSection);
                    //mazeCreate.segmentType = (MazeCreator.WallSegmentType)EditorGUILayout.EnumPopup ("Wall Segment Type", mazeCreate.segmentType);
                    if (GUILayout.Button("Place Wall Section"))
                    {
                        CreateWallSection();
                        //Selection.objects = new Object[]{selected};
                    }
                }
            }

            if (mazeCreate.mazeLayer == MazeCreator.MazeLayer.Floor)
            {
                if (mazeCreate.extras)
                {

                } else
                {
                    mazeCreate.floorType = (MazeCreator.FloorType)EditorGUILayout.EnumPopup("Floor Section Type", mazeCreate.floorType);

                    if (GUILayout.Button("Place Floor Section"))
                    {
                        CreateFloorSection();
                    }
                }
            }

            GUILayout.Label("Placing: " + placing.ToString());
            //when the placement options have been changed, destroy old section and create new from options
            if (placing && GUI.changed)
            {
                DestroyImmediate(selected);
                CreateNewSection();
            }
        }
    }

    void OnSceneGUI()
    {
        if (placing)
        {
            Rotate();
            Position();
            DrawPlacementGrid();
            PlaceSection();
        }
        if (GUI.changed)
            EditorUtility.SetDirty(target);
        HandleUtility.Repaint();
    }

    private void DeleteMaze()
    {
        int count = mazeCreate.transform.childCount;
        for (int i = 0; i < count; i++)
        {
            DestroyImmediate(mazeCreate.transform.GetChild(0).gameObject);
        }
    }

	#region New Maze
    private void MakeNewMaze()
    {
        DeleteMaze();

        GameObject wallSectionsGroup = new GameObject("WallSectionsGroup");
        wallSectionsGroup.transform.SetParent(mazeCreate.transform);

        GameObject straightGroup = new GameObject("StraightGroup");
        straightGroup.transform.SetParent(wallSectionsGroup.transform);
        GameObject turnGroup = new GameObject("TurnGroup");
        turnGroup.transform.SetParent(wallSectionsGroup.transform);
        GameObject crossGroup = new GameObject("CrossGroup");
        crossGroup.transform.SetParent(wallSectionsGroup.transform);
        GameObject tjuncGroup = new GameObject("TJunctionGroup");
        tjuncGroup.transform.SetParent(wallSectionsGroup.transform);

        GameObject wallTorchGroup = new GameObject("WallTorchGroup");
        wallTorchGroup.transform.SetParent(mazeCreate.transform);
        GameObject trapGroup = new GameObject("TrapGroup");
        trapGroup.transform.SetParent(mazeCreate.transform);

        GameObject tombstoneGroup = new GameObject("TombstoneGroup");
        tombstoneGroup.transform.SetParent(trapGroup.transform);
        GameObject quicksandGroup = new GameObject("QuicksandGroup");
        quicksandGroup.transform.SetParent(trapGroup.transform);
        GameObject boulderGroup = new GameObject("BoulderGroup");
        boulderGroup.transform.SetParent(trapGroup.transform);
        GameObject trapdoorGroup = new GameObject("TrapdoorGroup");
        trapdoorGroup.transform.SetParent(trapGroup.transform);
        GameObject skeletonGrabGroup = new GameObject("SkeletonGrabGroup");
        skeletonGrabGroup.transform.SetParent(trapGroup.transform);

        GameObject floorGroup = new GameObject("FloorGroup");
        floorGroup.transform.SetParent(mazeCreate.transform);

        MazeCreator.MazeSection[,] maze = mazeCreate.CreateMaze((int)mazeSize.x, (int)mazeSize.y);
        Vector3 sectionRotation = Vector3.zero;
        for (int x = 0; x < (int)mazeSize.x; x++)
        {
            for (int y = 0; y < (int)mazeSize.y; y++)
            {
                if (maze [x, y] != null)
                {
                    //wall section
                    selected = Instantiate(mazeCreate.GetSectionPrefab(maze [x, y], ref sectionRotation),
				             new Vector3((float)x * 2f * mazeCreate.sectionSize.width, 0, 
				             (float)y * 2f * mazeCreate.sectionSize.height),
				            Quaternion.Euler(sectionRotation)) as GameObject;

                    if (mazeCreate.GetSectionPrefab(maze [x, y], ref sectionRotation) == mazeCreate.straight_Wall)
                        selected.transform.SetParent(straightGroup.transform);
                    if (mazeCreate.GetSectionPrefab(maze [x, y], ref sectionRotation) == mazeCreate.turn_Wall)
                        selected.transform.SetParent(turnGroup.transform);
                    if (mazeCreate.GetSectionPrefab(maze [x, y], ref sectionRotation) == mazeCreate.crossroad_Wall)
                        selected.transform.SetParent(crossGroup.transform);
                    if (mazeCreate.GetSectionPrefab(maze [x, y], ref sectionRotation) == mazeCreate.tJunction_Wall)
                        selected.transform.SetParent(tjuncGroup.transform);


                    selected = null;
                    //floor section
                    //TODO add correct floor section for wall section
                    //selected = Instantiate (mazeCreate.normal_Floor,
                    //                        new Vector3 ((float)x * 2f * mazeCreate.sectionSize.width, 0, 
                    //             (float)y * 2f * mazeCreate.sectionSize.height),
                    //                        Quaternion.Euler (sectionRotation)) as GameObject;
                    //selected.transform.SetParent(floorGroup.transform);
                    //selected = null;

                    // add torches
                    selected = Instantiate(mazeCreate.wallTorch,
                                            new Vector3((float)x * 2f * mazeCreate.sectionSize.width, 0, 
                                 (float)y * 2f * mazeCreate.sectionSize.height),
                                            Quaternion.Euler(sectionRotation)) as GameObject;
                    selected.transform.SetParent(wallTorchGroup.transform);
                    selected = null;

                    //traps
                    //50% chance to have a trap
                    if (Random.Range(0, 100) > 50)
                    {
                        GameObject trap, trapParent;//trap to spawn, parent of trap
                        int trapNum = Random.Range(0, 1000);
                        if (trapNum < 200)
                        {
                            trap = mazeCreate.boulder;
                            trapParent = boulderGroup;
                        } else if (trapNum < 300)
                        {
                            trap = mazeCreate.quicksand;
                            trapParent = quicksandGroup;
                        } else if (trapNum < 400)
                        {
                            trap = mazeCreate.tombstone;
                            trapParent = tombstoneGroup;
                        } else if (trapNum < 700)
                        {
                            trap = mazeCreate.skeletonGrab;
                            trapParent = skeletonGrabGroup;
                        } else
                        {
                            trap = mazeCreate.trapdoor;
                            trapParent = trapdoorGroup;
                        }

                        selected = Instantiate(trap,
                                            new Vector3((float)x * 2f * mazeCreate.sectionSize.width, 0, 
                                 (float)y * 2f * mazeCreate.sectionSize.height),
                                            Quaternion.Euler(sectionRotation)) as GameObject;
                        selected.transform.SetParent(trapParent.transform);
                        selected = null;
                    }
                    //TODO add random extras
                }
            }
        }
    }

	#endregion

	#region Walls
    private void CreateWallSection()
    {
        switch (mazeCreate.wallSection)
        {
            case MazeCreator.WallSectionType.Straight:
                SetSection(mazeCreate.straight_Wall);
                break;
            case MazeCreator.WallSectionType.StraightConnector:
                SetSection(mazeCreate.straightConnector_Wall);
                break;
            case MazeCreator.WallSectionType.Turn:
                SetSection(mazeCreate.turn_Wall);
                break;
            case MazeCreator.WallSectionType.TJunction:
                SetSection(mazeCreate.tJunction_Wall);
                break;
            case MazeCreator.WallSectionType.Stop:
                SetSection(mazeCreate.stopSection_Wall);
                break;
            case MazeCreator.WallSectionType.StopConnector:
                SetSection(mazeCreate.stopConnector_Wall);
                break;
            case MazeCreator.WallSectionType.Start:
                SetSection(mazeCreate.start_Wall);
                break;
        }
        //swap over to the next segment type on placement
        //mazeCreate.segmentType = mazeCreate.segmentType == MazeCreator.WallSegmentType.Segment ? MazeCreator.WallSegmentType.Conector : MazeCreator.WallSegmentType.Segment;
    }

    private void CreateWallExtra()
    {
        switch (mazeCreate.wallExtras)
        {
            case MazeCreator.WallExtras.SandEffect:
                break;
            case MazeCreator.WallExtras.Torch:
                break;
        }
    }

	#endregion

	#region Floors

    private void CreateFloorSection()
    {
        switch (mazeCreate.floorType)
        {
            case MazeCreator.FloorType.Normal:
                SetSection(mazeCreate.normal_Floor);
                break;
        }
    }

    private void CreateFloorExtra()
    {

    }

	#endregion

	#region General

    /// Creates a new section from the options selected.
    private void CreateNewSection()
    {
        if (mazeCreate.mazeLayer == MazeCreator.MazeLayer.Wall)
        {
            if (mazeCreate.extras)
            {
                CreateWallExtra();
            } else
            {
                CreateWallSection();
            }
        }
		
        if (mazeCreate.mazeLayer == MazeCreator.MazeLayer.Floor)
        {
            if (mazeCreate.extras)
            {
                CreateFloorExtra();
            } else
            {
                CreateFloorSection();
            }
        }
    }

    private void DrawPlacementGrid()
    {
        for (float x = -1; x < 2; x++)
        {
            for (float y = -1; y < 2; y++)
            {			
                Vector3 pos = new Vector3((mazeCreate.sectionSize.width * x), 0, (mazeCreate.sectionSize.width * y));
                Handles.SelectionFrame(0, selected.transform.position + pos, Quaternion.Euler(90, 0, 0), 3.81f);
            }
        }
        //Handles.SelectionFrame (0, selected.transform.position, Quaternion.Euler (90, 0, 0), 3.81f);
    }

    private void SetSection(GameObject original)
    {
        selected = Instantiate(original, Vector3.zero, Quaternion.identity) as GameObject;
        selected.transform.parent = mazeCreate.transform;
        placing = true;
    }

    private void Rotate()
    {
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.LeftArrow) 
                selected.transform.Rotate(0, 90, 0);			
            if (e.keyCode == KeyCode.RightArrow)
                selected.transform.Rotate(0, -90, 0);

            if (e.keyCode == KeyCode.UpArrow)
            {
                selected.transform.Rotate(0, 0, 180);
                upsideDown = !upsideDown;
            }
            if (e.keyCode == KeyCode.DownArrow)
            {
                selected.transform.Rotate(0, 0, -180);
                upsideDown = !upsideDown;
            }
        }
    }
    private Vector3 mousePos;
    private void Position()
    {
        if (e.type == EventType.MouseMove)
        {
            //get mouse position on screen
            //mousePos = SceneView.lastActiveSceneView.camera.ScreenToWorldPoint (new Vector3 (e.mousePosition.x, 0, Camera.current.pixelHeight - e.mousePosition.y));
            Ray ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                mousePos = hit.point;
            } else
            {
                mousePos = Vector3.zero;
            }
            //adjust position to snap to section size
            mousePos.y = upsideDown ? 5.48f : 0;
            mousePos.z = Mathf.Floor(mousePos.z / mazeCreate.sectionSize.width) * mazeCreate.sectionSize.width;
            mousePos.x = Mathf.Floor(mousePos.x / mazeCreate.sectionSize.height) * mazeCreate.sectionSize.height;
            //apply position
            selected.transform.position = mousePos;
        }
    }

    private void PlaceSection()
    {
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return)
            {
                selected = null;
                CreateNewSection();
            }
        }
    }
	#endregion
}
