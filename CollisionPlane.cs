using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosMessageTypes.Geometry;

public class CollisionPlane
{
     public string id;
     public Pose pose;
     public Vector3 dimensions;

    // constructor
    public CollisionPlane(string planeID, Pose planePose, Vector3 planeDimensions)
    {
        id = planeID;
        pose = planePose;
        dimensions = planeDimensions;
    }

    public void hello()
    {
        Debug.Log("hello");
    }

   
}