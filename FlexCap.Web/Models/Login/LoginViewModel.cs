// using FlexCap.Web.Models.Login;
// using Microsoft.AspNetCore.Mvc;
//
// namespace FlexCap.Web.Controllers.Login
// {
//     public class LoginController : Controller
//     {
//         [HttpGet]
//         public IActionResult Index()
//         {
//             return View();
//         }
//
//         [HttpPost]
//         public IActionResult Login(LoginViewModel model)
//         {
//             if (ModelState.IsValid)
//             {
//                 return RedirectToAction("Index", "Home");
//             }
//
//             return View("Index");
//         }
//     }
//
// }