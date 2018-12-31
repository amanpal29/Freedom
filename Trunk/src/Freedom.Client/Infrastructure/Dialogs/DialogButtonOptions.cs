using System;

namespace Freedom.Client.Infrastructure.Dialogs
{
    [Flags]
    public enum DialogButtonOptions
    {
        None = 0x0000,

        IsDefault = 0x0001,

        IsCancel = 0x0002,

        /// <summary>
        /// Button does something that is destructive and not easily reversable (eg. Close without saving)
        /// and should therefore be rendered away from "safe" buttons.
        /// </summary>
        IsDangerous = 0x0004,

        /// <summary>
        /// Normally when an async DialogButtonViewModel is executing, 
        /// It blocks (i.e. disables) all the other buttons on the dialog.
        /// If this option is set, it only blocks itself.
        /// </summary>
        IsNonBlocking = 0x0008
    }
}