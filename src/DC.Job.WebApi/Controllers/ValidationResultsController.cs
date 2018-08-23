using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Reports.ValidationReport;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValidationResultsController : ControllerBase
    {
        private readonly IKeyValuePersistenceService _keyValuePersistenceService;
        private readonly ILogger _logger;
        private readonly IIlrJobQueueManager _jobQueueManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly string _reportFileName = "{0}/{1}/Validation Errors Report {2}.json";

        public ValidationResultsController(
            IKeyValuePersistenceService keyValuePersistenceService,
            ILogger logger,
            IIlrJobQueueManager jobQueueManager,
            IDateTimeProvider dateTimeProvider)
        {
            _keyValuePersistenceService = keyValuePersistenceService;
            _logger = logger;
            _jobQueueManager = jobQueueManager;
            _dateTimeProvider = dateTimeProvider;
        }

        [HttpGet("{ukprn}/{jobId}")]
        public async Task<IActionResult> Get(long ukprn, long jobId)
        {
            _logger.LogInfo($"Get request recieved for validation errors for ukprn {ukprn}, job id: {jobId}");

            if (jobId < 1 || ukprn < 1)
            {
                _logger.LogWarning($"invalid jobId : {jobId}, ukprn : {ukprn}");
                return new BadRequestResult();
            }

            var job = _jobQueueManager.GetJobById(jobId);
            if (job == null || job.Ukprn != ukprn)
            {
                _logger.LogWarning($"No job found for jobId : {jobId}, ukprn : {ukprn}");
                return new BadRequestResult();
            }

            var fileName = GetFileName(ukprn, jobId, job.DateTimeSubmittedUtc);
            try
            {
                var exists = await _keyValuePersistenceService.ContainsAsync(fileName);
                if (exists)
                {
                    var data = await _keyValuePersistenceService.GetAsync(fileName);
                    return Ok(JsonConvert.DeserializeObject(data));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured trying to get validation errors for file name : {fileName}", ex);
                return new BadRequestResult();
            }

            return null;
        }

        public string GetFileName(long ukprn, long jobId, DateTime dateTimeUtc)
        {
            var jobDateTime = _dateTimeProvider.ConvertUtcToUk(dateTimeUtc).ToString("yyyyMMdd-HHmmss");
            return string.Format(_reportFileName, ukprn, jobId, jobDateTime);
        }
    }
}