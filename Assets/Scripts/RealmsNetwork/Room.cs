using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace RealmsNetwork
{
   
    public class Room
    {
        public string name;
        public string playersNames;
        public int currentPlayersCount;
        public int maxPlayersCount;
        public bool isGameStarted;
        public string hashes;

        public Room(string name, string playersNames, int currentPlayersCount, int maxPlayersCount, bool isGameStarted, string hashes)
        {
            this.name = name;
            this.playersNames = playersNames;
            this.currentPlayersCount = currentPlayersCount;
            this.maxPlayersCount = maxPlayersCount;
            this.isGameStarted = isGameStarted;
            this.hashes = hashes;
        }
    }
}
