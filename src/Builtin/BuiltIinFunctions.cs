using Stonylang.Symbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Stonylang.Builtin
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new("print",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Any, null)), TypeSymbol.Void, null);

        public static readonly FunctionSymbol PrintLn = new("println",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Any, null)), TypeSymbol.Void, null);

        public static readonly FunctionSymbol Input = new("input",
            ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String, null);

        public static readonly FunctionSymbol Stringify = new("stringify",
            ImmutableArray.Create(new ParameterSymbol("value", TypeSymbol.Any, null)), TypeSymbol.String, null);

        public static readonly FunctionSymbol RandomInt = new("random",
            ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Int, null)), TypeSymbol.Int, null);

        public static readonly FunctionSymbol RandomFloat = new("random",
            ImmutableArray.Create(new ParameterSymbol("max", TypeSymbol.Float, null)), TypeSymbol.Float, null);

        internal static IEnumerable<FunctionSymbol> GetAll() => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol)f.GetValue(null));
    }
}
