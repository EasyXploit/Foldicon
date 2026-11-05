// Type with the app settings
namespace Foldicon.Core.Storage;

public sealed record AppSettings(bool RepairAtStartup = true, bool RepairNoticeShown = false);
