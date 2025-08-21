using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Common;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class JobTitlesController : ControllerBase
    {
        private readonly IListJobTitlesQueryHandler _listJobTitlesQueryHandler;
        private readonly IGetJobTitleByIdQueryHandler _getJobTitleByIdQueryHandler;

        public JobTitlesController(
            IListJobTitlesQueryHandler listJobTitlesQueryHandler,
            IGetJobTitleByIdQueryHandler getJobTitleByIdQueryHandler)
        {
            _listJobTitlesQueryHandler = listJobTitlesQueryHandler;
            _getJobTitleByIdQueryHandler = getJobTitleByIdQueryHandler;
        }

        /// <summary>
        /// Lista todos os cargos disponíveis
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PageResult<JobTitleResponse>>> List([FromQuery] ListJobTitlesRequest request)
        {
            try
            {
                var result = await _listJobTitlesQueryHandler.Handle(request, CancellationToken.None);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um cargo específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<JobTitleResponse>> GetById(Guid id)
        {
            try
            {
                var request = new GetJobTitleByIdRequest { Id = id };
                var result = await _getJobTitleByIdQueryHandler.Handle(request, CancellationToken.None);
                
                if (result == null)
                    return NotFound(new { error = "Cargo não encontrado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }



        /// <summary>
        /// Lista cargos disponíveis para criação de usuários (baseado na hierarquia do usuário atual)
        /// </summary>
        [HttpGet("available-for-creation")]
        public async Task<ActionResult<IEnumerable<JobTitleResponse>>> GetAvailableForCreation()
        {
            try
            {
                // TODO: Implementar lógica de permissão hierárquica
                // Por enquanto, retorna todos os cargos ativos
                var request = new ListJobTitlesRequest { IsActive = true };
                var result = await _listJobTitlesQueryHandler.Handle(request, CancellationToken.None);
                return Ok(result.Items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
