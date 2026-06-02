using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Data;
using Avalonia.Threading;
using System;
using System.Linq;
using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.File;
using FileDBInterface.Model;
using FileDB.Model;
using FileDB.Infrastructure;

namespace FileDB.Views.Search;

public partial class FileView : UserControl
{
    private FileViewModel? viewModel;
    private IDatabaseAccessProvider? dbAccessProvider;

    public FileView()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
        {
            viewModel = ServiceLocator.Resolve<FileViewModel>();
            DataContext = viewModel;
            
            dbAccessProvider = ServiceLocator.Resolve<IDatabaseAccessProvider>();
            
            // Wire up event handlers after the canvas is loaded
            this.Loaded += OnLoaded;
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var canvas = this.FindControl<Views.Search.File.BoundingBoxCanvas>("BboxCanvas");
        if (canvas is not null)
        {
            canvas.BoundingBoxSaved += OnBoundingBoxSaved;
            
            // Find the FileCategorizationView and set up binding to its expander state
            var categView = this.FindControl<Views.Search.File.FileCategorizationView>("CategView");
            if (categView is not null)
            {
                // Attempt to set up binding immediately or wait for DataContext
                int retryCount = 0;
                void SetupBinding()
                {
                    retryCount++;
                    if (categView.DataContext is FileCategorizationViewModel categViewModel)
                    {
                        var binding = new Binding
                        {
                            Path = nameof(FileCategorizationViewModel.CategorizationIsExpanded),
                            Source = categViewModel,
                            Mode = BindingMode.OneWay
                        };
                        canvas.Bind(Views.Search.File.BoundingBoxCanvas.ForceShowBoundingBoxesProperty, binding);
                    }
                    else if (retryCount < 50) // Retry up to 50 times (5 seconds)
                    {
                        // Retry after a short delay
                        DispatcherTimer.Run(() =>
                        {
                            SetupBinding();
                            return false;
                        }, TimeSpan.FromMilliseconds(100));
                    }
                }
                
                SetupBinding();
            }
        }
    }

    private void OnBoundingBoxSaved(int personId, PersonBoundingBox bbox)
    {
        if (viewModel?.SelectedFile is null || dbAccessProvider is null)
            return;

        var fileId = viewModel.SelectedFile.Id;
        
        // Check if the person is already in the file; if not, add them
        if (!dbAccessProvider.DbAccess.GetPersonsFromFile(fileId).Any(p => p.Id == personId))
        {
            dbAccessProvider.DbAccess.InsertFilePerson(fileId, personId);
        }
        
        dbAccessProvider.DbAccess.UpdateFilePersonBoundingBox(fileId, personId, bbox);
        
        // Reload bounding boxes using the ViewModel's method which properly notifies bindings
        viewModel.LoadFilePersonBoundingBoxes(fileId);
        
        // Clear placement state
        viewModel.IsPlacingBoundingBox = false;
        viewModel.PlacingPersonId = -1;
        
        // Notify that bbox state has changed
        Messenger.Send(new BoundingBoxStateChanged(fileId, personId, HasBbox: true));
        
        // Get canvas reference
        var canvas = this.FindControl<Views.Search.File.BoundingBoxCanvas>("BboxCanvas");
        var categView = this.FindControl<Views.Search.File.FileCategorizationView>("CategView");
        
        // Expand the categorization view and force show bboxes
        if (categView?.DataContext is FileCategorizationViewModel categViewModel)
        {
            categViewModel.CategorizationIsExpanded = true;
            // Directly set ForceShowBoundingBoxes and explicitly redraw
            if (canvas is not null)
            {
                canvas.ForceShowBoundingBoxes = true;
                // Use dispatcher to ensure redraw happens on next frame
                Dispatcher.UIThread.Post(() =>
                {
                    canvas.InvalidateVisual();
                });
            }
        }
        else if (canvas is not null)
        {
            // Fallback: if binding not available, directly set canvas property
            canvas.ForceShowBoundingBoxes = true;
            Dispatcher.UIThread.Post(() =>
            {
                canvas.InvalidateVisual();
            });
        }
    }
}
