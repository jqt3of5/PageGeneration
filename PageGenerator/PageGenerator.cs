using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using System.CodeDom;
using System.CodeDom.Compiler;

namespace PageGenerator
{
    public class PageGenerator
    {
        CodeTypeDeclaration targetClass;
        CodeCompileUnit targetUnit;
        public PageGenerator()
        {
            
            var pageGeneratorNamespace = new CodeNamespace("PageGenerator");
            pageGeneratorNamespace.Imports.Add(new CodeNamespaceImport("System"));

            targetClass = new CodeTypeDeclaration("Page");
            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;

            pageGeneratorNamespace.Types.Add(targetClass);


            targetUnit = new CodeCompileUnit();
            targetUnit.Namespaces.Add(pageGeneratorNamespace);
        }

        public void AddFields()
        {
            var idValueField = new CodeMemberField();
            idValueField.Attributes = MemberAttributes.Public;
            idValueField.Name = "Id";
            idValueField.Type = new CodeTypeReference(typeof(string));

            targetClass.Members.Add(idValueField);


        }

        public void AddEntryPoint()
        {
            var start = new CodeEntryPointMethod();

            var objectCreate = new CodeObjectCreateExpression(new CodeTypeReference("PageGenerator"));

            start.Statements.Add(new CodeVariableDeclarationStatement(new CodeTypeReference("PageGenerator"), "testClass", objectCreate));


        }

        public void GenerateCSharpCode(string filename)
        {
            var provider = CodeDomProvider.CreateProvider("CSharp");
            var options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                provider.GenerateCodeFromCompileUnit(targetUnit, writer, options);
            }
        }
    }
}
