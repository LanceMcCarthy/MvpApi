namespace MvpApi.Common.Models
{
    /// <summary>
    /// Model for the MVP's answers for award consideration questions.
    /// </summary>
    public class AwardConsiderationAnswerModel
    {
        /// <summary>
        /// Gets or sets the AwardQuestionId.
        /// </summary>
        public string AwardQuestionId { get; set; }

        /// <summary>
        /// Gets or sets the Answer.
        /// </summary>
        public string Answer { get; set; }
    }
}