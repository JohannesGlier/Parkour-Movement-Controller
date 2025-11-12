using UnityEngine;

public enum ControllerType
{
    FIRST_PERSON, TOP_DOWN, THIRD_PERSON
}

public class ControllerSettings : Singleton<ControllerSettings>
{
    [Header("Controller Type")]
    [SerializeField] ControllerType controllerType;

    [Header("Camera Reference")]
    [SerializeField] Camera activeCam;

    public ControllerType ActiveControllerType => controllerType;
    public Camera ActiveCamera => activeCam;
}
