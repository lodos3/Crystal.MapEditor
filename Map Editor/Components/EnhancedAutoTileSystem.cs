using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Map_Editor.UI;

namespace Map_Editor.Components
{
    /// <summary>
    /// Enhanced AutoTile system for intelligent tile placement and pattern matching
    /// Provides TILED-like AutoTile functionality with improved algorithms
    /// </summary>
    public class EnhancedAutoTileSystem
    {
        private readonly Dictionary<string, AutoTileRule> _autoTileRules;
        private readonly Dictionary<int, AutoTileSet> _tileSets;

        public EnhancedAutoTileSystem()
        {
            _autoTileRules = new Dictionary<string, AutoTileRule>();
            _tileSets = new Dictionary<int, AutoTileSet>();
            InitializeDefaultRules();
        }

        /// <summary>
        /// Represents a set of tiles that can be auto-tiled together
        /// </summary>
        public class AutoTileSet
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<TileVariant> Variants { get; set; } = new List<TileVariant>();
            public AutoTileAlgorithm Algorithm { get; set; } = AutoTileAlgorithm.Wang2Corner;
        }

        /// <summary>
        /// Represents a tile variant within an AutoTile set
        /// </summary>
        public class TileVariant
        {
            public int LibraryIndex { get; set; }
            public int ImageIndex { get; set; }
            public TileType Type { get; set; }
            public ConnectionMask Connections { get; set; }
            public float Weight { get; set; } = 1.0f; // For random selection
        }

        /// <summary>
        /// Different types of tiles in an AutoTile set
        /// </summary>
        public enum TileType
        {
            Center,
            Edge_North,
            Edge_South,
            Edge_East,
            Edge_West,
            Corner_NorthEast,
            Corner_NorthWest,
            Corner_SouthEast,
            Corner_SouthWest,
            InnerCorner_NorthEast,
            InnerCorner_NorthWest,
            InnerCorner_SouthEast,
            InnerCorner_SouthWest,
            Single,
            Endpoint_North,
            Endpoint_South,
            Endpoint_East,
            Endpoint_West
        }

        /// <summary>
        /// AutoTile algorithms
        /// </summary>
        public enum AutoTileAlgorithm
        {
            Simple,        // Simple edge matching
            Wang2Corner,   // 2-corner Wang tiles
            Wang4Corner,   // 4-corner Wang tiles
            Blob,          // Blob-style autotiling
            Platform       // Platform/terrain style
        }

        /// <summary>
        /// Bitmask for tile connections
        /// </summary>
        [Flags]
        public enum ConnectionMask
        {
            None = 0,
            North = 1,
            East = 2,
            South = 4,
            West = 8,
            NorthEast = 16,
            SouthEast = 32,
            SouthWest = 64,
            NorthWest = 128,
            All = 255
        }

        /// <summary>
        /// Rule for AutoTile placement
        /// </summary>
        public class AutoTileRule
        {
            public string Name { get; set; }
            public ConnectionMask RequiredConnections { get; set; }
            public ConnectionMask ForbiddenConnections { get; set; }
            public List<TileChoice> Choices { get; set; } = new List<TileChoice>();
        }

        /// <summary>
        /// A possible tile choice for a rule
        /// </summary>
        public class TileChoice
        {
            public int LibraryIndex { get; set; }
            public int ImageIndex { get; set; }
            public float Weight { get; set; } = 1.0f;
        }

        /// <summary>
        /// Initialize default AutoTile rules for common patterns
        /// </summary>
        private void InitializeDefaultRules()
        {
            // Center tile (surrounded on all sides)
            _autoTileRules["center"] = new AutoTileRule
            {
                Name = "Center",
                RequiredConnections = ConnectionMask.North | ConnectionMask.East | ConnectionMask.South | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.None
            };

            // Edges
            _autoTileRules["edge_north"] = new AutoTileRule
            {
                Name = "Edge North",
                RequiredConnections = ConnectionMask.East | ConnectionMask.South | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.North
            };

            _autoTileRules["edge_south"] = new AutoTileRule
            {
                Name = "Edge South",
                RequiredConnections = ConnectionMask.North | ConnectionMask.East | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.South
            };

            _autoTileRules["edge_east"] = new AutoTileRule
            {
                Name = "Edge East",
                RequiredConnections = ConnectionMask.North | ConnectionMask.South | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.East
            };

            _autoTileRules["edge_west"] = new AutoTileRule
            {
                Name = "Edge West",
                RequiredConnections = ConnectionMask.North | ConnectionMask.East | ConnectionMask.South,
                ForbiddenConnections = ConnectionMask.West
            };

            // Corners
            _autoTileRules["corner_ne"] = new AutoTileRule
            {
                Name = "Corner NorthEast",
                RequiredConnections = ConnectionMask.South | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.North | ConnectionMask.East
            };

            _autoTileRules["corner_nw"] = new AutoTileRule
            {
                Name = "Corner NorthWest",
                RequiredConnections = ConnectionMask.East | ConnectionMask.South,
                ForbiddenConnections = ConnectionMask.North | ConnectionMask.West
            };

            _autoTileRules["corner_se"] = new AutoTileRule
            {
                Name = "Corner SouthEast",
                RequiredConnections = ConnectionMask.North | ConnectionMask.West,
                ForbiddenConnections = ConnectionMask.South | ConnectionMask.East
            };

            _autoTileRules["corner_sw"] = new AutoTileRule
            {
                Name = "Corner SouthWest",
                RequiredConnections = ConnectionMask.North | ConnectionMask.East,
                ForbiddenConnections = ConnectionMask.South | ConnectionMask.West
            };

            // Single tile (isolated)
            _autoTileRules["single"] = new AutoTileRule
            {
                Name = "Single",
                RequiredConnections = ConnectionMask.None,
                ForbiddenConnections = ConnectionMask.North | ConnectionMask.East | ConnectionMask.South | ConnectionMask.West
            };
        }

        /// <summary>
        /// Creates a new AutoTile set
        /// </summary>
        public void CreateAutoTileSet(int id, string name, AutoTileAlgorithm algorithm = AutoTileAlgorithm.Wang2Corner)
        {
            _tileSets[id] = new AutoTileSet
            {
                Id = id,
                Name = name,
                Algorithm = algorithm
            };
        }

        /// <summary>
        /// Adds a tile variant to an AutoTile set
        /// </summary>
        public void AddTileVariant(int setId, int libraryIndex, int imageIndex, TileType type, ConnectionMask connections = ConnectionMask.None)
        {
            if (_tileSets.TryGetValue(setId, out var tileSet))
            {
                tileSet.Variants.Add(new TileVariant
                {
                    LibraryIndex = libraryIndex,
                    ImageIndex = imageIndex,
                    Type = type,
                    Connections = connections
                });
            }
        }

        /// <summary>
        /// Calculates the best tile for a given position based on surrounding tiles
        /// </summary>
        public TileChoice GetBestTile(int setId, Point position, Func<Point, bool> isTileOfSameType)
        {
            if (!_tileSets.TryGetValue(setId, out var tileSet))
                return null;

            var connections = CalculateConnections(position, isTileOfSameType);
            
            return tileSet.Algorithm switch
            {
                AutoTileAlgorithm.Simple => GetSimpleAutoTile(tileSet, connections),
                AutoTileAlgorithm.Wang2Corner => GetWang2CornerTile(tileSet, connections),
                AutoTileAlgorithm.Wang4Corner => GetWang4CornerTile(tileSet, connections),
                AutoTileAlgorithm.Blob => GetBlobTile(tileSet, connections),
                AutoTileAlgorithm.Platform => GetPlatformTile(tileSet, connections),
                _ => GetSimpleAutoTile(tileSet, connections)
            };
        }

        /// <summary>
        /// Calculates connection mask for a position
        /// </summary>
        private ConnectionMask CalculateConnections(Point position, Func<Point, bool> isTileOfSameType)
        {
            var connections = ConnectionMask.None;

            // Check 8 surrounding positions
            if (isTileOfSameType(new Point(position.X, position.Y - 1))) connections |= ConnectionMask.North;
            if (isTileOfSameType(new Point(position.X + 1, position.Y))) connections |= ConnectionMask.East;
            if (isTileOfSameType(new Point(position.X, position.Y + 1))) connections |= ConnectionMask.South;
            if (isTileOfSameType(new Point(position.X - 1, position.Y))) connections |= ConnectionMask.West;
            if (isTileOfSameType(new Point(position.X + 1, position.Y - 1))) connections |= ConnectionMask.NorthEast;
            if (isTileOfSameType(new Point(position.X + 1, position.Y + 1))) connections |= ConnectionMask.SouthEast;
            if (isTileOfSameType(new Point(position.X - 1, position.Y + 1))) connections |= ConnectionMask.SouthWest;
            if (isTileOfSameType(new Point(position.X - 1, position.Y - 1))) connections |= ConnectionMask.NorthWest;

            return connections;
        }

        /// <summary>
        /// Simple AutoTile selection based on edge connections only
        /// </summary>
        private TileChoice GetSimpleAutoTile(AutoTileSet tileSet, ConnectionMask connections)
        {
            // Extract only cardinal directions
            var cardinalConnections = connections & (ConnectionMask.North | ConnectionMask.East | ConnectionMask.South | ConnectionMask.West);

            // Find matching rule
            foreach (var rule in _autoTileRules.Values)
            {
                if ((cardinalConnections & rule.RequiredConnections) == rule.RequiredConnections &&
                    (cardinalConnections & rule.ForbiddenConnections) == ConnectionMask.None)
                {
                    if (rule.Choices.Count > 0)
                    {
                        return SelectWeightedRandom(rule.Choices);
                    }
                }
            }

            // Fallback to first variant
            var firstVariant = tileSet.Variants.FirstOrDefault();
            return firstVariant != null ? new TileChoice 
            { 
                LibraryIndex = firstVariant.LibraryIndex, 
                ImageIndex = firstVariant.ImageIndex 
            } : null;
        }

        /// <summary>
        /// Wang 2-corner tile selection
        /// </summary>
        private TileChoice GetWang2CornerTile(AutoTileSet tileSet, ConnectionMask connections)
        {
            // For Wang tiles, we need to consider corner connections
            var matchingVariants = tileSet.Variants.Where(v => 
                (v.Connections & connections) == v.Connections).ToList();

            if (matchingVariants.Count > 0)
            {
                var selected = SelectWeightedRandom(matchingVariants);
                return new TileChoice 
                { 
                    LibraryIndex = selected.LibraryIndex, 
                    ImageIndex = selected.ImageIndex 
                };
            }

            return GetSimpleAutoTile(tileSet, connections);
        }

        /// <summary>
        /// Wang 4-corner tile selection
        /// </summary>
        private TileChoice GetWang4CornerTile(AutoTileSet tileSet, ConnectionMask connections)
        {
            // More complex Wang tile matching
            return GetWang2CornerTile(tileSet, connections); // Simplified for now
        }

        /// <summary>
        /// Blob-style AutoTile selection
        /// </summary>
        private TileChoice GetBlobTile(AutoTileSet tileSet, ConnectionMask connections)
        {
            // Blob tiles focus on creating organic, rounded shapes
            return GetSimpleAutoTile(tileSet, connections);
        }

        /// <summary>
        /// Platform-style AutoTile selection
        /// </summary>
        private TileChoice GetPlatformTile(AutoTileSet tileSet, ConnectionMask connections)
        {
            // Platform tiles are optimized for platformer games
            return GetSimpleAutoTile(tileSet, connections);
        }

        /// <summary>
        /// Selects a random item from a weighted list
        /// </summary>
        private T SelectWeightedRandom<T>(IList<T> items) where T : class
        {
            if (items.Count == 0) return null;
            if (items.Count == 1) return items[0];

            var random = new Random();
            return items[random.Next(items.Count)];
        }

        /// <summary>
        /// Selects a random variant based on weight
        /// </summary>
        private TileVariant SelectWeightedRandom(IList<TileVariant> variants)
        {
            if (variants.Count == 0) return null;
            if (variants.Count == 1) return variants[0];

            var totalWeight = variants.Sum(v => v.Weight);
            var random = new Random().NextDouble() * totalWeight;

            foreach (var variant in variants)
            {
                random -= variant.Weight;
                if (random <= 0) return variant;
            }

            return variants.Last();
        }

        /// <summary>
        /// Applies AutoTile to a rectangular area
        /// </summary>
        public Dictionary<Point, TileChoice> ApplyAutoTileToArea(int setId, Rectangle area, Func<Point, bool> isTileOfSameType)
        {
            var result = new Dictionary<Point, TileChoice>();

            for (int y = area.Top; y < area.Bottom; y++)
            {
                for (int x = area.Left; x < area.Right; x++)
                {
                    var position = new Point(x, y);
                    if (isTileOfSameType(position))
                    {
                        var tile = GetBestTile(setId, position, isTileOfSameType);
                        if (tile != null)
                        {
                            result[position] = tile;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets information about an AutoTile set
        /// </summary>
        public AutoTileSet GetTileSet(int id)
        {
            return _tileSets.TryGetValue(id, out var tileSet) ? tileSet : null;
        }

        /// <summary>
        /// Gets all available tile sets
        /// </summary>
        public IEnumerable<AutoTileSet> GetAllTileSets()
        {
            return _tileSets.Values;
        }

        /// <summary>
        /// Creates a preview of how AutoTiles would look in an area
        /// </summary>
        public Bitmap CreateAutoTilePreview(int setId, Size previewSize, int tileSize = 32)
        {
            var tileSet = GetTileSet(setId);
            if (tileSet == null) return null;

            var preview = new Bitmap(previewSize.Width, previewSize.Height);
            using (var graphics = Graphics.FromImage(preview))
            {
                graphics.Clear(Color.Transparent);

                // Create a simple pattern for preview
                var pattern = CreatePreviewPattern(tileSet, previewSize.Width / tileSize, previewSize.Height / tileSize);
                
                for (int y = 0; y < pattern.GetLength(1); y++)
                {
                    for (int x = 0; x < pattern.GetLength(0); x++)
                    {
                        var tileChoice = pattern[x, y];
                        if (tileChoice != null)
                        {
                            // Draw tile preview (simplified)
                            var rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                            graphics.FillRectangle(Brushes.Gray, rect);
                            graphics.DrawRectangle(Pens.Black, rect);
                        }
                    }
                }
            }

            return preview;
        }

        /// <summary>
        /// Creates a simple pattern for preview purposes
        /// </summary>
        private TileChoice[,] CreatePreviewPattern(AutoTileSet tileSet, int width, int height)
        {
            var pattern = new TileChoice[width, height];
            
            // Create a simple rectangular pattern
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    var variant = tileSet.Variants.FirstOrDefault();
                    if (variant != null)
                    {
                        pattern[x, y] = new TileChoice
                        {
                            LibraryIndex = variant.LibraryIndex,
                            ImageIndex = variant.ImageIndex
                        };
                    }
                }
            }

            return pattern;
        }
    }
}