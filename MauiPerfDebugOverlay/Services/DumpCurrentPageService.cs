using MauiPerfDebugOverlay.InternalControls;
using MauiPerfDebugOverlay.Models.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPerfDebugOverlay.Services
{
    internal class DumpCurrentPageService
    {

        //Method copied from https://github.com/davidortinau/Plugin.Maui.DebugOverlay
        //original author David Ortinau
        private Microsoft.Maui.Controls.Element GetCurrentActivePage()
        {
            var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage;
            if (mainPage == null)
            {
                return null;
            }

            // If the main page is a Shell, get the current page from it
            if (mainPage is Microsoft.Maui.Controls.Shell shell)
            {
                // Try to get the current page from Shell
                var currentPage = shell.CurrentPage;
                if (currentPage != null)
                {
                    return currentPage;
                }

                // Fallback: try to navigate the Shell hierarchy
                var currentItem = shell.CurrentItem;
                if (currentItem != null)
                {
                    // Shell hierarchy: Shell -> ShellItem -> ShellSection -> ShellContent -> Page
                    // currentItem is a ShellItem, so we need to get its current section, then current content
                    var currentSection = currentItem.CurrentItem; // This gets the ShellSection
                    if (currentSection != null)
                    {
                        var currentContent = currentSection.CurrentItem; // This gets the ShellContent
                        if (currentContent?.Content is Microsoft.Maui.Controls.Page contentPage)
                        {
                            return contentPage;
                        }
                    }
                }
            }

            // If the main page is a NavigationPage, get the current page
            if (mainPage is Microsoft.Maui.Controls.NavigationPage navigationPage)
            {
                return navigationPage.CurrentPage;
            }

            // If the main page is a TabbedPage, get the current page
            if (mainPage is Microsoft.Maui.Controls.TabbedPage tabbedPage)
            {
                return tabbedPage.CurrentPage;
            }

            // If the main page is a FlyoutPage, get the detail page
            if (mainPage is Microsoft.Maui.Controls.FlyoutPage flyoutPage)
            {
                return flyoutPage.Detail;
            }

            // Default: return the main page itself
            return mainPage;
        }


        public TreeNode DumpCurrentPage()
        {
            var currentPage = GetCurrentActivePage();
            if (currentPage == null)
                return new TreeNode { Name = "NoActivePage" };

            return DumpElement(currentPage);
        }


        private TreeNode DumpElement(Element element)
        {
            var node = new TreeNode
            {
                Id = element.Id,
                Name = element.GetType().Name,
                Children = new List<TreeNode>()
            };

            if (element.GetType() == typeof(Controls.PerformanceOverlayView))
                return node;

            if (element is Layout layout)
            {
                foreach (var child in layout.Children)
                    if (child is Element newElement)
                        node.Children.Add(DumpElement(newElement));
            }
            else if (element is ContentView contentView && contentView.Content is Element cvElement)
            {
                node.Children.Add(DumpElement(cvElement));
            }
            else if (element is ContentPage contentPage && contentPage.Content is Element cpElement)
            {
                node.Children.Add(DumpElement(cpElement));
            }
            else if (element is Shell shell)
            {
                foreach (var item in shell.Items)
                    if (item is Element newElement)
                        node.Children.Add(DumpElement(newElement));
            }
            else if (element is ScrollView scrollView && scrollView.Content is Element svElement)
            {
                node.Children.Add(DumpElement(svElement));
            }
            else if (element is Frame frame && frame.Content is Element fElement)
            {
                node.Children.Add(DumpElement(fElement));
            }
            else if (element is Border border && border.Content is Element bElement)
            {
                node.Children.Add(DumpElement(bElement));
            }

            return node;
        }
    }
}
