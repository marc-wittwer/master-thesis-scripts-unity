using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class JointController : MonoBehaviour
{
    ROSConnection ros;

    [SerializeField] private ArticulationBody[] robotJoints = new ArticulationBody[11];
    [SerializeField] GameObject parent;

    public static readonly string[] robotLinkNames =
    {   "base_footprint/panda_link0",
        "base_footprint/panda_link0/panda_link1/",
        "base_footprint/panda_link0/panda_link1/panda_link2/",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8/panda_hand/panda_rightfinger",
        "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4/panda_link5/panda_link6/panda_link7/panda_link8/panda_hand/panda_leftfinger",
    };

    void Start()
    {
        Debug.Log("Start joint controller");
        ros = ROSConnection.GetOrCreateInstance();

        var linkName = string.Empty;
        for (var i = 0; i < robotLinkNames.Length; i++)
        {
            robotJoints[i] = parent.transform.Find(robotLinkNames[i]).gameObject.GetComponent<ArticulationBody>();
        }

        ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>("/joint_states", JointStatesCallback);
    }

    public void JointStatesCallback(JointStateMsg msg)
    {
        SetJointAngle((float)msg.position[1], 1);
        SetJointAngle((float)msg.position[2], 2);
        SetJointAngle((float)msg.position[4], 3);
        SetJointAngle((float)msg.position[5], 4);
        SetJointAngle((float)msg.position[6], 5);
        SetJointAngle((float)msg.position[7], 6);
        SetJointAngle((float)msg.position[8], 7);

        SetJointAngle((float)msg.position[0], 9); // 9 = right_finger
        SetJointAngle((float)msg.position[3], 10); // 10 = left_finger
    }

    public void SetJointAngle(float angle, int jointIndex)
    {
        robotJoints[jointIndex].jointPosition = new ArticulationReducedSpace(angle);
    }



    //public void setJoints()
    //{

    //    float closedFingerPosition = 0.00022648140655199178f;
    //    float openFingerPosition = 0.03f;
    //    //SetJointAngle(openFingerPosition,9); //right_finger
    //    SetJointAngle(openFingerPosition, 10); //left_finger
    //    return;


    //    float randomNumber = Random.Range(-Mathf.PI, Mathf.PI);

    //    SetJointAngle(randomNumber, 3);

    //    randomNumber = Random.Range(-Mathf.PI, Mathf.PI);
    //    SetJointAngle(randomNumber, 4);

    //    randomNumber = Random.Range(-Mathf.PI, Mathf.PI);
    //    SetJointAngle(randomNumber, 5);
    //    randomNumber = Random.Range(-Mathf.PI, Mathf.PI);
    //    SetJointAngle(randomNumber, 2);
    //    randomNumber = Random.Range(-Mathf.PI, Mathf.PI);
    //    SetJointAngle(randomNumber, 1);

    //    // link 1: Y
    //    // link 2: X
    //    // link 3: X
    //    // link 4: X
    //    // link 5: X
    //    // link 6: X
    //    // link 7: X


    //    string linkName = "base_footprint/panda_link0/panda_link1/panda_link2/panda_link3/panda_link4";
    //    GameObject myLink = parent.transform.Find(linkName).gameObject;
    //    Debug.Log(myLink.transform.position);
    //    Debug.Log(myLink.transform.rotation);

    //    ArticulationBody articulatedBody = myLink.GetComponent<ArticulationBody>();

    //    //articulatedBody.jointPosition = new ArticulationReducedSpace(1.5f,0f, 0f);
    //    //articulatedBody.jointAcceleration = new ArticulationReducedSpace(0f, 0f, 0f);
    //    //articulatedBody.jointForce = new ArticulationReducedSpace(0f, 0f, 0f);
    //    //articulatedBody.jointVelocity = new ArticulationReducedSpace(0f, 0f, 0f);

    //    //articulatedBody.jointPosition = new ArticulationReducedSpace(-1.5f);
    //    //articulatedBody.jointAcceleration = new ArticulationReducedSpace(0f);
    //    //articulatedBody.jointForce = new ArticulationReducedSpace(0f);
    //    //articulatedBody.jointVelocity = new ArticulationReducedSpace(0f);


    //    // List<float> m_FloatList = new List<float>();

    //    // articulatedBody.GetJointPositions( m_FloatList );
    //    //Debug.Log(m_FloatList );

    //    //var pos = new List<float> { 45f, 45f, 45f};
    //    //articulatedBody.SetJointPositions(pos);

    //    // float jointTarget = -1.0f;
    //    // var joint1XDrive = articulatedBody.xDrive;
    //    // joint1XDrive.target = (float)(jointTarget) * Mathf.Rad2Deg;
    //    // articulatedBody.xDrive = joint1XDrive;

    //    Debug.Log("Set xDrive");

    //    // Debug.Log(joint1XDrive.target);

    //    // Debug.Log(myLink.transform.rotation);

    //    // myLink.transform.FindChild()

    //    // m_Spot.transform.Find(LinkNames[i]).GetComponent<ArticulationBody>();

    //    //int i = 3;
    //    // Vector3 position = new Vector3(0.067f,1.52f,-0.37f);
    //    // float jointTarget = 1.5f;
    //    // var joint1XDrive = robotJoints[i].xDrive;
    //    // joint1XDrive.target = (float)(jointTarget) * Mathf.Rad2Deg;
    //    //robotJoints[i].xDrive = joint1XDrive;
    //    // Debug.Log(joint1XDrive.target);
    //}
     

    // Update is called once per frame
    void Update()
    {

    }

    public void OnDestroy()
    {

    }
}
