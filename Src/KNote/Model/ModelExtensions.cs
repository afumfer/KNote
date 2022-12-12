using KNote.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model;

public static class ModelExtensions
{
    public static TInfo GetSimpleDto<TInfo>(this ModelBase entity)
        where TInfo : ModelBase, new()
    {
        var info = new TInfo();
        UtilCopyProperties(info, entity);
        return info;
    }

    public static void SetSimpleDto(this ModelBase dest, ModelBase source)
    {
        if (source != null)
            UtilCopyProperties(dest, source);
    }

    public static void UtilCopyProperties(object dest, object src)
    {
        if ((dest == null) || (src == null) )
            return;

        Type tDest = dest.GetType();
        Type tSrc = src.GetType();
        PropertyInfo[] properties = tDest.GetProperties();
        PropertyInfo piSrc;

        foreach (PropertyInfo pi in properties)
        {
            piSrc = tSrc.GetProperty(pi.Name);
            if (piSrc != null && pi.CanWrite)
                pi.SetValue(dest, piSrc.GetValue(src, null), null);
        }
    }
}
