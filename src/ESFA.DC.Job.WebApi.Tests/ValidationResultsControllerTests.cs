using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.ILR.ValidationErrors.Interface.Models;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Job.WebApi.Controllers;
using ESFA.DC.JobQueueManager;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using ESFA.DC.Serialization.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ESFA.DC.Job.WebApi.Tests
{
    public class ValidationResultsControllerTests
    {
        [Fact]
        public void ValidationErrorsTest_KeyNotFound()
        {
            var keyValuePersistenceService = new Mock<IKeyValuePersistenceService>();
            var logger = new Mock<ESFA.DC.Logging.Interfaces.ILogger>();

            keyValuePersistenceService.Setup(x => x.ContainsAsync("1_1_1", default(CancellationToken))).Returns(Task.FromResult(false));

            var controller = new ValidationResultsController(keyValuePersistenceService.Object, logger.Object, new Mock<IFileUploadJobManager>().Object, new Mock<IDateTimeProvider>().Object);
            var result = controller.Get(1, 1).Result;
            result.Should().BeAssignableTo<BadRequestResult>();
        }

        [Fact]
        public void ValidationErrorsTest_KeyFound()
        {
            var keyValuePersistenceService = new Mock<IKeyValuePersistenceService>();
            var logger = new Mock<ESFA.DC.Logging.Interfaces.ILogger>();
            var jobMetaServiceMock = new Mock<IFileUploadJobManager>();

            var mockLogger = new Mock<ILogger>();
            jobMetaServiceMock.Setup(x => x.GetJobById(1)).Returns(new FileUploadJob() { JobId = 1, Ukprn = 1 });

            var validationerrors = "1/1/Validation Errors Report 00010101-000000.json";
            keyValuePersistenceService.Setup(x => x.ContainsAsync(validationerrors, default(CancellationToken))).Returns(Task.FromResult(true));

            keyValuePersistenceService.Setup(x => x.GetAsync(validationerrors, default(CancellationToken))).Returns(Task.FromResult("{\"test\":\"1\"}"));

            var controller = new ValidationResultsController(keyValuePersistenceService.Object, logger.Object, jobMetaServiceMock.Object, new DateTimeProvider.DateTimeProvider());
            var result = controller.Get(1, 1).Result;
            result.Should().BeAssignableTo<OkObjectResult>("{\"test\":\"1\"}");
        }
    }
}
