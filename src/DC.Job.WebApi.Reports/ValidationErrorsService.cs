using System.Collections.Generic;
using System.Threading.Tasks;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.KeyGenerator.Interface;
using ESFA.DC.Serialization.Interfaces;

namespace DC.Job.WebApi.Reports
{
    public class ValidationErrorsService : IValidationErrorsService
    {
        private readonly IKeyValuePersistenceService _ioPersistenceService;
        private readonly ISerializationService _serializationService;
        private readonly IKeyGenerator _keyGenerator;

        public ValidationErrorsService(IKeyValuePersistenceService ioPersistenceService, ISerializationService serializationService, IKeyGenerator keyGenerator)
        {
            _ioPersistenceService = ioPersistenceService;
            _serializationService = serializationService;
            _keyGenerator = keyGenerator;
        }

        public async Task<IEnumerable<ValidationError>> GetValidationErrorsAsync(long ukprn, long jobId)
        {
            var storageKey = _keyGenerator.GenerateKey(ukprn, jobId, TaskKeys.ValidationErrors,"_");

            var reportExists = await _ioPersistenceService.ContainsAsync(storageKey);

            if (reportExists)
            {
                var data = await _ioPersistenceService.GetAsync(storageKey);

                var result = _serializationService.Deserialize<IEnumerable<ValidationError>>(data);
                return result;
            }
            return new List<ValidationError>();
        }
    }
}
