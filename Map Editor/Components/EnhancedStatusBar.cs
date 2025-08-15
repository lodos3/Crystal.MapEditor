using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Map_Editor.UI;

namespace Map_Editor.Components
{
    /// <summary>
    /// Enhanced status bar for displaying map editor information
    /// Shows cache statistics, performance metrics, and current tool information
    /// </summary>
    public class EnhancedStatusBar : StatusStrip
    {
        private ToolStripStatusLabel _coordinatesLabel;
        private ToolStripStatusLabel _zoomLabel;
        private ToolStripStatusLabel _layerLabel;
        private ToolStripStatusLabel _cacheLabel;
        private ToolStripStatusLabel _performanceLabel;
        private ToolStripStatusLabel _toolLabel;
        private ToolStripProgressBar _progressBar;
        
        private System.Timers.Timer _updateTimer;
        private DateTime _lastUpdate = DateTime.Now;
        private int _frameCount = 0;
        private float _currentFps = 0;

        public EnhancedStatusBar()
        {
            InitializeComponents();
            SetupTimer();
            DarkTheme.ApplyToControl(this);
        }

        private void InitializeComponents()
        {
            SuspendLayout();

            // Coordinates label
            _coordinatesLabel = new ToolStripStatusLabel("Position: -");
            _coordinatesLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _coordinatesLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Zoom label
            _zoomLabel = new ToolStripStatusLabel("Zoom: 100%");
            _zoomLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _zoomLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Layer label
            _layerLabel = new ToolStripStatusLabel("Layer: Background");
            _layerLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _layerLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Tool label
            _toolLabel = new ToolStripStatusLabel("Tool: Paint");
            _toolLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _toolLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Cache label
            _cacheLabel = new ToolStripStatusLabel("Cache: 0 items");
            _cacheLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _cacheLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Performance label
            _performanceLabel = new ToolStripStatusLabel("FPS: --");
            _performanceLabel.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _performanceLabel.TextAlign = ContentAlignment.MiddleLeft;

            // Progress bar
            _progressBar = new ToolStripProgressBar();
            _progressBar.Visible = false;
            _progressBar.Size = new Size(100, 16);

            // Add spring to push performance info to the right
            var springLabel = new ToolStripStatusLabel();
            springLabel.Spring = true;

            // Add items to status strip
            Items.AddRange(new ToolStripItem[]
            {
                _coordinatesLabel,
                new ToolStripSeparator(),
                _zoomLabel,
                new ToolStripSeparator(),
                _layerLabel,
                new ToolStripSeparator(),
                _toolLabel,
                new ToolStripSeparator(),
                _progressBar,
                springLabel,
                _cacheLabel,
                new ToolStripSeparator(),
                _performanceLabel
            });

            ResumeLayout(false);
        }

        private void SetupTimer()
        {
            _updateTimer = new System.Timers.Timer(1000); // Update every second
            _updateTimer.Elapsed += UpdateTimer_Elapsed;
            _updateTimer.Start();
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(UpdateCacheAndPerformanceInfo));
            }
            else
            {
                UpdateCacheAndPerformanceInfo();
            }
        }

        private void UpdateCacheAndPerformanceInfo()
        {
            try
            {
                // Update cache information
                var cacheStats = MapCache.GetCacheStats();
                _cacheLabel.Text = $"Cache: {cacheStats.ItemCount} items ({cacheStats.TotalMemoryFormatted})";

                // Update FPS
                _performanceLabel.Text = $"FPS: {_currentFps:F1}";
            }
            catch
            {
                // Ignore update errors
            }
        }

        /// <summary>
        /// Updates the mouse coordinates display
        /// </summary>
        public void UpdateCoordinates(Point mapCoordinates, Point screenCoordinates)
        {
            _coordinatesLabel.Text = $"Map: {mapCoordinates.X}, {mapCoordinates.Y} | Screen: {screenCoordinates.X}, {screenCoordinates.Y}";
        }

        /// <summary>
        /// Updates the zoom level display
        /// </summary>
        public void UpdateZoom(float zoomPercentage)
        {
            _zoomLabel.Text = $"Zoom: {zoomPercentage:F0}%";
        }

        /// <summary>
        /// Updates the current layer display
        /// </summary>
        public void UpdateCurrentLayer(string layerName)
        {
            _layerLabel.Text = $"Layer: {layerName}";
        }

        /// <summary>
        /// Updates the current tool display
        /// </summary>
        public void UpdateCurrentTool(string toolName)
        {
            _toolLabel.Text = $"Tool: {toolName}";
        }

        /// <summary>
        /// Updates FPS counter (call this from your render loop)
        /// </summary>
        public void UpdateFPS()
        {
            _frameCount++;
            var now = DateTime.Now;
            var elapsed = (now - _lastUpdate).TotalSeconds;
            
            if (elapsed >= 1.0)
            {
                _currentFps = _frameCount / (float)elapsed;
                _frameCount = 0;
                _lastUpdate = now;
            }
        }

        /// <summary>
        /// Shows progress bar with a message
        /// </summary>
        public void ShowProgress(string message = null, int maximum = 100)
        {
            _progressBar.Maximum = maximum;
            _progressBar.Value = 0;
            _progressBar.Visible = true;
            
            if (!string.IsNullOrEmpty(message))
            {
                _progressBar.ToolTipText = message;
            }
        }

        /// <summary>
        /// Updates progress bar value
        /// </summary>
        public void UpdateProgress(int value, string message = null)
        {
            if (_progressBar.Visible)
            {
                _progressBar.Value = Math.Max(0, Math.Min(_progressBar.Maximum, value));
                
                if (!string.IsNullOrEmpty(message))
                {
                    _progressBar.ToolTipText = message;
                }
            }
        }

        /// <summary>
        /// Hides the progress bar
        /// </summary>
        public void HideProgress()
        {
            _progressBar.Visible = false;
            _progressBar.ToolTipText = "";
        }

        /// <summary>
        /// Shows a temporary message in the status bar
        /// </summary>
        public void ShowMessage(string message, int durationMs = 3000)
        {
            var originalText = _coordinatesLabel.Text;
            _coordinatesLabel.Text = message;
            
            // Use a timer to restore the original text
            var messageTimer = new System.Timers.Timer(durationMs);
            messageTimer.Elapsed += (s, e) =>
            {
                messageTimer.Stop();
                messageTimer.Dispose();
                
                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        if (_coordinatesLabel.Text == message) // Only restore if it hasn't changed
                        {
                            _coordinatesLabel.Text = originalText;
                        }
                    }));
                }
                else
                {
                    if (_coordinatesLabel.Text == message)
                    {
                        _coordinatesLabel.Text = originalText;
                    }
                }
            };
            messageTimer.Start();
        }

        /// <summary>
        /// Updates cache hit rate tooltip
        /// </summary>
        public void UpdateCacheTooltip()
        {
            var cacheStats = MapCache.GetCacheStats();
            _cacheLabel.ToolTipText = $"Cache Hit Rate: {cacheStats.HitRate:P1}\n" +
                                     $"Memory Usage: {cacheStats.TotalMemoryFormatted}\n" +
                                     $"Items: {cacheStats.ItemCount}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _updateTimer?.Stop();
                _updateTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}