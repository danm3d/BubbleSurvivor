using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.UI;

//[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
public class MazeCreator : MonoBehaviour
{
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

    private void MazeDFS(ref MazeSection[,] maze, Vector2 pos, int arriveDirection)
    {
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
                maze [(int)pos.x, (int)pos.y].north = true;
                MazeDFS(ref maze, new Vector2(pos.x, pos.y + 1), current);
            } else if (current == 1 && (int)pos.x + 1 < maze.GetLength(0) && maze [(int)pos.x + 1, (int)pos.y] == null)
            {
                maze [(int)pos.x, (int)pos.y].east = true;
                MazeDFS(ref maze, new Vector2(pos.x + 1, pos.y), current);
            } else if (current == 2 && pos.y - 1 >= 0 && maze [(int)pos.x, (int)pos.y - 1] == null)
            {
                maze [(int)pos.x, (int)pos.y].south = true;
                MazeDFS(ref maze, new Vector2(pos.x, pos.y - 1), current);
            } else if (current == 3 && pos.x - 1 >= 0 && maze [(int)pos.x - 1, (int)pos.y] == null)
            {
                maze [(int)pos.x, (int)pos.y].west = true;
                MazeDFS(ref maze, new Vector2(pos.x - 1, pos.y), current);
            } 
            visited [current] = true;
        }
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
    public Vector2 mazeSize = new Vector2(5, 5);//size of the new maze
    private List<Vector3> newVert = new List<Vector3>();
    private List<int> newTri = new List<int>();
    private List<List<Vector2>> polyPoints = new List<List<Vector2>>();
    public float uvScale = 1;
    public Vector2 uvPosition;
    public int uvRotation;

    public Mesh MakeMazeMesh(MazeSection[,] maze)
    {
        newVert = new List<Vector3>();
        newTri = new List<int>();
        float xVert = 0f, yVert = 0f;
        polyPoints = new List<List<Vector2>>();

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (maze [x, y] != null)
                {
                    xVert = (x * blockSize) * 2f;
                    yVert = (y * blockSize) * 2f;

                    //all section types have the base center square
                    AddQuad(xVert, yVert);

                    //add additional squares according to directions of the section
                    if (maze [x, y].north)
                    {
                        AddQuad(xVert, yVert + blockSize);
                    }
                    if (maze [x, y].east)
                    {
                        AddQuad(xVert + blockSize, yVert);
                    }

                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = newVert.ToArray();
        mesh.uv = AddUV();
        mesh.triangles = newTri.ToArray();
        mesh.subMeshCount = 1;
        mesh.SetTriangles(newTri.ToArray(), 0);
        mesh.RecalculateNormals();
        mesh.Optimize();

        return mesh;
    }

    private void AddQuad(float xBase, float yBase)
    {
        if (!newVert.Contains(new Vector3(xBase, 0, yBase)))
        {
            newVert.Add(new Vector3(xBase, 0, yBase));
        }
        if (!newVert.Contains(new Vector3(xBase, 0, yBase + blockSize)))
        {
            newVert.Add(new Vector3(xBase, 0, yBase + blockSize));
        }
        if (!newVert.Contains(new Vector3(xBase + blockSize, 0, yBase + blockSize)))
        {
            newVert.Add(new Vector3(xBase + blockSize, 0, yBase + blockSize));
        }
        if (!newVert.Contains(new Vector3(xBase + blockSize, 0, yBase)))
        {
            newVert.Add(new Vector3(xBase + blockSize, 0, yBase));
        }

        List<Vector2> quadPoints = new List<Vector2>();
        quadPoints.Add(new Vector2(xBase, yBase));
        quadPoints.Add(new Vector2(xBase, yBase + blockSize));
        quadPoints.Add(new Vector2(xBase + blockSize, yBase + blockSize));
        quadPoints.Add(new Vector2(xBase + blockSize, yBase));
        quadPoints.Add(new Vector2(xBase, yBase));

        if (!polyPoints.Contains(quadPoints))
            polyPoints.Add(quadPoints);

        AddTri(xBase, yBase);
    }

    private void AddTri(float xBase, float yBase)
    {
        //triangle 1
        int index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase, 0, yBase);
        }
        );
        newTri.Add(index);
        
        index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase, 0, yBase + blockSize);
        }
        );
        newTri.Add(index);
        
        index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase + blockSize, 0, yBase + blockSize);
        }
        );
        newTri.Add(index);

        //triangle 2
        index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase + blockSize, 0, yBase + blockSize);
        }
        );
        newTri.Add(index);
        
        index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase + blockSize, 0, yBase);
        }
        );
        newTri.Add(index);
        
        index = newVert.FindIndex(
            delegate(Vector3 obj)
        {
            return obj == new Vector3(xBase, 0, yBase);
        }
        );
        newTri.Add(index);
    }

    private Vector2[] AddUV()
    {
        var scale = uvScale != 0 ? (1 / uvScale) : 0;
        var matrix = Matrix4x4.TRS(new Vector3(-uvPosition.x, 0, -uvPosition.y), Quaternion.Euler(0, uvRotation, 0), new Vector3(scale * blockSize, scale * blockSize, scale));
        var uv = new Vector2[newVert.Count];
        for (int i = 0; i < uv.Length; i++)
        {
            var p = matrix.MultiplyPoint(newVert [i]);
            uv [i] = new Vector2(p.x, p.z);
        }
        return uv;
    }

    public void RemakeUV()
    {
        GetComponentInChildren<MeshFilter>().sharedMesh.uv = AddUV();
    }

    public void SetMazePoints()
    {
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.pathCount = polyPoints.Count;
        for (int i = 0; i < polyPoints.Count; i++)
        {
            polyCollider.SetPath(i, polyPoints [i].ToArray());
        }
    }

    #endregion Make Maze Mesh

}
