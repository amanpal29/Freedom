﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Freedom.Domain.Exceptions;
using Freedom.Domain.Services.Time;

namespace Freedom.Domain.Model
{
	public partial class FreedomRepository : IDisposable
	{
		private readonly ITimeService _timeService = IoC.TryGet<ITimeService>() ?? new LocalTimeService();

		private readonly FreedomLocalContext _db;
		private readonly Guid _currentUserId;
		private readonly DateTime _modifyDateTime;
		private bool _disposed;

		public FreedomRepository(FreedomLocalContext db, Guid currentUserId)
			: this(db, currentUserId, default(DateTime))
		{
		}

		public FreedomRepository(FreedomLocalContext db, Guid currentUserId, DateTime modifyDateTime)
		{
			_db = db;
			_currentUserId = currentUserId;
			_modifyDateTime = modifyDateTime > default(DateTime) ? modifyDateTime : _timeService.UtcNow;
		}

		public FreedomLocalContext Context
		{
			get { return _db; }
		}

		public void DetectChanges()
		{
			_db.ChangeTracker.DetectChanges();
		}

		public Task<int> SaveChangesAsync()
		{
			_db.ChangeTracker.DetectChanges();

			_db.UpdateAuditProperties(_currentUserId, _modifyDateTime);

			return _db.SaveChangesAsync();
		}
	
		// Explicitally updates the AuditProperties of a given AggregateRoot entity
		public void UpdateAuditProperties(AggregateRoot entity, EntityState state = EntityState.Modified)
		{
			if (state.HasFlag(EntityState.Added))
			{
				entity.CreatedBy = null;
				entity.CreatedById = _currentUserId;
				entity.CreatedDateTime = _modifyDateTime;
			}

			if (!state.HasFlag(EntityState.Unchanged))
			{
				if(entity.ModifiedById != _currentUserId)
					entity.ModifiedBy = null;

				entity.ModifiedById = _currentUserId;
				entity.ModifiedDateTime = _modifyDateTime;
			}
		}

		#region Get Methods

		public async Task<ApplicationSetting> GetApplicationSettingAsync(Guid id)
		{
			return await _db.ApplicationSetting.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<MarketIndex> GetMarketIndexAsync(Guid id)
		{
			return await _db.MarketIndex.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<Notification> GetNotificationAsync(Guid id)
		{
			return await _db.Notification.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<Role> GetRoleAsync(Guid id)
		{
			IQueryable<Role> baseQuery = _db.Role.Where(x => x.Id == id);

			Role result = await baseQuery.SingleOrDefaultAsync();

			await LoadChildrenAsync(baseQuery);

			return result;
		}

		public async Task<Stock> GetStockAsync(Guid id)
		{
			return await _db.Stock.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<StockExchange> GetStockExchangeAsync(Guid id)
		{
			return await _db.StockExchange.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<Strategy> GetStrategyAsync(Guid id)
		{
			return await _db.Strategy.SingleOrDefaultAsync(x => x.Id == id);
		}

		public async Task<User> GetUserAsync(Guid id)
		{
			IQueryable<User> baseQuery = _db.User.Where(x => x.Id == id);

			User result = await baseQuery.SingleOrDefaultAsync();

			await LoadChildrenAsync(baseQuery);

			return result;
		}

		public async Task<Watchlist> GetWatchlistAsync(Guid id)
		{
			IQueryable<Watchlist> baseQuery = _db.Watchlist.Where(x => x.Id == id);

			Watchlist result = await baseQuery.SingleOrDefaultAsync();

			await LoadChildrenAsync(baseQuery);

			return result;
		}

		#endregion

		#region LoadChildren Methods

		public async Task LoadChildrenAsync(IQueryable<Role> entities)
		{
			await entities.SelectMany(x => x.Permissions).LoadAsync();
		}

		public async Task LoadChildrenAsync(IQueryable<User> entities)
		{
			await entities.SelectMany(x => x.UserRole).LoadAsync();
		}

		public async Task LoadChildrenAsync(IQueryable<Watchlist> entities)
		{
			await entities.SelectMany(x => x.WatchlistStock).LoadAsync();
		}

		#endregion

		#region Add Methods

		public ApplicationSetting Add(ApplicationSetting item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			ApplicationSetting entity = new ApplicationSetting();

			entity.Copy(item);

			_db.ApplicationSetting.Add(entity);

			return entity;
		}

		public MarketIndex Add(MarketIndex item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			MarketIndex entity = new MarketIndex();

			entity.Copy(item);

			_db.MarketIndex.Add(entity);

			return entity;
		}

		public Notification Add(Notification item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Notification entity = new Notification();

			entity.Copy(item);

			_db.Notification.Add(entity);

			return entity;
		}

		public Role Add(Role item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Role entity = new Role();

			entity.Copy(item);

			foreach (Permission child in item.Permissions)
				Add(entity.Permissions, child);

			_db.Role.Add(entity);

			return entity;
		}

		public Stock Add(Stock item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Stock entity = new Stock();

			entity.Copy(item);

			_db.Stock.Add(entity);

			return entity;
		}

		public StockExchange Add(StockExchange item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			StockExchange entity = new StockExchange();

			entity.Copy(item);

			_db.StockExchange.Add(entity);

			return entity;
		}

		public Strategy Add(Strategy item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Strategy entity = new Strategy();

			entity.Copy(item);

			_db.Strategy.Add(entity);

			return entity;
		}

		public User Add(User item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			User entity = new User();

			entity.Copy(item);

			foreach (Guid id in item.RoleIds)
				_db.UserRole.Add(new UserRole(entity.Id, id));

			_db.User.Add(entity);

			return entity;
		}

		public Watchlist Add(Watchlist item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			Watchlist entity = new Watchlist();

			entity.Copy(item);

			foreach (Guid id in item.StockIds)
				_db.WatchlistStock.Add(new WatchlistStock(entity.Id, id));

			_db.Watchlist.Add(entity);

			return entity;
		}

		private static void Add(ICollection<Permission> collection, Permission item)
		{
			if (item == collection)
				throw new ArgumentNullException("collection");

			if (item == null)
				throw new ArgumentNullException("item");

			Permission entity = new Permission();

			entity.Copy(item);

			collection.Add(entity);
		}

		private static void Add(ICollection<UserRole> collection, UserRole item)
		{
			if (item == collection)
				throw new ArgumentNullException("collection");

			if (item == null)
				throw new ArgumentNullException("item");

			UserRole entity = new UserRole();

			entity.Copy(item);

			collection.Add(entity);
		}

		private static void Add(ICollection<WatchlistStock> collection, WatchlistStock item)
		{
			if (item == collection)
				throw new ArgumentNullException("collection");

			if (item == null)
				throw new ArgumentNullException("item");

			WatchlistStock entity = new WatchlistStock();

			entity.Copy(item);

			collection.Add(entity);
		}

		#endregion

		#region Update Methods

		public async Task UpdateAsync(ApplicationSetting item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<ApplicationSetting> baseQuery = _db.ApplicationSetting.Where(x => x.Id == item.Id);

			ApplicationSetting existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);


			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(MarketIndex item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<MarketIndex> baseQuery = _db.MarketIndex.Where(x => x.Id == item.Id);

			MarketIndex existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);


			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(Notification item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<Notification> baseQuery = _db.Notification.Where(x => x.Id == item.Id);

			Notification existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);

		}

		public async Task UpdateAsync(Role item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<Role> baseQuery = _db.Role.Where(x => x.Id == item.Id);

			Role existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			await LoadChildrenAsync(baseQuery);

			existingItem.Copy(item);

			UpdateChildren(existingItem.Permissions, item.Permissions);

			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(Stock item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<Stock> baseQuery = _db.Stock.Where(x => x.Id == item.Id);

			Stock existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);


			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(StockExchange item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<StockExchange> baseQuery = _db.StockExchange.Where(x => x.Id == item.Id);

			StockExchange existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);


			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(Strategy item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<Strategy> baseQuery = _db.Strategy.Where(x => x.Id == item.Id);

			Strategy existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			existingItem.Copy(item);


			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(User item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<User> baseQuery = _db.User.Where(x => x.Id == item.Id);

			User existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			await LoadChildrenAsync(baseQuery);

			existingItem.Copy(item);

			UpdateIntermediate(existingItem.UserRole, item.Id, item.RoleIds);

			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		public async Task UpdateAsync(Watchlist item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			IQueryable<Watchlist> baseQuery = _db.Watchlist.Where(x => x.Id == item.Id);

			Watchlist existingItem = await baseQuery.FirstOrDefaultAsync();

			if (existingItem == null)
				throw new ConcurrencyException(ConcurrencyExceptionCode.ItemNotFound);

			await LoadChildrenAsync(baseQuery);

			existingItem.Copy(item);

			UpdateIntermediate(existingItem.WatchlistStock, item.Id, item.StockIds);

			UpdateAuditProperties(existingItem, EntityState.Modified);
		}

		#endregion

		#region Child Collection Update Methods

		private void UpdateChildren(ICollection<Permission> target, ICollection<Permission> source)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (source == null)
				throw new ArgumentNullException("source");

			List<Permission> itemsToDelete = target.ToList();

			foreach(Permission item in source)
			{
				Permission existingItem = itemsToDelete.SingleOrDefault(p => p.Id == item.Id);

				if (existingItem != null)
				{
					itemsToDelete.Remove(existingItem);

					existingItem.Copy(item);
				}
				else
				{
					Add(target, item);
				}
			}

			foreach(Permission item in itemsToDelete)
			{
				_db.Permission.Remove(item);
			}
		}

		private void UpdateIntermediate(ICollection<UserRole> target, Guid parentId, ICollection<Guid> keys)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (keys == null)
				throw new ArgumentNullException("keys");

			List<UserRole> itemsToRemove = target.ToList();

			foreach (Guid key in keys)
			{
				if (itemsToRemove.RemoveAll(x => x.RoleId == key) > 0)
					continue;

				target.Add(new UserRole(parentId, key));
			}

			foreach (UserRole item in itemsToRemove)
			{
				_db.UserRole.Remove(item);
			}
		}

		private void UpdateIntermediate(ICollection<WatchlistStock> target, Guid parentId, ICollection<Guid> keys)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (keys == null)
				throw new ArgumentNullException("keys");

			List<WatchlistStock> itemsToRemove = target.ToList();

			foreach (Guid key in keys)
			{
				if (itemsToRemove.RemoveAll(x => x.StockId == key) > 0)
					continue;

				target.Add(new WatchlistStock(parentId, key));
			}

			foreach (WatchlistStock item in itemsToRemove)
			{
				_db.WatchlistStock.Remove(item);
			}
		}

		#endregion

		#region Delete Methods

		public async Task<bool> DeleteApplicationSettingAsync(Guid id)
		{
			ApplicationSetting existingItem = await _db.ApplicationSetting.FindAsync(id);

			if (existingItem == null) return false;

			_db.ApplicationSetting.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteMarketIndexAsync(Guid id)
		{
			MarketIndex existingItem = await _db.MarketIndex.FindAsync(id);

			if (existingItem == null) return false;

			_db.MarketIndex.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteNotificationAsync(Guid id)
		{
			Notification existingItem = await _db.Notification.FindAsync(id);

			if (existingItem == null) return false;

			_db.Notification.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteRoleAsync(Guid id)
		{
			Role existingItem = await _db.Role.FindAsync(id);

			if (existingItem == null) return false;

			_db.Role.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteStockAsync(Guid id)
		{
			Stock existingItem = await _db.Stock.FindAsync(id);

			if (existingItem == null) return false;

			_db.Stock.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteStockExchangeAsync(Guid id)
		{
			StockExchange existingItem = await _db.StockExchange.FindAsync(id);

			if (existingItem == null) return false;

			_db.StockExchange.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteStrategyAsync(Guid id)
		{
			Strategy existingItem = await _db.Strategy.FindAsync(id);

			if (existingItem == null) return false;

			_db.Strategy.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteUserAsync(Guid id)
		{
			User existingItem = await _db.User.FindAsync(id);

			if (existingItem == null) return false;

			_db.User.Remove(existingItem);

			return true;
		}

		public async Task<bool> DeleteWatchlistAsync(Guid id)
		{
			Watchlist existingItem = await _db.Watchlist.FindAsync(id);

			if (existingItem == null) return false;

			_db.Watchlist.Remove(existingItem);

			return true;
		}

		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				_db.Dispose();
			}
		}

		#endregion
	}
}

