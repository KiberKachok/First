using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RealmsNetwork
{
    public class Player
    {
        public string name;
        public string hash;

        public Player(string name, string hash)
        {
            this.name = name;
            this.hash = hash;
        }
    }
}