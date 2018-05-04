using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DC.POC.JobScheduler.Data.Enums;
using DC.POC.JobScheduler.Data.Repository;
using DC.POC.WebApi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DC.POC.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Job")]
    public class JobController : Controller
    {
        private readonly IJobSchedularRepository _jobSchedularRepository;

        public JobController(IJobSchedularRepository jobSchedularRepository)
        {
            _jobSchedularRepository = jobSchedularRepository;
        }

        // GET: api/Job
        [HttpGet]
        public IActionResult Get()
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            var jobsList = _jobSchedularRepository.GetAllJobs().Select(x =>
                new Job()
                {
                    FileName = x.FileName,
                    StorageReference = x.StorageReference,
                    Ukprn = x.Ukprn.HasValue ? x.Ukprn.Value : 0,
                    DateTimeSubmittedUtc = TimeZoneInfo.ConvertTime(x.DateTimeSubmittedUtc, TimeZoneInfo.Local, britishZone).ToString("MM/dd/yyyy hh:mm:ss "),
                    JobId = x.JobId,
                    Priority = x.Priority,
                    DateTimeUpdatedUtc = x.DateTimeUpdatedUtc.HasValue ? TimeZoneInfo.ConvertTime(x.DateTimeUpdatedUtc.Value, TimeZoneInfo.Local, britishZone).ToString("MM/dd/yyyy hh:mm:ss") : string.Empty,
                    JobType = x.JobType,
                    Status = x.Status,
                    RowVersion = System.Convert.ToBase64String(x.RowVersion)
                }).OrderByDescending(x =>
            {
                if (x.Status == (int)EnumJobStatus.Completed)
                {
                    return 10;
                }
                if (x.Status == (int)EnumJobStatus.Failed)
                {
                    return 20;
                }
                else if (x.Status == (int)EnumJobStatus.FailedRetry)
                {
                    return 30;
                }
                else if (x.Status == (int)EnumJobStatus.FailedRetry)
                {
                    return 40;
                }
                else if (x.Status == (int)EnumJobStatus.MovedForProcessing || x.Status == (int)EnumJobStatus.Processing)
                {
                    return 50;
                }
                else
                {
                    return 60;
                }
            }).ThenByDescending(x => x.Priority).ThenBy(x => x.JobId);
            return Ok(jobsList.ToList());
        }

        // POST: api/Job
        [HttpPost]
        public ActionResult Post([FromBody]Job job)
        {
            if (job == null)
            {
                return BadRequest();
            }

            try
            {
                var jobEntity = Convert(job);
                if (jobEntity.JobId > 0)
                {
                   var result = _jobSchedularRepository.UpdateJob(jobEntity);
                    if (result)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                   job.JobId = _jobSchedularRepository.AddJob(jobEntity);
                    if (job.JobId > 0)
                    {
                        return Ok(job.JobId);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
            }

            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest();
            }

            try
            {
                var job = _jobSchedularRepository.GetJobById(id);
                if (job == null)
                {
                    return BadRequest();
                }

                _jobSchedularRepository.RemoveJobFromQueue(job);
                return Ok();
            }

            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        private JobScheduler.Data.Entities.Job Convert(Job job)
        {
            DateTime.TryParse(job.DateTimeSubmittedUtc, out var dateTimeSubmitted);

            var jobEntity = new JobScheduler.Data.Entities.Job()
            {
                JobId = job.JobId,
                Status = job.JobId == 0 ? (short)EnumJobStatus.Ready : job.Status,
                Priority = job.Priority == 0 ? (short)1 : job.Priority,
                FileName = job.FileName,
                Ukprn = job.Ukprn,
                JobType = job.JobType > 0 ? job.JobType : (int)EnumJobType.IlrSubmission,
                RowVersion = job.JobId > 0 ? System.Convert.FromBase64String(job.RowVersion) : null,
                StorageReference = job.StorageReference,
                DateTimeSubmittedUtc = dateTimeSubmitted
            };
            return jobEntity;
        }
    }
}
