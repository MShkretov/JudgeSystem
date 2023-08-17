using JudgeSystem.Contracts;
using JudgeSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace JudgeSystem.Controllers
{
    public class CodeController : Controller
    {
        private readonly ICodeService _codeService;

        public CodeController(ICodeService codeService)
        {
            _codeService = codeService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var tasks = _codeService.GetTasks();
            return View(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> RunCode(int taskIndex, string userCode, string input)
        {
            var result = await _codeService.RunCodeAndCheckOutputAsync(taskIndex, userCode, input);
            return Json(result);
        }
    }
}