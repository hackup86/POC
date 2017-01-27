using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LoginTest.Controllers
{
    public class AuditController : Controller
    {
        // GET: Audit
        [Authorize]
        public ActionResult Index()
        {
            var DBContext = new DBEntities();
            var Userlist = from AUDITORIAS in DBContext.AUDITORIAS orderby AUDITORIAS.IDAUDITORIA select AUDITORIAS;
            var users = new List<AUDITORIAS>();
            if (Userlist.Any())
            {
                foreach (var user in Userlist)
                {
                    users.Add(new AUDITORIAS()
                    {
                        IDAUDITORIA = user.IDAUDITORIA,
                        IDUSUARIO = user.IDUSUARIO,
                        ACCION = user.ACCION,
                        TIMESTAMP = user.TIMESTAMP,
                        USUARIOS = user.USUARIOS
                    });
                }

            }
            return View(users);
        }

        // GET: Audit/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Audit/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Audit/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Audit/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Audit/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Audit/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Audit/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
