using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Freedom.Domain.Model;
using Freedom.Annotations;

namespace Freedom.Domain.Services.Security
{
    [CollectionDataContract(Namespace = Constants.ContractNamespace, Name = "Permissions", ItemName = "Permission")]
    public class FreedomPermissionSet : ICollection<string>, IEnumerable<SystemPermission>
    {
        #region Constructors

        public FreedomPermissionSet()
        {
        }

        public FreedomPermissionSet(params SystemPermission[] permissions)
            :this((IEnumerable<SystemPermission>) permissions)
        {
        }

        public FreedomPermissionSet([NotNull] IEnumerable<SystemPermission> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            _permissions = new HashSet<SystemPermission>(permissions);
        }

        public FreedomPermissionSet([NotNull] IEnumerable<string> permissions)
        {
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));

            _permissions = new HashSet<SystemPermission>();

            foreach (string permission in permissions)
            {
                SystemPermission systemPermission;

                if (Enum.TryParse(permission, true, out systemPermission))
                    _permissions.Add(systemPermission);
            }
        }

        #endregion

        #region ICollection<SystemPermission>

        private HashSet<SystemPermission> _permissions;

        public ICollection<SystemPermission> Permissions => _permissions ?? (_permissions = new HashSet<SystemPermission>());

        public static implicit operator HashSet<SystemPermission>(FreedomPermissionSet permissionSet)
        {
            return permissionSet._permissions;
        }

        public void Add(SystemPermission item)
        {
            Permissions.Add(item);
        }

        public bool Contains(SystemPermission item)
        {
            return _permissions?.Contains(item) ?? false;
        }


        public bool Remove(SystemPermission item)
        {
            return _permissions?.Remove(item) ?? false;
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<string> GetEnumerator()
        {
            foreach (SystemPermission permission in Permissions)
                yield return permission.ToString();
        }

        IEnumerator<SystemPermission> IEnumerable<SystemPermission>.GetEnumerator()
        {
            return (_permissions ?? Enumerable.Empty<SystemPermission>()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<string>

        public void Add(string item)
        {
            SystemPermission permission;

            if (Enum.TryParse(item, true, out permission))
                Permissions.Add(permission);
        }

        public void Clear()
        {
            _permissions?.Clear();
        }

        public bool Contains(string item)
        {
            SystemPermission permission;

            return _permissions != null &&
                   Enum.TryParse(item, true, out permission) &&
                   _permissions.Contains(permission);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            if (_permissions == null) return;

            foreach (SystemPermission permission in _permissions)
                array[arrayIndex++] = permission.ToString();
        }

        public bool Remove(string item)
        {
            SystemPermission permission;

            return _permissions != null &&
                   Enum.TryParse(item, true, out permission) &&
                   _permissions.Remove(permission);
        }

        public int Count => _permissions?.Count ?? 0;

        public bool IsReadOnly => false;

        #endregion
    }
}