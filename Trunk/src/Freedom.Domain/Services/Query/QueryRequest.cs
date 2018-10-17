using System;
using System.Runtime.Serialization;
using Freedom.Domain.Services.Repository;
using Freedom.Constraints;

namespace Freedom.Domain.Services.Query
{
    [DataContract(Namespace = Constants.ContractNamespace)]
    public class QueryRequest
    {
        public QueryRequest()
        {
        }

        public QueryRequest(DateTime? pointInTime, ResolutionGraph resolutionGraph, Constraint constraint)
        {
            PointInTime = pointInTime;
            ResolutionGraph = resolutionGraph;
            Constraint = constraint;
        }

        [DataMember(EmitDefaultValue = false)]
        public DateTime? PointInTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ResolutionGraph ResolutionGraph { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Constraint Constraint { get; set; }
    }
}
