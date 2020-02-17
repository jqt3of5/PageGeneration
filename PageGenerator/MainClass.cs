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
            Main(null);
            return true;
        }

        void Main(string[] args)
        {
            AppDomain.CurrentDomain.Load(InterfacesAssembly);
            var pageInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s =>
                    s.GetTypes())
                    .Where(t =>
                        t.CustomAttributes.Any(a =>
                            a.AttributeType == typeof(PageAttribute)));



            foreach (var page in pageInterfaces)
            {
                var generator = new PageBuilder(page.Name + "_generated", page.Name);

                var properties = page.GetProperties()
                    .Where(p => 
                        p.CustomAttributes.Any(a => 
                            a.AttributeType == typeof(ElementAttribute)));

                foreach (var property in properties) 
                {
                    //TODO: Assert property type must inherit from Element / Property type must have a constructor that takes two arguments
                    if (property.CustomAttributes.Count(a => a.AttributeType == typeof(ElementAttribute)) > 1 )
                    {
                        Log.LogWarning("Property: " + property.Name + " has multiple Element attributes. Only the first one will be used");
                    }
                    var elementAttribute = property.CustomAttributes.First(a => a.AttributeType == typeof(ElementAttribute));
                    var locator = elementAttribute.NamedArguments.First(a => a.MemberName == "Locator").TypedValue.Value as string;
                    if (locator == null || locator == "")
                    {
                        Log.LogError("Property: " + property.Name + " must have the Locator property set on the Element attribute");
                        return;
                    }
                    
                    var findBy = elementAttribute.NamedArguments.First(a => a.MemberName == "FindBy").TypedValue.Value as string;

                    generator.AddElementProperty(property.Name, property.PropertyType, locator, findBy);
                }
                
                generator.GenerateCSharpCode("generated/" + page.Name + ".g.cs");
            }

        }

        
    }
}
