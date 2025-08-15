using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Map_Editor.UI;

namespace Map_Editor.Components
{
    /// <summary>
    /// Enhanced Library Viewer component for viewing Shanda/Wemade Mir2 and Mir3 tiles and objects
    /// Provides improved navigation, search, and preview capabilities similar to modern asset browsers
    /// </summary>
    public partial class EnhancedLibraryViewer : UserControl
    {
        private ListBox _libraryListBox;
        private ListView _itemListView;
        private PictureBox _previewPictureBox;
        private TextBox _searchTextBox;
        private ComboBox _filterComboBox;
        private Label _infoLabel;
        private SplitContainer _mainSplitContainer;
        private SplitContainer _rightSplitContainer;
        private Panel _searchPanel;
        private ImageList _imageList;

        private Dictionary<int, int> _indexList;
        private MirVerSion _currentVersion;
        private string _searchFilter = "";
        private string _typeFilter = "All";

        public event EventHandler<LibraryItemSelectedEventArgs> ItemSelected;

        public EnhancedLibraryViewer()
        {
            InitializeComponent();
            SetupEventHandlers();
            DarkTheme.ApplyToControl(this);
        }

        private void InitializeComponent()
        {
            _imageList = new ImageList();
            _imageList.ImageSize = new Size(64, 64);
            _imageList.ColorDepth = ColorDepth.Depth32Bit;

            // Main split container (library list | item view + preview)
            _mainSplitContainer = new SplitContainer();
            _mainSplitContainer.Dock = DockStyle.Fill;
            _mainSplitContainer.SplitterDistance = 200;
            _mainSplitContainer.Panel1MinSize = 150;
            _mainSplitContainer.Panel2MinSize = 300;

            // Right split container (item view | preview + info)
            _rightSplitContainer = new SplitContainer();
            _rightSplitContainer.Dock = DockStyle.Fill;
            _rightSplitContainer.Orientation = Orientation.Horizontal;
            _rightSplitContainer.SplitterDistance = 300;
            _rightSplitContainer.Panel1MinSize = 200;
            _rightSplitContainer.Panel2MinSize = 100;

            // Search panel
            _searchPanel = new Panel();
            _searchPanel.Height = 60;
            _searchPanel.Dock = DockStyle.Top;

            // Search textbox
            _searchTextBox = new TextBox();
            _searchTextBox.PlaceholderText = "Search items...";
            _searchTextBox.Location = new Point(10, 10);
            _searchTextBox.Size = new Size(150, 23);

            // Filter combobox
            _filterComboBox = new ComboBox();
            _filterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _filterComboBox.Location = new Point(170, 10);
            _filterComboBox.Size = new Size(100, 23);
            _filterComboBox.Items.AddRange(new[] { "All", "Tiles", "Objects", "SmTiles", "Walls" });
            _filterComboBox.SelectedIndex = 0;

            // Library list box
            _libraryListBox = new ListBox();
            _libraryListBox.Dock = DockStyle.Fill;
            _libraryListBox.IntegralHeight = false;

            // Item list view
            _itemListView = new ListView();
            _itemListView.Dock = DockStyle.Fill;
            _itemListView.View = View.LargeIcon;
            _itemListView.LargeImageList = _imageList;
            _itemListView.MultiSelect = false;
            _itemListView.ShowGroups = false;
            _itemListView.VirtualMode = true; // For better performance with large lists
            _itemListView.VirtualListSize = 0;

            // Preview picture box
            _previewPictureBox = new PictureBox();
            _previewPictureBox.Dock = DockStyle.Fill;
            _previewPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            _previewPictureBox.BackColor = Color.Black;

            // Info label
            _infoLabel = new Label();
            _infoLabel.Dock = DockStyle.Bottom;
            _infoLabel.Height = 60;
            _infoLabel.Padding = new Padding(10);
            _infoLabel.Text = "Select an item to view details";

            // Setup layout
            _searchPanel.Controls.Add(_searchTextBox);
            _searchPanel.Controls.Add(_filterComboBox);

            _mainSplitContainer.Panel1.Controls.Add(_libraryListBox);
            _mainSplitContainer.Panel1.Controls.Add(_searchPanel);

            _rightSplitContainer.Panel1.Controls.Add(_itemListView);
            _rightSplitContainer.Panel2.Controls.Add(_previewPictureBox);
            _rightSplitContainer.Panel2.Controls.Add(_infoLabel);

            _mainSplitContainer.Panel2.Controls.Add(_rightSplitContainer);

            this.Controls.Add(_mainSplitContainer);
            this.Size = new Size(800, 600);
        }

        private void SetupEventHandlers()
        {
            _libraryListBox.SelectedIndexChanged += LibraryListBox_SelectedIndexChanged;
            _itemListView.SelectedIndexChanged += ItemListView_SelectedIndexChanged;
            _itemListView.RetrieveVirtualItem += ItemListView_RetrieveVirtualItem;
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;
            _filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
            _previewPictureBox.DoubleClick += PreviewPictureBox_DoubleClick;
        }

        public void LoadLibraries(Dictionary<int, int> indexList, MirVerSion version)
        {
            _indexList = indexList;
            _currentVersion = version;

            _libraryListBox.Items.Clear();
            
            // Load library names based on version
            foreach (var kvp in _indexList)
            {
                if (Libraries.ListItems[kvp.Value] != null)
                {
                    var listItem = Libraries.ListItems[kvp.Value];
                    _libraryListBox.Items.Add(new LibraryDisplayItem
                    {
                        Text = listItem.Text,
                        LibraryIndex = kvp.Value,
                        ItemCount = Libraries.MapLibs[kvp.Value]?.Images?.Count ?? 0
                    });
                }
            }

            if (_libraryListBox.Items.Count > 0)
            {
                _libraryListBox.SelectedIndex = 0;
            }
        }

        private void LibraryListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_libraryListBox.SelectedItem is LibraryDisplayItem selectedLib)
            {
                LoadLibraryItems(selectedLib.LibraryIndex);
            }
        }

        private void LoadLibraryItems(int libraryIndex)
        {
            var library = Libraries.MapLibs[libraryIndex];
            if (library?.Images == null) return;

            // Filter items based on search and type
            var filteredItems = new List<LibraryItemInfo>();
            
            for (int i = 0; i < library.Images.Count; i++)
            {
                var itemName = $"Item {i}";
                
                // Apply search filter
                if (!string.IsNullOrEmpty(_searchFilter) && 
                    !itemName.ToLower().Contains(_searchFilter.ToLower()))
                    continue;

                // Apply type filter
                if (_typeFilter != "All")
                {
                    var libraryName = Libraries.ListItems[libraryIndex]?.Text ?? "";
                    if (!libraryName.ToLower().Contains(_typeFilter.ToLower()))
                        continue;
                }

                filteredItems.Add(new LibraryItemInfo
                {
                    Index = i,
                    LibraryIndex = libraryIndex,
                    Name = itemName
                });
            }

            _itemListView.VirtualListSize = filteredItems.Count;
            _currentFilteredItems = filteredItems;
            _itemListView.Invalidate();

            // Update info
            UpdateInfoLabel(libraryIndex, filteredItems.Count);
        }

        private List<LibraryItemInfo> _currentFilteredItems = new List<LibraryItemInfo>();

        private void ItemListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (e.ItemIndex >= 0 && e.ItemIndex < _currentFilteredItems.Count)
            {
                var item = _currentFilteredItems[e.ItemIndex];
                var listViewItem = new ListViewItem(item.Name);
                listViewItem.Tag = item;
                
                // Try to get cached preview
                var cachedPreview = MapCache.GetCachedPreview(item.LibraryIndex, item.Index, "library");
                if (cachedPreview != null)
                {
                    var imageKey = $"{item.LibraryIndex}_{item.Index}";
                    if (!_imageList.Images.ContainsKey(imageKey))
                    {
                        _imageList.Images.Add(imageKey, cachedPreview);
                    }
                    listViewItem.ImageKey = imageKey;
                }
                else
                {
                    // Generate preview asynchronously
                    GeneratePreviewAsync(item);
                }

                e.Item = listViewItem;
            }
        }

        private void GeneratePreviewAsync(LibraryItemInfo item)
        {
            // Use background thread to generate preview
            var bgWorker = new System.ComponentModel.BackgroundWorker();
            bgWorker.DoWork += (s, e) =>
            {
                try
                {
                    var library = Libraries.MapLibs[item.LibraryIndex];
                    if (library?.Images[item.Index] != null)
                    {
                        var image = library.Images[item.Index];
                        var preview = CreateThumbnail(image.Image, 64, 64);
                        
                        if (preview != null)
                        {
                            MapCache.CachePreview(item.LibraryIndex, item.Index, "library", preview);
                            e.Result = new { Item = item, Preview = preview };
                        }
                    }
                }
                catch
                {
                    // Ignore errors in background generation
                }
            };

            bgWorker.RunWorkerCompleted += (s, e) =>
            {
                if (e.Result != null)
                {
                    dynamic result = e.Result;
                    var imageKey = $"{result.Item.LibraryIndex}_{result.Item.Index}";
                    if (!_imageList.Images.ContainsKey(imageKey))
                    {
                        _imageList.Images.Add(imageKey, result.Preview);
                        _itemListView.Invalidate(); // Refresh to show new image
                    }
                }
            };

            bgWorker.RunWorkerAsync();
        }

        private Bitmap CreateThumbnail(Bitmap original, int width, int height)
        {
            if (original == null) return null;

            try
            {
                var thumbnail = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(thumbnail))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.DrawImage(original, 0, 0, width, height);
                }
                return thumbnail;
            }
            catch
            {
                return null;
            }
        }

        private void ItemListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_itemListView.SelectedIndices.Count > 0)
            {
                var selectedIndex = _itemListView.SelectedIndices[0];
                if (selectedIndex >= 0 && selectedIndex < _currentFilteredItems.Count)
                {
                    var item = _currentFilteredItems[selectedIndex];
                    ShowPreview(item);
                    
                    // Raise selection event
                    ItemSelected?.Invoke(this, new LibraryItemSelectedEventArgs
                    {
                        LibraryIndex = item.LibraryIndex,
                        ItemIndex = item.Index,
                        ItemName = item.Name
                    });
                }
            }
        }

        private void ShowPreview(LibraryItemInfo item)
        {
            try
            {
                var library = Libraries.MapLibs[item.LibraryIndex];
                if (library?.Images[item.Index] != null)
                {
                    var image = library.Images[item.Index];
                    _previewPictureBox.Image = image.Image;
                    
                    UpdateInfoLabel(item.LibraryIndex, -1, item);
                }
            }
            catch (Exception ex)
            {
                _infoLabel.Text = $"Error loading preview: {ex.Message}";
            }
        }

        private void UpdateInfoLabel(int libraryIndex, int totalItems, LibraryItemInfo selectedItem = null)
        {
            var libraryName = Libraries.ListItems[libraryIndex]?.Text ?? "Unknown";
            
            if (selectedItem != null)
            {
                var library = Libraries.MapLibs[libraryIndex];
                var image = library?.Images[selectedItem.Index];
                
                _infoLabel.Text = $"Library: {libraryName}\n" +
                                 $"Item: {selectedItem.Index} ({selectedItem.Name})\n" +
                                 $"Size: {image?.Width ?? 0}x{image?.Height ?? 0}";
            }
            else
            {
                _infoLabel.Text = $"Library: {libraryName}\n" +
                                 $"Total Items: {totalItems}\n" +
                                 $"Version: {_currentVersion}";
            }
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            _searchFilter = _searchTextBox.Text;
            RefreshCurrentLibrary();
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _typeFilter = _filterComboBox.SelectedItem?.ToString() ?? "All";
            RefreshCurrentLibrary();
        }

        private void RefreshCurrentLibrary()
        {
            if (_libraryListBox.SelectedItem is LibraryDisplayItem selectedLib)
            {
                LoadLibraryItems(selectedLib.LibraryIndex);
            }
        }

        private void PreviewPictureBox_DoubleClick(object sender, EventArgs e)
        {
            // Double-click to use selected item
            if (_itemListView.SelectedIndices.Count > 0)
            {
                var selectedIndex = _itemListView.SelectedIndices[0];
                if (selectedIndex >= 0 && selectedIndex < _currentFilteredItems.Count)
                {
                    var item = _currentFilteredItems[selectedIndex];
                    
                    // Trigger item selection event for usage
                    ItemSelected?.Invoke(this, new LibraryItemSelectedEventArgs
                    {
                        LibraryIndex = item.LibraryIndex,
                        ItemIndex = item.Index,
                        ItemName = item.Name,
                        IsDoubleClick = true
                    });
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imageList?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Supporting classes
    public class LibraryDisplayItem
    {
        public string Text { get; set; }
        public int LibraryIndex { get; set; }
        public int ItemCount { get; set; }

        public override string ToString()
        {
            return $"{Text} ({ItemCount} items)";
        }
    }

    public class LibraryItemInfo
    {
        public int Index { get; set; }
        public int LibraryIndex { get; set; }
        public string Name { get; set; }
    }

    public class LibraryItemSelectedEventArgs : EventArgs
    {
        public int LibraryIndex { get; set; }
        public int ItemIndex { get; set; }
        public string ItemName { get; set; }
        public bool IsDoubleClick { get; set; }
    }
}