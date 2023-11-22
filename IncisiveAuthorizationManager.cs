using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;

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
            return null;
        }
    }
}
