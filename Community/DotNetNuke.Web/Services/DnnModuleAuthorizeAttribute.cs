// // DotNetNukeŽ - http://www.dotnetnuke.com
// // Copyright (c) 2002-2012
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.

using System;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Web.Services
{
    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public DnnModuleAuthorizeAttribute()
        {
            AccessLevel = SecurityAccessLevel.Host;
        }

        public string PermissionKey { get; set; }
        public SecurityAccessLevel AccessLevel { get; set; }

        protected virtual ModuleInfo FindModuleInfo(HttpContextBase context)
        {
            return context.FindModuleInfo();
        }

        // This method must be thread-safe since it is called by the caching module.
        protected override bool AuthorizeCore(HttpContextBase context)
        {
            var activeModule = FindModuleInfo(context);

            if (activeModule != null)
            {
                return ModulePermissionController.HasModuleAccess(AccessLevel, PermissionKey, activeModule);
            }

            return false;
        }
    }
}