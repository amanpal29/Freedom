using System.ComponentModel;

namespace Freedom.Domain.Model
{
    public enum AutoNumberMode
    {
        [Description("Don't auto-number")]
        Never = 0,

        [Description("When starting the workflow")]
        OnCreate = 0x1,     // Client side, when starting the workflow

        // OnSave = 0x2,    // Client side, when saving the workflow for the first time  // reserverd for future use.

        [Description("When the workflow is saved back to the server")]
        OnCommit = 0x4      // Server side, when committed to the server
    }
}
