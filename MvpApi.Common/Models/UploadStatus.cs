namespace MvpApi.Common.Models
{
    /// <summary>
    /// Used to denote an object's upload status to the MVP API. An example can be found in ContributionsModel.cs
    /// </summary>
    public enum UploadStatus
    {
        Pending,
        InProgress,
        Success,
        Failed,
        None
    }
}