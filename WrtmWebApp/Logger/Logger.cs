using System;
using WrtmWebApp.Models;

namespace WrtmWebApp
{
    public class Logger
    {
        private wrtmDBEntities1 db;

        public Logger(wrtmDBEntities1 logdb)
        {
            db = logdb;
        }
        private void LogMessage(string severity, string msg)
        {
            if (severity == null || msg == null ||
                    severity.Length == 0 || msg.Length == 0)
                return;

            Log log = new Log();
            log.Id = 2;
            log.Datetime = DateTime.Now;
            log.Severity = severity;
            log.Message = msg;

            if (db != null)
            {
                db.Logs.Add(log);
                db.SaveChanges();
            }
        }

        public void Error(string msg)
        {
            LogMessage("ERROR", msg);
        }

        public void Warning(string msg)
        {
            LogMessage("WARNING", msg);
        }

        public void Debug(string msg)
        {
            LogMessage("DEBUG", msg);
        }
    }
}