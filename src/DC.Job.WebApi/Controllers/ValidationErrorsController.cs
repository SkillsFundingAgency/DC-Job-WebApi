using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DC.Job.WebApi.Reports;
using ESFA.DC.ILR.ValidationErrorReport.Model;
using ESFA.DC.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValidationErrorsController : ControllerBase
    {
        private readonly IValidationErrorsService _validationErrorsService;
        private readonly ILogger _logger;

        public ValidationErrorsController(IValidationErrorsService validationErrorsService, ILogger logger)
        {
            _validationErrorsService = validationErrorsService;
            _logger = logger;
        }

        [HttpGet("{ukprn}/{jobId}")]
        public async Task<IEnumerable<ReportValidationError>> Get(long ukprn, long jobId)
        {
            _logger.LogInfo($"Get request recieved for validation errors ukprn : {ukprn}, Job id: {jobId}");

            return await _validationErrorsService.GetValidationErrorsAsync(ukprn, jobId);
        }
    }
}