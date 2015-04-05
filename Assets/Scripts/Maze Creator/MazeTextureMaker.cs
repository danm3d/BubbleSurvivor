using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MazeTextureMaker : MonoBehaviour
{
	public RawImage guiImage;
	public Texture2D[] mazeSections = new Texture2D[11];

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

		public MazeSection ()
		{
		}

		public MazeSection (int _sectionType, bool _north, bool _east, bool _south, bool _west)
		{
			sectionType = _sectionType;
			north = _north;
			east = _east;
			south = _south;
			west = _west;
		}
	}
	/// Array containing all of the section details.
	public MazeSection[] mazeSectionTypes = new MazeSection[11];

	void Start ()
	{
		mazeSectionTypes [0] = new MazeSection (0, true, true, true, true);//crossroad
		mazeSectionTypes [1] = new MazeSection (1, false, true, false, true);//east west 
		mazeSectionTypes [2] = new MazeSection (2, true, false, true, false);//north south
		mazeSectionTypes [3] = new MazeSection (3, true, true, true, false);//east facing t-junction
		mazeSectionTypes [4] = new MazeSection (4, true, true, false, true);//north facing t-junction
		mazeSectionTypes [5] = new MazeSection (5, false, true, true, true);//south facing t-junction
		mazeSectionTypes [6] = new MazeSection (6, true, false, true, true);//west facing t-junction
		mazeSectionTypes [7] = new MazeSection (7, true, false, false, true);//north west bend
		mazeSectionTypes [8] = new MazeSection (8, false, false, true, true);//south west bend
		mazeSectionTypes [9] = new MazeSection (9, true, true, false, false);//north east bend
		mazeSectionTypes [10] = new MazeSection (10, false, true, true, false);//east south bend
	}

	private int GetSectionType (MazeSection section)
	{
		int val = -1;
		//regular sections
		for (int i = 0; i < 11; i++) {
			if (section.north == mazeSectionTypes [i].north && 
				section.east == mazeSectionTypes [i].east &&
				section.south == mazeSectionTypes [i].south &&
				section.west == mazeSectionTypes [i].west) {
				val = i;
			}
		}
		//dead end types
		if (val == -1) {
			if (section .north || section .south) {
				val = 2;
			} else if (section .east || section .west) {
				val = 1;
			}
		}
		return val;
	}

	private void MazeDFS (ref MazeSection[,] maze, Vector2 pos, int arriveDirection)
	{
		maze [(int)pos.x, (int)pos.y] = new MazeSection ();
		//set the direction it has been arrived from
		if (arriveDirection == 0) {
			maze [(int)pos.x, (int)pos.y].south = true;
		} else if (arriveDirection == 1) {
			maze [(int)pos.x, (int)pos.y].west = true;
		} else if (arriveDirection == 2) {
			maze [(int)pos.x, (int)pos.y].north = true;
		} else if (arriveDirection == 3) {
			maze [(int)pos.x, (int)pos.y].east = true;
		}

		bool[] visited = new bool[] {false, false, false, false};
		int current = Random.Range (0, 4);
		for (int i = 0; i < 4; i++) {
			//find the next unvisited tile
			while (visited [current] == true) {
				current = Random.Range (0, 4);
			}
			if (current == 0 && (int)pos.y + 1 < maze.GetLength (1) && maze [(int)pos.x, (int)pos.y + 1] == null) {
				maze [(int)pos.x, (int)pos.y].north = true;
				MazeDFS (ref maze, new Vector2 (pos.x, pos.y + 1), current);
			} else if (current == 1 && (int)pos.x + 1 < maze.GetLength (0) && maze [(int)pos.x + 1, (int)pos.y] == null) {
				maze [(int)pos.x, (int)pos.y].east = true;
				MazeDFS (ref maze, new Vector2 (pos.x + 1, pos.y), current);
			} else if (current == 2 && pos.y - 1 >= 0 && maze [(int)pos.x, (int)pos.y - 1] == null) {
				maze [(int)pos.x, (int)pos.y].south = true;
				MazeDFS (ref maze, new Vector2 (pos.x, pos.y - 1), current);
			} else if (current == 3 && pos.x - 1 >= 0 && maze [(int)pos.x - 1, (int)pos.y] == null) {
				maze [(int)pos.x, (int)pos.y].west = true;
				MazeDFS (ref maze, new Vector2 (pos.x - 1, pos.y), current);
			} 
			visited [current] = true;
		}
	}

	public MazeSection[,] CreateMaze (int width, int height)
	{
		MazeSection[,] output = new MazeSection[width, height];
		MazeDFS (ref output, new Vector2 (5, 0), 2);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				output [x, y].sectionType = GetSectionType (output [x, y]);
			}
		}
		return output;
	}
	
	public Texture2D MakeMazeTex ()
	{
		int xSize = 20;
		int ySize = 20;
		MazeSection[,] maze = CreateMaze (xSize, ySize);
		Texture2D output = new Texture2D (400, 400);
		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {
				if (maze [x, y] != null && maze [x, y].sectionType != -1) {
					output.SetPixels (x * 16, y * 16, 16, 16, mazeSections [maze [x, y].sectionType].GetPixels ());
				}
			}
		}
		output.Apply ();
		return output;
	}

	public void SetMazeTexture ()
	{
		guiImage.texture = null;
		guiImage.texture = MakeMazeTex ();
	}

}
