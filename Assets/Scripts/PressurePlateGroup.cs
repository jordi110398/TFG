using UnityEngine;

public class PressurePlateGroup : MonoBehaviour
{
    public PressurePlate[] plates; // Assigna les 3 plaques a l'Inspector
    public MovingDoor porta;       // Assigna la porta a l'Inspector

    private int platesActive = 0;

    private void OnEnable()
    {
        foreach (var plate in plates)
        {
            plate.OnActivated.AddListener(OnPlateActivated);
            plate.OnDeactivated.AddListener(OnPlateDeactivated);
        }
    }

    private void OnDisable()
    {
        foreach (var plate in plates)
        {
            plate.OnActivated.RemoveListener(OnPlateActivated);
            plate.OnDeactivated.RemoveListener(OnPlateDeactivated);
        }
    }

    private void OnPlateActivated()
    {
        platesActive++;
        if (platesActive == plates.Length)
            porta.OpenDoor();
    }

    private void OnPlateDeactivated()
    {
        platesActive = Mathf.Max(0, platesActive - 1);
        porta.CloseDoor();
    }
}
