using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

//[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class MazeCreator : MonoBehaviour
{
    //brick size width=381 height=182.88
    public Rect sectionSize = new Rect(0, 0, 7.62f, 7.62f);

	#region Wall section prefabs
    public GameObject straight_Wall, straightConnector_Wall;
    public GameObject turn_Wall;
    public GameObject stopSection_Wall, stopConnector_Wall;
    public GameObject tJunction_Wall;
    public GameObject start_Wall;
    public GameObject crossroad_Wall;

    public GameObject wallTorch, wallSandEffect;
	#endregion

	#region Floor section prefabs

    public GameObject normal_Floor;

	#endregion

    public enum MazeLayer
    {
        Wall = 0,
        Floor = 1
    }
    public MazeLayer mazeLayer;
    public bool extras = false;

	#region Wall layer

    public enum WallSectionType
    {
        Straight = 0,
        StraightConnector = 1,
        Turn = 2,
        Stop = 3,
        StopConnector = 4,
        TJunction = 5,
        Start = 6
    }
    public WallSectionType wallSection;

    public enum WallExtras
    {
        Torch = 0,
        SandEffect = 1
    }
    public WallExtras wallExtras;

	#endregion

//	public enum WallSegmentType
//	{
//		Segment = 0,
//		Conector = 1
//	}
//	public WallSegmentType segmentType;

	#region Floor layer

    public enum FloorType
    {
        Normal = 0,
        CenterHole = 1,
        CornerHole = 2
    }
    public FloorType floorType;

    public enum TrapType
    {
        SkeletonGrabber = 0,
        TrapDoor = 1,
        Boulder = 2,
        Tombstone = 3,
        Quicksand = 4
    }
    public TrapType trapType;

    public enum TrapPosition
    {
        Center = 0,
        Corner = 1
    }
    public TrapPosition trapPosition;

    public enum FloorExtras
    {
        Sand = 0,
        Planks = 1,
        Vegetation = 3
    }
    public FloorExtras floorExtras;

	#endregion

    //not used
    public enum TrapRotation
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    public GameObject quicksand, skeletonGrab, tombstone, boulder, trapdoor;

	#region Make New Maze

    /// Image used to display the maze image under the actual maze.
    public RawImage layoutImage;

    public class MazeSection
    {
        /// Image type used for this section.
        public int sectionType;
        /// Northern leading path.
        public bool north;
        /// Eastern leading path.
        public bool east;
        /// Southern leading path.
        public bool south;
        /// Western leading path.
        public bool west;
		
        public MazeSection()
        {
        }
		
        public MazeSection(int _sectionType, bool _north, bool _east, bool _south, bool _west)
        {
            sectionType = _sectionType;
            north = _north;
            east = _east;
            south = _south;
            west = _west;
        }
    }
    /// Array containing all of the section details.
    private MazeSection[] mazeSectionTypes = new MazeSection[11];
	
    private int GetSectionType(MazeSection section)
    {
        int val = -1;
        //regular sections
        for (int i = 0; i < 11; i++)
        {
            if (section.north == mazeSectionTypes [i].north && 
                section.east == mazeSectionTypes [i].east &&
                section.south == mazeSectionTypes [i].south &&
                section.west == mazeSectionTypes [i].west)
            {
                val = i;
                break;
            }
        }
        //dead end types
        if (val == -1)
        {
            if (section .north || section .south)
            {
                val = 2;
            } else if (section .east || section .west)
            {
                val = 1;
            }
        }
        return val;
    }

    //private float deadStart = 8;
    //private float deadEnd = 12;

    private void MazeDFS(ref MazeSection[,] maze, Vector2 pos, int arriveDirection)
    {
        //if (!(pos.x >= deadStart && pos.x <= deadEnd) || !(pos.y >= deadStart && pos.y <= deadEnd)) {
        maze [(int)pos.x, (int)pos.y] = new MazeSection();
        //set the direction it has been arrived from
        if (arriveDirection == 0)
        {
            maze [(int)pos.x, (int)pos.y].south = true;
        } else if (arriveDirection == 1)
        {
            maze [(int)pos.x, (int)pos.y].west = true;
        } else if (arriveDirection == 2)
        {
            maze [(int)pos.x, (int)pos.y].north = true;
        } else if (arriveDirection == 3)
        {
            maze [(int)pos.x, (int)pos.y].east = true;
        }
		
        bool[] visited = new bool[] {false, false, false, false};
        int current = Random.Range(0, 4);
        for (int i = 0; i < 4; i++)
        {
            //find the next unvisited tile
            while (visited [current] == true)
            {
                current = Random.Range(0, 4);
            }
            if (current == 0 && (int)pos.y + 1 < maze.GetLength(1) && maze [(int)pos.x, (int)pos.y + 1] == null)
            {
                //if (!(pos.y + 1 >= deadStart && pos.y + 1 <= deadEnd) || !(pos.x >= deadStart && pos.x <= deadEnd))
                maze [(int)pos.x, (int)pos.y].north = true;
                MazeDFS(ref maze, new Vector2(pos.x, pos.y + 1), current);
            } else if (current == 1 && (int)pos.x + 1 < maze.GetLength(0) && maze [(int)pos.x + 1, (int)pos.y] == null)
            {
                //if (!(pos.x + 1 >= deadStart && pos.x + 1 <= deadEnd) || !(pos.y >= deadStart && pos.y <= deadEnd))
                maze [(int)pos.x, (int)pos.y].east = true;
                MazeDFS(ref maze, new Vector2(pos.x + 1, pos.y), current);
            } else if (current == 2 && pos.y - 1 >= 0 && maze [(int)pos.x, (int)pos.y - 1] == null)
            {
                //if (!(pos.y - 1 >= deadStart && pos.y - 1 <= deadEnd) || !(pos.x >= deadStart && pos.x <= deadEnd))
                maze [(int)pos.x, (int)pos.y].south = true;
                MazeDFS(ref maze, new Vector2(pos.x, pos.y - 1), current);
            } else if (current == 3 && pos.x - 1 >= 0 && maze [(int)pos.x - 1, (int)pos.y] == null)
            {
                //if (!(pos.x - 1 >= deadStart && pos.x - 1 <= deadEnd) || !(pos.y >= deadStart && pos.y <= deadEnd))
                maze [(int)pos.x, (int)pos.y].west = true;
                MazeDFS(ref maze, new Vector2(pos.x - 1, pos.y), current);
            } 
            visited [current] = true;
        }
        //}
    }
	
    public MazeSection[,] CreateMaze(int width, int height)
    {
        SetSectionTypes();
        MazeSection[,] output = new MazeSection[width, height];
        MazeDFS(ref output, new Vector2(0, 0), 2);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (output [x, y] != null)
                    output [x, y].sectionType = GetSectionType(output [x, y]);
            }
        }
        SaveMazeImage(output);
        return output;
    }

    private void SetSectionTypes()
    {
        mazeSectionTypes [0] = new MazeSection(0, true, true, true, true);//crossroad
        mazeSectionTypes [1] = new MazeSection(1, false, true, false, true);//east west 
        mazeSectionTypes [2] = new MazeSection(2, true, false, true, false);//north south
        mazeSectionTypes [3] = new MazeSection(3, true, true, true, false);//east facing t-junction
        mazeSectionTypes [4] = new MazeSection(4, true, true, false, true);//north facing t-junction
        mazeSectionTypes [5] = new MazeSection(5, false, true, true, true);//south facing t-junction
        mazeSectionTypes [6] = new MazeSection(6, true, false, true, true);//west facing t-junction
        mazeSectionTypes [7] = new MazeSection(7, true, false, false, true);//north west bend
        mazeSectionTypes [8] = new MazeSection(8, false, false, true, true);//south west bend
        mazeSectionTypes [9] = new MazeSection(9, true, true, false, false);//north east bend
        mazeSectionTypes [10] = new MazeSection(10, false, true, true, false);//east south bend
    }

    public GameObject GetSectionPrefab(MazeSection section, ref Vector3 rotation)
    {
        if (section == null)
            return null;
        switch (section.sectionType)
        {
            case 0:
                rotation = Vector3.zero;
                return crossroad_Wall;
            case 1:
                rotation = Vector3.zero;
                return straight_Wall;
            case 2:
                rotation = new Vector3(0, 90, 0);
                return straight_Wall;
            case 3:
                rotation = new Vector3(0, 90, 0);
                return tJunction_Wall;
            case 4:
                rotation = new Vector3(0, 0, 0);
                return tJunction_Wall;
            case 5:
                rotation = new Vector3(0, 180, 0);
                return tJunction_Wall;
            case 6:
                rotation = new Vector3(0, -90, 0);
                return tJunction_Wall;
            case 7:
                rotation = new Vector3(0, 0, 0);
                return turn_Wall;
            case 8:
                rotation = new Vector3(0, -90, 0);
                return turn_Wall;
            case 9:
                rotation = new Vector3(0, 90, 0);
                return turn_Wall;
            case 10:
                rotation = new Vector3(0, 180, 0);
                return turn_Wall;
        }
        return null;
    }

    private void SaveMazeImage(MazeSection[,] maze)
    {
        Texture2D[] mazeSections = new Texture2D[11];
        for (int i = 0; i < 11; i++)
        {
            mazeSections [i] = Resources.Load<Texture2D>("Corridor Types/" + i.ToString());
        }

        Texture2D output = new Texture2D(400, 400);
        output.anisoLevel = 9;
        output.mipMapBias = -5;

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (maze [x, y] != null)
                    output.SetPixels(x * 16, y * 16, 16, 16, mazeSections [maze [x, y].sectionType].GetPixels());

            }
        }
        output.Apply();
        DestroyImmediate(layoutImage.texture);
        layoutImage.texture = output;
        System.IO.File.WriteAllBytes(Application.dataPath + "/" + "MazePicture.png", output.EncodeToPNG());
    }

    #region Maze mesh

    public float blockSize = 2f;

    public Mesh MakeMazeMesh(MazeSection[,] maze)
    {

        List<Vector3> newVert = new List<Vector3>();
        List<Vector2> newUV = new List<Vector2>();
        List<int> newTri = new List<int>();
        int vertCount = 0;
        float xVert = 0f, yVert = 0f;

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (maze [x, y] != null)
                {
                    xVert = (x * blockSize) * 2f;
                    yVert = (y * blockSize) * 2f;

                    vertCount = newVert.Count;
                    //all section types have the base center square
                    newVert.Add(new Vector3(xVert, 0, yVert));
                    newVert.Add(new Vector3(xVert, 0, yVert + blockSize));
                    newVert.Add(new Vector3(xVert + blockSize, 0, yVert + blockSize));
                    newVert.Add(new Vector3(xVert + blockSize, 0, yVert));

                    AddUVTri(ref newUV, ref newTri, vertCount);
                    //add additional squares according to directions of the section
                    if (maze [x, y].north)
                    {
                        vertCount = newVert.Count;
                        newVert.Add(new Vector3(xVert, 0, yVert + blockSize));
                        newVert.Add(new Vector3(xVert, 0, yVert + blockSize + blockSize));
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert + blockSize + blockSize));
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert + blockSize));
                        
                        AddUVTri(ref newUV, ref newTri, vertCount);
                    }
                    if (maze [x, y].east)
                    {
                        vertCount = newVert.Count;
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert));
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert + blockSize));
                        newVert.Add(new Vector3(xVert + blockSize + blockSize, 0, yVert + blockSize));
                        newVert.Add(new Vector3(xVert + blockSize + blockSize, 0, yVert));
                        
                        AddUVTri(ref newUV, ref newTri, vertCount);
                    }
                    if (maze [x, y].south)
                    {
                        vertCount = newVert.Count;
                        newVert.Add(new Vector3(xVert, 0, yVert - blockSize));
                        newVert.Add(new Vector3(xVert, 0, yVert));
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert));
                        newVert.Add(new Vector3(xVert + blockSize, 0, yVert - blockSize));
                        
                        AddUVTri(ref newUV, ref newTri, vertCount);
                    }
                    if (maze [x, y].west)
                    {
                        vertCount = newVert.Count;
                        newVert.Add(new Vector3(xVert - blockSize, 0, yVert));
                        newVert.Add(new Vector3(xVert - blockSize, 0, yVert + blockSize));
                        newVert.Add(new Vector3(xVert - blockSize + blockSize, 0, yVert + blockSize));
                        newVert.Add(new Vector3(xVert - blockSize + blockSize, 0, yVert));
                        
                        AddUVTri(ref newUV, ref newTri, vertCount);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = newVert.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.triangles = newTri.ToArray();
        mesh.subMeshCount = 1;
        mesh.SetTriangles(newTri.ToArray(), 0);
        mesh.RecalculateNormals();
        mesh.Optimize();

        return mesh;
    }

    private void AddUVTri(ref List<Vector2> uvList, ref List<int> triList, int vertCount)
    {
        uvList.Add(new Vector2(0, 0));
        uvList.Add(new Vector2(0, 1));
        uvList.Add(new Vector2(1, 1));
        uvList.Add(new Vector2(1, 0));
        //triangle 1
        triList.Add(vertCount);
        triList.Add(vertCount + 1);
        triList.Add(vertCount + 2);
        //triangle 2
        triList.Add(vertCount + 2);
        triList.Add(vertCount + 3);
        triList.Add(vertCount);
    }

    #endregion Make Maze Mesh

	#endregion

}
