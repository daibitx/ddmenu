using Daibitx.Identity.Authorization.Scope;
using XinMenu.Models;

namespace XinMenu.Data
{
    public class AuthConfig : ApiScope
    {
        public override string Name => "AppSetting";

        public override void Build(ApiScopeBuilder builder)
        {


            builder.AddAdministratorRole(RoleDefinetion.Admin.ToString(), "管理员");

            builder.AddUser("18236158701", "CaoNiMa99373", "家猪饲养员", new[] { RoleDefinetion.Admin.ToString() });

            builder.AddUser("15139630620", "zhuangjieyu", "家猪", new[] { RoleDefinetion.User.ToString() });
        }
    }

}
