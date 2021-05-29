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
        private wrtmDBEntities1 db;
        private Logger log;

        public Poller()
        {
            db = new wrtmDBEntities1();
            log = new Logger(db);
        }
        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }

        public class LoginResponse
        {
            public string token { get; set; }
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
                        log.Debug(host+": "+"logged into");
                        return true;
                    }
                    else
                    {
                        log.Error(host + ": " + "invalid token");
                        return false;
                    }
                }
                else
                {
                    log.Error(host + ": " + "invalid credentials or offline");
                    return false;
                }
            }
            catch
            {
                log.Error(host + ": " + "connection error");
                return false;
            }
        }

        private Configuration getConfig(string mac, wrtmDBEntities1 db)
        {
            Configuration cfg = null;
            List<Configuration> configs = db.Configurations.ToList();
            foreach (Configuration c in configs)
            {
                if (c.Mac == mac)
                {
                    cfg = c;
                    break;
                }
            }

            return cfg;
        }

        public void UpdateAll()
        {
            List<Device> list = db.Devices.ToList();
            foreach (Device dev in list)
            {
                string state = "OFFLINE";
                Configuration cfg = getConfig(dev.Mac, db);
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
