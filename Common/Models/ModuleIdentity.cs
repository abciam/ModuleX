namespace Common.Models;

public readonly struct ModuleIdentity(string name, Guid guid, string version) : IEquatable<ModuleIdentity>
{
    public string Name { get; } = name;
    public Guid Guid { get; } = guid;
    public string Version { get; } = version;

    public bool Equals(ModuleIdentity other)
    {
        return Name.Equals(other.Name, StringComparison.Ordinal) &&
               Guid.Equals(other.Guid) &&
               Version.Equals(other.Version, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => obj is ModuleIdentity other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(Name, Guid, Version);
    public override readonly string ToString() => $"{Name}:{Guid}@{Version}";

    public static bool operator ==(ModuleIdentity left, ModuleIdentity right) => left.Equals(right);
    public static bool operator !=(ModuleIdentity left, ModuleIdentity right) => !left.Equals(right);
}