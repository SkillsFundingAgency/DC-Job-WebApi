using System;
using System.Linq;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models.Enums;
using ESFA.DC.Logging.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/job")]
    public class JobController : Controller
    {
        private readonly IJobQueueManager _jobQueueManager;
        private readonly ILogger _logger;

        public JobController(IJobQueueManager jobQueueManager, ILogger logger)
        {
            _jobQueueManager = jobQueueManager;
            _logger = logger;
        }

        // GET: api/Job
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInfo($"Get request recieved for all jobs");

            try
            {
                var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

                var jobsList = _jobQueueManager.GetAllJobs().ToList();
                jobsList.ForEach(x =>
                {
                    x.DateTimeSubmittedUtc = TimeZoneInfo.ConvertTime(
                        DateTime.SpecifyKind(x.DateTimeSubmittedUtc, DateTimeKind.Unspecified), TimeZoneInfo.Local, britishZone);
                    x.DateTimeUpdatedUtc = TimeZoneInfo.ConvertTime(
                        DateTime.SpecifyKind(x.DateTimeUpdatedUtc.GetValueOrDefault(), DateTimeKind.Unspecified), TimeZoneInfo.Local, britishZone);
                });

                jobsList = jobsList.OrderByDescending(x =>
                {
                    switch (x.Status)
                    {
                        case JobStatus.Completed:
                            return 10;
                        case JobStatus.Failed:
                            return 20;
                        case JobStatus.FailedRetry:
                            return 30;
                        case JobStatus.Paused:
                            return 40;
                        case JobStatus.MovedForProcessing:
                        case JobStatus.Processing:
                            return 50;
                        default:
                            return 60;
                    }
                }).ThenByDescending(x => x.Priority).ThenBy(x => x.JobId).ToList();

                _logger.LogInfo($"Returning jobs list with count : {jobsList.Count}");
                return Ok(jobsList.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get all jobs failed", ex);
                return BadRequest();
            }
        }

        // GET: api/Job
        [HttpGet("{jobId}")]
        public IActionResult Get(long jobId)
        {
            _logger.LogInfo($"Request recieved to get the with Id : {jobId}");

            if (jobId == 0)
            {
                _logger.LogWarning($"Request recieved with Job id 0");
                return BadRequest();
            }

            var job = _jobQueueManager.GetJobById(jobId);
            _logger.LogInfo($"Returning job successfully with job id :{job.JobId}");
            return Ok(job);
        }

        [HttpPost]
        public ActionResult Post([FromBody]JobQueueManager.Models.Job job)
        {
            _logger.LogInfo("Post for job recieved for job : {@job} ", new[] { job });
            if (job == null)
            {
                _logger.LogWarning($"Job Post request received with empty data");
                return BadRequest();
            }

            if (!Enum.IsDefined(typeof(JobStatus), job.Status))
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
                if (job.JobId > 0)
                {
                    if (job.Status == JobStatus.Ready || job.Status == JobStatus.Paused ||
                        job.Status == JobStatus.FailedRetry)
                    {
                        _logger.LogInfo($"Going to update job with job Id : {job.JobId}");

                        var result = _jobQueueManager.UpdateJob(job);
                        if (result)
                        {
                            _logger.LogInfo($"Successfully updated job with job Id : {job.JobId}");
                            return Ok();
                        }
                        else
                        {
                            _logger.LogWarning($"Update job failed for job Id : {job.JobId}");
                            return BadRequest();
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Update job rejected because job status is not updateable for job Id : {job.JobId}, status : {job.Status}");
                        return BadRequest("Job with status of Ready,Paused or FailedRetry can only be updated.");
                    }
                }
                else
                {
                    _logger.LogInfo($"Create Job request received with object : {job} ");

                    job.JobId = _jobQueueManager.AddJob(job);
                    if (job.JobId > 0)
                    {
                        _logger.LogInfo($"Created job successfully with Id : {job.JobId} ");
                        return Ok(job.JobId);
                    }
                    else
                    {
                        _logger.LogInfo("Create job failed for job : {@job} ", new[] { job });
                        return BadRequest();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Post for job failed for job : {@job} ", ex, new[] { job });

                return BadRequest();
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            _logger.LogInfo($"Delete for job recieved for job id : {id} ");

            if (id == 0)
            {
                return BadRequest();
            }

            try
            {
                _jobQueueManager.RemoveJobFromQueue(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete for job failed for job : {id} ", ex);

                return BadRequest();
            }
        }
    }
}
