using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace ProtoInspector.Helpers
{
    public static class ProtoHelpers
    {
        private static async Task<string> CreateProtoFile(string path, string content)
        {
            var fileName = Path.GetRandomFileName() + ".proto";
            var fullPath = Path.Combine(path, fileName);
            await File.WriteAllTextAsync(fullPath, content);
            return fullPath;
        }

        private static async Task<string> RunProtoc(string dir, string inputFile)
        {
            await Process.Start($"protoc --csharp_out={dir} {inputFile}").WaitForExitAsync();
            var cSharpFiles = Directory.EnumerateFiles(dir, "*.cs", SearchOption.TopDirectoryOnly);
            return cSharpFiles.FirstOrDefault();
        }
        public static async Task<string> CreateProtoClass(string description)
        {
            var tempDirName = Path.GetRandomFileName();
            var tempDirInfo = Directory.CreateDirectory(tempDirName);
            var protoFile = await CreateProtoFile(tempDirInfo.FullName, description);
            var classFileName = await RunProtoc(tempDirInfo.FullName, protoFile);
            if (string.IsNullOrEmpty(classFileName))
                throw new System.Exception("Error creating csharp output file.");
            var classContent = await File.ReadAllTextAsync(classFileName);
            tempDirInfo.Delete(true);
            return classContent;
        }
    }
}