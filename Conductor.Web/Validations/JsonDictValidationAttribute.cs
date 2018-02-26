using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Conductor.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Conductor.Web.Validations
{
    public class JsonDictValidationAttribute : ValidationAttribute
    {
        public JsonDictValidationAttribute()
        {}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var schema = JsonSchema.Parse(@"{'type': 'object','additionalProperties': {'type': 'string'} }");
            
            return JToken.Parse((string)value).IsValid(schema) ? ValidationResult.Success : new ValidationResult("Invalid Json dict");
        }
    }
}