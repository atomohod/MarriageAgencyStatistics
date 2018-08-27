using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using MarriageAgencyStatistics.Bootstrapper;

namespace MarriageAgencyStatistics.WindowsService
{
    public partial class MarriageAgencyStatistics : ServiceBase
    {
        private BackgroundJobServer _backgroundJobServer;

        public MarriageAgencyStatistics()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _backgroundJobServer = new BackgroundJobServer();
        }

        protected override void OnStop()
        {
            _backgroundJobServer.Dispose();
        }
    }
}
