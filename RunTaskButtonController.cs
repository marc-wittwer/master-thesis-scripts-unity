using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GlobalState;
using RosMessageTypes.Std;

public class RunTaskButtonController : MonoBehaviour
{
    ROSConnection ros;

     void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterRosService<ExecuteTaskRequest, ExecuteTaskResponse>("execute_task");
        ros.RegisterPublisher<StringMsg>("call_panda");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunTaskViaROS()
    {
        //Debug.Log("HELLLO");

        StringMsg msg = new StringMsg();
        msg.data = "start_task";
        ros.Publish("call_panda", msg);


        //string taskName = "execute_start_to_end_pose_task";
        //ExecuteTaskRequest serviceRequest = new ExecuteTaskRequest(taskName);
        //ros.SendServiceMessage<ExecuteTaskResponse>("execute_task", serviceRequest, TaskServiceCallback);


    }

    public void TaskServiceCallback(ExecuteTaskResponse response)
    {
        Debug.Log(response);
    }
}

 