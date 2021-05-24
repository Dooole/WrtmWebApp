using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using WrtmWebApp.Models;

namespace WrtmWebApp.Polling
{
    public class Poller
    {      
        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class LoginResponse
        {
            public string token { get; set; }
        }

        public void LogMsg(string msg)
        {
            string textFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "log.txt");
            File.WriteAllText(textFilePath, DateTime.Now.ToString()+":"+msg);
        }

        public bool Update(string host, string user, string pass)
        {
            try
            {
                LoginRequest loginreq = new LoginRequest()
                {
                    username = user,
                    password = pass
                };

                HttpConnection Conn = new HttpConnection(host, 3);
                string resp = Conn.Post("login", JsonConvert.SerializeObject(loginreq));
                if (resp.Length > 0)
                {
                    LoginResponse loginresp = JsonConvert.DeserializeObject<LoginResponse>(resp);
                    if (loginresp.token.Length == 64) // SHA256 string len
                    {
                        Conn.TokenSet(loginresp.token);
                        LogMsg("logged in to: "+host);
                        return true;
                    }
                    else
                    {
                        LogMsg("Invalid token");
                        return false;
                    }
                }
                else
                {
                    LogMsg("Invalid credentials");
                    return false;
                }
            }
            catch
            {
                LogMsg("Connection error");
                return false;
            }
        }

        public void UpdateAll()
        {
            wrtmDBEntities db = new wrtmDBEntities();
            List<Device> list = db.Devices.ToList();
            foreach (Device dev in list)
            {
                string state = "OFFLINE";
                Configuration cfg = db.Configurations.Find(dev.Id);
                if (cfg != null && Update(cfg.Ipaddr, "root", "root"))
                {
                    state = "ONLINE";
                }

                dev.State = state;
                db.Entry(dev).State = EntityState.Modified;
                db.SaveChanges();
            }
            return;
        }
    }
}
