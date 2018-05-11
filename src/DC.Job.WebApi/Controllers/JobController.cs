using System;
using System.Linq;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/job")]
    public class JobController : Controller
    {
        private readonly IJobQueueManager _jobQueueManager;

        public JobController(IJobQueueManager jobQueueManager)
        {
            _jobQueueManager = jobQueueManager;
        }

        // GET: api/Job
        [HttpGet]
        public IActionResult Get()
        {
            var britishZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            var jobsList = _jobQueueManager.GetAllJobs().ToList();
            jobsList.ForEach(x =>
            {
                x.DateTimeSubmittedUtc =
                    TimeZoneInfo.ConvertTime(x.DateTimeSubmittedUtc, TimeZoneInfo.Local, britishZone);
                x.DateTimeUpdatedUtc = TimeZoneInfo.ConvertTime(x.DateTimeUpdatedUtc.GetValueOrDefault(), TimeZoneInfo.Local, britishZone);
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
            return Ok(jobsList.ToList());
        }

        [HttpPost]
        public ActionResult Post([FromBody]JobQueueManager.Models.Job job)
        {
            if (job == null)
            {
                return BadRequest();
            }

            try
            {
                if (job.JobId > 0)
                {
                   var result = _jobQueueManager.UpdateJob(job);
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
                   job.JobId = _jobQueueManager.AddJob(job);
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

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
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
                return BadRequest();
            }
        }
    }
}
