﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using GAABA.WebAPI.Service;

[assembly: OwinStartup(typeof(GAABA.WebAPI.Startup))]

namespace GAABA.WebAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            var a = new GetAdwordsCampaigns().Run(null);
        }
    }
}
