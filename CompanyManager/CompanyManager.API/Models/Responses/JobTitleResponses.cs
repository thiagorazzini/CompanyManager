using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    /// <summary>
    /// Response containing a list of available job titles
    /// </summary>
    public class JobTitlesResponse : BaseResponse
    {
        /// <summary>
        /// List of available job titles
        /// </summary>
        public IEnumerable<string> JobTitles { get; set; } = Enumerable.Empty<string>();
        
        /// <summary>
        /// Total count of unique job titles
        /// </summary>
        public int Total { get; set; }
    }

    /// <summary>
    /// Response for a single job title
    /// </summary>
    public class JobTitleResponse : BaseResponse
    {
        /// <summary>
        /// The job title value
        /// </summary>
        public string JobTitle { get; set; } = string.Empty;
        
        /// <summary>
        /// Number of employees with this job title
        /// </summary>
        public int EmployeeCount { get; set; }
    }
}
