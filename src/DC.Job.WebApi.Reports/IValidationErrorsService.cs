using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.ILR.ValidationErrorReport.Model;

namespace DC.Job.WebApi.Reports
{
    public interface IValidationErrorsService
    {
        Task<IEnumerable<ReportValidationError>> GetValidationErrorsAsync(long ukprn, long jobId);
    }
}
