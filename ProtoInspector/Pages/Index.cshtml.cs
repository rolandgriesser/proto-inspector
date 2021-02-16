using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
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
            var assembly = LoadAssembly(assemblyStream);
            ExtractedTypes = ReflectionHelpers.GetTypes<Google.Protobuf.IMessage>(assembly);
            return Page();
        }

        private Assembly LoadAssembly(Stream stream)
        {
            return AssemblyLoadContext.Default.LoadFromStream(stream);
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

        public async Task<IActionResult> OnPostParseMessageAsync()
        {
            if (string.IsNullOrEmpty(SelectedType))
            {
                ErrorMessage = "No type selected.";
                return Page();
            }
            if (string.IsNullOrEmpty(HexMessage))
            {
                ErrorMessage = "No hex message provided.";
                return Page();
            }
            ErrorMessage = SelectedType;
            return Page();
        }
    }
}
