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
	[KnownType(typeof(Notification))]
	[KnownType(typeof(NotificationClass))]
	[KnownType(typeof(Priority))]

	public abstract partial class NotificationBase : EntityBase
	{
		[DataMember(EmitDefaultValue = false)]
		public NotificationClass Class
		{
			get { return _class; }
			set
			{
				if (_class == value) return;
				_class = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private NotificationClass _class;

		[DataMember(EmitDefaultValue = false)]
		public Priority Priority
		{
			get { return _priority; }
			set
			{
				if (_priority == value) return;
				_priority = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private Priority _priority;

		[DataMember(EmitDefaultValue = false)]
		public string Title
		{
			get { return _title; }
			set
			{
				if (_title == value) return;
				_title = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private string _title;

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
		public string Payload
		{
			get { return _payload; }
			set
			{
				if (_payload == value) return;
				_payload = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private string _payload;

		[DataMember(EmitDefaultValue = false)]
		public DateTime CreatedDateTime
		{
			get { return _createdDateTime; }
			set
			{
				if (_createdDateTime == value) return;
				_createdDateTime = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private DateTime _createdDateTime;

		[DataMember(EmitDefaultValue = false)]
		public DateTimeOffset? ReceivedDateTime
		{
			get { return _receivedDateTime; }
			set
			{
				if (_receivedDateTime == value) return;
				_receivedDateTime = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private DateTimeOffset? _receivedDateTime;

		[DataMember(EmitDefaultValue = false)]
		public DateTimeOffset? ReadDateTime
		{
			get { return _readDateTime; }
			set
			{
				if (_readDateTime == value) return;
				_readDateTime = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private DateTimeOffset? _readDateTime;

		[DataMember(EmitDefaultValue = false)]
		public Guid? CreatedById
		{
			get { return _createdById; }
			set
			{
				if (_createdById == value) return;
				_createdById = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private Guid? _createdById;

		[DataMember(EmitDefaultValue = false)]
		public Guid RecipientId
		{
			get { return _recipientId; }
			set
			{
				if (_recipientId == value) return;
				_recipientId = value;
				MarkAsChanged();
				OnPropertyChanged();
			}
		}
		private Guid _recipientId;

		public override void Copy(Entity entity)
		{
			base.Copy(entity);

			NotificationBase source = entity as NotificationBase;

			if (source == null)
				throw new ArgumentException("entity", "entity must be an instance of NotificationBase.");

			Class = source._class;
			Priority = source._priority;
			Title = source._title;
			Description = source._description;
			Payload = source._payload;
			CreatedDateTime = source._createdDateTime;
			ReceivedDateTime = source._receivedDateTime;
			ReadDateTime = source._readDateTime;
			CreatedById = source._createdById;
			RecipientId = source._recipientId;
		}
	}
}
