using System.Windows;
using System.Windows.Controls;

namespace DockerAquarium.Presentation.Behaviors;

/// <summary>
/// Attached behavior to bind tooltip DataContext to parent element's DataContext
/// Solves the WPF issue where tooltips don't inherit DataContext
/// </summary>
public static class ToolTipDataContextBehavior
{
    public static bool GetBindTooltipDataContext(DependencyObject obj)
    {
        return (bool)obj.GetValue(BindTooltipDataContextProperty);
    }

    public static void SetBindTooltipDataContext(DependencyObject obj, bool value)
    {
        obj.SetValue(BindTooltipDataContextProperty, value);
    }

    public static readonly DependencyProperty BindTooltipDataContextProperty =
        DependencyProperty.RegisterAttached(
            "BindTooltipDataContext",
            typeof(bool),
            typeof(ToolTipDataContextBehavior),
            new PropertyMetadata(false, OnBindTooltipDataContextChanged));

    private static void OnBindTooltipDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (!(bool)e.NewValue)
            return;

        if (d is FrameworkElement element)
        {
            // Bind immediately if DataContext is already set
            if (element.DataContext != null)
            {
                BindTooltipDataContext(element);
            }

            element.Loaded += (_, _) =>
            {
                BindTooltipDataContext(element);
            };

            element.DataContextChanged += (_, _) =>
            {
                BindTooltipDataContext(element);
            };
        }
    }

    private static void BindTooltipDataContext(FrameworkElement element)
    {
        var tooltip = element.ToolTip as ToolTip;
        if (tooltip != null && element.DataContext != null)
        {
            tooltip.DataContext = element.DataContext;
        }
    }
}

