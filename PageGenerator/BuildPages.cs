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
            foreach(var file in Directory.GetFiles(".", "*.cs"))
            {
                var source = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(source);

               
                var compilation = CSharpCompilation
                    .Create("Pages.Interface")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(string).Assembly.Location),
                                    MetadataReference.CreateFromFile(typeof(ElementAttribute).Assembly.Location))
                    .AddSyntaxTrees(tree);

                var model = compilation.GetSemanticModel(tree);

                foreach (var d in compilation.GetDiagnostics())
                {
                    Console.WriteLine(CSharpDiagnosticFormatter.Instance.Format(d));
                }

                var rootNode = tree.GetCompilationUnitRoot();
                //var rootNode = tree.GetRoot();

                var interfaceNodes = rootNode
                    .DescendantNodes()
                    .OfType<InterfaceDeclarationSyntax>();
                foreach (var node in interfaceNodes)
                {
                    var attributes = model.GetTypeInfo(node).Type.GetAttributes();
                    
                }
                    /*.Where(c =>
                        model
                        .GetTypeInfo(c)
                        .Type.GetAttributes()
                        .Any(attribute =>
                            attribute.AttributeClass.Name == typeof(PageAttribute).Name));*/

                foreach (var interfaceNode in interfaceNodes)
                {
                    foreach(var propertyNode in interfaceNode.Members.OfType<PropertyDeclarationSyntax>())
                    {

                        var elementAttributes = model.GetSymbolInfo(propertyNode).Symbol.GetAttributes().Where(attribute => attribute.AttributeClass.Name == typeof(ElementAttribute).Name);

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
                            var findBy = attribute.NamedArguments.FirstOrDefault(arg => arg.Key == "FindBy").Value;
                            
                            //generator.AddElementProperty(propertyNode.Identifier, property.PropertyType, locator, findBy);
                        }
                        else
                        {
                            Log.LogError("Property: " + propertyNode.Identifier + " must be attributed with ElementAttribute");
                            return false;
                        }
                    }
                }
            }

            

           /* var pageInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s =>
                    s.GetTypes())
                    .Where(t =>
                        t.CustomAttributes.Any(a =>
                            a.AttributeType == typeof(PageAttribute)));

            foreach (var page in pageInterfaces)
            {
                var generator = new PageBuilder(page.Name + "_generated", page.Name, page.Namespace);

                var nonAccessorMethods = page.GetMethods().Where(m => !page.GetProperties().Any(p => m.Name.EndsWith(p.Name)));
                if (nonAccessorMethods.Any())
                {
                    foreach (var method in nonAccessorMethods)
                    {
                        Log.LogError("Methods are not supported on Page object interfaces. Please use extension methods (Method: " + method.Name + ", Page: " + page.Name + ")");
                    }
                    return false;
                }
                
                
                foreach (var property in page.GetProperties()) 
                {
                    var elementAttributions = property.CustomAttributes.Count(a => a.AttributeType == typeof(ElementAttribute));
                    if (elementAttributions > 1)
                    {
                        Log.LogWarning("Property: " + property.Name + " has multiple Element attributes. Only the first one will be used");
                    }

                    if (elementAttributions > 0)
                    {
                        var elementAttribute = property.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(ElementAttribute));
                        var locator = elementAttribute.NamedArguments.FirstOrDefault(a => a.MemberName == "Locator").TypedValue.Value as string;

                        if (locator == null || locator == "")
                        {
                            Log.LogError("Property: " + property.Name + " must have the Locator property set on the Element attribute");
                            return false;
                        }

                        var findBy = elementAttribute.NamedArguments.FirstOrDefault(a => a.MemberName == "FindBy").TypedValue.Value as string;
                        generator.AddElementProperty(property.Name, property.PropertyType, locator, findBy);
                    }
                    else
                    {
                        Log.LogError("Property: " + property.Name + " must be attributed with ElementAttribute");
                        return false;
                        //generator.AddProperty(property);
                    }
                }
                
                generator.GenerateCSharpCode("generated/" + page.Name + ".g.cs");
            }
            */
            return true;
        }
    }
}
