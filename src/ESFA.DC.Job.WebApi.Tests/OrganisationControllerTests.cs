using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESFA.DC.Data.Organisations.Model;
using ESFA.DC.Data.Organisations.Model.Interface;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.Job.WebApi.Controllers;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.Job.WebApi.Tests
{
    public class OrganisationControllerTests
    {
        [Fact]
        public void GetSearchResults_NullSearchTerm()
        {
            var orgController = new OrganisationController(null, null, null);
            orgController.SearchProviders(null).Result.Should().BeNull();
        }

        [Fact]
        public void GetSearchResults_Name_Test()
        {
            var orgnisationServiceMock = new Mock<IOrganisationService>();
            var fileUploadManagerMock = new Mock<IFileUploadJobManager>();

            fileUploadManagerMock.Setup(x => x.GetLatestJobByUkprn(It.IsAny<long[]>()))
                .Returns(new List<FileUploadJob>());

            var orgController = new OrganisationController(orgnisationServiceMock.Object, GetContext(), fileUploadManagerMock.Object);
            var result = orgController.SearchProviders("Org").Result;
            result.Count().Should().Be(3);
        }

        [Fact]
        public void GetSearchResults_Ukprn_Test()
        {
            var orgnisationServiceMock = new Mock<IOrganisationService>();
            var fileUploadManagerMock = new Mock<IFileUploadJobManager>();

            fileUploadManagerMock.Setup(x => x.GetLatestJobByUkprn(new long[] { 1000 }))
                .Returns(new List<FileUploadJob>()
                {
                    new FileUploadJob()
                    {
                        Ukprn = 1000,
                        SubmittedBy = "test",
                        DateTimeSubmittedUtc = new DateTime(2018, 10, 10),
                        NotifyEmail = "test@test.com"
                    }
                });

            var orgController = new OrganisationController(orgnisationServiceMock.Object, GetContext(), fileUploadManagerMock.Object);
            var result = orgController.SearchProviders("1000").Result;
            result.Count().Should().Be(1);
            result.Any(x =>
                    x.Ukprn == 1000 &&
                    x.Name.Equals("Org1", StringComparison.CurrentCultureIgnoreCase) &&
                    x.LastSubmittedBy.Equals("test", StringComparison.CurrentCultureIgnoreCase) &&
                    x.LastSubmittedDateUtc == new DateTime(2018, 10, 10) &&
                    x.LastSubmittedByEmail.Equals("test@test.com", StringComparison.InvariantCultureIgnoreCase))
                .Should()
                .BeTrue();
        }

        private IOrganisationsContext GetContext()
        {
            var options = new DbContextOptionsBuilder<OrganisationsContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var orginsationContext = new OrganisationsContext(options);

            orginsationContext.AddRange(
                new List<MasterOrganisations>
                {
                    new MasterOrganisations() { Ukprn = 1000, OrgDetails = new OrgDetails() { Name = "Org1", Ukprn = 1000 } },
                    new MasterOrganisations() { Ukprn = 2000, OrgDetails = new OrgDetails() { Name = "Org2", Ukprn = 2000 } },
                    new MasterOrganisations() { Ukprn = 3000, OrgDetails = new OrgDetails() { Name = "Org3", Ukprn = 3000 } },
                    new MasterOrganisations() { Ukprn = 4000, OrgDetails = new OrgDetails() { Name = "XYYYYY", Ukprn = 4000 } },
                });
            orginsationContext.SaveChanges();

            return orginsationContext;
        }
    }
}
