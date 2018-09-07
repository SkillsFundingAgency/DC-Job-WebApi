using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.Job.Models;
using ESFA.DC.Job.Models.Enums;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
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
        private readonly IFileUploadMetaDataManager _fileUploadMetaDataManager;

        public JobController(IJobManager jobManager, ILogger logger, IDateTimeProvider dateTimeProvider, IFileUploadMetaDataManager fileUploadMetaDataManager)
        {
            _jobManager = jobManager;
            _logger = logger;
            _dateTimeProvider = dateTimeProvider;
            _fileUploadMetaDataManager = fileUploadMetaDataManager;
        }

        // GET: api/Job
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInfo($"Get request recieved for all jobs");

            try
            {
                var jobsList = _jobManager.GetAllJobs().ToList();
                TransformDates(jobsList);

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

                _logger.LogInfo($"Returning jobs list with count : {jobsList.Count}");
                return Ok(jobsList.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get all jobs failed", ex);
                return BadRequest();
            }
        }

        [HttpGet("{ukprn}/{jobId}")]
        public IActionResult Get(long ukprn, long jobId)
        {
            _logger.LogInfo($"Request recieved to get the with Id : {jobId}, ukprn : {ukprn}");

            if (jobId == 0 || ukprn == 0)
            {
                _logger.LogWarning($"Request recieved with Job id or ukprn 0");
                return BadRequest();
            }

            var metaData = _fileUploadMetaDataManager.GetJobMetaData(jobId);
            if (metaData?.Ukprn != ukprn)
            {
                _logger.LogWarning($"Job id {jobId} ukprn {ukprn} not found");
                return BadRequest();
            }

            var job = _jobManager.GetJobById(jobId);

            var jobDto = new FileUploadJobDto()
            {
                JobId = job.JobId,
                DateTimeSubmittedUtc = job.DateTimeSubmittedUtc,
                DateTimeUpdatedUtc = job.DateTimeUpdatedUtc,
                NotifyEmail = job.NotifyEmail,
                Priority = job.Priority,
                SubmittedBy = job.SubmittedBy,
                Status = (short)job.Status,
                JobType = (short)job.JobType,
                Ukprn = metaData.Ukprn,
                CollectionName = metaData.CollectionName,
                FileName = metaData.FileName,
                FileSize = metaData.FileSize,
                IsFirstStage = metaData.IsFirstStage,
                PeriodNumber = metaData.PeriodNumber,
                RowVersion = job.RowVersion,
                StorageReference = metaData.StorageReference,
            };

            _logger.LogInfo($"Returning job successfully with job id :{job.JobId}");
            return Ok(jobDto);
        }

        [HttpGet("{ukprn}")]
        public IActionResult Get(long ukprn)
        {
            _logger.LogInfo($"Request recieved to get the with ukprn : {ukprn}");

            if (ukprn == 0)
            {
                _logger.LogWarning($"Request recieved with ukprn 0");
                return BadRequest();
            }

            var jobsList = _jobManager.GetJobsByUkprn(ukprn).OrderByDescending(x => x.DateTimeSubmittedUtc).ToList();
            TransformDates(jobsList);

            _logger.LogInfo($"Returning {jobsList.Count} jobs successfully for ukprn :{ukprn}");
            return Ok(jobsList);
        }

        [HttpGet("{jobId}/status")]
        public ActionResult GetStatus(long jobId)
        {
            _logger.LogInfo($"GetJobStatus for jobId recieved {jobId}");

            if (jobId == 0)
            {
                _logger.LogWarning($"Job Get status request received with empty data");
                return BadRequest();
            }

            try
            {
                var result = _jobManager.GetJobById(jobId);
                if (result != null)
                {
                    _logger.LogInfo($"Successfully Got job for job Id : {jobId}");
                    return Ok(result.Status);
                }
                else
                {
                    _logger.LogWarning($"Get status failed for job Id : {jobId}");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Get status for job failed for jobId : {jobId}", ex);

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

            _logger.LogInfo("Post for job recieved for job : {@jobStatusDto} ", new[] { jobStatusDto });

            if (jobStatusDto.JobId == 0)
            {
                _logger.LogWarning($"Job Post request received with empty data");
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

                var metaData = _fileUploadMetaDataManager.GetJobMetaData(jobStatusDto.JobId);

                //If we are changing from Waiting to Ready, it means processing should go to second stage
                if (job.Status == JobStatusType.Waiting &&
                    (JobStatusType)jobStatusDto.JobStatus == JobStatusType.Ready)
                {
                    _fileUploadMetaDataManager.UpdateJobStage(job.JobId, metaData.IsFirstStage);
                }

                var result = _jobManager.UpdateJobStatus(job.JobId, (JobStatusType)jobStatusDto.JobStatus);

                if (result)
                {
                    _logger.LogInfo($"Successfully updated job status for job Id : {jobStatusDto.JobId}");
                    return Ok();
                }
                else
                {
                    _logger.LogWarning($"Update status failed for job Id : {jobStatusDto.JobId}");
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Post for job failed for job : {@jobStatusDto}", ex, new[] { jobStatusDto });

                return BadRequest();
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody]FileUploadJobDto job)
        {
            _logger.LogInfo("Post for job recieved for job : {@job} ", new[] { job });
            if (job == null)
            {
                _logger.LogWarning($"Job Post request received with empty data");
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
                var jobModel = new Models.Job()
                {
                    Status = (JobStatusType)job.Status,
                    JobId = job.JobId,
                    JobType = (JobType)job.JobType,
                    Priority = job.Priority,
                    SubmittedBy = job.SubmittedBy,
                    NotifyEmail = job.NotifyEmail,
                    DateTimeSubmittedUtc = job.DateTimeSubmittedUtc,
                };

                if (job.JobId > 0)
                {
                    if (job.Status == (short)JobStatusType.Ready || job.Status == (short)JobStatusType.Paused ||
                        job.Status == (short)JobStatusType.FailedRetry)
                    {
                        _logger.LogInfo($"Going to update job with job Id : {job.JobId}");

                        var result = _jobManager.UpdateJob(jobModel);
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

                    job.JobId = _jobManager.AddJob(jobModel);
                    if (job.JobId > 0)
                    {
                        _fileUploadMetaDataManager.AddJobMetData(new FileUploadJobMetaData()
                        {
                            JobId = job.JobId,
                            Ukprn = job.Ukprn ?? 0,
                            CollectionName = job.CollectionName,
                            FileSize = job.FileSize,
                            PeriodNumber = job.PeriodNumber,
                            IsFirstStage = job.IsFirstStage,
                            StorageReference = job.StorageReference,
                            FileName = job.FileName,
                        });
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
                _jobManager.RemoveJobFromQueue(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete for job failed for job : {id} ", ex);

                return BadRequest();
            }
        }

        private void TransformDates(List<Models.Job> jobsList)
        {
            jobsList.ForEach(x =>
            {
                x.DateTimeSubmittedUtc = _dateTimeProvider.ConvertUtcToUk(x.DateTimeSubmittedUtc);
                x.DateTimeUpdatedUtc = _dateTimeProvider.ConvertUtcToUk(x.DateTimeUpdatedUtc.GetValueOrDefault());
            });
        }
    }
}
