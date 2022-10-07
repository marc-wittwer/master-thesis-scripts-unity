using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GlobalState;
using RosMessageTypes.Geometry;
using Microsoft.MixedReality.Toolkit.UI;

public class TaskSpeedController : MonoBehaviour
{

    ROSConnection ros;
    bool fixed_velocity = false;
    float current_velocity = 0.5f;

    float sliderFactor = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<PlanningSceneMsg>("/global_planning_scene", PlanningSceneCallback);
        ros.RegisterPublisher<TaskUpdateMsg>("task_update");

        // get reference to Text field
        TextMesh sliderLabel = GameObject.Find("TaskSpeedSliderLabel").GetComponent<TextMesh>();

        // add callbacks to slider functions
        var slider = GameObject.Find("TaskSpeedSlider").GetComponent<PinchSlider>();

        float a = slider.SliderValue;

        slider.OnValueUpdated.AddListener((data) =>
        {

            float newVelocity = sliderFactor * data.NewValue;

            sliderLabel.text = "Task Speed\n" + newVelocity.ToString("0.00") + " m/s";

            TaskUpdateMsg msg = new TaskUpdateMsg();
            msg.pose_type = 3;
            msg.pose = new PoseMsg();
            msg.fixed_velocity = fixed_velocity;
            msg.task_velocity = newVelocity;

            ros.Publish("task_update", msg);

            //Debug.Log("Onvalue updated slider");

        });

        Interactable checkbox = GameObject.Find("SpeedConstraintCheckbox").GetComponent<Interactable>();
        checkbox.OnClick.AddListener(() =>
        {
            //clicked on checkbox
            TaskUpdateMsg msg = new TaskUpdateMsg();
            msg.pose_type = 3;
            msg.pose = new PoseMsg();
            msg.fixed_velocity = !fixed_velocity;
            msg.task_velocity = current_velocity;
            fixed_velocity = !fixed_velocity;

            checkbox.IsToggled = fixed_velocity;

            ros.Publish("task_update", msg);



        });

        //var pointerHandlerStart = taskStartMarker.AddComponent<PointerHandler>();
        //pointerHandlerStart.OnPointerDragged.AddListener((e) =>
        //{
        //    isMovingStartMarker = true;
        //});

    }

    public void PlanningSceneCallback(PlanningSceneMsg msg)
    {
        fixed_velocity = msg.fixed_velocity;
        current_velocity = msg.task_velocity;

        // display checkbox status
        var slider = GameObject.Find("TaskSpeedSlider").GetComponent<PinchSlider>();

        // only update if vlaue has changed
        float newSliderValue = current_velocity / sliderFactor;
        if (newSliderValue != slider.SliderValue)
        {
            slider.SliderValue = newSliderValue;
        }

    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
