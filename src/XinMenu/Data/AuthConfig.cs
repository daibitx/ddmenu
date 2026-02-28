using Daibitx.Identity.Authorization.Scope;
using XinMenu.Models;

namespace XinMenu.Data
{
    public class AuthConfig : ApiScope
    {
        public override string Name => "XinMenu.Scope";

        public override void Build(ApiScopeBuilder builder)
        {
            #region 菜谱管理权限
            builder.AddPermission("Recipe", "Read", "读取菜谱列表");
            builder.AddPermission("Recipe", "ReadDetail", "读取菜谱详情");
            builder.AddPermission("Recipe", "Create", "创建菜谱");
            builder.AddPermission("Recipe", "Update", "更新菜谱");
            builder.AddPermission("Recipe", "Delete", "删除菜谱");
            builder.AddPermission("Recipe", "Favorite", "收藏/取消收藏菜谱");
            #endregion

            #region 菜谱分类管理权限
            builder.AddPermission("Category", "Read", "读取分类列表");
            builder.AddPermission("Category", "ReadDetail", "读取分类详情");
            builder.AddPermission("Category", "Create", "创建分类");
            builder.AddPermission("Category", "Update", "更新分类");
            builder.AddPermission("Category", "Delete", "删除分类");
            #endregion

            #region 原料管理权限
            builder.AddPermission("Ingredient", "Read", "读取原料列表");
            builder.AddPermission("Ingredient", "ReadDetail", "读取原料详情");
            builder.AddPermission("Ingredient", "Create", "创建原料");
            builder.AddPermission("Ingredient", "Update", "更新原料");
            builder.AddPermission("Ingredient", "Delete", "删除原料");
            #endregion

            #region 饮食记录管理权限
            builder.AddPermission("FoodLog", "Read", "读取饮食记录");
            builder.AddPermission("FoodLog", "ReadCalendar", "读取饮食日历");
            builder.AddPermission("FoodLog", "Create", "创建饮食记录");
            builder.AddPermission("FoodLog", "Update", "更新饮食记录");
            builder.AddPermission("FoodLog", "Delete", "删除饮食记录");
            #endregion

            #region 用户管理权限
            builder.AddPermission("User", "ReadProfile", "读取用户资料");
            builder.AddPermission("User", "UpdateProfile", "更新用户资料");
            builder.AddPermission("User", "ChangePassword", "修改密码");
            builder.AddPermission("User", "UploadAvatar", "上传头像");
            #endregion

            #region 定义角色和默认权限
            // 普通用户角色
            builder.AddRole(RoleDefinetion.User.ToString(), "普通用户", new[] {
                // 菜谱权限
                "Recipe.Read",
                "Recipe.ReadDetail",
                "Recipe.Favorite",
                // 分类权限
                "Category.Read",
                // 原料权限
                "Ingredient.Read",
                "Ingredient.ReadDetail",
                // 饮食记录权限
                "FoodLog.Read",
                "FoodLog.ReadCalendar",
                "FoodLog.Create",
                "FoodLog.Update",
                "FoodLog.Delete",
                // 用户权限
                "User.ReadProfile",
                "User.UpdateProfile",
                "User.ChangePassword",
                "User.UploadAvatar",
            });

            // 管理员角色 - 拥有所有权限
            builder.AddAdministratorRole(RoleDefinetion.Admin.ToString(), "管理员");

            // 默认用户配置
            builder.AddUser("18236158701", "CaoNiMa99373", "家猪饲养员", new[] { RoleDefinetion.Admin.ToString() });
            builder.AddUser("15139630620", "zhuangjieyu", "家猪", new[] { RoleDefinetion.User.ToString() });
            #endregion
        }
    }
}
