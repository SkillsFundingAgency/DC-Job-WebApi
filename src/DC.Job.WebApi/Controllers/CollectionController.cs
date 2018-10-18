using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Route("api/collections")]
    public class CollectionController : Controller
    {
        private readonly IOrganisationService _organisationService;

        public CollectionController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        // GET api/values/5
        [HttpGet("{ukprn}/{collectionType}")]
        public async Task<IEnumerable<Collection>> Get(long ukprn, string collectionType)
        {
            return await _organisationService.GetAvailableCollectionsAsync(ukprn, collectionType);
        }
    }
}