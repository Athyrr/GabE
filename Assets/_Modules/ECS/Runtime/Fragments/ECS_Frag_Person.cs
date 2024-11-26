using Unity.Entities;


/// <summary>
/// Component data for a person entity.
/// </summary>
public struct ECS_Frag_Person : IComponentData
{
    /// <summary>
    /// The age of a person.
    /// </summary>
    public int Age;

    /// <summary>
    /// The amount of stamina a person has.
    /// </summary>
    public float Stamina;

    /// <summary>
    /// Is the person happy.
    /// </summary>
    public bool IsHappy;

    /// <summary>
    /// Is the person happy.
    /// </summary>
    public bool IsAlive;
}

