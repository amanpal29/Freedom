using System.Runtime.Serialization;

namespace Freedom.Constraints
{
    [DataContract(Namespace = Constraint.Namespace)]
    public class OrderByExpression
    {
        public OrderByExpression()
        {
        }

        public OrderByExpression(string fieldName, bool descending = false)
        {
            FieldName = fieldName;
            Descending = descending;
        }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public bool Descending { get; set; }
    }
}