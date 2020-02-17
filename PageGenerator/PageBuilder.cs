using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

using System.CodeDom;
using System.CodeDom.Compiler;
using System.Linq;
using Page.Core;

namespace Page.Generator
{
    public class PageBuilder
    {
        CodeTypeDeclaration targetClass;
        CodeCompileUnit targetUnit;
        public PageBuilder(string className, string baseName)
        {
            var pageGeneratorNamespace = new CodeNamespace("PageGenerator");
            pageGeneratorNamespace.Imports.Add(new CodeNamespaceImport("System"));

            targetClass = new CodeTypeDeclaration(className);
            targetClass.IsClass = true;
            targetClass.TypeAttributes = TypeAttributes.Public;
            
            targetClass.BaseTypes.Add(new CodeTypeReference(baseName));

            pageGeneratorNamespace.Types.Add(targetClass);

            targetUnit = new CodeCompileUnit();
            targetUnit.Namespaces.Add(pageGeneratorNamespace);
        }

        public PageBuilder AddElementProperty(string name, Type type, string locator, string findBy)
        {
            var ivar = new CodeMemberField();
            ivar.Attributes = MemberAttributes.Private;
            ivar.Name = name + "_ivar";
            ivar.Type = new CodeTypeReference("Lazy", new CodeTypeReference(type));

            targetClass.Members.Add(ivar);

            var property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public;
            property.Name = name;
            property.Type = new CodeTypeReference(type);
            property.HasGet = true;

            var expression = new CodeMethodReturnStatement(
                new CodePropertyReferenceExpression(
                    new CodeFieldReferenceExpression(
                        new CodeThisReferenceExpression(), ivar.Name), 
                "Value"));
            
            property.GetStatements.Add(expression);

            targetClass.Members.Add(property);

            return this;
        }

        public PageBuilder AddField(string name, Type type)
        {
            var idValueField = new CodeMemberField();
            idValueField.Attributes = MemberAttributes.Public;
            idValueField.Name = name;
            idValueField.Type = new CodeTypeReference(type);

            targetClass.Members.Add(idValueField);

            return this;
        }

        public PageBuilder AddMethod(string name, Type returnType, params Type [] parameters)
        {
            var method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public;
            method.Name = name;
            method.ReturnType = new CodeTypeReference(returnType);

            var returnStatement = new CodeMethodReturnStatement();

            method.Statements.Add(returnStatement);

            targetClass.Members.Add(method);

            return this;
        }

        public PageBuilder AddWindowProperty(string name, Type type, string id, string findBy)
        {
            var property = new CodeMemberProperty();
            property.Attributes = MemberAttributes.Public;
            property.Name = name;
            property.Type = new CodeTypeReference(type);
            property.HasGet = true;
            
            targetClass.Members.Add(property);

            return this;
        }

        public PageBuilder AddCloseWindowMethod()
        {
            var method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Public;
            method.Name = "CloseWindow";
            method.ReturnType = new CodeTypeReference(typeof(void));

            targetClass.Members.Add(method);

            return this;
        }


        public void GenerateCSharpCode(string filename)
        {
            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public;

            //constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "str"));

            targetClass.Members.Add(constructor);


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
