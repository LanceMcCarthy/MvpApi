using CommonHelpers.Common;

namespace MvpApi.Common.Models
{
    /// <summary>
    /// A model to hold both the Award Consideration question and answer for rendering in the UI form
    /// </summary>
    public class QuestionnaireItem : BindableBase
    {
        private AwardConsiderationQuestionModel _questionItem;
        private AwardConsiderationAnswerModel _answerItem;

        /// <summary>
        /// Gets or sets the Question object.
        /// </summary>
        public AwardConsiderationQuestionModel QuestionItem
        {
            get => _questionItem;
            set => SetProperty(ref _questionItem, value);
        }

        /// <summary>
        /// Gets or sets the Answer object.
        /// </summary>
        public AwardConsiderationAnswerModel AnswerItem
        {
            get => _answerItem;
            set => SetProperty(ref _answerItem, value);
        }
    }
}