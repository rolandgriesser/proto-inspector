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
            var fileName = "OuterProtoClass.proto";
            var fullPath = Path.Combine(path, fileName);
            System.Console.WriteLine($"Writing proto file to {fullPath}.");
            await File.WriteAllTextAsync(fullPath, content);
            System.Console.WriteLine($"Wrote proto file to {fullPath}.");
            return fullPath;
        }

        private static async Task<string> RunProtoc(string dir, string inputFile)
        {
            if (!dir.EndsWith(Path.DirectorySeparatorChar))
                dir += Path.DirectorySeparatorChar;
            // var command = $"/usr/local/bin/protoc --csharp_out={dir} -I {dir} {inputFile}";
            var procStartInfo = new ProcessStartInfo("protoc")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                Arguments = $"--csharp_out={dir} -I {dir} {inputFile}"
            };
            System.Console.WriteLine($"Starting process {procStartInfo.FileName} {procStartInfo.Arguments}.");
            var proc = new Process { StartInfo = procStartInfo };
            proc.Start();
            await proc.WaitForExitAsync();
            System.Console.WriteLine($"Finished protoc, exit code: {proc.ExitCode}");
            var output = proc.StandardOutput.ReadToEnd();
            System.Console.WriteLine("Protoc output: " + output);

            var cSharpFiles = Directory.EnumerateFiles(dir, "*.cs", SearchOption.TopDirectoryOnly);
            System.Console.WriteLine("Found the following C# files:");
            System.Console.WriteLine(string.Join('\n', cSharpFiles));
            return cSharpFiles.FirstOrDefault();
        }
        public static async Task<string> CreateProtoClass(string description)
        {
            var tempDirName = Path.GetRandomFileName();
            var tempDirPath = Path.Combine(Path.GetTempPath(), tempDirName);
            var tempDirInfo = Directory.CreateDirectory(tempDirPath);
            try
            {
                var protoFile = await CreateProtoFile(tempDirInfo.FullName, description);
                var classFileName = await RunProtoc(tempDirInfo.FullName, protoFile);
                if (string.IsNullOrEmpty(classFileName))
                    throw new System.Exception("Error creating csharp output file.");
                var classContent = await File.ReadAllTextAsync(classFileName);
                System.Console.WriteLine($"Successfully read C# file: {classFileName}");
                return classContent;
            }
            finally
            {
                tempDirInfo.Delete(true);
                System.Console.WriteLine($"Successfully deleted temp dir: {tempDirInfo.FullName}");
            }
        }
    }
}