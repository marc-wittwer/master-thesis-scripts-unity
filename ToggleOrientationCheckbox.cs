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

public class ToggleOrientationCheckbox : MonoBehaviour
{

    ROSConnection ros;

    bool fixed_orientation = false;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<PlanningSceneMsg>("/global_planning_scene", PlanningSceneCallback);
        ros.RegisterPublisher<TaskUpdateMsg>("task_update");

        Interactable checkbox = GameObject.Find("OrientationConstraintCheckbox").GetComponent<Interactable>();
        checkbox.OnClick.AddListener(() =>
        {
            //Debug.Log("Clicked constraint checkbox");
            TaskUpdateMsg msg = new TaskUpdateMsg();
            msg.pose_type = 2;
            msg.pose = new PoseMsg();
            msg.fixed_orientation = !fixed_orientation;
            fixed_orientation = !fixed_orientation;

            checkbox.IsToggled = fixed_orientation;

            ros.Publish("task_update", msg);

            // TODO: Change the task_end_marker's rotation equal to the task_start_marker  
        });
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}

    //public void ToggleCheckboxAndUpdateTaskMarkerInGlobalState()
    //{
    //    Debug.Log("Hello");
    //    TaskUpdateMsg msg = new TaskUpdateMsg();
    //    msg.pose_type = 2;
    //    msg.pose = new PoseMsg();
    //    msg.fixed_orientation = !fixed_orientation;
    //    fixed_orientation = !fixed_orientation;



    //    ros.Publish("task_update", msg);
    //}

    public void PlanningSceneCallback(PlanningSceneMsg msg)
    {
        fixed_orientation = msg.fixed_orientation;
    }

}
