using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class HighscoreManager : MonoBehaviour
{
	[Serializable]
	private class HighscoreTable
	{
		private float[] bestTimes = new float[10];
		private int gameOverCounter = 0;

		public bool RecordValue(float newTime)
		{
			int pos = -1;
			//find out if the new score is better(larger) than any previous record.
			for (int i = 0; i < 10; i++)
			{
				if (newTime > bestTimes[i])
				{
					pos = i;
					break;
				}
			}
			if (pos > -1)
			{
				float temp1 = 0f;
				float temp2 = 0f;
				temp1 = bestTimes[pos];
				bestTimes[pos] = newTime;
				for (int i = pos + 1; i < 10; i++)
				{
					temp2 = bestTimes[i];
					bestTimes[i] = temp1;
					temp1 = temp2;
				}

				return true;
			}
			else
				return false;
		}

		public override string ToString()
		{
			string output = "";
			for (int i = 0; i < 10; i++)
			{
				output += "\n" + (i + 1).ToString() + ". " + bestTimes[i].ToString("n2");
			}
			return output;
		}

		public bool PlayAds()
		{
			gameOverCounter++;
			if (gameOverCounter >= 4)
			{
				gameOverCounter = 0;
				return true;
			}
			else
				return false;
		}
	}

	private HighscoreTable highscoreTable;

	private void Start()
	{
		LoadScores();
	}

	private void OnDestroy()
	{
		SaveScores();
	}

	private void LoadScores()
	{
		string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Highscores.hs";
		if (File.Exists(fileName))
		{
			BinaryFormatter bf = new BinaryFormatter();
			using (FileStream fileStr = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				highscoreTable = (HighscoreTable)bf.Deserialize(fileStr);
			}
		}
		else
		{
			highscoreTable = new HighscoreTable();
		}
	}

	private void SaveScores()
	{
		string fileName = Application.persistentDataPath + @"/BubbleSurvivor/Highscores.hs";
		BinaryFormatter bf = new BinaryFormatter();
		Directory.CreateDirectory(Application.persistentDataPath + @"/BubbleSurvivor");
		using (FileStream fileStr = new FileStream(fileName, FileMode.Create, FileAccess.Write))
		{
			bf.Serialize(fileStr, highscoreTable);
		}
	}

	public bool RecordValue(float newTime)
	{
		return highscoreTable.RecordValue(newTime);
	}

	public string GetScores()
	{
		return highscoreTable.ToString();
	}

	public bool ShowAds()
	{
		return highscoreTable.PlayAds();
	}
}