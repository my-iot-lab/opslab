using BootstrapAdmin.DataAccess.Models;

namespace BootStarpAdmin.DataAccess.FreeSql.Extensions;

internal static class FreeSqlExtensions
{
    public static void Mapper(this IFreeSql freeSql)
    {
        // 枚举类型添加注解 [FreeSql.DataAnnotations.Column(MapType = typeof(int))]
        freeSql.Aop.ConfigEntityProperty += (s, e) =>
        {
            if (e.Property.PropertyType.IsEnum)
            {
                e.ModifyResult.MapType = typeof(int?);
            }
        };
    }
}
