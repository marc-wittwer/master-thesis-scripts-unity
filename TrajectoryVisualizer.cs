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
using System.Linq;

public class TrajectoryVisualizer : MonoBehaviour
{
    ROSConnection ros;

    Material trajectoryPointMaterial;

    private static System.Random random = new System.Random();
    

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<PoseArrayMsg>("/trajectory_ee_positions", TrajectoryEEPosesCallback);

        trajectoryPointMaterial = Resources.Load("Materials/TrajectoryPointMaterial", typeof(Material)) as Material;

     }

     

    

    public void TrajectoryEEPosesCallback(PoseArrayMsg msg)
    {
        Debug.Log("Received trajectories");
        PoseMsg[] poses = msg.poses;

        // remove all existing trajectory_points in loop
        GameObject[] gameObjects = FindObjectsOfType<GameObject>() as GameObject[];

        for (var i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i].name.Contains("trajectory_point"))
            {
                Destroy(gameObjects[i].gameObject);
            }
        }

        GameObject robotFrame = GameObject.Find("Robot Frame");

        foreach (PoseMsg pose in poses)
        {
            Debug.Log(pose.position);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "trajectory_point_"+ RandomString(10);
            //sphere.transform.position = new Vector3((float)pose.position.x, (float)pose.position.y, (float)pose.position.z);
            
            //TODO: Order of setting position and parenting child could make a difference
            sphere.transform.parent = robotFrame.transform;

            Vector3 unity_position = RosSharp.TransformExtensions.Ros2Unity(new Vector3((float)pose.position.x, (float)pose.position.y, (float)pose.position.z));
            //sphere.transform.position = unity_position;

            sphere.transform.localPosition = unity_position;
            //sphere.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(item.pose.position);

            sphere.GetComponent<Renderer>().material = trajectoryPointMaterial;
            //sphere.GetComponent<SphereCollider>().enabled = false;
            //sphere.SetActive(false);
            float radius = 0.025f;
            sphere.transform.localScale = new Vector3(radius, radius, radius);
        }

           
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

}
