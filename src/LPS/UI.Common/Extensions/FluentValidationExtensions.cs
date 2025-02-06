using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPS.UI.Common.Extensions
{
    public static class FluentValidationExtensions
    {
        public static void PrintValidationErrors(this ValidationResult validationResult)
        {
            var groupedErrors = validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToList()
                );

            foreach (var kv in groupedErrors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{kv.Key}:");
                Console.ResetColor();

                foreach (var error in kv.Value)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"- {error}");
                    Console.ResetColor();
                }
            }
        }
    }

}
