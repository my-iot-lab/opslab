namespace Ops.Host.Common.Utils;

public static class PageHelper
{
    /// <summary>
    /// 计算页数
    /// </summary>
    /// <param name="count">总数量</param>
    /// <param name="pageSize">每页数量</param>
    /// <returns></returns>
    /// <exception cref="System.DivideByZeroException"></exception>
    public static long GetPageCount(long count, int pageSize)
    {
        if (count == 0)
        {
            return 0;
        }

        if (count <= pageSize)
        {
            return 1;
        }

        return count / pageSize + (count % pageSize > 0 ? 1 : 0);
    }
}
