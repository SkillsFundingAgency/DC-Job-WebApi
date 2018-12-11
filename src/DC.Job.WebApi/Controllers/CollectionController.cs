using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.JobQueueManager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Route("api/collections")]
    public class CollectionController : Controller
    {
        private readonly IOrganisationService _organisationService;
        private readonly ICollectionService _collectionService;

        public CollectionController(IOrganisationService organisationService, ICollectionService collectionService)
        {
            _organisationService = organisationService;
            _collectionService = collectionService;
        }

        // GET api/values/5
        [HttpGet("{ukprn}/{collectionType}")]
        public async Task<IEnumerable<Collection>> Get(long ukprn, string collectionType)
        {
            return await _organisationService.GetAvailableCollectionsAsync(ukprn, collectionType);
        }

        // GET api/values/5
        [HttpGet("{collectionType}")]
        public async Task<Collection> Get(string collectionType)
        {
            return await _collectionService.GetCollectionAsync(collectionType);
        }
    }
}