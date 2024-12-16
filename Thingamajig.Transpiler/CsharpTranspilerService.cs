using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Thingamajig.Transpiler
{
    public class CsharpTranspilerService : ICsharpTranspilerService
    {
        private readonly Dictionary<string, string> _typePrimitiveCatalog;

        public CsharpTranspilerService()
        {
            _typePrimitiveCatalog = TypeCatalog.GetTypePrimitiveCatalog();
        }

        public string ConvertCSharpToTypescript(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            
            var classDefinitions = syntaxTree.GetCompilationUnitRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            var modelClasses = classDefinitions.Select(x => new 
            {
                ClassName = x.Identifier.ToString(),
                Properties = x.Members
                    .OfType<PropertyDeclarationSyntax>()
                    .Select(p =>
                    {
                        var type = p.Type.ToString().Replace("?", "");
                        
                        var isAutoProperty = p.ExpressionBody == null 
                            && (p.AccessorList == null
                            || p.AccessorList.Accessors
                                .All(a => a.Body == null 
                                    && a.ExpressionBody == null));

                        return new
                        {
                            Name = p.Identifier.ToString(),
                            Type = type,
                            IsAutoProperty = isAutoProperty,
                            IsNullable = p.Type.ToString().EndsWith('?'),
                        };
                    })
            });

            foreach (var item in modelClasses)
                _typePrimitiveCatalog[item.ClassName] = item.ClassName;

            var typescriptInferfaces = modelClasses
                .Select(cSharpClass => new
                {
                    InterfaceName = cSharpClass.ClassName,
                    Properties = cSharpClass.Properties.Select(csharpProperty => new
                    {
                        csharpProperty.Name,
                        csharpProperty.IsNullable,
                        Type = ResolveTsType(csharpProperty.Type)
                    })
                });

            var sb = new StringBuilder();

            foreach (var tsInterface in typescriptInferfaces)
            {
                sb.AppendLine($"export interface {tsInterface.InterfaceName} {{");
                
                foreach (var property in tsInterface.Properties)
                {
                    var formattedPropertyName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
                    var type = property.IsNullable ? $"{property.Type} | null" : property.Type;
                    
                    sb.AppendLine($"\t{property.Name}: {type};");
                }

                sb.AppendLine("}");
            }

            return sb.ToString();
        }

        private string ResolveTsType(string cSharpType)
        {
            if (_typePrimitiveCatalog.TryGetValue(cSharpType, out var primitiveType)) 
                return primitiveType;

            if (cSharpType.EndsWith("[]"))
            {
                var potentialType = cSharpType.Split('[').First();

                if (_typePrimitiveCatalog.TryGetValue(potentialType, out var type))
                    return type + "[]";
            }

            if (TypeCatalog.CSharpCollectionTypes.Any(cSharpType.StartsWith))
            {
                int Pos1 = cSharpType.IndexOf('<') + 1;
                int Pos2 = cSharpType.IndexOf('>');
                return cSharpType[Pos1..Pos2] + "[]";
            }

            throw new Exception($"Type {cSharpType} not found in catalog");
        }
    }
}
