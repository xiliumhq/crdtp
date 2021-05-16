namespace Xilium.Crdtp.Core
{
    // TODO(dmitry.azaraev): Review Unit-type implementation.

    /// <summary>
    /// Unit-like type with single possible value of null.
    /// This type used to represent empty parameters or empty response.
    /// </summary>
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
