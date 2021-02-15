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
        static Action<string> Write = Console.WriteLine;
        /// <summary>
        /// 
        /// </summary>https://laurentkempe.com/2019/02/18/dynamically-compile-and-run-code-using-dotNET-Core-3.0/

        [Fact]
        public void Test1()
        {
            // var proto = System.IO.File.ReadAllText("../../../person.proto");
            // System.IO.File.WriteAllText("temp.proto", proto);
            // Process.Start("protoc --csharp_out=. temp.proto");
            // var provider = CodeDomProvider.CreateProvider("CSharp");
            // var options = new CompilerParameters();
            // var assembly = provider.CompileAssemblyFromFile(options, "../../../Person.cs");

            //             string source =
            //                         @"
            // namespace Foo
            // {
            //     public class Bar
            //     {
            //         public void SayHello()
            //         {
            //             System.Console.WriteLine(""Hello World"");
            //         }
            //     }
            // }
            //             ";

            //             Dictionary<string, string> providerOptions = new Dictionary<string, string>
            //                 {
            //                     {"CompilerVersion", "v3.5"}
            //                 };
            //             CSharpCodeProvider provider = new CSharpCodeProvider(providerOptions);

            //             CompilerParameters compilerParams = new CompilerParameters
            //             {
            //                 GenerateInMemory = true,
            //                 GenerateExecutable = false
            //             };

            //             CompilerResults results = provider.CompileAssemblyFromSource(compilerParams, source);

            //             if (results.Errors.Count != 0)
            //                 throw new Exception("Mission failed!");

            //             object o = results.CompiledAssembly.CreateInstance("Foo.Bar");
            //             MethodInfo mi = o.GetType().GetMethod("SayHello");
            //             mi.Invoke(o, null);

            //             System.Console.WriteLine("done");

            Write("Let's compile!");

            string codeToCompile = @"
            using System;
            namespace RoslynCompileSample
            {
                public class Writer
                {
                    public void Write(string message)
                    {
                        Console.WriteLine($""you said '{message}!'"");
                    }
                }
            }";

            Write("Parsing the code into the SyntaxTree");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(codeToCompile);

            string assemblyName = Path.GetRandomFileName();
            var refPaths = new[] {
                typeof(System.Object).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll")
            };
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            Write("Adding the following references");
            foreach (var r in refPaths)
                Write(r);

            Write("Compiling ...");
            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    Write("Compilation failed!");
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
                    Write("Compilation successful! Now instantiating and executing the code ...");
                    ms.Seek(0, SeekOrigin.Begin);

                    Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("RoslynCompileSample.Writer");
                    var instance = assembly.CreateInstance("RoslynCompileSample.Writer");
                    var meth = type.GetMember("Write").First() as MethodInfo;
                    meth.Invoke(instance, new[] { "joel" });
                }
            }
        }
    }
}
