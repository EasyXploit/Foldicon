// Type with the settings of the app
namespace Foldicon.Core.Storage;

public sealed record AppSettings(bool RepairAtStartup = true, bool RepairNoticeShown = false);
