using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Geometry;

public class ConveyorBeltItem
{
     public string name;
     public Pose pose;
     public Vector3 dimensions;

    public ConveyorBeltItem(string itemName, Pose itemPose, Vector3 itemDimensions)
    {
        name = itemName;
        pose = itemPose;
        dimensions = itemDimensions;
    }

   
}