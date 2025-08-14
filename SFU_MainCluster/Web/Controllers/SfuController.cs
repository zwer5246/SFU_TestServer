using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SFU_MainCluster.Web.Controllers;

public class SfuController : Controller
{
    [AllowAnonymous]
    public IActionResult ServerStatistics()
    {
        return View();
    }
    
    [Authorize]
    public IActionResult ServerLog()
    {
        return View();
    }
}