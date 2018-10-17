using System;

namespace DemoDataBuilder.OutputModel
{
    public class RiskAssessment : Entity
    {
        public Guid Facility;
        public Guid InspectorId;
        public DateTime RiskAssessmentDate;
        public int RiskScore;
        public string RiskRating;
    }
}
