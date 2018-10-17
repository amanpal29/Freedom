using System;

namespace Freedom.FullTextSearch
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IndexHintAttribute : Attribute
    {
        public static readonly IndexHintAttribute Default = new IndexHintAttribute(IndexHints.None);

        public IndexHintAttribute(IndexHints indexHints)
        {
            IndexHints = indexHints;
        }

        public IndexHints IndexHints { get;  }
    }
}