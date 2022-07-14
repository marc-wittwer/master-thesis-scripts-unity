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

public class GlobalStateController : MonoBehaviour
{

    [SerializeField] GameObject collisionPrimitive;
    [SerializeField] GameObject taskStartMarker;
    [SerializeField] GameObject taskEndMarker;
    [SerializeField] GameObject robotFrameAnchor;
    [SerializeField] GameObject robotFrame;

    [SerializeField] GameObject panda2;
    [SerializeField] GameObject rootArticulationBody;
    
    GameObject pandaWorkSpace;

    ROSConnection ros;

    List<CollisionPlane> globalPlanes = new List<CollisionPlane>();
    List<ConveyorBeltItem> globalConveyorBeltItems = new List<ConveyorBeltItem>();
    CollisionPlane selectedPlane = null;

    List<GameObject> collisionPlanes = new List<GameObject>();


    string movingPlaneId = "";
    bool isMovingStartMarker = false;
    bool isMovingEndMarker = false;

    Material collisionPlaneMaterial;
    Material workSpaceBoundsMaterial;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ROSConnection.GetOrCreateInstance().Subscribe<PlanningSceneMsg>("/global_planning_scene", PlanningSceneCallback);
        ros.RegisterPublisher<TaskUpdateMsg>("task_update");
        ros.RegisterPublisher<CollisionPlaneMsg>("collision_plane");

        collisionPlaneMaterial = Resources.Load("Materials/CollisionPlaneMaterial", typeof(Material)) as Material;
        workSpaceBoundsMaterial = Resources.Load("Materials/WorkSpaceBoundsMaterial", typeof(Material)) as Material;

        SetupTaskMarkers();
        SetupRobotWorkspace();
        SetupRobotFrameAnchor();
    }

    void Update()
    {
        SetCollisionPlanesPoses();
        SetConveyorBeltItemsPoses();

        UpdateTaskMarkersAppearance();
    }

    public void SetupTaskMarkers()
    {
        taskStartMarker.AddComponent<NearInteractionGrabbable>();
        taskStartMarker.AddComponent<ObjectManipulator>();
        taskStartMarker.name = "Task Start Marker";

        var pointerHandlerStart = taskStartMarker.AddComponent<PointerHandler>();
        pointerHandlerStart.OnPointerDragged.AddListener((e) =>
        {
            isMovingStartMarker = true;
        });
        pointerHandlerStart.OnPointerClicked.AddListener((e) =>
        {
            UpdateTaskMarkerInGlobalState(taskStartMarker);
            isMovingStartMarker = false;
        });

        taskEndMarker.AddComponent<NearInteractionGrabbable>();
        taskEndMarker.AddComponent<ObjectManipulator>();
        taskEndMarker.name = "Task End Marker";


        var pointerHandlerEnd = taskEndMarker.AddComponent<PointerHandler>();
        pointerHandlerEnd.OnPointerDragged.AddListener((e) =>
        {
            isMovingEndMarker = true;
        });
        pointerHandlerEnd.OnPointerClicked.AddListener((e) =>
        {
            UpdateTaskMarkerInGlobalState(taskEndMarker);
            isMovingEndMarker = false;
        });
    }

    void SetupRobotFrameAnchor()
    {
        var robotFrameAnchorPointerHandler = robotFrameAnchor.GetComponent<PointerHandler>();
        robotFrameAnchorPointerHandler.OnPointerClicked.AddListener((e) =>
        {
            float anchorHeight = 0.1f;
            Vector3 robotFrameOrigin = new Vector3(robotFrameAnchor.transform.position.x, robotFrameAnchor.transform.position.y - anchorHeight, robotFrameAnchor.transform.position.z);
            robotFrame.transform.SetPositionAndRotation(robotFrameOrigin, robotFrameAnchor.transform.rotation);
            rootArticulationBody.GetComponent<ArticulationBody>().TeleportRoot(robotFrameOrigin, robotFrameAnchor.transform.rotation);
        });
    }

    void SetupRobotWorkspace()
    {
        pandaWorkSpace = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pandaWorkSpace.name = "pandaWorkSpaceBounds";
        pandaWorkSpace.transform.position = new Vector3(0.0f, 0.354f, 0.0f);
        pandaWorkSpace.transform.parent = robotFrame.transform;
        pandaWorkSpace.GetComponent<Renderer>().material = workSpaceBoundsMaterial;
        pandaWorkSpace.GetComponent<SphereCollider>().enabled = false;
        pandaWorkSpace.SetActive(false);
        float radius = 1.66f;
        pandaWorkSpace.transform.localScale = new Vector3(radius, radius, radius);
    }

    public void SetCollisionPlanesPoses()
    {
        foreach (CollisionPlane plane in globalPlanes)
        {
            if (movingPlaneId == plane.id)
            {
                continue;
            }
            if (GameObject.Find(plane.id) != null)
            {
                GameObject planeObject = GameObject.Find(plane.id);


                // Constrain orientation to be parallel to floor-plane
                Quaternion quat = RosSharp.TransformExtensions.Ros2Unity(plane.pose.rotation);
                quat.eulerAngles = new Vector3(0.0f, quat.eulerAngles.y, 0.0f);

                planeObject.transform.SetPositionAndRotation(RosSharp.TransformExtensions.Ros2Unity(plane.pose.position), quat);

                planeObject.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(plane.pose.position);
                planeObject.transform.localRotation = quat;
            }
            else
            {
                // add plane to scene
                AddCollisionPlaneToScene(plane);
                 
            }
        }
    }

    public void SetConveyorBeltItemsPoses()
    {
        foreach (ConveyorBeltItem item in globalConveyorBeltItems)
        {
            if (GameObject.Find(item.name) != null)
            {
                GameObject itemObject = GameObject.Find(item.name);
                itemObject.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(item.pose.position);
                itemObject.transform.localRotation = RosSharp.TransformExtensions.Ros2Unity(item.pose.rotation);
            }
            else
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = item.name;
                cube.transform.SetPositionAndRotation(RosSharp.TransformExtensions.Ros2Unity(item.pose.position), RosSharp.TransformExtensions.Ros2Unity(item.pose.rotation));
                cube.transform.parent = robotFrame.transform;

                Vector3 scale = new Vector3((float)item.dimensions.x, (float)item.dimensions.y, (float)item.dimensions.z);
                cube.transform.localScale = RosSharp.TransformExtensions.Ros2Unity(scale);
            }
        }
    }
    public void AddCollisionPlaneToScene(CollisionPlane plane)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = plane.id;
        cube.transform.position = RosSharp.TransformExtensions.Ros2Unity(plane.pose.position);
        cube.transform.rotation = RosSharp.TransformExtensions.Ros2Unity(plane.pose.rotation);
        cube.transform.parent = robotFrame.transform;
        Vector3 scale = new Vector3((float)plane.dimensions.x, (float)plane.dimensions.y, (float)plane.dimensions.z);
        cube.transform.localScale = RosSharp.TransformExtensions.Ros2Unity(scale);

        cube.AddComponent<NearInteractionGrabbable>();
        cube.AddComponent<ObjectManipulator>();

        cube.GetComponent<Renderer>().material = collisionPlaneMaterial;


        var pointerHandler = cube.AddComponent<PointerHandler>();
        pointerHandler.OnPointerDragged.AddListener((e) =>
        {
            movingPlaneId = cube.name;
        });

        pointerHandler.OnPointerClicked.AddListener((e) =>
        {
            UpdateCollisionPlaneInGlobalState(cube);
            movingPlaneId = "";
        });

        BoundsControl boundsControl = cube.AddComponent<BoundsControl>();
        boundsControl.ScaleStarted.AddListener(() => {
            movingPlaneId = cube.name;
        });
        boundsControl.ScaleStopped.AddListener(() => {
            UpdateCollisionPlaneInGlobalState(cube);
            movingPlaneId = "";
        });
        boundsControl.FlattenAxis = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.FlattenModeType.DoNotFlatten;

        ScaleHandlesConfiguration scaleHandleConfiguration = boundsControl.ScaleHandlesConfig;
        scaleHandleConfiguration.HandleSize = 0.030f;
        scaleHandleConfiguration.ScaleBehavior = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.HandleScaleMode.NonUniform;

        RotationHandlesConfiguration rotationHandlesConfiguration = boundsControl.RotationHandlesConfig;
        rotationHandlesConfiguration.ShowHandleForX = false;
        rotationHandlesConfiguration.ShowHandleForY = false;
        rotationHandlesConfiguration.ShowHandleForZ = false;
    }

    public void UpdateCollisionPlaneInGlobalState(GameObject cube)
        {
        CollisionPlaneMsg msg = new CollisionPlaneMsg();
        msg.id = cube.name;
        msg.action = 1;
        msg.pose = new PoseStampedMsg();

        Transform transform = cube.transform;
        Vector3 position = new Vector3((float)transform.localPosition.x, (float)transform.localPosition.y, (float)transform.localPosition.z);
        Quaternion quat = new Quaternion((float)transform.localRotation.x, (float)transform.localRotation.y, (float)transform.localRotation.z, (float)transform.localRotation.w);

        msg.pose.pose.position.x = RosSharp.TransformExtensions.Unity2Ros(position).x;
        msg.pose.pose.position.y = RosSharp.TransformExtensions.Unity2Ros(position).y;
        msg.pose.pose.position.z = RosSharp.TransformExtensions.Unity2Ros(position).z;
        msg.pose.pose.orientation.x = RosSharp.TransformExtensions.Unity2Ros(quat).x;
        msg.pose.pose.orientation.y = RosSharp.TransformExtensions.Unity2Ros(quat).y;
        msg.pose.pose.orientation.z = RosSharp.TransformExtensions.Unity2Ros(quat).z;
        msg.pose.pose.orientation.w = RosSharp.TransformExtensions.Unity2Ros(quat).w;

        msg.dimensions.x = RosSharp.TransformExtensions.Unity2Ros(cube.transform.localScale).x;
        msg.dimensions.y = RosSharp.TransformExtensions.Unity2Ros(cube.transform.localScale).y;
        msg.dimensions.z = RosSharp.TransformExtensions.Unity2Ros(cube.transform.localScale).z;

        ros.Publish("collision_plane", msg);
    }

    public void UpdateTaskMarkerInGlobalState(GameObject marker)
    {
        Debug.Log(marker.name);

        TaskUpdateMsg msg = new TaskUpdateMsg();
        msg.pose_type = marker.name == "Task Start Marker" ? 0 : 1;
        msg.pose = new PoseMsg();

        Vector3 position = new Vector3((float)marker.transform.localPosition.x, (float)marker.transform.localPosition.y, (float)marker.transform.localPosition.z);

        msg.pose.position.x = RosSharp.TransformExtensions.Unity2Ros(position).x;
        msg.pose.position.y = RosSharp.TransformExtensions.Unity2Ros(position).y;
        msg.pose.position.z = RosSharp.TransformExtensions.Unity2Ros(position).z;

        Quaternion quat = new Quaternion((float)marker.transform.localRotation.x, (float)marker.transform.localRotation.y, (float)marker.transform.localRotation.z, (float)marker.transform.localRotation.w);
        msg.pose.orientation.x = RosSharp.TransformExtensions.Unity2Ros(quat).x;
        msg.pose.orientation.y = RosSharp.TransformExtensions.Unity2Ros(quat).y;
        msg.pose.orientation.z = RosSharp.TransformExtensions.Unity2Ros(quat).z;
        msg.pose.orientation.w = RosSharp.TransformExtensions.Unity2Ros(quat).w;

        ros.Publish("task_update", msg);
    }

    public void UpdateTaskMarkersAppearance()
    {
        pandaWorkSpace.SetActive(!TaskMarkerPositionsAreValid());
    }

    bool TaskMarkerPositionsAreValid()
    {
        float taskStartMarkerDistance = Math.Abs(Vector3.Distance(pandaWorkSpace.transform.localPosition, taskStartMarker.transform.localPosition));
        float taskEndMarkerDistance = Math.Abs(Vector3.Distance(pandaWorkSpace.transform.localPosition, taskEndMarker.transform.localPosition));

        float bounds = 0.75f;
        return (taskStartMarkerDistance < bounds && taskEndMarkerDistance < bounds) ? true : false;
    }

    public void PlanningSceneCallback(PlanningSceneMsg msg)
    {
        CollisionPlaneMsg[] planes = msg.planes;
        ConveyorBeltItemMsg[] conveyorBeltItems = msg.conveyor_belt_items;

        globalPlanes.Clear();
        globalConveyorBeltItems.Clear();

        foreach (CollisionPlaneMsg plane in planes)
        {
            Vector3 position = new Vector3((float)plane.pose.pose.position.x, (float)plane.pose.pose.position.y, (float)plane.pose.pose.position.z);
            Quaternion quaternion = new Quaternion((float)plane.pose.pose.orientation.x, (float)plane.pose.pose.orientation.y, (float)plane.pose.pose.orientation.z, (float)plane.pose.pose.orientation.w);
            Pose planePose = new Pose(position, quaternion);
            Vector3 dimensions = new Vector3((float)plane.dimensions.x, (float)plane.dimensions.y, (float)plane.dimensions.z);
            Debug.Log(quaternion);
            CollisionPlane collisionPlane = new CollisionPlane(plane.id, planePose, dimensions);
            globalPlanes.Add(collisionPlane);
        }

        foreach (ConveyorBeltItemMsg item in conveyorBeltItems)
        {
            Vector3 position = new Vector3((float)item.pose.position.x, (float)item.pose.position.y, (float)item.pose.position.z);
            Quaternion quaternion = new Quaternion((float)item.pose.orientation.x, (float)item.pose.orientation.y, (float)item.pose.orientation.z, (float)item.pose.orientation.w);
            Pose itemPose = new Pose(position, quaternion);
            Vector3 dimensions = new Vector3((float)item.dimensions.x, (float)item.dimensions.y, (float)item.dimensions.z);

            ConveyorBeltItem conveyorBeltItem = new ConveyorBeltItem(item.id, itemPose, dimensions);
            globalConveyorBeltItems.Add(conveyorBeltItem);
        }

        SetTaskMarkersPoses(msg);
    }

     

    public void SetTaskMarkersPoses(PlanningSceneMsg msg)
    {
        PoseMsg taskStartPose = msg.task_start_pose;
        PoseMsg taskEndPose = msg.task_end_pose;

        Vector3 startPosition = new Vector3((float)taskStartPose.position.x, (float)taskStartPose.position.y, (float)taskStartPose.position.z);
        Quaternion startRotation = new Quaternion((float)taskStartPose.orientation.x, (float)taskStartPose.orientation.y, (float)taskStartPose.orientation.z, (float)taskStartPose.orientation.w);
        Vector3 endPosition = new Vector3((float)taskEndPose.position.x, (float)taskEndPose.position.y, (float)taskEndPose.position.z);
        Quaternion endRotation = new Quaternion((float)taskEndPose.orientation.x, (float)taskEndPose.orientation.y, (float)taskEndPose.orientation.z, (float)taskEndPose.orientation.w);
 
        if (!isMovingStartMarker)
        {
            taskStartMarker.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(startPosition);
            taskStartMarker.transform.localRotation = RosSharp.TransformExtensions.Ros2Unity(startRotation);
        }
        if (!isMovingEndMarker)
        {
            taskEndMarker.transform.localPosition = RosSharp.TransformExtensions.Ros2Unity(endPosition);
            taskEndMarker.transform.localRotation = RosSharp.TransformExtensions.Ros2Unity(endRotation);
        }
    }

    
}
