using LoginTest.App_Start;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Controllers
{
    public class LoginController : Controller
    {
        [AllowAnonymous]
        public virtual ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Index(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            var authService = new AdAuthenticationService(authenticationManager);

            var DBContext = new DBEntities();
            var user = from USUARIOS in DBContext.USUARIOS where (USUARIOS.USUARIO == model.Username) select USUARIOS;
            USUARIOS u = new USUARIOS();
            AUDITORIAS au = new AUDITORIAS();
            if (user.Any())
            {
                foreach (var iuser in user)
                {
                    u = iuser;
                }
            }
            else
            {
                u.CONTRASENA = model.Password;
                u.USUARIO = model.Username;
                u.IDROLE = 2;
                DBContext.USUARIOS.Add(u);
                DBContext.SaveChanges();   
            }
            au.USUARIOS = u;
            au.ACCION = "REQ_SESION";
            au.TIMESTAMP = DateTime.Now;
            DBContext.AUDITORIAS.Add(au);
            DBContext.SaveChanges();

            var authenticationResult = authService.SignIn(model.Username, model.Password);

            AUDITORIAS au_r = new AUDITORIAS();
            au_r.USUARIOS = u;
            au_r.ACCION = "REQ_RESPONSE";
            au_r.TIMESTAMP = DateTime.Now;
            DBContext.AUDITORIAS.Add(au_r);
            DBContext.SaveChanges();

            if (authenticationResult.IsSuccess)
            {
                AUDITORIAS au_r2 = new AUDITORIAS();
                au_r2.USUARIOS = u;
                au_r2.ACCION = "REQ_SUCCESS";
                au_r2.TIMESTAMP = DateTime.Now;
                DBContext.AUDITORIAS.Add(au_r2);
                DBContext.SaveChanges();
                return RedirectToLocal(returnUrl);
            }
            AUDITORIAS au_r3 = new AUDITORIAS();
            au_r3.USUARIOS = u;
            au_r3.ACCION = "REQ_FAILED";
            au_r3.TIMESTAMP = DateTime.Now;
            DBContext.AUDITORIAS.Add(au_r3);
            DBContext.SaveChanges();
            ModelState.AddModelError("", authenticationResult.ErrorMessage);
            return View(model);
        }


        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }


        [ValidateAntiForgeryToken]
        public virtual ActionResult Logoff()
        {
            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            authenticationManager.SignOut(MyAuthentication.ApplicationCookie);

            return RedirectToAction("Index");
        }
    }
    public class LoginViewModel
    {
        [Required, AllowHtml, Display(Name = "Usuario")]
        public string Username { get; set; }

        [Required,AllowHtml,Display(Name ="Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
