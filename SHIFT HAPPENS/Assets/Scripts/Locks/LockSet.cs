using System.Collections.Generic;

public class LockSet
{
    private readonly HashSet<object> sources = new HashSet<object>();

    public bool IsLocked => sources.Count > 0;

    public void Add(object source)
    {
        sources.Add(source);
    }

    public void Remove(object source)
    {
        sources.Remove(source);
    }
}
