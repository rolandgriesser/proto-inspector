using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using ProtoInspector.Helpers;

namespace ProtoInspector.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostLoadTypesAsync()
        {
            if (string.IsNullOrEmpty(Proto))
            {
                ErrorMessage = "You have to provide a proto description.";
                return Page();
            }
            var csharpCode = await Helpers.ProtoHelpers.CreateProtoClass(Proto);
            var assemblyStream = Helpers.ReflectionHelpers.CreateAssemblyFromCode(csharpCode);
            Assembly = assemblyStream.ToArray().ToHexString();
            System.Console.WriteLine("Set Assembly property to: " + Assembly.Substring(0, 20) + "...");
            var assembly = LoadAssembly(assemblyStream);
            ExtractedTypes = ReflectionHelpers.GetTypes<Google.Protobuf.IMessage>(assembly).ToList();
            System.Console.WriteLine($"Found {ExtractedTypes.Count} classes that implement Google.Protobuf.IMessage:");
            System.Console.WriteLine(string.Join('\n', ExtractedTypes));
            if (ExtractedTypes.Count == 0)
            {
                ErrorMessage = "Couldn't find any types that implement Google.Protobuf.IMessage in assembly.";
            }
            return Page();
        }

        private Assembly LoadAssembly(Stream stream)
        {
            var assembly = AssemblyLoadContext.Default.LoadFromStream(stream);
            System.Console.WriteLine("Successfully loaded assembly from stream.");
            return assembly;
        }
        private Assembly LoadAssembly(string hexString)
        {
            var memoryStream = new MemoryStream(hexString.HexToByteArray());
            return LoadAssembly(memoryStream);
        }

        [BindProperty]
        public string Proto { get; set; }
        [BindProperty]
        public string Assembly { get; set; }
        public List<Type> ExtractedTypes { get; set; } = new List<Type>();
        public SelectList ExtractedTypesItems => new SelectList(ExtractedTypes.Select(i => i.FullName));
        [BindProperty]
        public string SelectedType { get; set; }
        [BindProperty]
        public string HexMessage { get; set; }
        [BindProperty]
        public string JsonText { get; set; }

        public string ErrorMessage { get; set; }

        public IActionResult OnPostToJson()
        {
            var type = LoadTypeFromAssembly();
            if (type == null)
                return Page();

            var protoObject = Activator.CreateInstance(type);
            if (protoObject == null)
            {
                ErrorMessage = "Couldn't create object of the specified type.";
                System.Console.WriteLine(ErrorMessage);
                return Page();
            }
            System.Console.WriteLine("Successfully created object of specified type.");
            if (!(protoObject is Google.Protobuf.IMessage message))
            {
                ErrorMessage = "Created object doesn't implement Google.Protobuf.IMessage.";
                System.Console.WriteLine(ErrorMessage);
                return Page();
            }
            var ms = new MemoryStream(HexMessage.HexToByteArray());
            message.MergeFrom(ms);
            System.Console.WriteLine("Successfully merged data.");
            JsonText = JsonSerializer.Serialize(message, type);
            return Page();
        }
        public IActionResult OnPostFromJson()
        {
            var type = LoadTypeFromAssembly();
            if (type == null)
                return Page();

            var jsonText = string.IsNullOrEmpty(JsonText) ? "{}" : JsonText;
            var deserializedObject = JsonSerializer.Deserialize(jsonText, type);
            if (deserializedObject == null)
            {
                ErrorMessage = "Couldn't deserialize Json.";
                System.Console.WriteLine(ErrorMessage);
                return Page();
            }
            System.Console.WriteLine($"Successfully deserialized object: {deserializedObject.ToString()}.");
            if (!(deserializedObject is Google.Protobuf.IMessage message))
            {
                ErrorMessage = "Deserialized object doesn't implement Google.Protobuf.IMessage.";
                System.Console.WriteLine(ErrorMessage);
                return Page();
            }
            var ms = new MemoryStream();
            message.WriteTo(ms);
            HexMessage = ms.ToArray().ToHexString();
            return Page();
        }

        private Type LoadTypeFromAssembly()
        {
            if (string.IsNullOrEmpty(Assembly))
            {
                ErrorMessage = "Assembly not loaded.";
                return null;
            }
            if (string.IsNullOrEmpty(SelectedType))
            {
                ErrorMessage = "No type selected.";
                return null;
            }

            var assembly = LoadAssembly(Assembly);
            ExtractedTypes = ReflectionHelpers.GetTypes<Google.Protobuf.IMessage>(assembly).ToList();
            var type = assembly.GetType(SelectedType);
            if (type == null)
            {
                ErrorMessage = $"Couldn't find type {SelectedType} in loaded assembly";
                System.Console.WriteLine(ErrorMessage);
                return null;
            }
            System.Console.WriteLine($"Successfully loaded type {SelectedType} from assembly.");
            return type;
        }
    }
}
