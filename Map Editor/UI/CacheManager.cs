using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Map_Editor.UI
{
    /// <summary>
    /// Caching system to optimize map tile and object rendering performance
    /// Implements LRU (Least Recently Used) cache eviction strategy
    /// </summary>
    public class CacheManager
    {
        private readonly Dictionary<string, CacheItem> _cache;
        private readonly LinkedList<string> _lruList;
        private readonly int _maxCacheSize;
        private readonly object _lockObject = new object();

        public CacheManager(int maxCacheSize = 1000)
        {
            _maxCacheSize = maxCacheSize;
            _cache = new Dictionary<string, CacheItem>();
            _lruList = new LinkedList<string>();
        }

        /// <summary>
        /// Represents a cached item with its data and access information
        /// </summary>
        private class CacheItem
        {
            public object Data { get; set; }
            public DateTime LastAccessed { get; set; }
            public LinkedListNode<string> LruNode { get; set; }
            public long MemorySize { get; set; }
        }

        /// <summary>
        /// Adds or updates an item in the cache
        /// </summary>
        public void Set<T>(string key, T data) where T : class
        {
            if (string.IsNullOrEmpty(key) || data == null) return;

            lock (_lockObject)
            {
                var memorySize = EstimateMemorySize(data);

                if (_cache.ContainsKey(key))
                {
                    // Update existing item
                    var existingItem = _cache[key];
                    existingItem.Data = data;
                    existingItem.LastAccessed = DateTime.UtcNow;
                    existingItem.MemorySize = memorySize;

                    // Move to front of LRU list
                    _lruList.Remove(existingItem.LruNode);
                    existingItem.LruNode = _lruList.AddFirst(key);
                }
                else
                {
                    // Add new item
                    var newItem = new CacheItem
                    {
                        Data = data,
                        LastAccessed = DateTime.UtcNow,
                        MemorySize = memorySize
                    };

                    newItem.LruNode = _lruList.AddFirst(key);
                    _cache[key] = newItem;

                    // Check if we need to evict items
                    EvictIfNecessary();
                }
            }
        }

        /// <summary>
        /// Retrieves an item from the cache
        /// </summary>
        public T Get<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) return null;

            lock (_lockObject)
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    // Update access time and move to front
                    item.LastAccessed = DateTime.UtcNow;
                    _lruList.Remove(item.LruNode);
                    item.LruNode = _lruList.AddFirst(key);

                    return item.Data as T;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a key exists in the cache
        /// </summary>
        public bool Contains(string key)
        {
            lock (_lockObject)
            {
                return _cache.ContainsKey(key);
            }
        }

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            lock (_lockObject)
            {
                if (_cache.TryGetValue(key, out var item))
                {
                    _lruList.Remove(item.LruNode);
                    _cache.Remove(key);

                    // Dispose if it's disposable
                    if (item.Data is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Clears all items from the cache
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                // Dispose all disposable items
                foreach (var item in _cache.Values)
                {
                    if (item.Data is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _cache.Clear();
                _lruList.Clear();
            }
        }

        /// <summary>
        /// Gets cache statistics
        /// </summary>
        public CacheStats GetStats()
        {
            lock (_lockObject)
            {
                var totalMemory = _cache.Values.Sum(item => item.MemorySize);
                return new CacheStats
                {
                    ItemCount = _cache.Count,
                    TotalMemoryBytes = totalMemory,
                    HitRate = CalculateHitRate()
                };
            }
        }

        private long _totalRequests = 0;
        private long _cacheHits = 0;

        /// <summary>
        /// Tracks cache performance
        /// </summary>
        private double CalculateHitRate()
        {
            return _totalRequests > 0 ? (double)_cacheHits / _totalRequests : 0.0;
        }

        /// <summary>
        /// Evicts least recently used items if cache is too large
        /// </summary>
        private void EvictIfNecessary()
        {
            while (_cache.Count > _maxCacheSize)
            {
                var lruKey = _lruList.Last.Value;
                Remove(lruKey);
            }
        }

        /// <summary>
        /// Estimates memory usage of an object
        /// </summary>
        private long EstimateMemorySize(object obj)
        {
            switch (obj)
            {
                case Bitmap bitmap:
                    return bitmap.Width * bitmap.Height * 4; // Assume 32-bit RGBA

                case byte[] array:
                    return array.Length;

                case string str:
                    return str.Length * 2; // Unicode strings

                case MLibrary.MImage image:
                    return image.Width * image.Height * 4;

                default:
                    return 1024; // Default estimate for unknown types
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// Cache performance statistics
    /// </summary>
    public class CacheStats
    {
        public int ItemCount { get; set; }
        public long TotalMemoryBytes { get; set; }
        public double HitRate { get; set; }

        public string TotalMemoryFormatted => FormatBytes(TotalMemoryBytes);

        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Specialized cache for map-related data with predefined cache keys
    /// </summary>
    public static class MapCache
    {
        private static readonly CacheManager _cache = new CacheManager(2000);

        // Cache key generators
        public static string GetTileKey(int libIndex, int imageIndex) => $"tile_{libIndex}_{imageIndex}";
        public static string GetObjectKey(int libIndex, int imageIndex) => $"object_{libIndex}_{imageIndex}";
        public static string GetPreviewKey(int libIndex, int index, string type) => $"preview_{type}_{libIndex}_{index}";

        // Tile caching
        public static void CacheTile(int libIndex, int imageIndex, Bitmap tile)
        {
            _cache.Set(GetTileKey(libIndex, imageIndex), tile);
        }

        public static Bitmap GetCachedTile(int libIndex, int imageIndex)
        {
            return _cache.Get<Bitmap>(GetTileKey(libIndex, imageIndex));
        }

        // Object caching
        public static void CacheObject(int libIndex, int imageIndex, MLibrary.MImage image)
        {
            _cache.Set(GetObjectKey(libIndex, imageIndex), image);
        }

        public static MLibrary.MImage GetCachedObject(int libIndex, int imageIndex)
        {
            return _cache.Get<MLibrary.MImage>(GetObjectKey(libIndex, imageIndex));
        }

        // Preview caching
        public static void CachePreview(int libIndex, int index, string type, Bitmap preview)
        {
            _cache.Set(GetPreviewKey(libIndex, index, type), preview);
        }

        public static Bitmap GetCachedPreview(int libIndex, int index, string type)
        {
            return _cache.Get<Bitmap>(GetPreviewKey(libIndex, index, type));
        }

        // Cache management
        public static CacheStats GetCacheStats() => _cache.GetStats();
        public static void ClearCache() => _cache.Clear();

        public static void Dispose()
        {
            _cache?.Dispose();
        }
    }
}