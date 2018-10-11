using System.Threading.Tasks;
using ESFA.DC.CollectionsManagement.Models;
using ESFA.DC.CollectionsManagement.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Route("api/returns-calendar")]
    public class ReturnsCalendarController : Controller
    {
        private readonly IReturnCalendarService _retrunCalendarService;

        public ReturnsCalendarController(IReturnCalendarService retrunCalendarService)
        {
            _retrunCalendarService = retrunCalendarService;
        }

        // GET api/values/5
        [HttpGet("{collectionName}/current")]
        public async Task<ReturnPeriod> GetCurrent(string collectionName)
        {
             return await _retrunCalendarService.GetCurrentPeriodAsync(collectionName);
        }

        // GET api/values/5
        [HttpGet("{collectionName}/next")]
        public async Task<ReturnPeriod> GetNext(string collectionName)
        {
            return await _retrunCalendarService.GetNextPeriodAsync(collectionName);
        }
    }
}