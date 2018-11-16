using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Route("api/org")]
    public class OrganisationController : Controller
    {
        private readonly IOrganisationService _organisationService;

        public OrganisationController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        // GET api/values/5
        [HttpGet("{ukprn}")]
        public async Task<IEnumerable<CollectionType>> Get(long ukprn)
        {
            if (ukprn == 0)
            {
                return null;
            }

            return await _organisationService.GetAvailableCollectionTypesAsync(ukprn);
        }

        [HttpGet("{ukprn}/{collectionName}")]
        public async Task<Collection> Get(long ukprn, string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return null;
            }

            return await _organisationService.GetCollectionAsync(ukprn, collectionName);
        }
    }
}
