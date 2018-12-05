using System;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.Data.Organisations.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.ReferenceData.FCS.Model.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ESFA.DC.Job.WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/file-validation")]
    public class FileValidationController : ControllerBase
    {
        private readonly IFcsContext _fcsContext;
        private readonly IOrganisationsContext _organisationsContext;
        private readonly ILogger _logger;

        public FileValidationController(
            IFcsContext fcsContext,
            IOrganisationsContext organisationsContext,
            ILogger logger)
        {
            _fcsContext = fcsContext;
            _organisationsContext = organisationsContext;
            _logger = logger;
        }

        [HttpGet("org/{ukprn}")]
        public async Task<IActionResult> ValidateUkprnOrganisation(long ukprn)
        {
            _logger.LogInfo($"Get request recieved to validate ukprn for orgs db : {ukprn}");

            if (ukprn < 1)
            {
                _logger.LogWarning($"invalid ukprn : {ukprn}");
                return new BadRequestResult();
            }

            try
            {
                var existsAny = _organisationsContext.MasterOrganisations
                    .Any(ca => ca.Ukprn == ukprn);
                if (existsAny)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Error occured trying to validate ukprn for orgs db : {ukprn}", ex);
                return new BadRequestResult();
            }

            return NotFound();
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
                var existsAny = _fcsContext.ContractAllocation
                    .Any(ca => ca.DeliveryUkprn == ukprn && ca.ContractAllocationNumber == contractReference);
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