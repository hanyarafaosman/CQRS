namespace Waseet.CQRS;

/// <summary>
/// Represents a void type, since Void is not a valid return type in C#.
/// </summary>
public struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Default and only value of the Unit type.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Determines whether the specified Unit is equal to the current Unit.
    /// </summary>
    public bool Equals(Unit other) => true;

    /// <summary>
    /// Determines whether the specified object is equal to the current Unit.
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string that represents the current Unit.
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// Determines whether two Unit instances are equal.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Determines whether two Unit instances are not equal.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
