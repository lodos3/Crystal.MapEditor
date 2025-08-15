using System;
using System.Drawing;
using System.Windows.Forms;

namespace Map_Editor.UI
{
    /// <summary>
    /// Enhanced input handler for smooth WASD map scrolling and improved keybinds
    /// Mimics modern map editor controls like TILED
    /// </summary>
    public class EnhancedInputHandler
    {
        private readonly Main _parentForm;
        private readonly Timer _scrollTimer;
        private readonly int _scrollSpeed = 5;
        private readonly int _fastScrollSpeed = 15;
        
        // Key states for smooth scrolling
        private bool _wPressed = false;
        private bool _aPressed = false;
        private bool _sPressed = false;
        private bool _dPressed = false;
        private bool _shiftPressed = false;
        private bool _ctrlPressed = false;

        public EnhancedInputHandler(Main parentForm)
        {
            _parentForm = parentForm ?? throw new ArgumentNullException(nameof(parentForm));
            
            // Setup scroll timer for smooth movement
            _scrollTimer = new Timer();
            _scrollTimer.Interval = 16; // ~60 FPS for smooth scrolling
            _scrollTimer.Tick += ScrollTimer_Tick;
            
            SetupKeyboardHooks();
        }

        private void SetupKeyboardHooks()
        {
            _parentForm.KeyPreview = true;
            _parentForm.KeyDown += OnKeyDown;
            _parentForm.KeyUp += OnKeyUp;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Handle modifier keys
            _shiftPressed = e.Shift;
            _ctrlPressed = e.Control;

            // Handle WASD movement keys
            switch (e.KeyCode)
            {
                case Keys.W:
                    if (!_wPressed)
                    {
                        _wPressed = true;
                        StartScrolling();
                    }
                    e.Handled = true;
                    break;

                case Keys.A:
                    if (!_aPressed)
                    {
                        _aPressed = true;
                        StartScrolling();
                    }
                    e.Handled = true;
                    break;

                case Keys.S:
                    if (!_sPressed)
                    {
                        _sPressed = true;
                        StartScrolling();
                    }
                    e.Handled = true;
                    break;

                case Keys.D:
                    if (!_dPressed)
                    {
                        _dPressed = true;
                        StartScrolling();
                    }
                    e.Handled = true;
                    break;

                // Enhanced keybinds similar to TILED
                case Keys.Space:
                    // Toggle between different edit modes
                    if (e.Control)
                    {
                        ToggleEditMode();
                        e.Handled = true;
                    }
                    break;

                case Keys.G:
                    // Toggle grid
                    if (_parentForm.chkDrawGrids != null)
                    {
                        _parentForm.chkDrawGrids.Checked = !_parentForm.chkDrawGrids.Checked;
                    }
                    e.Handled = true;
                    break;

                case Keys.H:
                    // Toggle help/layers panel
                    ToggleLayersPanel();
                    e.Handled = true;
                    break;

                case Keys.Oemplus:
                case Keys.Add:
                    // Zoom in
                    _parentForm.PerformZoomIn();
                    e.Handled = true;
                    break;

                case Keys.OemMinus:
                case Keys.Subtract:
                    // Zoom out
                    _parentForm.PerformZoomOut();
                    e.Handled = true;
                    break;

                case Keys.D0:
                    // Reset zoom
                    if (e.Control)
                    {
                        ResetZoom();
                        e.Handled = true;
                    }
                    break;

                case Keys.Tab:
                    // Cycle through tabs
                    CycleTabs();
                    e.Handled = true;
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            // Handle modifier keys
            _shiftPressed = e.Shift;
            _ctrlPressed = e.Control;

            // Handle WASD movement keys
            switch (e.KeyCode)
            {
                case Keys.W:
                    _wPressed = false;
                    CheckStopScrolling();
                    e.Handled = true;
                    break;

                case Keys.A:
                    _aPressed = false;
                    CheckStopScrolling();
                    e.Handled = true;
                    break;

                case Keys.S:
                    _sPressed = false;
                    CheckStopScrolling();
                    e.Handled = true;
                    break;

                case Keys.D:
                    _dPressed = false;
                    CheckStopScrolling();
                    e.Handled = true;
                    break;
            }
        }

        private void StartScrolling()
        {
            if (!_scrollTimer.Enabled)
            {
                _scrollTimer.Start();
            }
        }

        private void CheckStopScrolling()
        {
            if (!_wPressed && !_aPressed && !_sPressed && !_dPressed)
            {
                _scrollTimer.Stop();
            }
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            if (_parentForm.map == null) return;

            int speed = _shiftPressed ? _fastScrollSpeed : _scrollSpeed;
            int deltaX = 0;
            int deltaY = 0;

            if (_wPressed) deltaY -= speed; // Up
            if (_sPressed) deltaY += speed; // Down
            if (_aPressed) deltaX -= speed; // Left
            if (_dPressed) deltaX += speed; // Right

            if (deltaX != 0 || deltaY != 0)
            {
                var currentPoint = _parentForm.GetMapPoint();
                var newX = Math.Max(0, Math.Min(_parentForm.GetMapWidth() - 1, currentPoint.X + deltaX));
                var newY = Math.Max(0, Math.Min(_parentForm.GetMapHeight() - 1, currentPoint.Y + deltaY));
                
                _parentForm.Jump(newX, newY);
            }
        }

        private void ToggleEditMode()
        {
            // Cycle through different editing layers/modes
            if (_parentForm.cmbEditorLayer?.Items.Count > 0)
            {
                var currentIndex = _parentForm.cmbEditorLayer.SelectedIndex;
                var nextIndex = (currentIndex + 1) % _parentForm.cmbEditorLayer.Items.Count;
                _parentForm.cmbEditorLayer.SelectedIndex = nextIndex;
            }
        }

        private void ToggleLayersPanel()
        {
            // Toggle visibility of layer options
            if (_parentForm.chkBack != null)
            {
                var visible = _parentForm.chkBack.Checked;
                _parentForm.chkBack.Checked = !visible;
                _parentForm.chkMidd.Checked = !visible;
                _parentForm.chkFront.Checked = !visible;
            }
        }

        private void ResetZoom()
        {
            // Reset zoom to default level
            try
            {
                while (_parentForm.CanZoomOut())
                {
                    _parentForm.PerformZoomOut();
                }
                // Then zoom in to a comfortable level
                for (int i = 0; i < 3; i++)
                {
                    if (_parentForm.CanZoomIn())
                        _parentForm.PerformZoomIn();
                }
            }
            catch
            {
                // Ignore zoom errors
            }
        }

        private void CycleTabs()
        {
            if (_parentForm.tabControl1?.TabPages.Count > 0)
            {
                var currentIndex = _parentForm.tabControl1.SelectedIndex;
                var nextIndex = _shiftPressed ? 
                    (currentIndex - 1 + _parentForm.tabControl1.TabPages.Count) % _parentForm.tabControl1.TabPages.Count :
                    (currentIndex + 1) % _parentForm.tabControl1.TabPages.Count;
                _parentForm.tabControl1.SelectedIndex = nextIndex;
            }
        }

        public void Dispose()
        {
            _scrollTimer?.Stop();
            _scrollTimer?.Dispose();
            
            if (_parentForm != null)
            {
                _parentForm.KeyDown -= OnKeyDown;
                _parentForm.KeyUp -= OnKeyUp;
            }
        }
    }
}