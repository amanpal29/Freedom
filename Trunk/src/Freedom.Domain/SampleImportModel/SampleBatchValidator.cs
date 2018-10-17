using System;
using System.Collections.Generic;
using Hedgerow;

namespace Hedgehog.SampleImportModel
{
    public class SampleBatchValidator
    {
        public IEnumerable<string> Errors => _errorList;
        private readonly IList<string> _errorList = new List<string>();

        public bool Validate(SampleBatch sampleBatch)
        {
            bool bValid = ValidateFileCreationDate(sampleBatch);
            foreach (SampleDetail sampleDetail in sampleBatch.SampleDetails)
            {
                bValid &= ValidateCollectedDate(sampleDetail);
                bValid &= ValidateSubmittedDate(sampleDetail);
                bValid &= ValidateAnalyzedDate(sampleDetail);
            }
            return bValid;
        }

        private bool ValidateAnalyzedDate(SampleDetail sampleDetail)
        {
            if (sampleDetail.SampleAnalyzedDate.IsMissing)
            {
                _errorList.Add("Sample Analyzed Date is a mandatory field and was not provided. Batch rejected");
                return false;
            }
            if (sampleDetail.SampleAnalyzedDate.HasParseError)
            {
                _errorList.Add("Sample Analyzed Date unreadable and must be provided in an ISO 8601 format. Batch rejected");
                return false;
            }

            if (sampleDetail.SampleSubmittedDate.HasValue)
            {
                if (!DatesMustBeEqualOrInOrder(sampleDetail.SampleSubmittedDate.Value, sampleDetail.SampleAnalyzedDate,
                    "Sample Analyzed Date " + sampleDetail.SampleAnalyzedDate +
                    " cannot be prior to Sample Submitted Date " + sampleDetail.SampleSubmittedDate +
                    ". Batch rejected."))
                {
                    return false;
                }
            }
            else
            {
                if (!DatesMustBeEqualOrInOrder(sampleDetail.SampleCollectedDate, sampleDetail.SampleAnalyzedDate,
                    "Sample Analyzed Date " + sampleDetail.SampleAnalyzedDate +
                    " cannot be prior to Sample Collected Date " + sampleDetail.SampleCollectedDate +
                    ". Batch rejected."))
                {
                    return false;
                }
            }

            return DateIsInThePast(sampleDetail.SampleAnalyzedDate, "Sample Analyzed Date " + sampleDetail.SampleAnalyzedDate + " cannot be in the future. Batch rejected.");
        }

        private bool ValidateSubmittedDate(SampleDetail sampleDetail)
        {
            if (sampleDetail.SampleSubmittedDate != null)
            {
                if (sampleDetail.SampleSubmittedDate.Value.HasParseError)
                {
                    _errorList.Add("Sample Submitted Date unreadable and must be provided in an ISO 8601 format. Batch rejected");
                    return false;
                }

                if (!DatesMustBeEqualOrInOrder(sampleDetail.SampleCollectedDate, sampleDetail.SampleSubmittedDate.Value,
                    "Sample Submitted Date " + sampleDetail.SampleSubmittedDate +
                    " cannot occur before Sample Collection Date " + sampleDetail.SampleCollectedDate +
                    ". Batch rejected."))
                    return false;
                return DateIsInThePast(sampleDetail.SampleSubmittedDate.Value, "Sample Submitted Date " + sampleDetail.SampleSubmittedDate + " cannot be in the future. Batch rejected.");
            }

            return true;
        }

        private bool ValidateCollectedDate(SampleDetail sampleDetail)
        {
            if (sampleDetail.SampleCollectedDate.IsMissing)
            {
                _errorList.Add("Sample Collected Date is a mandatory field and was not provided. Batch rejected");
               return false;
            }

            if (sampleDetail.SampleCollectedDate.HasParseError)
            {
                _errorList.Add("Sample Collected Date unreadable and must be provided in an ISO 8601 format. Batch rejected");
                return false;
            }
            return DateIsInThePast(sampleDetail.SampleCollectedDate, "Sample Collected Date " + sampleDetail.SampleCollectedDate + " cannot be in the future. Batch rejected.");
        }

        private bool ValidateFileCreationDate(SampleBatch sampleBatch)
        {
            if (sampleBatch.FileCreationDateTimeOffset.IsMissing)
            {
                _errorList.Add("File Creation Date is a mandatory field and was not provided. Batch rejected");
                return false;
            }

            if (sampleBatch.FileCreationDateTimeOffset.HasParseError)
            {
                _errorList.Add("File Creation Date unreadable and must be provided in an ISO 8601 format. Batch rejected");
                return false;
            }

            return DateIsInThePast(sampleBatch.FileCreationDateTimeOffset, "File Creation Date " + sampleBatch.FileCreationDateTimeOffset + " cannot be in the future. Batch rejected.");
        }

        private bool DateIsInThePast(DateTimeOffset dateTimeOffset, string errorMessage)
        {
            bool dateIsInThePast = dateTimeOffset <= DateTimeOffset.Now;
            if (dateIsInThePast) return true;

            _errorList.Add(errorMessage);
            return false;
        }
        private bool DatesMustBeEqualOrInOrder(SerializableDateTimeOffset earlyDate, SerializableDateTimeOffset laterDate, string errorMessage)
        {
            if (earlyDate > laterDate)
            {
                _errorList.Add(errorMessage);
                return false;
            }
            return true;
        }
    }
}
