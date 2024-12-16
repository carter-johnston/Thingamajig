namespace Thingamajig.Transpiler
{
    public static class TypeCatalog
    {
        public static Dictionary<string, string> GetTypePrimitiveCatalog()
            => new()
            {
                {"bool", TypeScript.Boolean },

                {"int", TypeScript.Number },
                {"float", TypeScript.Number },
                {"decimal", TypeScript.Number },

                {"string", TypeScript.String },
            };

        public static readonly string[] CSharpCollectionTypes = [
            "IList",
            "ICollection",
            "IEnumerable",
            "List"
        ];
    }

    public static class TypeScript
    {
        public static readonly string Boolean = "boolean";
        public static readonly string String = "string";
        public static readonly string Number = "number";
    }
}
