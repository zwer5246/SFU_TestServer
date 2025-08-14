using MessagesModels;
using MessagesModels.Enums;
using MessagesModels.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Coordinator = SFU_MainCluster.SFU.Main.Server.Coordinator.Coordinator;

namespace SFU_MainCluster.Web.Controllers
{
    public class SessionController : Controller
    {
        private readonly ILogger<SessionController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Coordinator _coordinator;
        
        public SessionController(ILogger<SessionController> logger, UserManager<IdentityUser> userManager, Coordinator coordinator)
        {
            _logger = logger;
            _userManager = userManager;
            _coordinator = coordinator;
        }
        
        [Authorize]
        public async Task<IActionResult> ActiveSession(string id)
        {
            var result = _coordinator.GetSessionInfo(id, out var curentSession);
            var user = await _userManager.GetUserAsync(User);
            if (result == GetRoomResult.NoError)
            {
                if (curentSession.HostId == user!.Id)
                {
                    ViewBag.User = await _userManager.GetUserAsync(User);
                    return View(curentSession);
                }
                else
                {
                    ViewBag.User = await _userManager.GetUserAsync(User);
                    return View(curentSession);
                }
            }
            else
            {
                TempData["Error"] = ErrorProvider.GetErrorDescription(result);
                return View();
            }
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> StartNewSession()
        {
            var user = await _userManager.GetUserAsync(User);

            return View();
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> StartNewSession(RoomModel newSession)
        {
            var user = await _userManager.GetUserAsync(User);
            newSession.HostId = user.Id;
            CreateRoomResult result = _coordinator.CreateRoom(newSession);
            TempData["Error"] = result;
            return View(newSession);
        }
        
        [HttpGet]
        [Authorize]
        public IActionResult SessionsList()
        {
            _coordinator.GetAllSessions(out var sessions);
            return View(sessions);
        }
        
        [HttpPost]
        [Authorize]
        public IActionResult SessionsList(RoomModel session)
        {
            return RedirectToAction("ActiveSession", "Session", new { id = session.Id });
        }
    }
}
