using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Indexed;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.JobQueueManager.Interfaces;
using ESFA.DC.Jobs.Model.Enums;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Remotion.Linq.Parsing.ExpressionVisitors.MemberBindings;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/file-validation")]
    public class FileValidationController : ControllerBase
    {
        private readonly IFcsContext _fcsContext;
        private readonly ILogger _logger;

        public FileValidationController(
            //IFcsContext fcsContext,
            ILogger logger)
        {
            //_fcsContext = fcsContext;
            _logger = logger;
        }

        [HttpGet("conref/{ukprn}/{contractReference}")]
        public async Task<IActionResult> ValidateContractReference(long ukprn, string contractReference)
        {
            _logger.LogInfo($"Get request recieved to validate contract reference for ukprn {ukprn}, contract ref: {contractReference}");

            if (ukprn < 1 || string.IsNullOrEmpty(contractReference))
            {
                _logger.LogWarning($"invalid contract reference : {contractReference} or ukprn : {ukprn}");
                return new BadRequestResult();
            }

            try
            {
                var existsAny = false;
                //var existsAny = _fcsContext.ContractAllocations
                //    .Any(ca => ca.DeliveryUKPRN == ukprn && ca.ContractAllocationNumber == contractReference);
                if (existsAny)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error occured trying to validate contract reference errors for ukprn : {ukprn}, contract reference : {contractReference}",
                    ex);
                return new BadRequestResult();
            }

            return NotFound();
        }
    }
}