using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Page.Core;
using System;
using System.Linq;

namespace Page.Generator
{
    public class BuildPages : Task
    {

        [Required]
        public string InterfacesAssembly { get; set; }
        public override bool Execute()
        {
            var assembly = AppDomain.CurrentDomain.Load(InterfacesAssembly);
            
            var pageInterfaces = AppDomain.CurrentDomain.GetAssemblies()
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

            return true;
        }
    }
}
