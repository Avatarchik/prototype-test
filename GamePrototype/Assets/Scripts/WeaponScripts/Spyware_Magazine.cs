using System;
using UnityEngine;

namespace Spyware
{
    public class Spyware_Magazine : Spyware_PhysInteractable
    {
        public int m_numRounds = 30;
        public bool IsExtractable = true;

        public bool HasARound()
        {
            return this.m_numRounds > 0 && this.IsExtractable;
        }
    }
}