using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Freedom.SystemData
{
    [CollectionDataContract(Namespace = SystemDataConstants.Namespace, Name = "SystemData", ItemName = "Section")]
    public class SystemDataCollection : IList<SystemDataSection>
    {
        private readonly List<SystemDataSection> _sections = new List<SystemDataSection>();

        public static SystemDataCollection GetCurrentData()
        {
            SystemDataCollection collection = new SystemDataCollection();

            IEnumerable<ISystemDataProvider> providers = IoC.GetAll<ISystemDataProvider>();

            foreach (ISystemDataProvider systemDataProvider in providers)
                systemDataProvider.LoadData(collection);

            return collection;
        }

        public SystemDataSection this[string sectionName]
        {
            get
            {
                foreach (SystemDataSection section in _sections)
                    if (string.Compare(section.Name, sectionName, StringComparison.OrdinalIgnoreCase) == 0)
                        return section;

                SystemDataSection newSection = new SystemDataSection(sectionName);

                _sections.Add(newSection);

                return newSection;
            }
        }

        public void Merge(IEnumerable<SystemDataSection> sections)
        {
            foreach (SystemDataSection section in sections)
                this[section.Name].Items.AddRange(section.Items);
        }

        public IEnumerator<SystemDataSection> GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _sections.GetEnumerator();
        }

        public void Add(SystemDataSection item)
        {
            _sections.Add(item);
        }

        public void Clear()
        {
            _sections.Clear();
        }

        public bool Contains(SystemDataSection item)
        {
            return _sections.Contains(item);
        }

        public void CopyTo(SystemDataSection[] array, int arrayIndex)
        {
            _sections.CopyTo(array, arrayIndex);
        }

        public bool Remove(SystemDataSection item)
        {
            return _sections.Remove(item);
        }

        public int Count => _sections.Count;

        public bool IsReadOnly => false;

        public int IndexOf(SystemDataSection item)
        {
            return _sections.IndexOf(item);
        }

        public void Insert(int index, SystemDataSection item)
        {
            _sections.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _sections.RemoveAt(index);
        }

        public SystemDataSection this[int index]
        {
            get { return _sections[index]; }
            set { _sections[index] = value; }
        }
    }
}