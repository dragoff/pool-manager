using System;
using System.Collections.Generic;

namespace ObjectPool
{
    public class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        internal readonly List<T> m_List;
        internal bool m_CollectionCheck;
        private readonly Func<T> m_CreateFunc;
        private readonly int m_MaxSize;

        public int CountAll { get; private set; }

        public int CountActive => this.CountAll - this.CountInactive;

        public int CountInactive => this.m_List.Count;

        public ObjectPool(Func<T> createFunc, bool collectionCheck = true, int defaultCapacity = 10)
        {
            this.m_CollectionCheck = collectionCheck;
            this.m_CreateFunc = createFunc;
            this.m_List = new List<T>(defaultCapacity);

            for (int index = 0; index < defaultCapacity; ++index)
                this.m_List.Add(m_CreateFunc());
        }
        
        public ObjectPool(IEnumerable<T> items, Func<T> createFunc, bool collectionCheck = true, int defaultCapacity = 10)
        {
            this.m_CollectionCheck = collectionCheck;
            this.m_CreateFunc = createFunc;
            this.m_List = new List<T>(defaultCapacity);
            this.m_List.AddRange(items);
        }

        public T Get()
        {
            T obj;
            if (this.m_List.Count == 0)
            {
                obj = this.m_CreateFunc();
                ++this.CountAll;
            }
            else
            {
                int index = this.m_List.Count - 1;
                obj = this.m_List[index];
                this.m_List.RemoveAt(index);
            }
            return obj;
        }

        public PooledObject<T> Get(out T v) => new PooledObject<T>(v = this.Get(), this);

        public void Release(T element)
        {
            if (this.m_CollectionCheck && this.m_List.Count > 0)
            {
                for (int index = 0; index < this.m_List.Count; ++index)
                {
                    if ((object)element == (object)this.m_List[index])
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }
            if (this.CountInactive < this.m_MaxSize)
                this.m_List.Add(element);
        }

        public void Clear()
        {
            this.m_List.Clear();
            this.CountAll = 0;
        }

        public void Dispose() => this.Clear();
    }
}
