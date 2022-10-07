using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GlobalState;
using RosMessageTypes.Geometry;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;

public class PickupCubeController : MonoBehaviour
{
    GameObject pickupCube;

    ROSConnection ros;


    // Start is called before the first frame update
    void Start()
    {
        pickupCube = GameObject.Find("PickupCube");

        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<PoseStampedMsg>("pickup_cube_pose", PickupCubePoseCallback);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PickupCubePoseCallback(PoseStampedMsg msg)
    {
        PoseMsg pose = msg.pose;

        Vector3 cubePosition = new Vector3((float)pose.position.x, (float)pose.position.y, (float)pose.position.z);
        Quaternion cubeRotation = new Quaternion((float)pose.orientation.x, (float)pose.orientation.y, (float)pose.orientation.z, (float)pose.orientation.w);

        pickupCube.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(cubePosition);
        pickupCube.transform.localRotation = RosSharp.TransformExtensions.Ros2Unity(cubeRotation);
    }

}
