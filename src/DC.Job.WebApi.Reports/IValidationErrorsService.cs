using System.Collections.Generic;
using System.Threading.Tasks;

namespace DC.Job.WebApi.Reports
{
    public interface IValidationErrorsService
    {
        Task<IEnumerable<ValidationError>> GetValidationErrorsAsync(long ukprn, long jobId);
    }
}
