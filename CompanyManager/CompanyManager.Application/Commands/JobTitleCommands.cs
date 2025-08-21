namespace CompanyManager.Application.Commands
{
    public class CreateJobTitleCommand
    {
        public string Name { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateJobTitleCommand
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteJobTitleCommand
    {
        public Guid Id { get; set; }
    }
}









