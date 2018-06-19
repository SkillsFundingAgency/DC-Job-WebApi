using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ESFA.DC.DateTime.Provider;
using ESFA.DC.Job.WebApi.Controllers;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.JobQueueManager.Models.Enums;
using ESFA.DC.JobStatus.Dto;
using ESFA.DC.JobStatus.Interface;
using ESFA.DC.Logging.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ESFA.DC.Job.WebApi.Tests
{
    public class JobControllerTests
    {
        [Fact]
        public void GetAllJobs_EmptyList_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetAllJobs()).Returns(new List<JobQueueManager.Models.Job>());
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (OkObjectResult)controller.Get();
            result.StatusCode.Should().Be(200);
            result.Value.Should().BeAssignableTo<List<JobQueueManager.Models.Job>>();
        }

        [Fact]
        public void GetAllJobs_OrderedList_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var jobs = new List<JobQueueManager.Models.Job>()
            {
                new JobQueueManager.Models.Job()
                {
                    JobId = 1,
                    Status = JobStatusType.Completed,
                    Priority = 3,
                },
                new JobQueueManager.Models.Job()
                {
                    JobId = 2,
                    Status = JobStatusType.Ready,
                    Priority = 5,
                },
                new JobQueueManager.Models.Job()
                {
                    JobId = 3,
                    Status = JobStatusType.Ready,
                    Priority = 1,
                },
                new JobQueueManager.Models.Job()
                {
                    JobId = 4,
                    Status = JobStatusType.Ready,
                    Priority = 5,
                },
                new JobQueueManager.Models.Job()
                {
                    JobId = 4,
                    Status = JobStatusType.Completed,
                    Priority = 100,
                },
            };
            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetAllJobs()).Returns(jobs);
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (OkObjectResult)controller.Get();
            result.StatusCode.Should().Be(200);

            var outputJobs = (List<JobQueueManager.Models.Job>)result.Value;
            outputJobs.Count.Should().Be(5);
            outputJobs.First().JobId.Should().Be(2);
        }

        [Fact]
        public void GetJobByJobId_ZeroJobIdValue_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<JobQueueManager.Models.Job>());
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestResult)controller.Get(1000, 0);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void GetJobByJobId_UkprnZeroValue_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<JobQueueManager.Models.Job>());
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestResult)controller.Get(0, 100);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void GetJobByJobId_Success_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(new JobQueueManager.Models.Job()
            {
                JobId = 2,
                Status = JobStatusType.Ready,
                Priority = 5,
                Ukprn = 1000,
            });
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (OkObjectResult)controller.Get(1000, 2);
            result.StatusCode.Should().Be(200);

            var outputJob = (JobQueueManager.Models.Job)result.Value;
            outputJob.JobId.Should().Be(2);
            outputJob.Status.Should().Be(JobStatusType.Ready);
            outputJob.Priority.Should().Be(5);
            outputJob.Ukprn.Should().Be(1000);
        }

        [Fact]
        public void GetJobsForUkprn_UkprnZeroValue_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<JobQueueManager.Models.Job>());
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestResult)controller.Get(0);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void GetJobsForUkprn_Success_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            var mockLogger = new Mock<ILogger>();
            var jobs = new List<JobQueueManager.Models.Job>()
            {
                new JobQueueManager.Models.Job()
                {
                    JobId = 1,
                    Status = JobStatusType.Completed,
                    Priority = 3,
                    Ukprn = 1000,
                },
                new JobQueueManager.Models.Job()
                {
                    JobId = 2,
                    Status = JobStatusType.Ready,
                    Priority = 5,
                    Ukprn = 1000,
                },
            };

            jobqueServiceMock.Setup(x => x.GetJobsByUkprn(1000)).Returns(jobs);
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (OkObjectResult)controller.Get(1000);
            result.StatusCode.Should().Be(200);

            var outputJobs = (List<JobQueueManager.Models.Job>)result.Value;
            outputJobs.Count.Should().Be(2);
        }

        [Fact]
        public void PostJob_Null_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestResult)controller.Post((JobQueueManager.Models.Job)null);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_NewJob_Success_Test()
        {
            var job = new JobQueueManager.Models.Job()
            {
                Status = JobStatusType.Ready,
                JobType = JobType.IlrSubmission,
                Ukprn = 1000,
            };
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            jobqueServiceMock.Setup(x => x.AddJob(job)).Returns(1);

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (OkObjectResult)controller.Post(job);
            result.StatusCode.Should().Be(200);
            result.Value.Should().Be(1);
        }

        [Fact]
        public void PostJob_NewJob_SaveFailed_Test()
        {
            var job = new JobQueueManager.Models.Job()
            {
                Status = JobStatusType.Ready,
                JobType = JobType.IlrSubmission,
            };
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            jobqueServiceMock.Setup(x => x.AddJob(job)).Returns(0);

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestResult)controller.Post(job);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_NewJob_InvalidStatus_Failed_Test()
        {
            var job = new JobQueueManager.Models.Job();
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestObjectResult)controller.Post(job);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_NewJob_InvalidJobType_Failed_Test()
        {
            var job = new JobQueueManager.Models.Job()
            {
                Status = JobStatusType.Ready,
            };
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = (BadRequestObjectResult)controller.Post(job);
            result.StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_UpdateJob_Success_Test()
        {
            var job = new JobQueueManager.Models.Job()
            {
                JobId = 100,
                Status = JobStatusType.Ready,
                JobType = JobType.IlrSubmission,
                Ukprn = 1000,
            };
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            jobqueServiceMock.Setup(x => x.UpdateJob(job)).Returns(true);

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Post(job);
            result.Should().BeAssignableTo<OkResult>();
            ((OkResult)result).StatusCode.Should().Be(200);
        }

        [Theory]
        [InlineData(JobStatusType.MovedForProcessing)]
        [InlineData(JobStatusType.Processing)]
        [InlineData(JobStatusType.Completed)]
        [InlineData(JobStatusType.Failed)]
        public void PostJob_UpdateJob_InvalidStatus_Failed_Test(JobStatusType status)
        {
            var job = new JobQueueManager.Models.Job()
            {
                JobId = 100,
                Status = status,
                JobType = JobType.IlrSubmission,
                Ukprn = 1000,
            };
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Post(job);
            result.Should().BeAssignableTo<BadRequestObjectResult>();
            ((BadRequestObjectResult)result).StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_UpdateJobStatus_Failed_NullDtoTest()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Post((JobStatusDto)null);
            result.Should().BeAssignableTo<BadRequestResult>();
            ((BadRequestResult)result).StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_UpdateJobStatus_Failed_ZeroJobIdTest()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Post(new JobStatusDto());
            result.Should().BeAssignableTo<BadRequestResult>();
            ((BadRequestResult)result).StatusCode.Should().Be(400);
        }

        [Fact]
        public void PostJob_UpdateJobStatus_Success_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            jobqueServiceMock.Setup(x => x.UpdateJobStatus(100, JobStatusType.Completed)).Returns(true);

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Post(new JobStatusDto()
            {
                JobStatus = (int)JobStatusType.Completed,
                JobId = 100,
            });
            result.Should().BeAssignableTo<OkResult>();
            ((OkResult)result).StatusCode.Should().Be(200);
        }

        [Fact]
        public void Delete_ZeroId_Failed_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Delete(0);
            result.Should().BeAssignableTo<BadRequestResult>();
            ((BadRequestResult)result).StatusCode.Should().Be(400);
        }

        [Fact]
        public void Delete_Success_Test()
        {
            var jobqueServiceMock = new Mock<IJobQueueManager>();
            jobqueServiceMock.Setup(x => x.RemoveJobFromQueue(It.IsAny<long>()));

            var mockLogger = new Mock<ILogger>();
            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider());

            var result = controller.Delete(100);
            result.Should().BeAssignableTo<OkResult>();
            ((OkResult)result).StatusCode.Should().Be(200);
        }
    }
}