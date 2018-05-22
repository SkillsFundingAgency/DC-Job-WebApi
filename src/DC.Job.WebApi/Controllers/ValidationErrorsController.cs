using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.Logging.Interfaces;
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
        private readonly ILogger _logger;

        public ValidationErrorsController(IValidationErrorsReportService validationErrorsReportService, ILogger logger)
        {
            _validationErrorsReportService = validationErrorsReportService;
            _logger = logger;
        }

        [HttpGet("{ukprn}")]
        public async Task<IEnumerable<ValidationError>> Get(long ukprn)
        {
            _logger.LogInfo($"Get request recieved for validation errors ukprn : {ukprn}");

            return await _validationErrorsReportService.GetValidationErrorsAsync(ukprn);
        }
    }
}