using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Net;
using WrtmWebApp.Models;

namespace WrtmWebApp.Controllers
{
    public class UserController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: User
        public ActionResult Index()
        {
            AspNetUser root = null;
            List<AspNetUser> ulist = db.AspNetUsers.ToList();
            foreach (AspNetUser user in ulist)
            {
                if (user.UserName == "admin")
                {
                    root = user;
                    break;
                }
            }

            if (root != null)
            {
                ulist.Remove(root);
            }

            return View(ulist);
        }

        // GET: User/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null || id.Length == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetUser user = db.AspNetUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            AspNetUser user = db.AspNetUsers.Find(id);
            db.AspNetUsers.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
