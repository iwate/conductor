using System;
using System.ComponentModel.DataAnnotations;
using Conductor.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Conductor.Web.Validations
{
    public class CronValidationAttribute : ValidationAttribute
    {
        public CronValidationAttribute()
        {}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var cron = (string)value;
            try
            {
                Cron.Parse(cron);
                return ValidationResult.Success;
            }
            catch(ArgumentNullException)
            {
                return new ValidationResult("Required");
            }
            catch(ArgumentOutOfRangeException)
            {
                return  new ValidationResult("Format should be `s m h d M w`");
            }
            catch(ArgumentException ex)
            {
                return new ValidationResult(ex.Message);
            }
        }
    }
}