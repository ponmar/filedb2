using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using FileDBInterface.Model;
using FileDB.Model;
using System;
using System.Collections.Generic;
using FileDB.Infrastructure;

namespace FileDB.Views.Search.File;

/// <summary>
/// Custom control for drawing, editing, and displaying person bounding boxes on images.
/// Bounding boxes are stored in normalized [0.0, 1.0] range in the database.
/// When images are rotated, the stored coordinates are transformed via BoundingBoxRotator.
/// This canvas displays the current state without applying visual rotations (rotation is pre-applied in the database).
/// Boxes are rendered with drop shadow effects to ensure visibility on any background color.
/// </summary>
public class BoundingBoxCanvas : Canvas
{
    private Point dragStartPoint = new();
    private Rect? previewBoundingBox;

    public static readonly StyledProperty<IEnumerable<(PersonModel Person, PersonBoundingBox BBox)>> BoundingBoxesProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, IEnumerable<(PersonModel Person, PersonBoundingBox BBox)>>(
            nameof(BoundingBoxes),
            Array.Empty<(PersonModel, PersonBoundingBox)>()
        );

    public static readonly StyledProperty<bool> IsPlacingBoundingBoxProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, bool>(
            nameof(IsPlacingBoundingBox),
            false
        );

    public static readonly StyledProperty<int> PlacingPersonIdProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, int>(
            nameof(PlacingPersonId),
            -1
        );

    public static readonly StyledProperty<double> ImageWidthProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, double>(
            nameof(ImageWidth),
            1.0
        );

    public static readonly StyledProperty<double> ImageHeightProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, double>(
            nameof(ImageHeight),
            1.0
        );

    public static readonly StyledProperty<int> ImageRotationProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, int>(
            nameof(ImageRotation),
            0
        );

    public static readonly StyledProperty<bool> ForceShowBoundingBoxesProperty =
        AvaloniaProperty.Register<BoundingBoxCanvas, bool>(
            nameof(ForceShowBoundingBoxes),
            false
        );

    public IEnumerable<(PersonModel Person, PersonBoundingBox BBox)> BoundingBoxes
    {
        get => GetValue(BoundingBoxesProperty);
        set => SetValue(BoundingBoxesProperty, value);
    }

    public bool IsPlacingBoundingBox
    {
        get => GetValue(IsPlacingBoundingBoxProperty);
        set => SetValue(IsPlacingBoundingBoxProperty, value);
    }

    public int PlacingPersonId
    {
        get => GetValue(PlacingPersonIdProperty);
        set => SetValue(PlacingPersonIdProperty, value);
    }

    public double ImageWidth
    {
        get => GetValue(ImageWidthProperty);
        set => SetValue(ImageWidthProperty, value);
    }

    public double ImageHeight
    {
        get => GetValue(ImageHeightProperty);
        set => SetValue(ImageHeightProperty, value);
    }

    public int ImageRotation
    {
        get => GetValue(ImageRotationProperty);
        set => SetValue(ImageRotationProperty, value);
    }

    public bool ForceShowBoundingBoxes
    {
        get => GetValue(ForceShowBoundingBoxesProperty);
        set => SetValue(ForceShowBoundingBoxesProperty, value);
    }

    public BoundingBoxCanvas()
    {
        Background = new SolidColorBrush(Colors.Transparent);
        Cursor = new Cursor(StandardCursorType.Arrow);
        Focusable = true;
        SizeChanged += (_, _) => RedrawBoundingBoxes();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsPlacingBoundingBoxProperty || change.Property == PlacingPersonIdProperty)
        {
            UpdateCursor();
            // Focus the canvas so it can receive ESC key events
            if (IsPlacingBoundingBox && PlacingPersonId >= 0)
            {
                Focus();
            }
        }

        if (change.Property == BoundingBoxesProperty || change.Property == ImageWidthProperty || change.Property == ImageHeightProperty || change.Property == ImageRotationProperty || change.Property == ForceShowBoundingBoxesProperty)
        {
            RedrawBoundingBoxes();
        }
    }

    /// <summary>
    /// Calculate the visual bounds of the image as displayed in the canvas, accounting for Stretch="Uniform" centering.
    /// For 90°/270° rotations, swaps width/height for aspect ratio calculation (visual effect of rotation on dimensions).
    /// Note: The actual coordinate transformation for rotations is handled at the database level via BoundingBoxRotator.
    /// </summary>
    private Rect GetVisualImageBounds()
    {
        if (ImageWidth <= 0 || ImageHeight <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return Bounds;
        }

        double canvasWidth = Bounds.Width;
        double canvasHeight = Bounds.Height;
        
        // When image is rotated 90° or 270°, swap width and height
        double effectiveImageWidth = ImageWidth;
        double effectiveImageHeight = ImageHeight;
        
        int normalizedRotation = ((ImageRotation % 360) + 360) % 360;
        if (normalizedRotation == 90 || normalizedRotation == 270)
        {
            (effectiveImageWidth, effectiveImageHeight) = (effectiveImageHeight, effectiveImageWidth);
        }
        
        double imageAspectRatio = effectiveImageWidth / effectiveImageHeight;
        double canvasAspectRatio = canvasWidth / canvasHeight;

        double visualWidth, visualHeight;
        if (canvasAspectRatio > imageAspectRatio)
        {
            // Canvas is wider - image height fills canvas, width is constrained
            visualHeight = canvasHeight;
            visualWidth = canvasHeight * imageAspectRatio;
        }
        else
        {
            // Canvas is taller or equal - image width fills canvas, height is constrained
            visualWidth = canvasWidth;
            visualHeight = canvasWidth / imageAspectRatio;
        }

        // Center the image in the canvas
        double offsetX = (canvasWidth - visualWidth) / 2;
        double offsetY = (canvasHeight - visualHeight) / 2;

        return new Rect(offsetX, offsetY, visualWidth, visualHeight);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        dragStartPoint = e.GetPosition(this);

        if (IsPlacingBoundingBox && PlacingPersonId >= 0)
        {
            previewBoundingBox = null;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var currentPoint = e.GetPosition(this);

        if (IsPlacingBoundingBox && PlacingPersonId >= 0 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            previewBoundingBox = CreateBoundingBoxFromDrag(dragStartPoint, currentPoint);
            RedrawBoundingBoxes();
        }

        UpdateCursor();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IsPlacingBoundingBox && previewBoundingBox.HasValue)
        {
            var normalizedBBox = PixelsToNormalization(previewBoundingBox.Value);
            BoundingBoxSaved?.Invoke(PlacingPersonId, normalizedBBox);
            previewBoundingBox = null;
            dragStartPoint = new();
            IsPlacingBoundingBox = false;
            PlacingPersonId = -1;
            RedrawBoundingBoxes();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape && IsPlacingBoundingBox)
        {
            int personId = PlacingPersonId;
            IsPlacingBoundingBox = false;
            PlacingPersonId = -1;
            previewBoundingBox = null;
            dragStartPoint = new();
            RedrawBoundingBoxes();
            e.Handled = true;
            
            // Notify that placement was aborted (personId is still valid before reset)
            Messenger.Send(new PersonBoundingBoxPlacementAborted(-1, personId));
        }
    }

    private void RedrawBoundingBoxes()
    {
        Children.Clear();

        // Draw existing bounding boxes - show if ForceShowBoundingBoxes is true
        if (ForceShowBoundingBoxes)
        {
            foreach (var (person, bbox) in BoundingBoxes)
            {
                var pixelRect = NormalizationToPixels(bbox);
                var rectangle = new Rectangle
                {
                    Stroke = new SolidColorBrush(Colors.Green),
                    StrokeThickness = 2,
                    Fill = null,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 4,
                        Color = Colors.Black,
                        Opacity = 0.6,
                        OffsetX = 0,
                        OffsetY = 0
                    }
                };
                Canvas.SetLeft(rectangle, pixelRect.X);
                Canvas.SetTop(rectangle, pixelRect.Y);
                rectangle.Width = pixelRect.Width;
                rectangle.Height = pixelRect.Height;
                Children.Add(rectangle);

                var label = new TextBlock
                {
                    Text = person.FullName,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 12,
                    Width = pixelRect.Width,
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    Effect = new DropShadowEffect
                    {
                        BlurRadius = 4,
                        Color = Colors.Black,
                        Opacity = 0.9,
                        OffsetX = 1,
                        OffsetY = 1
                    }
                };
                Canvas.SetLeft(label, pixelRect.X);
                Canvas.SetTop(label, pixelRect.Bottom + 2);
                Children.Add(label);
            }
        }

        // Draw preview bbox while placing
        if (previewBoundingBox.HasValue)
        {
            var rectangle = new Rectangle
            {
                Stroke = new SolidColorBrush(Colors.Green),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Colors.Green) { Opacity = 0.3 },
                Effect = new DropShadowEffect
                {
                    BlurRadius = 4,
                    Color = Colors.Black,
                    Opacity = 0.6,
                    OffsetX = 0,
                    OffsetY = 0
                }
            };
            Canvas.SetLeft(rectangle, previewBoundingBox.Value.X);
            Canvas.SetTop(rectangle, previewBoundingBox.Value.Y);
            rectangle.Width = previewBoundingBox.Value.Width;
            rectangle.Height = previewBoundingBox.Value.Height;
            Children.Add(rectangle);
        }
        
        // Ensure visual is invalidated
        InvalidateVisual();
        InvalidateArrange();
    }

    private Rect CreateBoundingBoxFromDrag(Point start, Point end)
    {
        double x = Math.Min(start.X, end.X);
        double y = Math.Min(start.Y, end.Y);
        double width = Math.Abs(end.X - start.X);
        double height = Math.Abs(end.Y - start.Y);

        // Clamp to visual image bounds (not full canvas)
        var visualBounds = GetVisualImageBounds();
        x = Math.Max(visualBounds.X, x);
        y = Math.Max(visualBounds.Y, y);
        width = Math.Min(width, visualBounds.Width + visualBounds.X - x);
        height = Math.Min(height, visualBounds.Height + visualBounds.Y - y);

        return new Rect(x, y, width, height);
    }

    private PersonBoundingBox PixelsToNormalization(Rect pixelRect)
    {
        var visualBounds = GetVisualImageBounds();
        
        // Convert from canvas space to visual image space, then to normalized (0-1) space
        double x = (pixelRect.X - visualBounds.X) / visualBounds.Width;
        double y = (pixelRect.Y - visualBounds.Y) / visualBounds.Height;
        double width = pixelRect.Width / visualBounds.Width;
        double height = pixelRect.Height / visualBounds.Height;

        // Clamp to [0, 1]
        x = Math.Max(0, Math.Min(1, x));
        y = Math.Max(0, Math.Min(1, y));
        width = Math.Max(0, Math.Min(1 - x, width));
        height = Math.Max(0, Math.Min(1 - y, height));

        return new PersonBoundingBox(x, y, width, height);
    }

    private Rect NormalizationToPixels(PersonBoundingBox bbox)
    {
        var visualBounds = GetVisualImageBounds();
        
        // Convert from normalized (0-1) space to visual image space, then to canvas space
        double x = visualBounds.X + (bbox.X * visualBounds.Width);
        double y = visualBounds.Y + (bbox.Y * visualBounds.Height);
        double width = bbox.Width * visualBounds.Width;
        double height = bbox.Height * visualBounds.Height;

        return new Rect(x, y, width, height);
    }

    private void UpdateCursor()
    {
        if (IsPlacingBoundingBox && PlacingPersonId >= 0)
        {
            Cursor = new Cursor(StandardCursorType.Cross);
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

    public event Action<int, PersonBoundingBox>? BoundingBoxSaved;
}
