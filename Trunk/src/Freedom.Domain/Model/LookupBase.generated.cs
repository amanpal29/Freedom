//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Freedom.Domain.Infrastructure;
using Freedom.Domain.Services.Repository;
using Freedom.Collections;
using Freedom.ComponentModel;

namespace Freedom.Domain.Model
{
	[DataContract(Namespace = Namespace)]
	[KnownType(typeof(Lookup))]

	public abstract partial class LookupBase : AggregateRoot
	{
		[DataMember(EmitDefaultValue = false)]
		public string Description
		{
			get { return _description; }
			set
			{
				if (_description == value) return;
				_description = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private string _description;

		[DataMember(EmitDefaultValue = false)]
		public int SortOrder
		{
			get { return _sortOrder; }
			set
			{
				if (_sortOrder == value) return;
				_sortOrder = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private int _sortOrder = 0;

		public override void Copy(Entity entity)
		{
			base.Copy(entity);

			LookupBase source = entity as LookupBase;

			if (source == null)
				throw new ArgumentException("entity", "entity must be an instance of LookupBase.");

			Description = source._description;
			SortOrder = source._sortOrder;
		}
	}
}
