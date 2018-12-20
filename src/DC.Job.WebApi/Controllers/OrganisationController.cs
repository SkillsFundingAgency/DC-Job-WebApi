using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.Api.Models;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.Data.Organisations.Model;
using ESFA.DC.Data.Organisations.Model.Interface;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Route("api/org")]
    public class OrganisationController : Controller
    {
        private readonly IOrganisationService _organisationService;
        private readonly IOrganisationsContext _organisationsContext;
        private readonly IFileUploadJobManager _fileUploadMetaDataManager;

        public OrganisationController(
            IOrganisationService organisationService,
            IOrganisationsContext organisationsContext,
            IFileUploadJobManager fileUploadMetaDataManager)
        {
            _organisationService = organisationService;
            _organisationsContext = organisationsContext;
            _fileUploadMetaDataManager = fileUploadMetaDataManager;
        }

        // GET api/values/5
        [HttpGet("collection-types/{ukprn}")]
        public async Task<IEnumerable<CollectionType>> Get(long ukprn)
        {
            if (ukprn == 0)
            {
                return null;
            }

            return await _organisationService.GetAvailableCollectionTypesAsync(ukprn);
        }

        [HttpGet("collections/{ukprn}/{collectionName}")]
        public async Task<Collection> Get(long ukprn, string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return null;
            }

            return await _organisationService.GetCollectionAsync(ukprn, collectionName);
        }

        // GET api/values/5
        [HttpGet("{ukprn}")]
        public async Task<MasterOrganisations> GetProvider(long ukprn)
        {
            if (ukprn == 0)
            {
                return null;
            }

            return await _organisationsContext.MasterOrganisations.FirstOrDefaultAsync(x => x.Ukprn == ukprn);
        }

        // GET api/values/5
        [HttpGet("search/{searchTerm}")]
        public async Task<IEnumerable<ProviderDetail>> SearchProviders(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return null;
            }

            var result = new List<ProviderDetail>();
            long.TryParse(searchTerm, out var ukprn);

            var orgsList = await _organisationsContext.MasterOrganisations
                .Where(x => x.OrgDetails.Name.Contains(searchTerm) || (ukprn > 0 && x.Ukprn == ukprn))
                .Select(x => new { x.Ukprn, x.OrgDetails.Name })
                .ToListAsync();

            if (orgsList.Any())
            {
                //get all the last jobs for providers
                var ukprns = orgsList.Select(x => x.Ukprn).ToArray();

                var lastJobs = _fileUploadMetaDataManager.GetLatestJobByUkprn(ukprns).ToList();

                foreach (var org in orgsList)
                {
                    var job = lastJobs.SingleOrDefault(x => x.Ukprn == org.Ukprn);

                    var provider = new ProviderDetail()
                    {
                        Ukprn = org.Ukprn,
                        Name = org.Name,
                    };

                    if (job != null)
                    {
                        provider.LastSubmittedBy = job.SubmittedBy;
                        provider.LastSubmittedByEmail = job.NotifyEmail;
                        provider.LastSubmittedDateUtc = job.DateTimeSubmittedUtc;
                    }

                    result.Add(provider);
                }
            }

            return result;
        }
    }
}
