using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    /// <summary>
    /// This camera is just for scene testing.
    /// It must be deactivated in GameManager OnRoomStart
    /// </summary>
    public Camera sceneTestCamera;
    public Transform startPosition;
    public Transform cameraPosition;
    public Transform cameraPositionsParent;

    [HideInInspector]
    public bool CamIsMoving;

    private readonly List<Transform> cameraPositions = new List<Transform>();
    private readonly List<ObjectPlace> objectPlaces = new List<ObjectPlace>();
    private int positionIndex = 0;

    private void Start()
    {
        Init();
        GameManager.GetInstance().OnRoomStart(this);
    }

    private void Init()
    {
        CamIsMoving = true;
        InitCameraPositions();
        InitObjectPlaces();
        CamIsMoving = false;
    }

    public void InitCharacter(Character character)
    {
        RaycastHit hit = Calc.GetPointOnGround(startPosition.position);
        Vector3 startPoint = hit.point;
        character.SetPosition(startPoint);
    }

    private void InitCameraPositions()
    {
        Transform[] places = cameraPositionsParent.GetComponentsInChildren<Transform>();
        cameraPositions.AddRange(places);
        cameraPositions.RemoveAt(0);
    }

    private void InitObjectPlaces()
    {
        ObjectPlace[] places = GetComponentsInChildren<ObjectPlace>();
        objectPlaces.AddRange(places);
    }

    public void ShowObjectsPlaces(bool isVisible, Collectable collectable)
    {
        isVisible &= null != collectable;

        foreach (ObjectPlace place in objectPlaces)
        {
            place.SetVisible(
                isVisible
                && place.dropOrientation == collectable.dropOrientation
                && place.IsAvailable
            );
        }
    }

    public void ForceUpdateCameraPosition()
    {
        if (cameraPositions.Count <= 1)
        {
            return;
        }

        CamIsMoving = true;
        positionIndex = -1;
        int index = GetClosestCameraPosition();

        if (index < 0 || index >= cameraPositions.Count || index == positionIndex)
        {
            return;
        }
        
        positionIndex = index;
        Player player = GameManager.GetInstance().CurrentPlayer;
        Vector3 lookAt = player.transform.position;
        CameraFollowTarget.GetInstance().SetPosition(
            cameraPositions[positionIndex].position,
            lookAt
        );
        CamIsMoving = false;
    }

    private void CheckCameraPosition()
    {
        if (CamIsMoving || cameraPositions.Count <= 1)
        {
            return;
        }

        int index = GetClosestCameraPosition();

        if (index < 0 || index >= cameraPositions.Count || index == positionIndex)
        {
            return;
        }

        GotoCamera(index);
    }

    public int GetClosestCameraPosition()
    {
        Player player = GameManager.GetInstance().CurrentPlayer;
        Vector3 position = (player.IsMoving ? player.CurrentTarget : player.transform.position) + Vector3.up;
        float minDistance = float.MaxValue;
        int index = -1;

        for (int i = 0; i < cameraPositions.Count; i++)
        {
            Vector3 cameraPosition = CameraFollowTarget.GetInstance().cam.transform.localPosition;
            RaycastHit hit1 = Calc.GetPointOnGround(cameraPositions[i].position);
            cameraPosition += hit1.point;
            float distance = Vector3.Distance(position, cameraPosition);

            if (distance < minDistance)
            {
                index = i;
                minDistance = distance;
            }
        }

        return index;
    }

    private void GotoCamera(int index)
    {
        if (CamIsMoving)
            return;

        positionIndex = index;
        CamIsMoving = true;
        CameraFollowTarget.GetInstance().Goto(
            cameraPositions[positionIndex].position,
            MoveFinished
        );
    }

    private void MoveFinished()
    {
        CamIsMoving = false;
    }

    private void Update()
    {
        CheckCameraPosition();
    }
}
