
public class NerdEntity : EntityBrain, IFlashlightTarget
{
    // Percepción push "Iluminado por linterna"
    public void OnLit()
    {
        // Solo mientras persigue
        if (IsChasing)
        {
            Log("Iluminada por la linterna");
            Interrupt();
        }
    }
}
