public enum SoundCategory
{
    Music,
    Ambience,
    Player,
    Interaction,
    Environment,
    NPC,
    Entity,
    UI
}

public enum AudioBus
{
    Music,
    SFX,
    UI
}

public static class SoundCategoryExtensions
{
    public static AudioBus ToBus(this SoundCategory category)
    {
        switch (category)
        {
            case SoundCategory.Music: return AudioBus.Music;
            case SoundCategory.UI: return AudioBus.UI;
            default: return AudioBus.SFX;
        }
    }
}
