// Learn more about F# at http://fsharp.org

open System
open System.IO
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis
open System.Linq
open Microsoft.CodeAnalysis.CSharp.Syntax

let flatten = Array.fold (fun state item -> Array.append state item) [||] 

let trees = 
    let sourcefiles = Directory.GetFiles(".", "TestInterface.cs")
    let sources = Array.map File.ReadAllText sourcefiles 
    let parseText (s:string) = CSharpSyntaxTree.ParseText s
    Array.map parseText sources

let compilation = 
    let obj = MetadataReference.CreateFromFile(typeof<Object>.Assembly.Location)
    let compilationOptions = CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
    CSharpCompilation.Create("Pages").WithOptions(compilationOptions).AddReferences(obj).AddSyntaxTrees(trees)


let getPageInterfaceWithModel (tree : SyntaxTree) = 
    let interfaces = tree.GetCompilationUnitRoot().DescendantNodes().OfType<InterfaceDeclarationSyntax>().ToArray()
    let model = compilation.GetSemanticModel(tree, false)
    let isPageInterface (interf : InterfaceDeclarationSyntax) = model.GetDeclaredSymbol(interf).GetAttributes().Any(fun attribute -> attribute.AttributeClass.Name = "PageAttribute")
    let pageInterfaces = Array.filter isPageInterface interfaces
    Array.map (fun page -> (page, model)) pageInterfaces
       
let pageInterfacesWithModel = Array.map getPageInterfaceWithModel trees |> flatten


let generate (node : InterfaceDeclarationSyntax, model: SemanticModel) = 
    let elementNodes = node.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToArray()
    let propertyTemplate = "
    public {0} {1} {{ 
        get 
        {{
            Window.Activate();
            return t;
        }}
    }}
    "
    let namespaceName = model.GetDeclaredSymbol(node).ContainingNamespace.Name
    let interfaceName = node.Identifier.Text
    let implTemplate = "
    using System;
    namespace {0} {{
    public class {1} : {2}
    {{
    {3}
    }}
    }}
    "
    let implementation = String.Format( implTemplate, namespaceName, (interfaceName + "_impl"), interfaceName, "")
    
    File.WriteAllText (interfaceName + "_impl.cs", implementation)

[<EntryPoint>]
let main argv =
    Array.fold (fun s d -> 
        (printfn "%s" (CSharpDiagnosticFormatter.Instance.Format d)) 
        s) 0 (compilation.GetDiagnostics().ToArray()) |> ignore
    Array.map generate pageInterfacesWithModel |> ignore
    0 // return an integer exit code



