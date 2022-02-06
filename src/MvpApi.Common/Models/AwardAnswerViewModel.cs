namespace MvpApi.Common.Models
{
	public class AwardAnswerViewModel
	{

		/// <summary>
		/// Gets or sets the key of the question
		/// </summary>
		[Newtonsoft.Json.JsonProperty(PropertyName = "AwardQuestionId")]
		public string AwardQuestionId { get; set; }

		/// <summary>
		/// Gets or sets answer for the given award question
		/// </summary>
		[Newtonsoft.Json.JsonProperty(PropertyName = "Answer")]
		public string Answer { get; set; }
	}
}