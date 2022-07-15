using Stonylang.Symbols;

namespace Stonylang.Utility
{
    internal sealed class Conversion
    {
        public static readonly Conversion None = new(false, false, false);
        public static readonly Conversion Identity = new(true, true, true);
        public static readonly Conversion Implicit = new(true, false, true);
        public static readonly Conversion Explicit = new(true, false, false);

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Conversion.Identity;

            if (from == TypeSymbol.Int)
            {
                if (to == TypeSymbol.String)
                    return Conversion.Explicit;
            }
            else if (from == TypeSymbol.Float)
            {
                if (to == TypeSymbol.String)
                    return Conversion.Explicit;
            }
            else if (from == TypeSymbol.Bool)
            {
                if (to == TypeSymbol.String)
                    return Conversion.Explicit;
            }
            else if (from == TypeSymbol.String)
            {
                if (to == TypeSymbol.Int || to == TypeSymbol.Float)
                    return Conversion.Explicit;
            }
            else if (from == TypeSymbol.Int)
            {
                if (to == TypeSymbol.Float)
                    return Conversion.Implicit;
            }

            return Conversion.None;
        }
    }
}
