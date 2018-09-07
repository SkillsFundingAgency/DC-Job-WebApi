//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using ESFA.DC.DateTimeProvider.Interface;
//using ESFA.DC.Job.WebApi.Controllers;
//using ESFA.DC.JobStatus.Dto;
//using ESFA.DC.JobStatus.Interface;
//using ESFA.DC.Logging.Interfaces;
//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using Xunit;

//namespace ESFA.DC.Job.WebApi.Tests
//{
//    public class JobControllerTests
//    {
//        [Fact]
//        public void GetAllJobs_EmptyList_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetAllJobs()).Returns(new List<IlrJob>());
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.Get();
//            result.StatusCode.Should().Be(200);
//            result.Value.Should().BeAssignableTo<List<IlrJob>>();
//        }

//        [Fact]
//        public void GetAllJobs_OrderedList_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var jobs = new List<IlrJob>()
//            {
//                new IlrJob()
//                {
//                    JobId = 1,
//                    Status = JobStatusType.Completed,
//                    Priority = 3,
//                },
//                new IlrJob()
//                {
//                    JobId = 2,
//                    Status = JobStatusType.Ready,
//                    Priority = 5,
//                },
//                new IlrJob()
//                {
//                    JobId = 3,
//                    Status = JobStatusType.Ready,
//                    Priority = 1,
//                },
//                new IlrJob()
//                {
//                    JobId = 4,
//                    Status = JobStatusType.Ready,
//                    Priority = 5,
//                },
//                new IlrJob()
//                {
//                    JobId = 4,
//                    Status = JobStatusType.Completed,
//                    Priority = 100,
//                },
//            };
//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetAllJobs()).Returns(jobs);
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.Get();
//            result.StatusCode.Should().Be(200);

//            var outputJobs = (List<IlrJob>)result.Value;
//            outputJobs.Count.Should().Be(5);
//            outputJobs.First().JobId.Should().Be(2);
//        }

//        [Fact]
//        public void GetJobByJobId_ZeroJobIdValue_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<IlrJob>());
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new DateTimeProvider.DateTimeProvider());

//            var result = (BadRequestResult)controller.Get(1000, 0);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void GetJobByJobId_UkprnZeroValue_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<IlrJob>());
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.Get(0, 100);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void GetJobByJobId_Success_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(new IlrJob()
//            {
//                JobId = 2,
//                Status = JobStatusType.Ready,
//                Priority = 5,
//                Ukprn = 1000,
//            });
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.Get(1000, 2);
//            result.StatusCode.Should().Be(200);

//            var outputJob = (IlrJob)result.Value;
//            outputJob.JobId.Should().Be(2);
//            outputJob.Status.Should().Be(JobStatusType.Ready);
//            outputJob.Priority.Should().Be(5);
//            outputJob.Ukprn.Should().Be(1000);
//        }

//        [Fact]
//        public void GetJobsForUkprn_UkprnZeroValue_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(It.IsAny<IlrJob>());
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.Get(0);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void GetJobsForUkprn_Success_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            var mockLogger = new Mock<ILogger>();
//            var jobs = new List<IlrJob>()
//            {
//                new IlrJob()
//                {
//                    JobId = 1,
//                    Status = JobStatusType.Completed,
//                    Priority = 3,
//                    Ukprn = 1000,
//                },
//                new IlrJob()
//                {
//                    JobId = 2,
//                    Status = JobStatusType.Ready,
//                    Priority = 5,
//                    Ukprn = 1000,
//                },
//            };

//            jobqueServiceMock.Setup(x => x.GetJobsByUkprn(1000)).Returns(jobs);
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.Get(1000);
//            result.StatusCode.Should().Be(200);

//            var outputJobs = (List<IlrJob>)result.Value;
//            outputJobs.Count.Should().Be(2);
//        }

//        [Fact]
//        public void PostJob_Null_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.Post((IlrJob)null);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_NewJob_Success_Test()
//        {
//            var job = new IlrJob()
//            {
//                Status = JobStatusType.Ready,
//                Ukprn = 1000,
//            };
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            jobqueServiceMock.Setup(x => x.AddJob(job)).Returns(1);

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.Post(job);
//            result.StatusCode.Should().Be(200);
//            result.Value.Should().Be(1);
//        }

//        [Fact]
//        public void PostJob_NewJob_SaveFailed_Test()
//        {
//            var job = new IlrJob()
//            {
//                Status = JobStatusType.Ready,
//            };
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            jobqueServiceMock.Setup(x => x.AddJob(job)).Returns(0);

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.Post(job);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_NewJob_InvalidStatus_Failed_Test()
//        {
//            var job = new IlrJob();
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestObjectResult)controller.Post(job);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_NewJob_InvalidJobType_Failed_Test()
//        {
//            var job = new IlrJob()
//            {
//                Status = JobStatusType.Ready,
//            };
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.Post(job);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_UpdateJob_Success_Test()
//        {
//            var job = new IlrJob()
//            {
//                JobId = 100,
//                Status = JobStatusType.Ready,
//                Ukprn = 1000,
//            };
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            jobqueServiceMock.Setup(x => x.UpdateJob(job)).Returns(true);

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Post(job);
//            result.Should().BeAssignableTo<OkResult>();
//            ((OkResult)result).StatusCode.Should().Be(200);
//        }

//        [Theory]
//        [InlineData(JobStatusType.MovedForProcessing)]
//        [InlineData(JobStatusType.Processing)]
//        [InlineData(JobStatusType.Completed)]
//        [InlineData(JobStatusType.Failed)]
//        public void PostJob_UpdateJob_InvalidStatus_Failed_Test(JobStatusType status)
//        {
//            var job = new IlrJob()
//            {
//                JobId = 100,
//                Status = status,
//                Ukprn = 1000,
//            };
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Post(job);
//            result.Should().BeAssignableTo<BadRequestObjectResult>();
//            ((BadRequestObjectResult)result).StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_UpdateJobStatus_Failed_NullDtoTest()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Post((JobStatusDto)null);
//            result.Should().BeAssignableTo<BadRequestResult>();
//            ((BadRequestResult)result).StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_UpdateJobStatus_Failed_ZeroJobIdTest()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Post(new JobStatusDto());
//            result.Should().BeAssignableTo<BadRequestResult>();
//            ((BadRequestResult)result).StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void PostJob_UpdateJobStatus_Success_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            jobqueServiceMock.Setup(x => x.UpdateJob(It.IsAny<IlrJob>())).Returns(true);
//            jobqueServiceMock.Setup(x => x.GetJobById(100)).Returns(new IlrJob() { JobId = 100 });

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Post(new JobStatusDto()
//            {
//                JobStatus = (int)JobStatusType.Completed,
//                JobId = 100,
//            });
//            result.Should().BeAssignableTo<OkResult>();
//            ((OkResult)result).StatusCode.Should().Be(200);
//        }

//        [Fact]
//        public void Delete_ZeroId_Failed_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Delete(0);
//            result.Should().BeAssignableTo<BadRequestResult>();
//            ((BadRequestResult)result).StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void Delete_Success_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();
//            jobqueServiceMock.Setup(x => x.RemoveJobFromQueue(It.IsAny<long>()));

//            var mockLogger = new Mock<ILogger>();
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = controller.Delete(100);
//            result.Should().BeAssignableTo<OkResult>();
//            ((OkResult)result).StatusCode.Should().Be(200);
//        }

//        [Fact]
//        public void GetJobByStatus_FailJobDontExist_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(1)).Returns(new IlrJob()
//            {
//                JobId = 1000,
//                Status = JobStatusType.Ready,
//                Priority = 5,
//                Ukprn = 1000,
//            });
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (BadRequestResult)controller.GetStatus(10);
//            result.StatusCode.Should().Be(400);
//        }

//        [Fact]
//        public void GetJobByStatus_Success_Test()
//        {
//            var jobqueServiceMock = new Mock<IIlrJobQueueManager>();

//            var mockLogger = new Mock<ILogger>();
//            jobqueServiceMock.Setup(x => x.GetJobById(2)).Returns(new IlrJob()
//            {
//                JobId = 2,
//                Status = JobStatusType.Ready,
//                Priority = 5,
//                Ukprn = 1000,
//            });
//            var controller = new JobController(jobqueServiceMock.Object, mockLogger.Object, new Mock<IDateTimeProvider>().Object);

//            var result = (OkObjectResult)controller.GetStatus(2);
//            result.StatusCode.Should().Be(200);
//            result.Value.Should().BeAssignableTo<JobStatusType>();
//            result.Value.Should().Be(JobStatusType.Ready);
//        }
//    }
//}