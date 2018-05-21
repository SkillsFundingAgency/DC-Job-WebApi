using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ValidationErrorStub
{
    public interface IValidationErrorsReportService
    {
        Task<IEnumerable<ValidationError>> GetValidationErrorsAsync(long ukprn);
    }
}
