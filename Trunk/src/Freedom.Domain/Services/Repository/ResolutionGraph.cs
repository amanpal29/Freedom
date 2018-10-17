using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Freedom.Annotations;

namespace Freedom.Domain.Services.Repository
{
    [CollectionDataContract(Namespace = Constants.ContractNamespace, ItemName = "Path")]
    public class ResolutionGraph : ICollection<string>, IEnumerable<ResolutionGraphNode>
    {
        public static readonly ResolutionGraph Empty = new ResolutionGraph();

        public ResolutionGraph()
        {
        }

        public ResolutionGraph(IEnumerable<string> paths)
        {
            ResolutionGraphNode root = new ResolutionGraphNode();

            root.AddRange(paths);

            if (root.Count > 0)
                Nodes = root;
        }

        public ResolutionGraph(params string[] paths)
        {
            if (paths.Length == 0) return;

            Nodes = new ResolutionGraphNode();

            Nodes.AddRange(paths);
        }

        public void Add(string path)
        {
            if (Nodes == null)
                Nodes = new ResolutionGraphNode();

            Nodes.Add(path);
        }

        public void AddRange(IEnumerable<string> paths)
        {
            if (Nodes == null)
                Nodes = new ResolutionGraphNode();

            Nodes.AddRange(paths);
        }

        public void Clear()
        {
            Nodes = null;
        }

        public bool Contains(string item)
        {
            if (string.IsNullOrEmpty(item) || Nodes == null)
                return false;

            ResolutionGraphNode current = Nodes;

            foreach (string part in item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                current = current.Find(part);

                if (current == null)
                    return false;
            }

            return true;
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            if (Nodes == null) return;

            using (IEnumerator<string> enumerator = GetEnumerator())
                while (enumerator.MoveNext())
                    array[arrayIndex++] = enumerator.Current;
        }

        public bool Remove(string item)
        {
            if (string.IsNullOrEmpty(item) || Nodes == null)
                return false;

            ResolutionGraphNode parent = Nodes;

            string[] parts = item.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < parts.Length - 2; i++)
            {
                parent = parent.Find(parts[i]);

                if (parent == null)
                    return false;
            }

            return parent.Remove(parts[parts.Length - 1]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Nodes != null
                ? new ResolutionGraphPathEnumerator(Nodes)
                : Enumerable.Empty<string>().GetEnumerator();
        }

        public IEnumerable<Type> GetResolvedTypes(Type root)
        {
            HashSet<Type> result = new HashSet<Type>();

            result.Add(root);

            AddResolvedTypes(root, Nodes, result);

            return result;
        }

        private static void AddResolvedTypes(Type currentType, IEnumerable<ResolutionGraphNode> nodes, ICollection<Type> result)
        {
            foreach (ResolutionGraphNode node in nodes)
            {
                Type memberType = RefelectionHelper.GetMemberType(currentType, node.Name);

                Type elementType = TypeHelper.GetElementType(memberType);

                result.Add(elementType);

                if (node.Count == 0) continue;

                AddResolvedTypes(elementType, node, result);
            }
        }

        public ResolutionGraphNode Nodes { get; private set; }

        IEnumerator<ResolutionGraphNode> IEnumerable<ResolutionGraphNode>.GetEnumerator()
        {
            return (Nodes ?? Enumerable.Empty<ResolutionGraphNode>()).GetEnumerator();
        }

        public int Count => Nodes?.Count ?? 0;

        public bool IsReadOnly => false;

        private class ResolutionGraphPathEnumerator : IEnumerator<string>
        {
            private readonly Queue<ResolutionGraphNode> _queue; 
            private ResolutionGraphNode _current;

            public ResolutionGraphPathEnumerator(IEnumerable<ResolutionGraphNode> rootNode)
            {
                _queue = new Queue<ResolutionGraphNode>(rootNode);
            }

            public bool MoveNext()
            {
                while (_queue.Count > 0)
                {
                    _current = _queue.Dequeue();

                    foreach (ResolutionGraphNode child in _current)
                        _queue.Enqueue(child);

                    if (_current.Count == 0)
                        return true;
                }

                return false;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public string Current => _current.Path;

            object IEnumerator.Current => _current.Path;

            public void Dispose()
            {
            }
        }
    }

    public class ResolutionGraphNode : List<ResolutionGraphNode>
    {
        internal ResolutionGraphNode()
        {
        }

        private ResolutionGraphNode(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public string Name { get; }

        public string Path { get; }

        public void Add([NotNull] string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            ResolutionGraphNode current = this;

            foreach (string name in path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
                current = current[name];
        }

        public void AddRange(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            foreach (string path in paths)
                Add(path);
        }

        public bool Remove(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            int index = FindIndex(x => x.Name == name);

            if (index < 0)
                return false;

            RemoveAt(index);

            return true;
        }

        public ResolutionGraphNode Find(string name)
        {
            return Find(x => x.Name == name);
        }

        public ResolutionGraphNode this[string name]
        {
            get
            {
                ResolutionGraphNode result = Find(name);

                if (result != null)
                    return result;

                result = new ResolutionGraphNode(name, Path != null ? Path + '.' + name : name);

                Add(result);

                return result;
            }
        }
    }
}
