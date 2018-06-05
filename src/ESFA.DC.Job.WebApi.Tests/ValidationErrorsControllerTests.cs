using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ESFA.DC.ILR.ValidationErrors.Interface.Models;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Job.WebApi.Controllers;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Serialization.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ESFA.DC.Job.WebApi.Tests
{
    public class ValidationErrorsControllerTests
    {
        [Fact]
        public void ValidationErrorsTest_KeyNotFound()
        {
            var keyValuePersistenceService = new Mock<IKeyValuePersistenceService>();
            var logger = new Mock<ESFA.DC.Logging.Interfaces.ILogger>();
            var keyGenerator = new Mock<IKeyGenerator>();
            var serializationService = new Mock<ISerializationService>();

            var validationerrors = "1_1_ValidationErrors";
            keyGenerator.Setup(x => x.GenerateKey(1, 1, TaskKeys.ValidationErrors))
                .Returns(validationerrors);
            keyValuePersistenceService.Setup(x => x.ContainsAsync("1_1_1")).Returns(Task.FromResult(false));

            var controller = new ValidationErrorsController(keyValuePersistenceService.Object, logger.Object, keyGenerator.Object, serializationService.Object);
            var result = controller.Get(1, 1).Result;
            result.Should().BeAssignableTo<IEnumerable<ValidationErrorDto>>();
            result.Count().Should().Be(0);
        }

        [Fact]
        public void ValidationErrorsTest_KeyFound()
        {
            var keyValuePersistenceService = new Mock<IKeyValuePersistenceService>();
            var logger = new Mock<ESFA.DC.Logging.Interfaces.ILogger>();
            var keyGenerator = new Mock<IKeyGenerator>();
            var serializationService = new Mock<ISerializationService>();

            var validationerrors = "1_1_ValidationErrors";
            keyGenerator.Setup(x => x.GenerateKey(1, 1, TaskKeys.ValidationErrors))
                .Returns(validationerrors);
            keyValuePersistenceService.Setup(x => x.ContainsAsync(validationerrors)).Returns(Task.FromResult(true));

            var data = new List<ValidationErrorDto>()
            {
                new ValidationErrorDto(),
                new ValidationErrorDto(),
            };
            keyValuePersistenceService.Setup(x => x.GetAsync(validationerrors)).Returns(Task.FromResult(string.Empty));
            serializationService.Setup(x => x.Deserialize<IEnumerable<ValidationErrorDto>>(It.IsAny<string>())).Returns(data);

            var controller = new ValidationErrorsController(keyValuePersistenceService.Object, logger.Object, keyGenerator.Object, serializationService.Object);
            var result = controller.Get(1, 1).Result;
            result.Should().BeAssignableTo<IEnumerable<ValidationErrorDto>>();
            result.Count().Should().Be(2);
        }
    }
}
