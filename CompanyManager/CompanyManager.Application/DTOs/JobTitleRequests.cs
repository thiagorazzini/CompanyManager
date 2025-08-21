using System.ComponentModel.DataAnnotations;
using CompanyManager.Application.Common;

namespace CompanyManager.Application.DTOs
{
    public class CreateJobTitleRequest
    {
        [Required(ErrorMessage = "Nome do cargo é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do cargo deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nível hierárquico é obrigatório")]
        [Range(1, 5, ErrorMessage = "Nível hierárquico deve estar entre 1 e 5")]
        public int HierarchyLevel { get; set; }

        [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }
    }

    public class UpdateJobTitleRequest
    {
        [Required(ErrorMessage = "Nome do cargo é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome do cargo deve ter no máximo 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nível hierárquico é obrigatório")]
        [Range(1, 5, ErrorMessage = "Nível hierárquico deve estar entre 1 e 5")]
        public int HierarchyLevel { get; set; }

        [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
        public string? Description { get; set; }
    }

    public class ListJobTitlesRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Name { get; set; }
        public int? HierarchyLevel { get; set; }
        public bool? IsActive { get; set; }
        
        public int PageSafe => Page < 1 ? 1 : Page;
        public int PageSizeSafe => PageSize < 1 ? 20 : PageSize;
        public int Offset => (PageSafe - 1) * PageSizeSafe;
        public int Take => PageSizeSafe;
    }

    public class GetJobTitleByIdRequest
    {
        [Required(ErrorMessage = "ID do cargo é obrigatório")]
        public Guid Id { get; set; }
    }
}
