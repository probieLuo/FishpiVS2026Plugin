using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FishpiVS2026Plugin.Helpers
{
    public static class AutoScrollBehavior
    {
        // 附加属性：是否启用智能自动滚动
        public static readonly DependencyProperty SmartAutoScrollProperty =
            DependencyProperty.RegisterAttached(
                "SmartAutoScroll",
                typeof(bool),
                typeof(AutoScrollBehavior),
                new PropertyMetadata(false, OnSmartAutoScrollChanged));

        // 阈值
        private const double Threshold = 10.0;
        private static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.RegisterAttached(
                "_ScrollViewer",
                typeof(ScrollViewer),
                typeof(AutoScrollBehavior));

        public static void SetSmartAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(SmartAutoScrollProperty, value);
        }

        public static bool GetSmartAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(SmartAutoScrollProperty);
        }

        private static void OnSmartAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListView listView && (bool)e.NewValue)
            {
                listView.Loaded += (s, ev) =>
                {
                    var scrollViewer = FindScrollViewer(listView);
                    if (scrollViewer != null)
                    {
                        listView.SetValue(ScrollViewerProperty, scrollViewer);
                        // 监听列表项变化
                        var itemsSource = listView.Items;
                        if (itemsSource is INotifyCollectionChanged notifyCollection)
                        {
                            notifyCollection.CollectionChanged += (sender, args) =>
                            {
                                if (args.Action == NotifyCollectionChangedAction.Add && IsAtBottom(scrollViewer))
                                {
                                    listView.Dispatcher.BeginInvoke(new Action(() =>
                                    {
                                        if (listView.Items.Count > 0)
                                        {
                                            listView.ScrollIntoView(listView.Items[listView.Items.Count - 1]);
                                        }
                                    }));
                                }
                            };
                        }
                    }
                };
            }
        }

        // 判断是否在底部
        private static bool IsAtBottom(ScrollViewer scrollViewer)
        {
            return scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - Threshold;
        }

        // 查找ScrollViewer
        private static ScrollViewer FindScrollViewer(DependencyObject obj)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is ScrollViewer sv) return sv;
                var result = FindScrollViewer(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}