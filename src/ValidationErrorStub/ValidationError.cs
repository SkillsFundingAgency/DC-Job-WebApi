using System;
using System.Collections.Generic;
using System.Text;

namespace ValidationErrorStub
{
    public class ValidationError
    {
        public string ErrorMessage { get; set; }

        public string LearnRefNumber { get; set; }

        public string FieldValues { get; set; }

        public string Severity { get; set; }

        public int AimSequenceNumber { get; set; }

        public string RuleId { get; set; }
    }
}
