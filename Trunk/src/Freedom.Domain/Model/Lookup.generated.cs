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
	[KnownType(typeof(MarketIndex))]
	[KnownType(typeof(StockExchange))]
	[KnownType(typeof(Strategy))]

	public abstract partial class Lookup : LookupBase
	{
		[DataMember(EmitDefaultValue = false)]
		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				if (_isActive == value) return;
				_isActive = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private bool _isActive = true;

		public override void Copy(Entity entity)
		{
			base.Copy(entity);

			Lookup source = entity as Lookup;

			if (source == null)
				throw new ArgumentException("entity", "entity must be an instance of Lookup.");

			IsActive = source._isActive;
		}
	}
}
