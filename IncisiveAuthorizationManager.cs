using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using System.Security.Principal;

namespace CTHarmonyAdapters
{
    internal class IncisiveAuthorizationManager : AuthorizationManagerBase
    {
        public override bool CheckCurrentUserPermission(string permissionId)
        {
            return true;
        }

        public override User GetCurrentUser()
        {
            User user = new User();
            string userName = null;
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            if (currentIdentity != null)
            {
                userName = currentIdentity.Name;
            }
            if (!string.IsNullOrEmpty(userName))
            {
                user.UserId = userName;
                user.Name = userName;
            }
            return user;
        }
    }
}
