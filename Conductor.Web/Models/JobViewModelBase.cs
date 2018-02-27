using System.ComponentModel.DataAnnotations;
using Conductor.Web.Validations;
using Microsoft.AspNetCore.Mvc;

namespace Conductor.Web.Models
{
    public abstract class JobViewModelBase
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Enabled { get; set; }
        [Required]
        public OS OS { get; set; } = OS.Windows;
        [Required]
        public double CPU { get; set; } = 2.5;
        [Required]
        public double Memory { get; set; } = 1.0;
        [Required]
        public bool Private { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        [JsonDictValidation]
        public string EnvVariables { get; set; } = "{\n  \"MSG\":\"Hello, World!\"\n}";
    }
}