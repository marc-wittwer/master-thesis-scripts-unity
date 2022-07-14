using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.GlobalState;

public class AddCollisionPlaneButtonController : MonoBehaviour
{
    ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CollisionPlaneMsg>("collision_plane");
    }

    public void AddPlaneToGlobalState()
    {
        CollisionPlaneMsg msg = new CollisionPlaneMsg();
        msg.id = RandomStringGenerator(8);
        msg.action = 0;
        msg.pose = new PoseStampedMsg();

        Vector3 rosDimensions = new Vector3(0.5f, 0.25f, 0.01f);
        msg.dimensions = new Vector3Msg((double)rosDimensions.x, (double)rosDimensions.y, (double)rosDimensions.z);

        ros.Publish("collision_plane", msg);
    }

    public void DeletePlaneFromGlobalState()
    {
        CollisionPlaneMsg msg = new CollisionPlaneMsg();
        // TODO: Add logic to delete selected plane
        msg.id = RandomStringGenerator(8);
        msg.action = 2;
        msg.pose = new PoseStampedMsg();
        msg.dimensions = new Vector3Msg();
        ros.Publish("collision_plane", msg);
    }

    string RandomStringGenerator(int length)
    {
        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        string generated_string = "";

        for (int i = 0; i < length; i++)
            generated_string += characters[Random.Range(0, length)];

        return generated_string;
    }

}
