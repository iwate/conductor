using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Conductor.Web.Models;
using Conductor.Core.Services;
using Conductor.Core.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Conductor.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IJobService _jobService;
        private readonly IJobRegistry _jobRegistry;
        private readonly IConfiguration _config;
        public HomeController(IJobService jobService, IJobRegistry jobRegistry, IConfiguration config)
        {
            _jobService = jobService;
            _jobRegistry = jobRegistry;
            _config = config;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            return View();
        }
        
        [HttpGet("/jobs")]
        public async Task<IActionResult> JobList()
        {
            var jobs = await _jobService.GetAsync();
            return View("List",jobs);
        }

        [HttpGet]
        public async Task<IActionResult> Exist([FromQuery]string name)
        {
            var job = await _jobService.FindAsync(name);
            return Json(job == null);
        }

        [HttpGet("/jobs/@form/create/@type/cron")]
        public IActionResult CronJobCreateForm()
        {
            return View(new CronJobViewModel());
        }

        [HttpPost("/jobs/@actions/create/@type/cron")]
        public async Task<IActionResult> CreateCronJob([FromForm]CronJobViewModel model)
        {
            var job = await _jobService.FindAsync(model.Name);
            if (job != null)
                ModelState.AddModelError(nameof(model.Name), "Conflict job name");

            if (!ModelState.IsValid)
                return View("CronJobCreateForm", model);

            await _jobService.AddAsync(new CronJobDefinition(model.Name)
            {
                Name = model.Name,
                Enabled = model.Enabled,
                OS = model.OS.ToString(),
                CPU = model.CPU,
                Memory = model.Memory,
                Private = model.Private,
                Image = model.Image,
                EnvVariables = model.EnvVariables,
                Cron = model.Cron
            });

            _jobRegistry.Enqueue(model.Name);

            return RedirectToAction("Definition", new { name = model.Name});
        }

        [HttpGet("/jobs/{name}/@form/update/@type/cron")]
        public async Task<IActionResult> CronJobUpdateForm([FromRoute]string name)
        {
            var job = await _jobService.FindAsync<CronJobDefinition>(name);
            if (job == null)
                return NotFound();

            return View(new CronJobViewModel
            { 
                Name = job.Name,
                Enabled = job.Enabled,
                OS = (OS)Enum.Parse(typeof(OS), job.OS),
                CPU = job.CPU,
                Memory = job.Memory,
                Private = job.Private,
                Image = job.Image,
                EnvVariables = job.EnvVariables,
                Cron = job.Cron
            });
        }

        [HttpPost("/jobs/{name}/@actions/update/@type/cron")]
        public async Task<IActionResult> UpdateCronJob([FromRoute]string name, [FromForm]CronJobViewModel model)
        {
            var job = await _jobService.FindAsync(name);
            if (job == null || job.GetJobType() != JobType.Cron)
                return NotFound();

            if (!ModelState.IsValid)
                return View("CronJobUpdateForm", model);

            await _jobService.UpdateAsync(new CronJobDefinition(model.Name)
            {
                Name = model.Name,
                Enabled = model.Enabled,
                OS = model.OS.ToString(),
                CPU = model.CPU,
                Memory = model.Memory,
                Private = model.Private,
                Image = model.Image,
                EnvVariables = model.EnvVariables,
                Cron = model.Cron
            });

            return RedirectToAction("Definition", new { name = model.Name});
        }

        [HttpGet("/jobs/@form/update/@type/queue")]
        public IActionResult QueueJobCreateForm()
        {
            return View(new QueueJobViewModel(){ CPU = 1});
        }

        [HttpPost("/jobs/@actions/create/@type/queue")]
        public async Task<IActionResult> CreateQueueJob([FromForm]QueueJobViewModel model)
        {
            var job = await _jobService.FindAsync(model.Name);
            if (job != null)
                ModelState.AddModelError(nameof(model.Name), "Conflict job name");

            if (!ModelState.IsValid)
                return View("QueueJobForm", model);
            
            await _jobService.AddAsync(new QueueJobDefinition(model.Name)
            {
                Name = model.Name,
                Enabled = model.Enabled,
                OS = model.OS.ToString(),
                CPU = model.CPU,
                Memory = model.Memory,
                Private = model.Private,
                Image = model.Image,
                EnvVariables = model.EnvVariables,
                Queue = model.Queue,
                ConnectionString = model.ConnectionString,
            });

            _jobRegistry.Enqueue(model.Name);            

            return RedirectToAction("Definition", new { name = model.Name});
        }

        [HttpGet("/jobs/{name}/@form/update/@type/queue")]
        public async Task<IActionResult> QueueJobUpdateForm([FromRoute]string name)
        {
            var job = await _jobService.FindAsync<QueueJobDefinition>(name);
            if (job == null)
                return NotFound();

            return View(new QueueJobViewModel
            { 
                Name = job.Name,
                Enabled = job.Enabled,
                OS = (OS)Enum.Parse(typeof(OS), job.OS),
                CPU = job.CPU,
                Memory = job.Memory,
                Private = job.Private,
                Image = job.Image,
                EnvVariables = job.EnvVariables,
                Queue = job.Queue,
                ConnectionString = job.ConnectionString,
            });
        }

        [HttpPost("/jobs/{name}/@actions/update/@type/queue")]
        public async Task<IActionResult> UpdateQueueJob([FromRoute]string name, [FromForm]QueueJobViewModel model)
        {
            var job = await _jobService.FindAsync(name);
            if (job == null || job.GetJobType() != JobType.Queue)
                return NotFound();
            
            if (!ModelState.IsValid)
                return View("QueueJobUpdateForm", model);
            
            await _jobService.UpdateAsync(new QueueJobDefinition(model.Name)
            {
                Name = model.Name,
                Enabled = model.Enabled,
                OS = model.OS.ToString(),
                CPU = model.CPU,
                Memory = model.Memory,
                Private = model.Private,
                Image = model.Image,
                EnvVariables = model.EnvVariables,
                Queue = model.Queue,
                ConnectionString = model.ConnectionString,
            });           

            return RedirectToAction("Definition", new { name = model.Name});
        }

        [HttpGet("/jobs/{name}")]
        public async Task<IActionResult> Definition([FromRoute]string name)
        {
            var def = await _jobService.FindAsync(name);
            if (def == null)
                return NotFound();

            return View(def);
        }

        [HttpGet("/jobs/{name}/history")]
        public async Task<IActionResult> History([FromRoute]string name, string skipToken = null, int limit = 10)
        {
            var jobs = await _jobService.GetHistoryAsync(name, skipToken, limit);
            return View(jobs);
        }

        [HttpGet("/jobs/{name}/history/{rowKey}")]
        public async Task<IActionResult> Result([FromRoute]string name, [FromRoute]string rowKey)
        {
            var result = await _jobService.FindResultAsync(name, rowKey);
            if (result == null)
                return NotFound();

            return View(new ResultViewModel
            {
                JobName = result.PartitionKey,
                Key = result.RowKey,
                ContainerName = result.ContainerName,
                Status = result.GetResultStatus(),
                StartAt = result.StartAt,
                FinishAt = result.FinishAt,
                Log = await _jobService.ReadLogAsync(result.LogUri)
            });
        }

        [HttpGet("/login")] 
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("/login")] 
        [AllowAnonymous]        
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var password = _config.GetSection("CookieAuth")["Password"];

            if (model.Password != password)
            {
                ViewBag.ErrMsg = "Password is invalid"; 
        
                return View(); 
            }

            var claims = new List<Claim>() 
            { 
                new Claim(ClaimTypes.Name,"conductor")
            };

            var userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "PasswordLogin"));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal, new AuthenticationProperties 
            { 
                ExpiresUtc = DateTime.UtcNow.AddMinutes(20), 
                IsPersistent = false, 
                AllowRefresh = true 
            }); 

            return RedirectToAction("Index", "Home"); 
        }

        [HttpGet("/logout")]
        [AllowAnonymous]        
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookie"); 
 
            return RedirectToAction("Index", "Home"); 
        }

        [HttpGet("/forbidden")] 
        [AllowAnonymous]        
        public IActionResult Forbidden()
        {
            return new StatusCodeResult(403);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
