using System.ComponentModel.DataAnnotations;
using Conductor.Web.Validations;

namespace Conductor.Web.Models
{
    public class QueueJobViewModel : JobViewModelBase
    {
        [Required]
        [RegularExpression(@"[a-zA-Z][0-9a-zA-Z_-]*")]
        public string Queue { get; set; }

        [Required]
        [AzureStorageValidation]
        public string ConnectionString { get; set; }
    }
}