using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Map_Editor.UI;

namespace Map_Editor.Components
{
    /// <summary>
    /// TILED-inspired layer management system for Crystal Map Editor
    /// Provides advanced layer controls, visibility management, and editing modes
    /// </summary>
    public class LayerManager
    {
        private readonly Dictionary<int, MapLayer> _layers;
        private int _nextLayerId = 1;
        private int _activeLayerId = -1;
        
        public event EventHandler<LayerEventArgs> LayerAdded;
        public event EventHandler<LayerEventArgs> LayerRemoved;
        public event EventHandler<LayerEventArgs> LayerChanged;
        public event EventHandler<LayerEventArgs> ActiveLayerChanged;

        public LayerManager()
        {
            _layers = new Dictionary<int, MapLayer>();
            InitializeDefaultLayers();
        }

        /// <summary>
        /// Represents a map layer with all its properties
        /// </summary>
        public class MapLayer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public LayerType Type { get; set; }
            public bool Visible { get; set; } = true;
            public bool Locked { get; set; } = false;
            public float Opacity { get; set; } = 1.0f;
            public BlendMode BlendMode { get; set; } = BlendMode.Normal;
            public Color TintColor { get; set; } = Color.White;
            public Point Offset { get; set; } = Point.Empty;
            public Dictionary<Point, LayerTile> Tiles { get; set; } = new Dictionary<Point, LayerTile>();
            public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
            public int ZOrder { get; set; }
        }

        /// <summary>
        /// Types of layers available
        /// </summary>
        public enum LayerType
        {
            Background,     // Background tiles
            Midground,      // Middle layer tiles  
            Foreground,     // Foreground tiles
            Object,         // Object layer
            Collision,      // Collision/physics layer
            Trigger,        // Trigger areas
            Light,          // Lighting information
            Animation,      // Animation markers
            Group           // Layer group
        }

        /// <summary>
        /// Blend modes for layer rendering
        /// </summary>
        public enum BlendMode
        {
            Normal,
            Multiply,
            Screen,
            Overlay,
            SoftLight,
            HardLight,
            ColorDodge,
            ColorBurn,
            Darken,
            Lighten,
            Difference,
            Exclusion
        }

        /// <summary>
        /// Represents a tile within a layer
        /// </summary>
        public class LayerTile
        {
            public int LibraryIndex { get; set; }
            public int ImageIndex { get; set; }
            public bool FlipHorizontal { get; set; }
            public bool FlipVertical { get; set; }
            public bool FlipDiagonal { get; set; }
            public float Rotation { get; set; }
            public Color Tint { get; set; } = Color.White;
            public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initialize default layers similar to the original application
        /// </summary>
        private void InitializeDefaultLayers()
        {
            AddLayer("Background", LayerType.Background);
            AddLayer("Midground", LayerType.Midground);
            AddLayer("Foreground", LayerType.Foreground);
            AddLayer("Objects", LayerType.Object);
            
            // Set background as active initially
            var backgroundLayer = _layers.Values.FirstOrDefault(l => l.Type == LayerType.Background);
            if (backgroundLayer != null)
            {
                _activeLayerId = backgroundLayer.Id;
            }
        }

        /// <summary>
        /// Adds a new layer
        /// </summary>
        public int AddLayer(string name, LayerType type, int? insertIndex = null)
        {
            var layer = new MapLayer
            {
                Id = _nextLayerId++,
                Name = name,
                Type = type,
                ZOrder = insertIndex ?? _layers.Count
            };

            // Adjust Z-orders if inserting
            if (insertIndex.HasValue)
            {
                foreach (var existingLayer in _layers.Values.Where(l => l.ZOrder >= insertIndex.Value))
                {
                    existingLayer.ZOrder++;
                }
            }

            _layers[layer.Id] = layer;
            LayerAdded?.Invoke(this, new LayerEventArgs(layer));
            
            return layer.Id;
        }

        /// <summary>
        /// Removes a layer
        /// </summary>
        public bool RemoveLayer(int layerId)
        {
            if (_layers.TryGetValue(layerId, out var layer))
            {
                _layers.Remove(layerId);
                
                // Adjust Z-orders
                foreach (var remainingLayer in _layers.Values.Where(l => l.ZOrder > layer.ZOrder))
                {
                    remainingLayer.ZOrder--;
                }
                
                // If this was the active layer, select another
                if (_activeLayerId == layerId)
                {
                    var nextLayer = _layers.Values.OrderBy(l => l.ZOrder).FirstOrDefault();
                    _activeLayerId = nextLayer?.Id ?? -1;
                    
                    if (nextLayer != null)
                    {
                        ActiveLayerChanged?.Invoke(this, new LayerEventArgs(nextLayer));
                    }
                }
                
                LayerRemoved?.Invoke(this, new LayerEventArgs(layer));
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Gets a layer by ID
        /// </summary>
        public MapLayer GetLayer(int layerId)
        {
            return _layers.TryGetValue(layerId, out var layer) ? layer : null;
        }

        /// <summary>
        /// Gets all layers ordered by Z-order
        /// </summary>
        public IEnumerable<MapLayer> GetAllLayers()
        {
            return _layers.Values.OrderBy(l => l.ZOrder);
        }

        /// <summary>
        /// Gets layers of a specific type
        /// </summary>
        public IEnumerable<MapLayer> GetLayersByType(LayerType type)
        {
            return _layers.Values.Where(l => l.Type == type).OrderBy(l => l.ZOrder);
        }

        /// <summary>
        /// Sets the active layer
        /// </summary>
        public void SetActiveLayer(int layerId)
        {
            if (_layers.ContainsKey(layerId))
            {
                var oldActiveId = _activeLayerId;
                _activeLayerId = layerId;
                
                var activeLayer = _layers[layerId];
                ActiveLayerChanged?.Invoke(this, new LayerEventArgs(activeLayer));
            }
        }

        /// <summary>
        /// Gets the currently active layer
        /// </summary>
        public MapLayer GetActiveLayer()
        {
            return _activeLayerId != -1 ? GetLayer(_activeLayerId) : null;
        }

        /// <summary>
        /// Moves a layer up in the Z-order
        /// </summary>
        public bool MoveLayerUp(int layerId)
        {
            var layer = GetLayer(layerId);
            if (layer == null) return false;
            
            var layerAbove = _layers.Values.Where(l => l.ZOrder == layer.ZOrder + 1).FirstOrDefault();
            if (layerAbove != null)
            {
                // Swap Z-orders
                layerAbove.ZOrder--;
                layer.ZOrder++;
                
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
                LayerChanged?.Invoke(this, new LayerEventArgs(layerAbove));
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Moves a layer down in the Z-order
        /// </summary>
        public bool MoveLayerDown(int layerId)
        {
            var layer = GetLayer(layerId);
            if (layer == null || layer.ZOrder == 0) return false;
            
            var layerBelow = _layers.Values.Where(l => l.ZOrder == layer.ZOrder - 1).FirstOrDefault();
            if (layerBelow != null)
            {
                // Swap Z-orders
                layerBelow.ZOrder++;
                layer.ZOrder--;
                
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
                LayerChanged?.Invoke(this, new LayerEventArgs(layerBelow));
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Toggles layer visibility
        /// </summary>
        public void ToggleLayerVisibility(int layerId)
        {
            var layer = GetLayer(layerId);
            if (layer != null)
            {
                layer.Visible = !layer.Visible;
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// Toggles layer lock state
        /// </summary>
        public void ToggleLayerLock(int layerId)
        {
            var layer = GetLayer(layerId);
            if (layer != null)
            {
                layer.Locked = !layer.Locked;
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// Sets layer opacity
        /// </summary>
        public void SetLayerOpacity(int layerId, float opacity)
        {
            var layer = GetLayer(layerId);
            if (layer != null)
            {
                layer.Opacity = Math.Max(0, Math.Min(1, opacity));
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// Renames a layer
        /// </summary>
        public void RenameLayer(int layerId, string newName)
        {
            var layer = GetLayer(layerId);
            if (layer != null && !string.IsNullOrWhiteSpace(newName))
            {
                layer.Name = newName.Trim();
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// Duplicates a layer
        /// </summary>
        public int DuplicateLayer(int layerId)
        {
            var sourceLayer = GetLayer(layerId);
            if (sourceLayer == null) return -1;
            
            var newLayerId = AddLayer($"{sourceLayer.Name} Copy", sourceLayer.Type);
            var newLayer = GetLayer(newLayerId);
            
            // Copy properties
            newLayer.Visible = sourceLayer.Visible;
            newLayer.Locked = sourceLayer.Locked;
            newLayer.Opacity = sourceLayer.Opacity;
            newLayer.BlendMode = sourceLayer.BlendMode;
            newLayer.TintColor = sourceLayer.TintColor;
            newLayer.Offset = sourceLayer.Offset;
            
            // Copy tiles
            foreach (var kvp in sourceLayer.Tiles)
            {
                newLayer.Tiles[kvp.Key] = new LayerTile
                {
                    LibraryIndex = kvp.Value.LibraryIndex,
                    ImageIndex = kvp.Value.ImageIndex,
                    FlipHorizontal = kvp.Value.FlipHorizontal,
                    FlipVertical = kvp.Value.FlipVertical,
                    FlipDiagonal = kvp.Value.FlipDiagonal,
                    Rotation = kvp.Value.Rotation,
                    Tint = kvp.Value.Tint,
                    Properties = new Dictionary<string, object>(kvp.Value.Properties)
                };
            }
            
            // Copy properties
            newLayer.Properties = new Dictionary<string, object>(sourceLayer.Properties);
            
            return newLayerId;
        }

        /// <summary>
        /// Places a tile on a layer
        /// </summary>
        public void PlaceTile(int layerId, Point position, int libraryIndex, int imageIndex)
        {
            var layer = GetLayer(layerId);
            if (layer == null || layer.Locked) return;
            
            layer.Tiles[position] = new LayerTile
            {
                LibraryIndex = libraryIndex,
                ImageIndex = imageIndex
            };
        }

        /// <summary>
        /// Removes a tile from a layer
        /// </summary>
        public void RemoveTile(int layerId, Point position)
        {
            var layer = GetLayer(layerId);
            if (layer == null || layer.Locked) return;
            
            layer.Tiles.Remove(position);
        }

        /// <summary>
        /// Gets a tile at a specific position from a layer
        /// </summary>
        public LayerTile GetTile(int layerId, Point position)
        {
            var layer = GetLayer(layerId);
            return layer?.Tiles.TryGetValue(position, out var tile) == true ? tile : null;
        }

        /// <summary>
        /// Gets all tiles at a position across all visible layers
        /// </summary>
        public IEnumerable<(MapLayer layer, LayerTile tile)> GetTilesAtPosition(Point position)
        {
            return GetAllLayers()
                .Where(l => l.Visible && l.Tiles.ContainsKey(position))
                .Select(l => (l, l.Tiles[position]));
        }

        /// <summary>
        /// Clears all tiles from a layer
        /// </summary>
        public void ClearLayer(int layerId)
        {
            var layer = GetLayer(layerId);
            if (layer != null && !layer.Locked)
            {
                layer.Tiles.Clear();
                LayerChanged?.Invoke(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// Merges multiple layers into one
        /// </summary>
        public int MergeLayers(IEnumerable<int> layerIds, string mergedLayerName)
        {
            var layers = layerIds.Select(GetLayer).Where(l => l != null).ToList();
            if (layers.Count == 0) return -1;
            
            // Create new merged layer
            var mergedLayerId = AddLayer(mergedLayerName, layers.First().Type);
            var mergedLayer = GetLayer(mergedLayerId);
            
            // Merge tiles from all layers (later layers override earlier ones)
            foreach (var layer in layers.OrderBy(l => l.ZOrder))
            {
                foreach (var kvp in layer.Tiles)
                {
                    mergedLayer.Tiles[kvp.Key] = new LayerTile
                    {
                        LibraryIndex = kvp.Value.LibraryIndex,
                        ImageIndex = kvp.Value.ImageIndex,
                        FlipHorizontal = kvp.Value.FlipHorizontal,
                        FlipVertical = kvp.Value.FlipVertical,
                        FlipDiagonal = kvp.Value.FlipDiagonal,
                        Rotation = kvp.Value.Rotation,
                        Tint = kvp.Value.Tint,
                        Properties = new Dictionary<string, object>(kvp.Value.Properties)
                    };
                }
            }
            
            return mergedLayerId;
        }

        /// <summary>
        /// Exports layer data for saving
        /// </summary>
        public LayerData ExportLayerData()
        {
            return new LayerData
            {
                Layers = _layers.Values.ToList(),
                ActiveLayerId = _activeLayerId,
                NextLayerId = _nextLayerId
            };
        }

        /// <summary>
        /// Imports layer data from save
        /// </summary>
        public void ImportLayerData(LayerData data)
        {
            _layers.Clear();
            
            foreach (var layer in data.Layers)
            {
                _layers[layer.Id] = layer;
            }
            
            _activeLayerId = data.ActiveLayerId;
            _nextLayerId = data.NextLayerId;
            
            // Notify about active layer
            var activeLayer = GetActiveLayer();
            if (activeLayer != null)
            {
                ActiveLayerChanged?.Invoke(this, new LayerEventArgs(activeLayer));
            }
        }
    }

    /// <summary>
    /// Event arguments for layer events
    /// </summary>
    public class LayerEventArgs : EventArgs
    {
        public LayerManager.MapLayer Layer { get; }
        
        public LayerEventArgs(LayerManager.MapLayer layer)
        {
            Layer = layer;
        }
    }

    /// <summary>
    /// Data structure for saving/loading layer information
    /// </summary>
    public class LayerData
    {
        public List<LayerManager.MapLayer> Layers { get; set; } = new List<LayerManager.MapLayer>();
        public int ActiveLayerId { get; set; }
        public int NextLayerId { get; set; }
    }
}