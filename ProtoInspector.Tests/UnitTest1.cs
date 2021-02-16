using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace ProtoInspector.Tests
{
    public class UnitTest1
    {
        /// <summary>
        /// 
        /// </summary>https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

        [Fact]
        public void Test1()
        {
            // var proto = System.IO.File.ReadAllText("../../../person.proto");
            // System.IO.File.WriteAllText("temp.proto", proto);
            // Process.Start("protoc --csharp_out=. temp.proto");

            var codeToCompile = System.IO.File.ReadAllText("../../../Person.cs");
            // string codeToCompile = @"
            // using System;
            // namespace RoslynCompileSample
            // {
            //     public class Person
            //     {
            //         public string Name  { get; set; }
            //     }
            // }";

            var syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            var assemblyName = Path.GetRandomFileName();
            var dd = typeof(Enumerable).GetTypeInfo().Assembly.Location;
            var coreDir = Directory.GetParent(dd);

            // MetadataReference.CreateFromFile(coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll")

            var refPaths = new[] {
                typeof(System.Object).GetTypeInfo().Assembly.Location,
                typeof(Google.Protobuf.IMessage).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                typeof(IEquatable<>).GetTypeInfo().Assembly.Location,
                coreDir.FullName + Path.DirectorySeparatorChar + "netstandard.dll",
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
            var references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            // Write("Adding the following references");
            // foreach (var r in refPaths)
            //     Write(r);

            // Write("Compiling ...");
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var types = assembly.GetTypes();

                    JsonSerializer.
                    // var exportedTypes = assembly.GetExportedTypes();
                    // var forwardedTypes = assembly.GetForwardedTypes();
                    // var type = assembly.GetType("RoslynCompileSample.Writer");
                    // var instance = assembly.CreateInstance("RoslynCompileSample.Writer");
                    // var meth = type.GetMember("Write").First() as MethodInfo;
                    // meth.Invoke(instance, new[] { "joel" });
                }
            }
        }
    }
}
