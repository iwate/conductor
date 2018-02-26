using System;
using System.ComponentModel.DataAnnotations;
using Conductor.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.WindowsAzure.Storage;

namespace Conductor.Web.Validations
{
    public class AzureStorageValidationAttribute : ValidationAttribute
    {
        public AzureStorageValidationAttribute()
        {}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            CloudStorageAccount account;
            
            return CloudStorageAccount.TryParse((string)value, out account) ? ValidationResult.Success : new ValidationResult("Invalid Azure storage connection string.");
        }
    }
}