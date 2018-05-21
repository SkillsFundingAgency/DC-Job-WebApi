using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ValidationErrorStub
{
    public class ValidationErrorsReportService : IValidationErrorsReportService
    {
        public async Task<IEnumerable<ValidationError>> GetValidationErrorsAsync(long ukprn)
        {
            return new List<ValidationError>()
            {
                new ValidationError()
                {
                    AimSequenceNumber = 1,
                    ErrorMessage = "Test error message",
                    FieldValues = string.Empty,
                    LearnRefNumber = "LEARN1",
                    RuleId = "RULE_1",
                    Severity = "Error"
                }
            };
        }
    }
}
