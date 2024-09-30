using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace Ambinity;

public class PageFactory : INavigationPageFactory
{
    private readonly ViewLocator _viewLocator;

    public PageFactory()
    {
        _viewLocator = new ViewLocator();
    }

    /// <inheritdoc />
    public Control? GetPage(Type srcType)
    {
        return null;
    }

    /// <inheritdoc />
    public Control GetPageFromObject(object target)
    {
        if (target == null)
            return null;
        Control control = _viewLocator.Build(target);
        control.DataContext = target;
        return control;
    }
}