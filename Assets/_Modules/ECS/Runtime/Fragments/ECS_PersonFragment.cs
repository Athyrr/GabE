using Unity.Entities;


/// <summary>
/// Component data for a person entity.
/// </summary>
public struct ECS_PersonFragment : IComponentData
{
    /// <summary>
    /// The age of a person.
    /// </summary>
    public int Age;

    /// <summary>
    /// Is the person exhausted.
    /// </summary>
    public bool IsExhausted;

    /// <summary>
    /// Is the person happy.
    /// </summary>
    public bool IsHappy;

    /// <summary>
    /// Is the person happy.
    /// </summary>
    public bool IsAlive;
}

