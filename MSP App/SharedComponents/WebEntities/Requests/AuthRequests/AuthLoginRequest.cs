﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.AuthRequests
{
    public class AuthLoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
