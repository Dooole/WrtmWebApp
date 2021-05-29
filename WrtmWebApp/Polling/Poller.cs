using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public class DeviceOperations
        {
            private HttpConnection conn;
            public class LoginRequest
            {
                public string username { get; set; }
                public string password { get; set; }
            }

            public class LoginResponse
            {
                public string token { get; set; }
            }

            public class Config
            {
                public class System
                {
                    public string hostname { get; set; }
                }
                public class Network
                {
                    public string ip { get; set; }
                    public string nm { get; set; }
                    public string gw { get; set; }
                    public IList<string> dns { get; set; }
                }
                public System system { get; set; }
                public Network network { get; set; }
            }

            public DeviceOperations(string host)
            {
                conn = new HttpConnection(host, 3);
            }

            public bool Login(string user, string pass)
            {
                try
                {
                    LoginRequest loginreq = new LoginRequest()
                    {
                        username = user,
                        password = pass
                    };

                    string resp = conn.Post("login", JsonConvert.SerializeObject(loginreq));
                    if (resp.Length > 0)
                    {
                        LoginResponse loginresp = JsonConvert.DeserializeObject<LoginResponse>(resp);
                        if (loginresp.token.Length == 64) // SHA256 string len
                        {
                            conn.TokenSet(loginresp.token);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            public void Logout()
            {
                try
                {
                    conn.Get("logout");
                }
                catch
                {
                    return;
                }
            }

            public Configuration Pull()
            {
                string configstr = conn.Get("config");
                if (configstr == null || configstr.Length == 0)
                    return null;

                try
                {
                    Config rawcfg = JsonConvert.DeserializeObject<Config>(configstr);

                    Configuration cfg = new Configuration();
                    cfg.Hostname = rawcfg.system.hostname;
                    cfg.Netmask = rawcfg.network.nm;
                    cfg.Gateway = rawcfg.network.gw;
                    cfg.Dns1 = rawcfg.network.dns[0];
                    cfg.Dns2 = rawcfg.network.dns[1];

                    return cfg;
                } catch
                {
                    return null;
                }
            }

            public bool Push(Configuration cfg)
            {
                Config rawcfg = new Config();
                rawcfg.system = new Config.System();
                rawcfg.system.hostname = cfg.Hostname;
                rawcfg.network = new Config.Network();
                rawcfg.network.nm = cfg.Netmask;
                rawcfg.network.gw = cfg.Gateway;
                rawcfg.network.dns = new List<string>();
                rawcfg.network.dns.Add(cfg.Dns1);
                rawcfg.network.dns.Add(cfg.Dns2);

                try
                {
                    conn.Post("config", JsonConvert.SerializeObject(rawcfg));
                }
                catch
                {
                    return false;
                }

                return true;
            }

            public string Changed(Configuration c1, Configuration c2)
            {
                string changed = "";

                if (c1.Hostname != c2.Hostname)
                    changed = changed + "Hostname";

                if (c1.Netmask != c2.Netmask)
                    changed = changed + " Netmask";

                if (c1.Gateway != c2.Gateway)
                    changed = changed + " Gateway";

                if (c1.Dns1 != c2.Dns1)
                    changed = changed + " Dns1";

                if (c1.Dns2 != c2.Dns2)
                    changed = changed + " Dns2";

                if (changed != "")
                    return changed;

                return null;
            }
        }
        
        private Configuration getDBConfig(string mac)
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

        public void Update()
        {
            List<Device> list = db.Devices.ToList();
            foreach (Device dev in list)
            {
                Configuration cfg = getDBConfig(dev.Mac);
                if (cfg == null)
                {
                    log.Error(dev.Mac + ": " + "configuration not found");
                    continue;
                }

                if (dev.Retries == null)
                {
                    dev.Retries = new int();
                    dev.Retries = 0;
                }

                string state = "OFFLINE";
                int retries = (int)dev.Retries;

                DeviceOperations dops = new DeviceOperations(cfg.Ipaddr);
                if (dops.Login("root", "root"))
                {
                    state = "OK";
                    retries = 0;

                    Configuration devcfg = dops.Pull();
                    if (devcfg != null)
                    {
                        string changed = dops.Changed(devcfg, cfg);
                        if (changed != null)
                        {
                            state = "CONFIGURING";
                            retries = 1;

                            log.Debug(cfg.Ipaddr + ": " + "changed: " + changed);
                            log.Debug(cfg.Ipaddr + ": " + "pushing new config");
                            if (!dops.Push(cfg))
                                log.Error(cfg.Ipaddr + ": " + "failed to push config");
                        }
                    }

                    dops.Logout();
                }

                if (retries > 0)
                {
                    if (retries > 5)
                    {
                        retries = 0;
                    }
                    else
                    {
                        state = "CONFIGURING";
                        retries++;
                    }
                }

                if (dev.State != state || dev.Retries != retries)
                {
                    dev.State = state;
                    dev.Retries = retries;
                    db.Entry(dev).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}
