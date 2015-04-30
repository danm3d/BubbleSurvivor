using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class LevelUnlockManager : MonoBehaviour
{

    [Serializable]
    public class ArenaUnlock
    {
        private float cost;//cost to unlock the arena
        public float Cost
        {
            get
            {
                return cost;
            }
        }

        private bool unlocked;//has it been bought
        public bool Unlocked
        {
            get
            {
                return unlocked;
            }
        }

        private string arenaName;
        public string ArenaName
        {
            get
            {
                return arenaName;
            }
        }

        public ArenaUnlock(float cost, string name)
        {
            this.cost = cost;
            arenaName = name;
        }

        public bool Buy(ref float currency)
        {
            if (currency >= cost)
            {
                currency -= cost;
                unlocked = true;
                return true;
            }
            return false;
        }
    }

    private List<ArenaUnlock> arenaUnlocks;
    public List<ArenaUnlock> ArenaUnlocks
    {
        get
        {
            if (arenaUnlocks == null)
                Load();
            return arenaUnlocks;
        }
    }
    	
    private void Load()
    {
        arenaUnlocks = Utilities.LoadClass<List<ArenaUnlock>>(Application.persistentDataPath + @"/BubbleSurvivor/ArenasUnlocked.au");
        if (arenaUnlocks == null)
        {
            arenaUnlocks = new List<ArenaUnlock>();

        }
    }

    private void Save()
    {
        Utilities.SaveClass<List<ArenaUnlock>>(Application.persistentDataPath + @"/BubbleSurvivor/ArenasUnlocked.au", arenaUnlocks);
    }
	
}