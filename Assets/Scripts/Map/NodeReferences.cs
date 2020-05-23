using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Map.Buildings;

namespace Map
{
    public class NodeReferences
    {
        public static Dictionary<string, int> EnergyCapDictionary = new Dictionary<string, int>()
        {
            {"BaseNode", 24},
            {"ProductionNode", 16},
            {"BatteryNode", 64},
            {"BarrierNode", 48},
        };
        
        public static List<NodeType> InteractableNodeTypes = new List<NodeType>()
        {
            NodeType.NODE,
            NodeType.START_NODE,
            NodeType.DATA_BUS
        };
    }
}