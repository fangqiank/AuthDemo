using AuthDemoAgain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemoAgain.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IAuthorizationService _authz;

        public DocumentsController(IAuthorizationService authz) => _authz = authz;

        // 资源型授权：逐篇文档把资源实例交给策略引擎判定，端点里没有 Owner 比对逻辑
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var visible = new List<Document>();
            foreach (var doc in MockDocumentStore.Documents)
                if ((await _authz.AuthorizeAsync(User, doc, "DocumentOwner")).Succeeded)
                    visible.Add(doc);
            return Ok(visible);
        }

        [HttpGet("{title}")]
        [Authorize]
        public async Task<IActionResult> Get(string title)
        {
            var doc = MockDocumentStore.Documents.FirstOrDefault(d => d.Title == title);
            if (doc == null) return NotFound();

            if (!(await _authz.AuthorizeAsync(User, doc, "DocumentOwner")).Succeeded)
                return Forbid();

            return Ok(doc);
        }
    }
}
