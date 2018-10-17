using System.ComponentModel;

namespace Freedom.TextBuilder
{
    public enum TextBuilderErrorCode
    {
        [Browsable(false)]
        NoError = -1,

        [Description("An unknown error occurred")]
        Unknown = 0,

        #region Format String Errors - The format string has invalid characters

        [Browsable(false)]
        FormatStringErrors = 0x10000,

        [Description("Opening brace without closing brace")]
        OpeningBraceWithoutClosingBrace,

        [Description("Closing brace without opening brace")]
        ClosingBraceWithoutOpeningBrace,

        [Description("Empty braces")]
        EmptyBraces,

        [Description("Empty variable format string")]
        EmptyFormat,

        [Description("The name of this variable is invalid")]
        BadPath,

        [Description("Argument referenced by index was not found.")]
        ArgumentNotFound,

        #endregion

        #region Format String Warnings - The format string doesn't match the type hierarchy

        [Browsable(false)]
        FormatStringWarnings = 0x20000,

        [Description("This variable was not found")]
        MemberNotFound,

        [Description("There is more than one variable by this name")]
        AmbiguousMatch,

        [Description("Attempted to access by index the value of an argument that isn't enumerable")]
        AccessByIndexOfNonEnumerableType,

        #endregion

        #region Run Time Errors - Couldn't resolve the variable at run time

        [Browsable(false)]
        RunTimeErrors = 0x40000,

        [Description("An exception occured while trying to format the variable")]
        FormatException,

        [Description("The value of an object was null")]
        NullReference,

        [Description("Attempted to access a variable by index with an invalid index")]
        ArgumentOutOfRange

        #endregion
    }
}