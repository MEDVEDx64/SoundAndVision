using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Massacre.Snv.Core.Configuration
{
    public class ChestDictionary<K, V> : IDictionary<K, V>
    {
        private IDictionary<K, V> d = new Dictionary<K, V>();
        private string fn;

        public bool WriteAllowed { get; set; }

        public ChestDictionary(string fn)
        {
            WriteAllowed = true;
            this.fn = fn;
        }

        void DictionaryChanged()
        {
            if (!WriteAllowed) return;
            try
            {
                using (var f = new StreamWriter(new FileStream(fn, FileMode.Create)))
                {
                    foreach (var k in d.Keys)
                    {
                        if (k.ToString().Trim().Length == 0 && d[k].ToString().Trim().Length == 0)
                            continue;
                        f.WriteLine(k + " " + d[k]);
                    }
                }
            }

            catch { }
        }

        public V EnsureValue(K key, V fallbackValue, bool dryRun = false)
        {
            if(!ContainsKey(key))
            {
                if(dryRun)
                {
                    return fallbackValue;
                }

                this[key] = fallbackValue;
            }

            return this[key];
        }

        public V this[K key]
        {
            get { return d[key]; }
            set
            {
                if(value == null)
                {
                    if(ContainsKey(key))
                    {
                        Remove(key);
                        DictionaryChanged();
                    }

                    return;
                }

                d[key] = value;
                DictionaryChanged();
            }
        }

        public int Count
        {
            get { return d.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ICollection<K> Keys
        {
            get { return d.Keys; }
        }

        public ICollection<V> Values
        {
            get { return d.Values; }
        }

        public void Add(KeyValuePair<K, V> item)
        {
            d.Add(item.Key, item.Value);
            DictionaryChanged();
        }

        public void Add(K key, V value)
        {
            d.Add(key, value);
            DictionaryChanged();
        }

        public void Clear()
        {
            d.Clear();
            DictionaryChanged();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return d.Contains(item);
        }

        public bool ContainsKey(K key)
        {
            return d.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            d.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return d.GetEnumerator();
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (d.Remove(item))
            {
                DictionaryChanged();
                return true;
            }
            return false;
        }

        public bool Remove(K key)
        {
            if (d.Remove(key))
            {
                DictionaryChanged();
                return true;
            }
            return false;
        }

        public bool TryGetValue(K key, out V value)
        {
            return d.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return d.GetEnumerator();
        }
    }
}
