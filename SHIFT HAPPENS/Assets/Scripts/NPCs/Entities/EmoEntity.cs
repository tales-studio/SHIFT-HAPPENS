using UnityEngine;

using Unity.Cinemachine;
using UnityEngine;

// Entidad del emo: al aparecer grita con temblor de cámara y desaparece
public class EmoEntity : EntityBrain
{
    [Header("Grito")]
    [SerializeField] private SoundId screamSound = SoundId.EntityStridentScream;

    [Tooltip("La cámara necesita un CinemachineImpulseListener.")]
    [SerializeField] private CinemachineImpulseSource impulseSource;

    protected override void Awake()
    {
        base.Awake();
        if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // Ya estaba en rango al activarse: grita salvo que Ramón se haya escondido
    public override bool PlayerInAttackRange()
    {
        return TargetPlayer == null || !TargetPlayer.IsHidden;
    }

    protected override float AttackDuration => Config.screamDuration;

    // Acción "Gritar": sonido + movimiento de cámara (no es un jumpscare)
    protected override void PerformAttack()
    {
        Log("Grita");
        AudioServices.Sfx.PlayAt(screamSound, transform.position);

        if (impulseSource != null)
            impulseSource.GenerateImpulseWithForce(Config.shakeForce);
    }
}
