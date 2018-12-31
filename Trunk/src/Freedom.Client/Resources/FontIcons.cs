using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Freedom.Client.Resources
{
    public static class FontIcons
    {
        public const string IconMissingColorCode = "#FFD30128";
        public const string IconMissingIcon = "\ue000";
        public const string IconMissingIconColor = IconMissingColorCode + IconMissingIcon;

        public const string CustomIcon = IconMissingIcon;
        public const string HedgehogIcon = "\ue270";
        public const string HedgehogSolidIcon = "\ue271";
        public const string HedgehogIconColor = "#FF783428\ue272#FFF7C386\ue273#FFB36743\ue274#FF000000\ue275";

        /// <summary>
        /// Gets the single char glyph of the monochrome equivilent of most color font icons.
        /// </summary>
        public static string GetMonochrome(string fontIconColor)
        {
            if (string.IsNullOrEmpty(fontIconColor))
                return fontIconColor;

            foreach (char c in fontIconColor)
            {
                if ('\ue000' <= c && c <= '\uefff')
                    return new string((char)(c & 0xfff8), 1);

                if ('\uf000' <= c && c <= '\uf8ff')
                    return new string(c, 1);
            }

            return IconMissingIcon;
        }

        
        //done
        #region Application Wide Icons

        public const string SaveIcon = "\ue5d0";
        public const string SaveIconColor = "#FFD30128\ue5d1#FF0068B4\ue5d2#FFF1F1F1\ue5d3#FFFFFFFF\ue5d4#FFA0A0A0\ue5d5#FF0722A5\ue5d6";

        public const string SaveAndCloseIcon = "\ue5d0";
        public const string SaveAndCloseIconColor = "#FFD30128\ue5d1#FF0068B4\ue5d2#FFF1F1F1\ue5d3#FFFFFFFF\ue5d4#FFA0A0A0\ue5d5#FF0722A5\ue5d6";

        public const string SaveAndPrintIcon = "\ue5d8";
        public const string SaveAndPrintIconColor = "#FF0068B4\ue5d9#FF2B8AD9\ue5da#FF5E5E60\ue5db#FFF1F1F1\ue5dc#FFFFFFFF\ue5dd#FFA0A0A0\ue5de#FF0722A5\ue5df";

        public const string SaveAsDraftIcon = "\ue5e0";
        public const string SaveAsDraftIconColor = "#FF5E5E60\ue5e1#FF0068B4\ue5e2#FFF1F1F1\ue5e3#FFFFFFFF\ue5e4#FFA0A0A0\ue5e5#FF0722A5\ue5e6";

        public const string PrintIcon = "\ue2d0";
        public const string PrintIconColor = "#FFF1F1F1\ue2d1#FF5E5E60\ue2d2#FF2B8AD9\ue2d3";

        public const string ClosePreviewIcon = "\ue0e0";
        public const string ClosePreviewIconColor = "#FFD30128\ue0e0";

        public const string LowPriorityIcon = ArrowDownIcon;
        public const string LowPriorityIconColor = "#FF008000" + LowPriorityIcon;

        public const string HighPriorityIcon = ExclamationIcon;
        public const string HighPriorityIconColor = "#FFFF0000" + HighPriorityIcon;

        public const string ConfidentialLogIcon = "\ue2d8";
        public const string ConfidentialLogIconColor = "#FFFFFFFF\ue2d9#FFA0A0A0\ue2da#FF648C25\ue2db#FFF0CA43\ue2dc#FF5E5E60\ue2dd";

        public const string ConfidentialShowIcon = ViewIcon;
        public const string ConfidentialShowIconColor = ViewIconColor;

        public const string DraftIcon = "\ue520";
        public const string DraftIconColor = "#FFF1F1F1\ue521#FF5E5E60\ue522";

        public const string MapIcon = "\ue200";
        public const string MapIconColor = "#FFA0A0A0\ue201#FFF34C44\ue202#FF5C1410\ue203";

        public const string MapWithWaterIconColor = "#FFA0A0A0\ue201#FFF34C44\ue202#FF5C1410\ue203#FF2B8AD9\ue204#FFBAD3E7\ue205";

        public const string MoreEllipsesIcon = "\ue001";
        public const string MoreEllipsesIconColor = ListColorCode + MoreEllipsesIcon;

        public const string PasteIcon = "\ue578";
        public const string PasteIconColor = "#FFAF7D01\ue578#FFF3E400\ue579#FF5BACE4\ue57a";

        public const string PickEffectiveDateIcon = "\ue330";
        public const string PickEffectiveDateIconColor = "#FFF1F1F1\ue331#FF0068B4\ue333#FFA0A0A0\ue332#FFD30128\ue334";

        public const string RefreshIcon = "\ue002";
        public const string RefreshIconColor = "#FF97D32F" + RefreshIcon;

        public const string VoidIcon = "\ue540";
        public const string VoidIconColor = "#FFD30128\ue541";

        public const string WarningIcon = "\ue380";
        public const string WarningIconColor = "#FFFAC845\ue381#FF000000\ue382";

        public const string SortBySubjectIcon = "\ue3f8";
        public const string SortBySubjectIconColor = "#FF8F7330\ue3f9#FF208EA2\ue3fa#FF93251D\ue3fb#FFFFFFFF\ue3fc";

        public const string SortByDateIcon = "\ue3f0";
        public const string SortByDateIconColor = "#FF000000\ue3f1#FFFFFFFF\ue3f2#FFC62F32\ue3f3#FF0C0C0C\ue3f4";

        public const string AddIcon = "\ue0d0";
        public const string ArrowDownIcon = "\ue150";
        public const string ArrowLeftIcon = "\ue170";
        public const string ArrowRightIcon = "\ue160";
        public const string ArrowUpIcon = "\ue140";
        public const string AutomatedIcon = "\ue180";
        public const string BinocularsIcon = "\uf01a";
        public const string CheckmarkIcon = "\ue1a0";
        public const string CheckmarkInGridIcon = CheckmarkIcon;
        public const string ConfirmIcon = ThumbsUpIcon;
        public const string ConnectivityIcon = "\ue210";
        public const string CopyIcon = "\uf014";
        public const string ClipboardIcon = "\uf01d";
        public const string CutIcon = "\uf074";
        public const string DeleteIcon = "\ue0e0";
        public const string DownloadIcon = ArrowDownIcon;
        public const string EditIcon = "\ue0f0";
        public const string EducationIcon = "\uf069";
        public const string EmailIcon = "\uf01e";
        public const string EraseIcon = "\uf01f";
        public const string ErrorIcon = "\uf020";
        public const string ExclamationIcon = "\uf010";
        public const string ExportIcon = ArrowUpIcon;
        public const string FileIcon = "\uf076";
        public const string FontGroupIcon = "\uf066";
        public const string GoogleMapIcon = MapIcon;
        public const string GridNoteIcon = "\ue240";
        public const string HashtagIcon = "\uf021";
        public const string HelpIcon = "\uf05c";
        public const string ImportIcon = ArrowDownIcon;
        public const string InformationIcon = "\uf022";
        public const string InternetIcon = "\uf023";
        public const string LinkIcon = "\ue120";
        public const string ListIcon = "\ue110";
        public const string MinusSignIcon = "\uf024";
        public const string PaperclipIcon = "\uf025";
        public const string ParagraphIcon = "\uf067";
        public const string PhoneIcon = "\uf026";
        public const string PhoneInternetIcon = "\uf027";
        public const string PlusSignIcon = AddIcon;
        public const string QuillIcon = "\uf04f";
        public const string RemoveIcon = DeleteIcon;
        public const string ResolveIcon = "\ue190";
        public const string ReassignIcon = ArrowRightIcon;
        public const string ReceiveIcon = ArrowDownIcon;
        public const string ReplaceIcon = ArrowLeftIcon;
        public const string ThumbsDownIcon = "\uf01c";
        public const string ThumbsUpIcon = "\uf01b";
        public const string TimeIcon = "\uf028";
        public const string TrashIcon = "\uf06e";
        public const string TriangleDownIcon = "\uf02a";
        public const string TriangleLeftIcon = "\uf02b";
        public const string TriangleRightIcon = "\uf02d";
        public const string TriangleUpIcon = "\uf02c";
        public const string UnlinkIcon = "\ue130";
        public const string ViewIcon = "\ue100";
        public const string WebLinkIcon = LinkIcon;
        public const string WindowWithArrowIcon = "\uf029";
        public const string WorkIcon = "\uf068";
        public const string UndoIcon = "\uf058";

        #endregion

        #region Administration Icons

        public const string ConfigurationIcon = "\ue378";
        public const string ConfigurationIconColor = "#FF97D32F\ue379#FF2B8AD9\ue37a#FFFFFFFF\ue37b";

        #endregion

        //done
        #region Backstage Icons

        public const string ChangePasswordIcon = "\ue348";
        public const string ChangePasswordIconColor = "#FFFAC845\ue349#FFCBA02F\ue34a#FF2B8AD9\ue34b";

        public const string ControlPanelIcon = ConfigurationIcon;
        public const string ControlPanelIconColor = ConfigurationIconColor;

        public const string ExitIcon = DeleteIcon;
        public const string ExitIconColor = DeleteColorCode + DeleteIcon;

        public const string FeedbackIcon = "\ue350";
        public const string FeedbackIconColor = "#FF0068B4\ue351#FF0722A5\ue352#FFF1F1F1\ue353#FF5E5E60\ue354";

        #endregion

        
        //done
        #region Contact Icons

        public const string CautionFlagIcon = "\uf033";

        public const string CompanyIcon = "\ue320";
        public const string CompanyIconColor = "#FF97d32f\ue321#FFA0A0A0\ue322#FFBAD3E7\ue323#FF5E5E60\ue324#FF7C7C7C\ue325";

        public const string ContactIcon = "\ue040";
        public const string ContactIconColor = "#FFDCDCC5\ue041#FFD30128\ue042";

        public const string FamilyIcon = "\ue318";
        public const string FamilyIconColor = "#FF603813\ue319#FFD30128\ue31a";

        public const string FemaleThumbnailIcon = "\uf035";

        public const string MaleThumbnailIcon = "\uf034";

        public const string MergeContactsIcon = "\ue5b0";
        public const string MergeContactsIconColor = "#FF0068B4\ue5b1#FF000000\ue5b2#FFD30128\ue5b3#FFFFFFFF\ue5b4";

        public const string NeutralThumbnailIcon = "\uf036";

        public const string VCardIcon = "\ue328";
        public const string VCardIconColor = "#FFF1F1F1\ue329#FFD30128\ue32a#FFA0A0A0\ue32b";

        public const string VCardImportIcon = "\ue5b8";
        public const string VCardImportIconColor = "#FF76A797\ue5b9#FFA0A0A0\ue5ba#FFF1F1F1\ue5bb#FFDCDCC5\ue5bc#FFD30128\ue5bd";

        public const string VCardExportIcon = "\ue5c0";
        public const string VCardExportIconColor = "#FF4D82B8\ue5c1#FFA0A0A0\ue5c2#FFF1F1F1\ue5c3#FFDCDCC5\ue5c4#FFD30128\ue5c5";

        #endregion

        
        //done
        #region Dashboard Icons

        public const string DashboardIcon = "\ue020";
        public const string DashboardIconColor = "#FF0068B4\ue021#FFF7931E\ue022#FF97D32F\ue023";

        public const string DashboardWithCogIconColor = "#FF0068B4\ue021#FFF7931E\ue022#FF97D32F\ue023#FF000000\ue024";

        public const string DashboardLayoutIconColor = "#FFF7931E\ue025#FF97D32F\ue026#FF0068B4\ue027";

        public const string ApplyDashboardLayoutIconColor = "#FFF7931E\ue025#FF97D32F\ue026#FF0068B4\ue027#FF293F09\ue268";

        public const string FactoryDefaultDashboardLayoutIconColor = "#FFF7931E\ue025#FF97D32F\ue026#FF0068B4\ue027#FFB36743\ue269";

        public const string ActionPlansIcon = "\uf017";
        public const string BarChartIcon = "\uf013";
        public const string CogIcon = "\uf012";
        public const string NewItemsIcon = "\uf018";
        public const string NotificationIcon = "\uf019";
        public const string OpenItemsIcon = "\uf016";
        public const string OverdueItemsIcon = "\uf011";
        public const string UpcomingItemsIcon = "\uf015";

        #endregion

        
        #region Email Templates Icon

        public const string EmailTemplatesIcon = "\ue5a8";
        public const string EmailTemplatesIconColor = "#FF5E5E60\ue5a9#FFF1F1F1\ue5aa#FF5E5E60\ue5ab#FF0068B4\ue5ac";

        #endregion

        
        //done
        #region Navigation Icons

        public const string FirstIcon = "\uf049";
        public const string RewindIcon = "\uf04a";
        public const string PreviousIcon = "\uf04b";
        public const string NextIcon = "\uf04c";
        public const string ForwardIcon = "\uf04d";
        public const string LastIcon = "\uf04e";

        #endregion

        
        //done
        #region Overlays Icons

        // Add New Icon
        public const string AddColorCode = "#FF293f09";
        public const string AddIconColor = AddColorCode + AddIcon; // big version
        public const string TLAddIcon = "\ue0d1"; // Top Left Third
        public const string TCAddIcon = "\ue0d2"; // Top Center Third
        public const string TRAddIcon = "\ue0d3"; // Top Right Third
        public const string CLAddIcon = "\ue0d4"; // Center Left Third
        public const string CCAddIcon = "\ue0d5"; // Center Center Third
        public const string CRAddIcon = "\ue0d6"; // Center Right Third
        public const string BLAddIcon = "\ue0d7"; // Bottom Left Third
        public const string BCAddIcon = "\ue0d8"; // Bottom Center Third
        public const string BRAddIcon = "\ue0d9"; // Bottom Right Third

        // Delete Icon
        public const string DeleteColorCode = "#FFD30128";
        public const string DeleteIconColor = DeleteColorCode + DeleteIcon; // big version
        public const string TLDeleteIcon = "\ue0e1"; // Top Left Third
        public const string TCDeleteIcon = "\ue0e2"; // Top Center Third
        public const string TRDeleteIcon = "\ue0e3"; // Top Right Third
        public const string CLDeleteIcon = "\ue0e4"; // Center Left Third
        public const string CCDeleteIcon = "\ue0e5"; // Center Center Third
        public const string CRDeleteIcon = "\ue0e6"; // Center Right Third
        public const string BLDeleteIcon = "\ue0e7"; // Bottom Left Third
        public const string BCDeleteIcon = "\ue0e8"; // Bottom Center Third
        public const string BRDeleteIcon = "\ue0e9"; // Bottom Right Third

        // Edit Icon
        public const string EditColorCode = "#FF580389";
        public const string EditIconColor = EditColorCode + EditIcon; // big version
        public const string TLEditIcon = "\ue0f1"; // Top Left Third
        public const string TCEditIcon = "\ue0f2"; // Top Center Third
        public const string TREditIcon = "\ue0f3"; // Top Right Third
        public const string CLEditIcon = "\ue0f4"; // Center Left Third
        public const string CCEditIcon = "\ue0f5"; // Center Center Third
        public const string CREditIcon = "\ue0f6"; // Center Right Third
        public const string BLEditIcon = "\ue0f7"; // Bottom Left Third
        public const string BCEditIcon = "\ue0f8"; // Bottom Center Third
        public const string BREditIcon = "\ue0f9"; // Bottom Right Third

        // View Icon
        public const string ViewColorCode = "#FF0722A5";
        public const string ViewIconColor = ViewColorCode + ViewIcon; // big version
        public const string TLViewIcon = "\ue101"; // Top Left Third
        public const string TCViewIcon = "\ue102"; // Top Center Third
        public const string TRViewIcon = "\ue103"; // Top Right Third
        public const string CLViewIcon = "\ue104"; // Center Left Third
        public const string CCViewIcon = "\ue105"; // Center Center Third
        public const string CRViewIcon = "\ue106"; // Center Right Third
        public const string BLViewIcon = "\ue107"; // Bottom Left Third
        public const string BCViewIcon = "\ue108"; // Bottom Center Third
        public const string BRViewIcon = "\ue109"; // Bottom Right Third

        // List Icon
        public const string ListColorCode = "#FF000000";
        public const string ListIconColor = ListColorCode + ListIcon; // big version
        public const string TLListIcon = "\ue111"; // Top Left Third
        public const string TCListIcon = "\ue112"; // Top Center Third
        public const string TRListIcon = "\ue113"; // Top Right Third
        public const string CLListIcon = "\ue114"; // Center Left Third
        public const string CCListIcon = "\ue115"; // Center Center Third
        public const string CRListIcon = "\ue116"; // Center Right Third
        public const string BLListIcon = "\ue117"; // Bottom Left Third
        public const string BCListIcon = "\ue118"; // Bottom Center Third
        public const string BRListIcon = "\ue119"; // Bottom Right Third

        // Link Icon
        public const string LinkColorCode = "#FF5E5E60";
        public const string LinkIconColor = LinkColorCode + LinkIcon; // big version
        public const string TLLinkIcon = "\ue121"; // Top Left Third
        public const string TCLinkIcon = "\ue122"; // Top Center Third
        public const string TRLinkIcon = "\ue123"; // Top Right Third
        public const string CLLinkIcon = "\ue124"; // Center Left Third
        public const string CCLinkIcon = "\ue125"; // Center Center Third
        public const string CRLinkIcon = "\ue126"; // Center Right Third
        public const string BLLinkIcon = "\ue127"; // Bottom Left Third
        public const string BCLinkIcon = "\ue128"; // Bottom Center Third
        public const string BRLinkIcon = "\ue129"; // Bottom Right Third

        // Unlink Icon
        public const string UnLinkColorCode = "#FFD30128";
        public const string UnLinkIconColor = UnLinkColorCode + UnlinkIcon; // big version
        public const string TLUnlinkIcon = "\ue131"; // Top Left Third
        public const string TCUnlinkIcon = "\ue132"; // Top Center Third
        public const string TRUnlinkIcon = "\ue133"; // Top Right Third
        public const string CLUnlinkIcon = "\ue134"; // Center Left Third
        public const string CCUnlinkIcon = "\ue135"; // Center Center Third
        public const string CRUnlinkIcon = "\ue136"; // Center Right Third
        public const string BLUnlinkIcon = "\ue137"; // Bottom Left Third
        public const string BCUnlinkIcon = "\ue138"; // Bottom Center Third
        public const string BRUnlinkIcon = "\ue139"; // Bottom Right Third

        // ArrowUp Icon - Export Up Overlays
        public const string ArrowUpColorCode = "#FF751b03";
        public const string ArrowUpIconColor = ArrowUpColorCode + ArrowUpIcon; // big version
        public const string TLArrowUpIcon = "\ue141"; // Top Left Third
        public const string TCArrowUpIcon = "\ue142"; // Top Center Third
        public const string TRArrowUpIcon = "\ue143"; // Top Right Third
        public const string CLArrowUpIcon = "\ue144"; // Center Left Third
        public const string CCArrowUpIcon = "\ue145"; // Center Center Third
        public const string CRArrowUpIcon = "\ue146"; // Center Right Third
        public const string BLArrowUpIcon = "\ue147"; // Bottom Left Third
        public const string BCArrowUpIcon = "\ue148"; // Bottom Center Third
        public const string BRArrowUpIcon = "\ue149"; // Bottom Right Third

        // ArrowDown Icon - Import Receive Down Overlays
        public const string ArrowDownColorCode = "#FF293f09";
        public const string ArrowDownIconColor = ArrowDownColorCode + ArrowDownIcon; // big version
        public const string TLArrowDownIcon = "\ue151"; // Top Left Third
        public const string TCArrowDownIcon = "\ue152"; // Top Center Third
        public const string TRArrowDownIcon = "\ue153"; // Top Right Third
        public const string CLArrowDownIcon = "\ue154"; // Center Left Third
        public const string CCArrowDownIcon = "\ue155"; // Center Center Third
        public const string CRArrowDownIcon = "\ue156"; // Center Right Third
        public const string BLArrowDownIcon = "\ue157"; // Bottom Left Third
        public const string BCArrowDownIcon = "\ue158"; // Bottom Center Third
        public const string BRArrowDownIcon = "\ue159"; // Bottom Right Third

        // ArrowRight Icon - Reassign Transfer Quick Overlays
        public const string ArrowRightColorCode = "#FFa58d00";
        public const string ArrowRightIconColor = ArrowRightColorCode + ArrowRightIcon; // big version
        public const string TLArrowRightIcon = "\ue161"; // Top Left Third
        public const string TCArrowRightIcon = "\ue162"; // Top Center Third
        public const string TRArrowRightIcon = "\ue163"; // Top Right Third
        public const string CLArrowRightIcon = "\ue164"; // Center Left Third
        public const string CCArrowRightIcon = "\ue165"; // Center Center Third
        public const string CRArrowRightIcon = "\ue166"; // Center Right Third
        public const string BLArrowRightIcon = "\ue167"; // Bottom Left Third
        public const string BCArrowRightIcon = "\ue168"; // Bottom Center Third
        public const string BRArrowRightIcon = "\ue169"; // Bottom Right Third

        // ArrowLeft Icon - Reverse Revoke Return Replace Overlays
        public const string ArrowLeftColorCode = "#FFD30128";
        public const string ArrowLeftIconColor = ArrowLeftColorCode + ArrowLeftIcon; // big version
        public const string TLArrowLeftIcon = "\ue171"; // Top Left Third
        public const string TCArrowLeftIcon = "\ue172"; // Top Center Third
        public const string TRArrowLeftIcon = "\ue173"; // Top Right Third
        public const string CLArrowLeftIcon = "\ue174"; // Center Left Third
        public const string CCArrowLeftIcon = "\ue175"; // Center Center Third
        public const string CRArrowLeftIcon = "\ue176"; // Center Right Third
        public const string BLArrowLeftIcon = "\ue177"; // Bottom Left Third
        public const string BCArrowLeftIcon = "\ue178"; // Bottom Center Third
        public const string BRArrowLeftIcon = "\ue179"; // Bottom Right Third

        // Automated Icon
        public const string AutomatedColorCode = "#FF293f09";
        public const string AutomatedIconColor = AutomatedColorCode + AutomatedIcon; // big version
        public const string TLAutomatedIcon = "\ue181"; // Top Left Third
        public const string TCAutomatedIcon = "\ue182"; // Top Center Third
        public const string TRAutomatedIcon = "\ue183"; // Top Right Third
        public const string CLAutomatedIcon = "\ue184"; // Center Left Third
        public const string CCAutomatedIcon = "\ue185"; // Center Center Third
        public const string CRAutomatedIcon = "\ue186"; // Center Right Third
        public const string BLAutomatedIcon = "\ue187"; // Bottom Left Third
        public const string BCAutomatedIcon = "\ue188"; // Bottom Center Third
        public const string BRAutomatedIcon = "\ue189"; // Bottom Right Third

        // Resolve Icon
        public const string ResolveColorCode = "#FFDFAC6F";
        public const string ResolveIconColor = ResolveColorCode + ResolveIcon; // big version
        public const string TLResolveIcon = "\ue191"; // Top Left Third
        public const string TCResolveIcon = "\ue192"; // Top Center Third
        public const string TRResolveIcon = "\ue193"; // Top Right Third
        public const string CLResolveIcon = "\ue194"; // Center Left Third
        public const string CCResolveIcon = "\ue195"; // Center Center Third
        public const string CRResolveIcon = "\ue196"; // Center Right Third
        public const string BLResolveIcon = "\ue197"; // Bottom Left Third
        public const string BCResolveIcon = "\ue198"; // Bottom Center Third
        public const string BRResolveIcon = "\ue199"; // Bottom Right Third

        // Checkmark Icon
        public const string CheckmarkColorCode = "#FF293F09";
        public const string CheckmarkIconColor = CheckmarkColorCode + CheckmarkIcon; // big version
        public const string TLCheckmarkIcon = "\ue1a1"; // Top Left Third
        public const string TCCheckmarkIcon = "\ue1a2"; // Top Center Third
        public const string TRCheckmarkIcon = "\ue1a3"; // Top Right Third
        public const string CLCheckmarkIcon = "\ue1a4"; // Center Left Third
        public const string CCCheckmarkIcon = "\ue1a5"; // Center Center Third
        public const string CRCheckmarkIcon = "\ue1a6"; // Center Right Third
        public const string BLCheckmarkIcon = "\ue1a7"; // Bottom Left Third
        public const string BCCheckmarkIcon = "\ue1a8"; // Bottom Center Third
        public const string BRCheckmarkIcon = "\ue1a9"; // Bottom Right Third

        #endregion
        
        //done
        #region Service Provider Icons

        public const string ServiceProviderIcon = "\ue0c0";
        public const string ServiceProviderIconColor = "#FFFFFFFF\ue0c1#FFA0A0A0\ue0c2#FF648C25\ue0c3#FFF0CA43\ue0c4";

        public const string UserIcon = ServiceProviderIcon;
        public const string UserIconColor = ServiceProviderIconColor;

        #endregion
                
    }
}
