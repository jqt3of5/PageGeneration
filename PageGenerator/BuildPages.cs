using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Page.Core;
using System;
using System.IO;
using System.Linq;

namespace Page.Generator
{
    public class BuildPages : Task
    {
        [Required]
        public string OutputDir { get; set; }

        public override bool Execute()
        {
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var trees = Directory
                .GetFiles(".", "*.cs")
                .Select(file => File.ReadAllText(file))
                //This ToList is necessary! No idea why. 
                .Select(source => CSharpSyntaxTree.ParseText(source)).ToList();
                
            var compilation = CSharpCompilation
                .Create("Pages.Interface")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location), 
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                                MetadataReference.CreateFromFile(typeof(ElementAttribute).Assembly.Location))
                .AddSyntaxTrees(trees);
            
            foreach (var d in compilation.GetDiagnostics())
            {
                Console.WriteLine(CSharpDiagnosticFormatter.Instance.Format(d));
            }

            foreach (var tree in trees)
            {
                var model = compilation.GetSemanticModel(tree);
                var rootNode = tree.GetCompilationUnitRoot();

                var interfacePageNodes = rootNode
                    .DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>()
                    .Where(c =>
                        model
                        .GetDeclaredSymbol(c)
                        .GetAttributes()
                        .Any(attribute =>
                            attribute.AttributeClass.Name == typeof(PageAttribute).Name));

                foreach (var interfaceNode in interfacePageNodes)
                {
                    var namespaceName = model.GetDeclaredSymbol(interfaceNode).ContainingNamespace.Name;
                    var generator = new PageBuilder(interfaceNode.Identifier.Text + "_generated", interfaceNode.Identifier.Text, namespaceName);

                    var nonAccessorMethods = interfaceNode
                        .DescendantNodes()
                        .OfType<MethodDeclarationSyntax>();

                    if (nonAccessorMethods.Any())
                    {
                        foreach (var method in nonAccessorMethods)
                        {
                            Log.LogError("Methods are not supported on Page object interfaces. Please use extension methods (Method: " + method.Identifier.Text + ", Page: " + interfaceNode.Identifier.Text + ")");
                        }
                        return false;
                    }

                    var properties = interfaceNode
                        .DescendantNodes()
                        .OfType<PropertyDeclarationSyntax>();

                    foreach (var propertyNode in properties)
                    {
                        var elementAttributes =
                            model
                            .GetDeclaredSymbol(propertyNode)
                            .GetAttributes()
                            .Where(attribute => attribute.AttributeClass.Name == typeof(ElementAttribute).Name);

                        if (elementAttributes.Count() > 1)
                        {
                            Log.LogWarning("Property: " + propertyNode.Identifier + " has multiple Element attributes. Only the last one will be used");
                        }

                        if (elementAttributes.Count() > 0)
                        {
                            var attribute = elementAttributes.LastOrDefault();
                            var locator = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "Locator").Value.Value as string;
                            if (locator == null || locator == "")
                            {
                                Log.LogError("Property: " + propertyNode.Identifier + " must have the Locator property set on the Element attribute");
                                return false;
                            }
                            var findBy = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "FindBy").Value.Value as string;

                            generator.AddElementProperty(propertyNode.Identifier.Text, propertyNode.Type.ToString(), locator, findBy);
                        }
                        else
                        {
                            Log.LogError("Property: " + propertyNode.Identifier + " must be attributed with ElementAttribute");
                            return false;
                        }
                    }
                    generator.GenerateCSharpCode(OutputDir + interfaceNode.Identifier.Text + ".g.cs");
                }
            }
            return true;
        }
    }
}
