using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.ILR.ValidationErrors.Interface;
using ESFA.DC.ILR.ValidationErrors.Interface.Models;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValidationErrorsController : ControllerBase
    {
        private readonly IKeyValuePersistenceService _keyValuePersistenceService;
        private readonly ISerializationService _serializationService;
        private readonly ILogger _logger;
        private readonly IKeyGenerator _keyGenerator;

        public ValidationErrorsController(IKeyValuePersistenceService keyValuePersistenceService, ILogger logger, IKeyGenerator keyGenerator, ISerializationService serializationService)
        {
            _keyValuePersistenceService = keyValuePersistenceService;
            _logger = logger;
            _keyGenerator = keyGenerator;
            _serializationService = serializationService;
        }

        [HttpGet("{ukprn}/{jobId}")]
        public async Task<IEnumerable<ValidationErrorDto>> Get(long ukprn, long jobId)
        {
            _logger.LogInfo($"Get request recieved for validation errors ukprn : {ukprn}, Job id: {jobId}");
            try
            {
                var validationErrorsKey = $"{_keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrors)}.json";
                var exists = await _keyValuePersistenceService.ContainsAsync(validationErrorsKey);
                if (exists)
                {
                    var data = await _keyValuePersistenceService.GetAsync(validationErrorsKey);
                    return _serializationService.Deserialize<IEnumerable<ValidationErrorDto>>(data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured trying to get validation errors ukprn : {ukprn}, Job id: {jobId}", ex);
                throw;
            }

            return new List<ValidationErrorDto>();
        }
    }
}