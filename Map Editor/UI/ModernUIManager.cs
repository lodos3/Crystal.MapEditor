using System;
using System.Drawing;
using System.Windows.Forms;
using Map_Editor;
using Map_Editor.UI;
using Map_Editor.Components;

namespace Map_Editor.UI
{
    /// <summary>
    /// Modern UI Manager that coordinates all the enhanced components
    /// Provides a unified interface for managing the modernized Crystal Map Editor
    /// </summary>
    public class ModernUIManager
    {
        private readonly Main _mainForm;
        private EnhancedStatusBar _statusBar;
        private LayerManager _layerManager;
        private EnhancedAutoTileSystem _autoTileSystem;
        private readonly DockPanel _dockPanel;

        // Library viewers for different versions
        private EnhancedLibraryViewer _wemadeMir2Viewer;
        private EnhancedLibraryViewer _shandaMir2Viewer;
        private EnhancedLibraryViewer _wemadeMir3Viewer;
        private EnhancedLibraryViewer _shandaMir3Viewer;

        public EnhancedStatusBar StatusBar => _statusBar;
        public LayerManager LayerManager => _layerManager;
        public EnhancedAutoTileSystem AutoTileSystem => _autoTileSystem;

        public ModernUIManager(Main mainForm)
        {
            _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));
            _dockPanel = new DockPanel();
            InitializeComponents();
            SetupEventHandlers();
        }

        private void InitializeComponents()
        {
            // Initialize enhanced status bar
            _statusBar = new EnhancedStatusBar();
            _mainForm.Controls.Add(_statusBar);

            // Initialize layer manager
            _layerManager = new LayerManager();

            // Initialize AutoTile system
            _autoTileSystem = new EnhancedAutoTileSystem();
            SetupDefaultAutoTileSets();

            // Initialize library viewers (they will replace the existing tab pages)
            InitializeLibraryViewers();
        }

        private void InitializeLibraryViewers()
        {
            _wemadeMir2Viewer = new EnhancedLibraryViewer();
            _shandaMir2Viewer = new EnhancedLibraryViewer();
            _wemadeMir3Viewer = new EnhancedLibraryViewer();
            _shandaMir3Viewer = new EnhancedLibraryViewer();

            // The viewers will be integrated into the existing tab control later
        }

        private void SetupEventHandlers()
        {
            // Layer manager events
            _layerManager.ActiveLayerChanged += OnActiveLayerChanged;
            _layerManager.LayerChanged += OnLayerChanged;

            // Library viewer events
            _wemadeMir2Viewer.ItemSelected += OnLibraryItemSelected;
            _shandaMir2Viewer.ItemSelected += OnLibraryItemSelected;
            _wemadeMir3Viewer.ItemSelected += OnLibraryItemSelected;
            _shandaMir3Viewer.ItemSelected += OnLibraryItemSelected;

            // Main form events for status updates
            if (_mainForm.MapPanel != null)
            {
                _mainForm.MapPanel.MouseMove += OnMapPanelMouseMove;
            }
        }

        /// <summary>
        /// Sets up default AutoTile sets based on the existing library structure
        /// </summary>
        private void SetupDefaultAutoTileSets()
        {
            // Create AutoTile sets for different tile types
            _autoTileSystem.CreateAutoTileSet(1, "Ground Tiles", EnhancedAutoTileSystem.AutoTileAlgorithm.Wang2Corner);
            _autoTileSystem.CreateAutoTileSet(2, "Wall Tiles", EnhancedAutoTileSystem.AutoTileAlgorithm.Platform);
            _autoTileSystem.CreateAutoTileSet(3, "Water Tiles", EnhancedAutoTileSystem.AutoTileAlgorithm.Blob);

            // Add some example tile variants (these would be configured based on actual library content)
            for (int i = 0; i < 16; i++)
            {
                _autoTileSystem.AddTileVariant(1, 0, i, EnhancedAutoTileSystem.TileType.Center);
            }
        }

        /// <summary>
        /// Integrates the enhanced components into the existing tab control
        /// </summary>
        public void IntegrateWithExistingUI()
        {
            var tabControl = _mainForm.tabControl1;
            if (tabControl == null) return;

            // Replace existing library tab pages with enhanced viewers
            ReplaceTabPage(tabControl, "tabWemadeMir2", _wemadeMir2Viewer, "Wemade Mir2");
            ReplaceTabPage(tabControl, "tabShandaMir2", _shandaMir2Viewer, "Shanda Mir2");
            ReplaceTabPage(tabControl, "tabWemadeMir3", _wemadeMir3Viewer, "Wemade Mir3");
            ReplaceTabPage(tabControl, "tabShandaMir3", _shandaMir3Viewer, "Shanda Mir3");

            // Load library data into viewers
            LoadLibraryData();
        }

        private void ReplaceTabPage(TabControl tabControl, string tabName, Control replacement, string displayName)
        {
            // Find the existing tab page
            TabPage existingTab = null;
            foreach (TabPage tab in tabControl.TabPages)
            {
                if (tab.Name == tabName)
                {
                    existingTab = tab;
                    break;
                }
            }

            if (existingTab != null)
            {
                // Create new tab page with enhanced viewer
                var newTab = new TabPage(displayName);
                newTab.Name = tabName;
                replacement.Dock = DockStyle.Fill;
                newTab.Controls.Add(replacement);
                
                // Replace the old tab
                int index = tabControl.TabPages.IndexOf(existingTab);
                tabControl.TabPages.RemoveAt(index);
                tabControl.TabPages.Insert(index, newTab);
                
                // Apply dark theme
                DarkTheme.ApplyToControl(newTab);
            }
        }

        private void LoadLibraryData()
        {
            try
            {
                // Load Wemade Mir2 libraries
                if (_mainForm._wemadeMir2IndexList != null)
                {
                    _wemadeMir2Viewer.LoadLibraries(_mainForm._wemadeMir2IndexList, Main.MirVerSion.WemadeMir2);
                }

                // Load Shanda Mir2 libraries
                if (_mainForm._shandaMir2IndexList != null)
                {
                    _shandaMir2Viewer.LoadLibraries(_mainForm._shandaMir2IndexList, Main.MirVerSion.ShandaMir2);
                }

                // Load Wemade Mir3 libraries
                if (_mainForm._wemadeMir3IndexList != null)
                {
                    _wemadeMir3Viewer.LoadLibraries(_mainForm._wemadeMir3IndexList, Main.MirVerSion.WemadeMir3);
                }

                // Load Shanda Mir3 libraries
                if (_mainForm._shandaMir3IndexList != null)
                {
                    _shandaMir3Viewer.LoadLibraries(_mainForm._shandaMir3IndexList, Main.MirVerSion.ShandaMir3);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading library data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnActiveLayerChanged(object sender, LayerEventArgs e)
        {
            _statusBar.UpdateCurrentLayer(e.Layer.Name);
        }

        private void OnLayerChanged(object sender, LayerEventArgs e)
        {
            // Layer changed - could trigger UI updates
        }

        private void OnLibraryItemSelected(object sender, LibraryItemSelectedEventArgs e)
        {
            // Update main form selection
            _mainForm.selectImageIndex = e.ItemIndex;
            
            if (e.IsDoubleClick)
            {
                // Double-click should activate the tool for placing this item
                _statusBar.ShowMessage($"Selected {e.ItemName} for placement");
            }
        }

        private void OnMapPanelMouseMove(object sender, MouseEventArgs e)
        {
            // Convert screen coordinates to map coordinates
            var mapCoords = ScreenToMapCoordinates(e.Location);
            _statusBar.UpdateCoordinates(mapCoords, e.Location);
        }

        private Point ScreenToMapCoordinates(Point screenCoords)
        {
            // This would need to be implemented based on the main form's coordinate system
            // For now, return a placeholder
            return new Point(
                screenCoords.X / 32, // Assuming 32px tiles
                screenCoords.Y / 32
            );
        }

        /// <summary>
        /// Updates the status bar with current tool information
        /// </summary>
        public void UpdateCurrentTool(string toolName)
        {
            _statusBar.UpdateCurrentTool(toolName);
        }

        /// <summary>
        /// Updates the zoom display
        /// </summary>
        public void UpdateZoom(float zoomPercentage)
        {
            _statusBar.UpdateZoom(zoomPercentage);
        }

        /// <summary>
        /// Updates FPS counter (call from render loop)
        /// </summary>
        public void UpdateFPS()
        {
            _statusBar.UpdateFPS();
        }

        /// <summary>
        /// Shows a progress operation
        /// </summary>
        public void ShowProgress(string message, int maximum = 100)
        {
            _statusBar.ShowProgress(message, maximum);
        }

        /// <summary>
        /// Updates progress
        /// </summary>
        public void UpdateProgress(int value, string message = null)
        {
            _statusBar.UpdateProgress(value, message);
        }

        /// <summary>
        /// Hides progress
        /// </summary>
        public void HideProgress()
        {
            _statusBar.HideProgress();
        }

        /// <summary>
        /// Shows a temporary message
        /// </summary>
        public void ShowMessage(string message, int durationMs = 3000)
        {
            _statusBar.ShowMessage(message, durationMs);
        }

        /// <summary>
        /// Creates a new layer
        /// </summary>
        public int CreateLayer(string name, LayerManager.LayerType type)
        {
            return _layerManager.AddLayer(name, type);
        }

        /// <summary>
        /// Gets the currently active layer
        /// </summary>
        public LayerManager.MapLayer GetActiveLayer()
        {
            return _layerManager.GetActiveLayer();
        }

        /// <summary>
        /// Sets the active layer
        /// </summary>
        public void SetActiveLayer(int layerId)
        {
            _layerManager.SetActiveLayer(layerId);
        }

        /// <summary>
        /// Applies AutoTile to a region
        /// </summary>
        public void ApplyAutoTile(int tileSetId, Rectangle area)
        {
            var activeLayer = GetActiveLayer();
            if (activeLayer == null) return;

            var results = _autoTileSystem.ApplyAutoTileToArea(tileSetId, area, pos =>
            {
                return activeLayer.Tiles.ContainsKey(pos);
            });

            foreach (var kvp in results)
            {
                _layerManager.PlaceTile(activeLayer.Id, kvp.Key, kvp.Value.LibraryIndex, kvp.Value.ImageIndex);
            }

            ShowMessage($"Applied AutoTile to {results.Count} positions");
        }

        public void Dispose()
        {
            _statusBar?.Dispose();
            _wemadeMir2Viewer?.Dispose();
            _shandaMir2Viewer?.Dispose();
            _wemadeMir3Viewer?.Dispose();
            _shandaMir3Viewer?.Dispose();
        }
    }

    /// <summary>
    /// Simple dock panel for organizing UI components
    /// </summary>
    public class DockPanel : Panel
    {
        public DockPanel()
        {
            BackColor = DarkTheme.SecondaryBackground;
            ForeColor = DarkTheme.PrimaryText;
        }
    }
}