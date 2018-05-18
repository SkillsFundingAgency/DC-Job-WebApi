using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ValidationErrorStub;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValidationErrorsController : ControllerBase
    {
        private readonly IValidationErrorsReportService _validationErrorsReportService;

        public ValidationErrorsController(IValidationErrorsReportService validationErrorsReportService)
        {
            _validationErrorsReportService = validationErrorsReportService;
        }

        [HttpGet("{jobId}")]
        public async Task<IEnumerable<ValidationError>> Get(long jobId)
        {
            return await _validationErrorsReportService.GetValidationErrorsAsync(jobId);
        }
    }
}