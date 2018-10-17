using System.Collections.Specialized;

namespace Freedom.Extensions
{
    public static class NotifyCollectionChangedEventArgsExtensions
    {
        public static bool Contains(this NotifyCollectionChangedEventArgs args, object item)
        {
            return args.Action == NotifyCollectionChangedAction.Reset ||
                   (args.NewItems != null && args.NewItems.Contains(item)) ||
                   (args.OldItems != null && args.OldItems.Contains(item));
        }
    }
}
