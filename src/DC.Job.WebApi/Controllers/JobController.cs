using System;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/job")]
    public class JobController : Controller
    {
        private readonly IJobManager _jobManager;
        private readonly ILogger _logger;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IFileUploadJobManager _fileUploadJobManager;

        public JobController(IJobManager jobManager, ILogger logger, IDateTimeProvider dateTimeProvider, IFileUploadJobManager fileUploadMetaDataManager)
        {
            _jobManager = jobManager;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _fileUploadJobManager = fileUploadMetaDataManager;
        }

        // GET: api/Job
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInfo("Get request received for all jobs");

            try
            {
                var jobsList = _fileUploadJobManager.GetAllJobs().ToList();

                jobsList = jobsList.OrderByDescending(x =>
                {
                    switch (x.Status)
                    {
                        case JobStatusType.Completed:
                            return 10;
                        case JobStatusType.Failed:
                            return 20;
                        case JobStatusType.FailedRetry:
                            return 30;
                        case JobStatusType.Paused:
                            return 40;
                        case JobStatusType.MovedForProcessing:
                        case JobStatusType.Processing:
                            return 50;
                        default:
                            return 60;
                    }
                }).ThenByDescending(x => x.Priority).ThenBy(x => x.JobId).ToList();

                _logger.LogInfo($"Returning jobs list with count: {jobsList.Count}");
                return Ok(jobsList.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError("Get all jobs failed", ex);
                return BadRequest();
            }
        }

        [HttpGet("{ukprn}/{jobId}")]
        public IActionResult GetById(long ukprn, long jobId)
        {
            _logger.LogInfo($"Request received to get the with Id: {jobId}; ukprn: {ukprn}");

            if (jobId == 0 || ukprn == 0)
            {
                _logger.LogWarning($"Request received with Job id {jobId} and ukprn {ukprn} one of which is 0");
                return BadRequest();
            }

            var job = _fileUploadJobManager.GetJobById(jobId);
            if (job?.Ukprn != ukprn)
            {
                _logger.LogWarning($"Job id {jobId} with ukprn {ukprn} not found");
                return BadRequest();
            }

            _logger.LogInfo($"Returning job successfully with job id: {job.JobId}");
            return Ok(job);
        }

        [HttpGet("{ukprn}")]
        public IActionResult GetForUkprn(long ukprn)
        {
            _logger.LogInfo($"Request received to get the with ukprn: {ukprn}");

            if (ukprn == 0)
            {
                _logger.LogWarning("Request received with ukprn 0");
                return BadRequest();
            }

            var jobsList = _fileUploadJobManager.GetJobsByUkprn(ukprn).OrderByDescending(x => x.DateTimeSubmittedUtc).ToList();

            _logger.LogInfo($"Returning {jobsList.Count} jobs successfully for ukprn: {ukprn}");
            return Ok(jobsList);
        }

        [HttpGet("{ukprn}/period/{period}")]
        public IActionResult GetForPeriod(long ukprn, int period)
        {
            _logger.LogInfo($"Request received to get the with ukprn: {ukprn}; period: {period}");

            if (ukprn == 0 || period == 0)
            {
                _logger.LogWarning($"Request received with ukprn {ukprn} and period {period} one of which is 0");
                return BadRequest();
            }

            var jobsList = _fileUploadJobManager.GetJobsByUkprnForPeriod(ukprn, period).ToList();

            _logger.LogInfo($"Returning {jobsList.Count} jobs successfully for ukprn :{ukprn}");
            return Ok(jobsList);
        }

        [HttpGet("{jobId}/status")]
        public ActionResult GetStatus(long jobId)
        {
            _logger.LogInfo($"GetJobStatus for jobId received {jobId}");

            if (jobId == 0)
            {
                _logger.LogWarning("Job Get status request received with empty data");
                return BadRequest();
            }

            try
            {
                var result = _jobManager.GetJobById(jobId);
                if (result != null)
                {
                    _logger.LogInfo($"Successfully Got job for job Id: {jobId}");
                    return Ok(result.Status);
                }

                _logger.LogWarning($"Get status failed for job Id: {jobId}");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get status for job failed for jobId: {jobId}", ex);

                return BadRequest();
            }
        }

        [HttpPost("cross-loading/status/{jobId}/{status}")]
        public ActionResult Post([FromRoute]long jobId, [FromRoute]JobStatusType status)
        {
            if (jobId == 0)
            {
                _logger.LogWarning("Job Post request received with empty data");
                return BadRequest();
            }

            try
            {
                var job = _jobManager.GetJobById(jobId);
                if (job == null)
                {
                    _logger.LogError($"JobId {jobId} is not valid for job status update");
                    return BadRequest("Invalid job Id");
                }

                var result = _jobManager.UpdateCrossLoadingStatus(job.JobId, status);

                if (result)
                {
                    _logger.LogInfo($"Successfully updated cross loading job status for job Id: {jobId}");
                    return Ok();
                }

                _logger.LogWarning($"Update cross loading status failed for job Id: {jobId}");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Post for cross loading status post job failed for job: {jobId}", ex);

                return BadRequest();
            }
        }

        [HttpPost("{status}")]
        public ActionResult Post([FromBody]JobStatusDto jobStatusDto)
        {
            if (jobStatusDto == null)
            {
                _logger.LogError($"Job Post request received with empty data for JobStatusDto");
                return BadRequest();
            }

            _logger.LogInfo("Post for job received for job: {@jobStatusDto} ", new[] { jobStatusDto });

            if (jobStatusDto.JobId == 0)
            {
                _logger.LogWarning("Job Post request received with empty data");
                return BadRequest();
            }

            if (!Enum.IsDefined(typeof(JobStatusType), jobStatusDto.JobStatus))
            {
                _logger.LogWarning($"Job Post request received with bad status {jobStatusDto.JobStatus}");
                return BadRequest("Status is not a valid value");
            }

            try
            {
                var job = _jobManager.GetJobById(jobStatusDto.JobId);
                if (job == null)
                {
                    _logger.LogError($"JobId {jobStatusDto.JobId} is not valid for job status update");
                    return BadRequest("Invalid job Id");
                }

                FileUploadJob metaData = _fileUploadJobManager.GetJobById(jobStatusDto.JobId);

                // If we are changing from Waiting to Ready, it means processing should go to second stage
                if (job.Status == JobStatusType.Waiting &&
                    (JobStatusType)jobStatusDto.JobStatus == JobStatusType.Ready)
                {
                    _fileUploadJobManager.UpdateJobStage(job.JobId, !metaData.IsFirstStage);
                }

                bool result = _jobManager.UpdateJobStatus(job.JobId, (JobStatusType)jobStatusDto.JobStatus);

                if (result)
                {
                    _logger.LogInfo($"Successfully updated job status for job Id: {jobStatusDto.JobId}");

                    // Todo: Remove this block of code at some point, it's to make cross loading work when we send a message, but we won't receive a response from the other system.
                    if (job.CrossLoadingStatus == null)
                    {
                        return Ok();
                    }

                    if ((JobStatusType)jobStatusDto.JobStatus != JobStatusType.Completed)
                    {
                        return Ok();
                    }

                    result = _jobManager.UpdateCrossLoadingStatus(job.JobId, JobStatusType.Completed);

                    if (result)
                    {
                        _logger.LogInfo($"Successfully updated cross loading job status for job Id: {jobStatusDto.JobId}");
                        return Ok();
                    }

                    _logger.LogWarning($"Update cross loading status failed for job Id: {jobStatusDto.JobId}");
                    return BadRequest();
                }

                _logger.LogWarning($"Update status failed for job Id: {jobStatusDto.JobId}");
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError("Post for job failed for job: {@jobStatusDto}", ex, new[] { jobStatusDto });

                return BadRequest();
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody]FileUploadJob job)
        {
            _logger.LogInfo("Post for job received for job: {@job}", new[] { job });
            if (job == null)
            {
                _logger.LogWarning("Job Post request received with empty data");
                return BadRequest();
            }

            if (!Enum.IsDefined(typeof(JobStatusType), job.Status))
            {
                _logger.LogWarning($"Job Post request received with bad status {job.Status}");
                return BadRequest("Status is not a valid value");
            }

            if (!Enum.IsDefined(typeof(JobType), job.JobType))
            {
                _logger.LogWarning($"Job Post request received with bad job type {job.JobType}");
                return BadRequest("Job type is not a valid value");
            }

            try
            {
                var jobModel = new Jobs.Model.Job
                {
                    Status = job.Status,
                    JobId = job.JobId,
                    JobType = job.JobType,
                    Priority = job.Priority,
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail,
                    DateTimeSubmittedUtc = job.DateTimeSubmittedUtc,
                };

                if (job.JobId > 0)
                {
                    if (job.Status == JobStatusType.Ready || job.Status == JobStatusType.Paused ||
                        job.Status == JobStatusType.FailedRetry)
                    {
                        _logger.LogInfo($"Going to update job with job Id: {job.JobId}");

                        var result = _jobManager.UpdateJob(jobModel);
                        if (result)
                        {
                            _logger.LogInfo($"Successfully updated job with job Id: {job.JobId}");
                            return Ok();
                        }

                        _logger.LogWarning($"Update job failed for job Id: {job.JobId}");
                        return BadRequest();
                    }

                    _logger.LogWarning($"Update job rejected because job status is not updateable for job Id: {job.JobId}; status: {job.Status}");
                    return BadRequest($"Only job with status of {nameof(JobStatusType.Ready)}, {nameof(JobStatusType.Paused)} or {nameof(JobStatusType.FailedRetry)} can be updated.");
                }

                _logger.LogInfo($"Create Job request received with object: {job} ");

                job.JobId = _fileUploadJobManager.AddJob(job);

                if (job.JobId > 0)
                {
                    _logger.LogInfo($"Created job successfully with Id: {job.JobId}");
                    return Ok(job.JobId);
                }

                _logger.LogInfo("Create job failed for job: {@job}", new[] { job });
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError("Post for job failed for job: {@job}", ex, new[] { job });

                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            _logger.LogInfo($"Delete for job received for job id: {id}");

            if (id == 0)
            {
                return BadRequest();
            }

            try
            {
                _jobManager.RemoveJobFromQueue(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete for job failed for job: {id}", ex);

                return BadRequest();
            }
        }
    }
}
