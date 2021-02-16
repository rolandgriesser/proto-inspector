using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

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
                Message = "You have to provide a proto description.";
                return Page();
            }
            ExtractedTypes = new List<Type>() { typeof(String) };
            return Page();
        }
        [BindProperty]
        public string Proto { get; set; }
        public List<Type> ExtractedTypes { get; set; } = new List<Type>();
        [BindProperty]
        public string SelectedType { get; set; }
        [BindProperty]
        public string HexMessage { get; set; }

        public string Message { get; set; }

        public async Task<IActionResult> OnPostParseMessageAsync()
        {
            if (string.IsNullOrEmpty(SelectedType))
            {
                Message = "No type selected.";
                return Page();
            }
            if (string.IsNullOrEmpty(HexMessage))
            {
                Message = "No hex message provided.";
                return Page();
            }
            Message = SelectedType;
            return Page();
        }
    }
}
