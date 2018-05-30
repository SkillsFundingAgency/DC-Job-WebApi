using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.ILR.ValidationErrors.Interface;
using ESFA.DC.ILR.ValidationErrors.Interface.Models;
using ESFA.DC.KeyGenerator.Interface;
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
        private readonly IKeyGenerator _keyGenerator;

        public ValidationErrorsController(IValidationErrorsService validationErrorsService, ILogger logger, IKeyGenerator keyGenerator)
        {
            _validationErrorsService = validationErrorsService;
            _logger = logger;
            _keyGenerator = keyGenerator;
        }

        [HttpGet("{ukprn}/{jobId}")]
        public async Task<IEnumerable<ValidationErrorDto>> Get(long ukprn, long jobId)
        {
            _logger.LogInfo($"Get request recieved for validation errors ukprn : {ukprn}, Job id: {jobId}");

            var validationErrorsKey = _keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrors, "_");
            var errorLookupKey = _keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrorsLookup, "_");
            return await _validationErrorsService.GetValidationErrorsAsync(validationErrorsKey, errorLookupKey);
        }
    }
}