using System;

namespace MvpApi.Services.Utilities
{
    public static class ServiceConstants
    {
        public static DateTime SubmissionStartDate { get; set; } = new DateTime(DateTime.Now.Year, 6, 1);

        public static DateTime SubmissionDeadline { get; set; } = new DateTime(DateTime.Now.Year + 1, 4, 1);
    }
}
