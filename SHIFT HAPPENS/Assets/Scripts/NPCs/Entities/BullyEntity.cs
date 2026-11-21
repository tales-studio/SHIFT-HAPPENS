using UnityEngine;

// Entidad del bully: persigue despacio.
// Se escapa alejándose más de escapeRange (pull) o escondiéndose (push, en la base).
public class BullyEntity : EntityBrain
{
    public override bool ShouldEscape()
    {
        return base.ShouldEscape() || DistanceToPlayer() > Config.escapeRange;
    }
}
