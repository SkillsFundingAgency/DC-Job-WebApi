using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Indexed;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Logging.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ValidationResultsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IFileUploadJobManager _fileUploadMetaDataManager;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly string _reportFileName = "{0}/{1}/Rule Violation Report {2}.json";
        private readonly IIndex<JobType, IKeyValuePersistenceService> _storagePersistenceServices;

        public ValidationResultsController(
            IIndex<JobType, IKeyValuePersistenceService> storagePersistenceServices,
            ILogger logger,
            IFileUploadJobManager fileUploadMetaDataManager,
            IDateTimeProvider dateTimeProvider)
        {
            _storagePersistenceServices = storagePersistenceServices;
            _logger = logger;
            _fileUploadMetaDataManager = fileUploadMetaDataManager;
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

            var job = _fileUploadMetaDataManager.GetJobById(jobId);
            if (job == null || job.Ukprn != ukprn)
            {
                _logger.LogWarning($"No job found for jobId : {jobId}, ukprn : {ukprn}");
                return new BadRequestResult();
            }

            var fileName = GetFileName(ukprn, jobId, job.DateTimeSubmittedUtc);
            try
            {
                var keyValuePersistenceService = GetKeyValuePersistenceService(job.JobType);
                var exists = await keyValuePersistenceService.ContainsAsync(fileName);
                if (exists)
                {
                    var data = await keyValuePersistenceService.GetAsync(fileName);
                    return Ok(JsonConvert.DeserializeObject(data));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured trying to get validation errors for file name : {fileName}", ex);
                return new BadRequestResult();
            }

            return new NotFoundResult();
        }

        public string GetFileName(long ukprn, long jobId, DateTime dateTimeUtc)
        {
            var jobDateTime = _dateTimeProvider.ConvertUtcToUk(dateTimeUtc).ToString("yyyyMMdd-HHmmss");
            return string.Format(_reportFileName, ukprn, jobId, jobDateTime);
        }

        public IKeyValuePersistenceService GetKeyValuePersistenceService(JobType jobType)
        {
            return _storagePersistenceServices[jobType];
        }
    }
}