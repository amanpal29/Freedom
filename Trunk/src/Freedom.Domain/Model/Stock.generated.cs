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
	[Reportable]

	public partial class Stock : AggregateRoot
	{
		public override string EntityTypeName
		{
			get { return "Stock"; }
		}

		[DataMember(EmitDefaultValue = false)]
		public string Symbol
		{
			get { return _symbol; }
			set
			{
				if (_symbol == value) return;
				_symbol = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private string _symbol;

		[DataMember(EmitDefaultValue = false)]
		public Guid StockExchangeId
		{
			get { return _stockExchangeId; }
			set
			{
				if (_stockExchangeId == value) return;
				_stockExchangeId = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private Guid _stockExchangeId;

		[DataMember(EmitDefaultValue = false)]
		public virtual StockExchange StockExchange
		{
			get { return _stockExchange; }
			set
			{
				if (object.ReferenceEquals(_stockExchange, value)) return;

				_stockExchange = value;

				if (value != null)
					StockExchangeId = value.Id;

				OnPropertyChanged();
			}
		}
		private StockExchange _stockExchange;

		[DataMember(EmitDefaultValue = false)]
		public override User CreatedBy
		{
			get { return _createdBy; }
			set
			{
				if (object.ReferenceEquals(_createdBy, value)) return;

				_createdBy = value;

				if (value != null)
					CreatedById = value.Id;

				OnPropertyChanged();
			}
		}
		private User _createdBy;

		[DataMember(EmitDefaultValue = false)]
		public override User ModifiedBy
		{
			get { return _modifiedBy; }
			set
			{
				if (object.ReferenceEquals(_modifiedBy, value)) return;

				_modifiedBy = value;

				if (value != null)
					ModifiedById = value.Id;

				OnPropertyChanged();
			}
		}
		private User _modifiedBy;

		public override void Copy(Entity entity)
		{
			base.Copy(entity);

			Stock source = entity as Stock;

			if (source == null)
				throw new ArgumentException("entity", "entity must be an instance of Stock.");

			Symbol = source._symbol;
			StockExchangeId = source._stockExchangeId;
		}
	}
}
