namespace MvpApi.Common.Models
{
    /// <summary>
    /// Model for the MVP award consideration questions.
    /// </summary>
    public class AwardConsiderationQuestionModel
    {
        /// <summary>
        /// Gets or sets the AwardQuestionId.
        /// </summary>
        public string AwardQuestionId { get; set; }

        /// <summary>
        /// Gets or sets the Answer.
        /// </summary>
        public string QuestionContent { get; set; }

        /// <summary>
        /// Gets or sets IsRequired. This value determines if the question must be answered.
        /// </summary>
        public bool IsRequired { get; set; } = true;
    }
}
