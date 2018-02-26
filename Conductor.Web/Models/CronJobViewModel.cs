using System.ComponentModel.DataAnnotations;
using Conductor.Web.Validations;

namespace Conductor.Web.Models
{
    public class CronJobViewModel : JobViewModelBase
    {
        [Required]
        [CronValidation]
        public string Cron { get; set; } = "0 0 * * * *";
    }
}