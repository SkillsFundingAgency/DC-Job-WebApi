using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESFA.DC.ILR.ValidationErrorReport.Model;
using ESFA.DC.ILR.ValidationService.IO.Model;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace DC.Job.WebApi.Reports
{
    public class ValidationErrorsService : IValidationErrorsService
    {
        private readonly IKeyValuePersistenceService _ioPersistenceService;
        private readonly ISerializationService _serializationService;
        private readonly IKeyGenerator _keyGenerator;
        private readonly ILogger _logger;

        public ValidationErrorsService(IKeyValuePersistenceService ioPersistenceService, ISerializationService serializationService, IKeyGenerator keyGenerator, ILogger logger)
        {
            _ioPersistenceService = ioPersistenceService;
            _serializationService = serializationService;
            _keyGenerator = keyGenerator;
            _logger = logger;
        }

        public async Task<IEnumerable<ReportValidationError>> GetValidationErrorsAsync(long ukprn, long jobId)
        {
            var result = new List<ReportValidationError>();
            try
            {
                var validationErrorsKey = _keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrors, "_");
                var errorLookupKey = _keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrorsLookup, "_");

                var reportExists = await _ioPersistenceService.ContainsAsync(validationErrorsKey);

                if (reportExists)
                {
                    _logger.LogInfo($"Error report exists for jobId: {jobId}, ukprn : {ukprn}");
                    var validationErrorsData = await _ioPersistenceService.GetAsync(validationErrorsKey);
                    var errorsLookupData = await _ioPersistenceService.GetAsync(errorLookupKey);

                    var validationErrors =
                        _serializationService.Deserialize<IEnumerable<ValidationError>>(validationErrorsData);
                    var errorMessageLookups =
                        _serializationService.Deserialize<IEnumerable<ValidationErrorMessageLookup>>(errorsLookupData);

                    validationErrors.ToList().ForEach(x =>
                        result.Add(new ReportValidationError
                        {
                            AimSequenceNumber = x.AimSequenceNumber,
                            LearnerReferenceNumber = x.LearnerReferenceNumber,
                            RuleName = x.RuleName,
                            Severity = x.Severity,
                            ErrorMessage = errorMessageLookups.Single(y => x.RuleName == y.RuleName).Message,
                        }));
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error occured trying to get validation errors for ukprn {ukprn}, jobid {jobId}", e);
                throw;
            }

            return result;
        }
    }
}
