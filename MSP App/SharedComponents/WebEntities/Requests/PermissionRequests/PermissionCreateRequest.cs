﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.WebEntities.Requests.PermissionRequests
{
    public class PermissionCreateRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
