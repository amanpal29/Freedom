//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Freedom.Domain.Model
{
	[DataContract(Namespace = Entity.Namespace)]
	public enum Priority : int
	{
		[EnumMember]
		Normal = 0,

		[EnumMember]
		High = 1,

		[EnumMember]
		Low = -1,

	}
}

