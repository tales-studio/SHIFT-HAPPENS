using UnityEngine;
using UnityEngine.InputSystem;

public class NPCFloorNavigationTest : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NPCFloorNavigator floorNavigator;
    [SerializeField] private NPCDestinationManager destinationManager;

    [Header("Test Controls")]
    [SerializeField] private bool useNumpadControls;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool key1 = useNumpadControls
            ? Keyboard.current.numpad1Key.wasPressedThisFrame
            : Keyboard.current.digit1Key.wasPressedThisFrame;

        bool key2 = useNumpadControls
            ? Keyboard.current.numpad2Key.wasPressedThisFrame
            : Keyboard.current.digit2Key.wasPressedThisFrame;

        bool key3 = useNumpadControls
            ? Keyboard.current.kKey.wasPressedThisFrame
            : Keyboard.current.digit3Key.wasPressedThisFrame;

        bool key4 = useNumpadControls
            ? Keyboard.current.numpad4Key.wasPressedThisFrame
            : Keyboard.current.digit4Key.wasPressedThisFrame;

        bool key5 = useNumpadControls
            ? Keyboard.current.numpad5Key.wasPressedThisFrame
            : Keyboard.current.digit5Key.wasPressedThisFrame;

        bool key6 = useNumpadControls
            ? Keyboard.current.numpad6Key.wasPressedThisFrame
            : Keyboard.current.digit6Key.wasPressedThisFrame;

        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            floorNavigator.CancelCurrentTrip();
        }

        if (key1)
            Request(NPCDestinationType.Bathroom, BuildingFloor.GroundFloor);

        if (key2)
            Request(NPCDestinationType.StudyRoom, BuildingFloor.GroundFloor);

        if (key3)
            Request(NPCDestinationType.Kitchen, BuildingFloor.GroundFloor);

        if (key4)
            Request(NPCDestinationType.Bathroom, BuildingFloor.FirstFloor);

        if (key5)
            Request(NPCDestinationType.Laundry, BuildingFloor.Basement);

        if (key6)
            Request(NPCDestinationType.Bathroom, BuildingFloor.SecondFloor);
    }

    private void Request(
        NPCDestinationType type,
        BuildingFloor floor)
    {
        if (floorNavigator == null || destinationManager == null)
            return;

        destinationManager.TryRequestRandomDestination(
            type,
            floor,
            floorNavigator,
            out NPCDestination destination
        );
    }
}
