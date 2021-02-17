using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace ProtoInspector.Helpers
{
    public static class ReflectionHelpers
    {
        private static string GetReferencePath(Type type, string dllName = null)
        {
            var location = type.GetTypeInfo().Assembly.Location;
            if (string.IsNullOrEmpty(dllName))
                return location;
            return Path.Combine(Path.GetDirectoryName(location), dllName);
        }

        public static MemoryStream CreateAssemblyFromCode(string code)
        {
            System.Console.WriteLine("Parsing C# syntax tree...");
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            System.Console.WriteLine("Parsed syntax tree.");

            var assemblyName = Path.GetRandomFileName();

            var refPaths = new[] {
                GetReferencePath(typeof(System.Object)),
                GetReferencePath(typeof(Google.Protobuf.IMessage)),
                GetReferencePath(typeof(Console)),
                GetReferencePath(typeof(IEquatable<>)),
                GetReferencePath(typeof(Enumerable), "netstandard.dll"),
                GetReferencePath(typeof(System.Runtime.GCSettings), "System.Runtime.dll")
            };
            var references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();

            // Write("Adding the following references");
            // foreach (var r in refPaths)
            //     Write(r);

            System.Console.WriteLine("Compiling assembly...");
            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("\t{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }

                throw new Exception("Couldn't create compilation from code.");
            }
            System.Console.WriteLine("Successfully compilated assembly.");
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static List<Type> GetTypes<T>(Assembly assembly)
        {
            return assembly.GetTypes().Where(i => i.IsAssignableFrom(typeof(T))).ToList();
        }
    }
}