using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class HandObstaclePublisher : MonoBehaviour
{
    [SerializeField] GameObject handObstacle;

    ROSConnection ros;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseMsg>("dynamic_obstacle_pose");

        handObstacle.AddComponent<NearInteractionGrabbable>();
        handObstacle.AddComponent<ObjectManipulator>();
        handObstacle.name = "handObstacle";

        handObstacle.AddComponent<PointerHandler>();

        float publishInterval = 0.25f;
        InvokeRepeating("PublishHandObstaclePosition", 1.0f, publishInterval);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PublishHandObstaclePosition()
    {
        PoseMsg msg = new PoseMsg();

        Vector3 position = new Vector3((float)handObstacle.transform.localPosition.x, (float)handObstacle.transform.localPosition.y, (float)handObstacle.transform.localPosition.z);

        msg.position.x = RosSharp.TransformExtensions.Unity2Ros(position).x;
        msg.position.y = RosSharp.TransformExtensions.Unity2Ros(position).y;
        msg.position.z = RosSharp.TransformExtensions.Unity2Ros(position).z;

        Quaternion quat = new Quaternion((float)handObstacle.transform.localRotation.x, (float)handObstacle.transform.localRotation.y, (float)handObstacle.transform.localRotation.z, (float)handObstacle.transform.localRotation.w);
        msg.orientation.x = RosSharp.TransformExtensions.Unity2Ros(quat).x;
        msg.orientation.y = RosSharp.TransformExtensions.Unity2Ros(quat).y;
        msg.orientation.z = RosSharp.TransformExtensions.Unity2Ros(quat).z;
        msg.orientation.w = RosSharp.TransformExtensions.Unity2Ros(quat).w;

        ros.Publish("dynamic_obstacle_pose", msg);
    }
}
