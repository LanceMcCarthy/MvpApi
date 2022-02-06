namespace MvpApi.Common.Models
{
	public class AwardQuestionViewModel
	{

		/// <summary>
		/// Gets or sets the key of the question
		/// </summary>
		[Newtonsoft.Json.JsonProperty(PropertyName = "AwardQuestionId")]
		public string AwardQuestionId { get; set; }

		/// <summary>
		/// Gets or sets html description of the question
		/// </summary>
		[Newtonsoft.Json.JsonProperty(PropertyName = "QuestionContent")]
		public string QuestionContent { get; set; }

		/// <summary>
		/// Gets or sets flag if award consideration question is required
		/// </summary>
		[Newtonsoft.Json.JsonProperty(PropertyName = "Required")]
		public System.Boolean? Required { get; set; }
	}
}
