using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WP7Helpers.Common
{
    public interface ISet<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        bool Add(T item);
        void ExceptWith(IEnumerable<T> other);
        void IntersectWith(IEnumerable<T> other);
        bool IsProperSubsetOf(IEnumerable<T> other);
        bool IsProperSupersetOf(IEnumerable<T> other);
        bool IsSubsetOf(IEnumerable<T> other);
        bool IsSupersetOf(IEnumerable<T> other);
        bool Overlaps(IEnumerable<T> other);
        bool SetEquals(IEnumerable<T> other);
        void SymmetricExceptWith(IEnumerable<T> other);
        void UnionWith(IEnumerable<T> other);
    }

    public class HashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private Dictionary<T, bool> _data;

        #region Implementation of public methods

        public HashSet()
        {
            _data = new Dictionary<T, bool>();
        }

        public HashSet(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection", "parameter must not be null");
            _data = new Dictionary<T, bool>();

            AddRange(collection);
        }

        public HashSet(IEqualityComparer<T> comparer)
        {
            _data = new Dictionary<T, bool>(comparer);
        }

        public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
        {
            _data = new Dictionary<T, bool>(comparer);

            AddRange(collection);
        }

        public IEqualityComparer<T> Comparer { get { return _data.Comparer; } }

        public int Count { get { return _data.Count; } }

        public bool Add(T item)
        {
            if (_data.ContainsKey(item))
                return false;

            _data.Add(item, true);
            return true;
        }

        public void Clear()
        {
            _data.Clear();
        }

        public bool Contains(T item)
        {
            return _data.ContainsKey(item);
        }

        public void CopyTo(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            AddRange(array);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex must not be greater than or equal to the array's length");

            AddRange(array.Skip(arrayIndex));
        }

        public void CopyTo(T[] array, int arrayIndex, int count)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "count must be greater than zero");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex must not be greater than or equal to the array's length");
            if (arrayIndex + count > array.Length)
                throw new ArgumentException("arrayIndex and count specify more items than possible");

            AddRange(array.Skip(arrayIndex).Take(count));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other)
                _data.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var result = _data.Keys.Where(other.Contains).ToList();
            _data = new Dictionary<T, bool>();
            AddRange(result);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var otherSet = new HashSet<T>(other);
            return (Count < otherSet.Count && _data.Keys.All(otherSet.Contains));
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (Count == 0)
                return false;

            var otherSet = new HashSet<T>(other);

            return (Count > otherSet.Count && otherSet.All(Contains));
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (Count == 0)
                return true;

            var otherSet = new HashSet<T>(other);
            return (Count <= otherSet.Count && _data.Keys.All(otherSet.Contains));
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return other.All(Contains);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return (other.Any(Contains));
        }

        public bool Remove(T item)
        {
            if (!_data.ContainsKey(item))
                return false;

            _data.Remove(item);
            return true;
        }

        public int RemoveWhere(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");

            var result = _data.Keys.Where(i => match(i)).ToList();
            var removed = (Count - result.Count);
            Clear();
            AddRange(result);

            return removed;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            var otherSet = new HashSet<T>();
            foreach (var item in other)
            {
                if (!Contains(item))
                    return false;

                otherSet.Add(item);
            }

            return (otherSet.Count == Count);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            foreach (var item in other.Where(Contains))
                Remove(item);
        }

        public void TrimExcess()
        {
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            AddRange(other);
        }

        #endregion

        private void AddRange(IEnumerable<T> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            foreach (var item in items)
                Add(item);
        }

        #region Remaining interface implementations

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.Keys.GetEnumerator();
        }

        #endregion
    }
}