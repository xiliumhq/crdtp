namespace Xilium.Crdtp.Core
{
    // TODO(dmitry.azaraev): Review Unit-type implementation.
    // TODO(dmitry.azaraev): Move to xi.lang

    /// <summary>
    /// The unit type (see https://en.wikipedia.org/wiki/Unit_type), which has
    /// only one value. This value is special and always uses the representation
    /// <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// This type have private constructor, so no instances should exist.
    /// Recommended way to "construct instance" of this type is use C# default
    /// operator <c>default(Unit)</c>.
    ///
    /// <para>This type used to represent empty command parameters or empty response.</para>
    /// </remarks>
    public sealed class Unit // : IEquatable<Unit>
    {
        private Unit() { }

        //public override int GetHashCode() => 0;

        //public override bool Equals(object? obj)
        //{
        //    if (obj == null) return true;

        //    // TODO(dmitry.azaraev): return obj is Unit; should be enough?

        //    if (obj is Unit)
        //    {
        //        if (obj == null) return true;
        //    }

        //    return false;
        //}

        //public bool Equals(Unit? other) => other == null;
    }
}
