using KNote.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Shared.Dto
{
    public static class DtoExtensions
    {
        // TODO: Refactorizar esto, hay código repetido aquí. 

        //public static TInfo GetInfo<TEntity, TInfo>(this TEntity entity)
        //    where TEntity : ModelBase // , new()
        //    where TInfo : new()
        //{
        //    var info = new TInfo();
        //    UtilCopyProperties(info, entity);
        //    return info;
        //}

        //public static void SetInfo<TEntity, TInfo>(this TEntity entity, TInfo info)
        //    where TEntity : ModelBase // , new()
        //    //where TInfo : new()
        //{
        //    if (info != null)
        //        UtilCopyProperties(entity, info);
        //}

        public static TInfo GetSimpleDto<TInfo>(this KntModelBase entity)
            where TInfo : KntModelBase, new()
        {
            var info = new TInfo();
            UtilCopyProperties(info, entity);
            return info;
        }

        public static void SetSimpleDto(this KntModelBase dest, KntModelBase source)
        {
            if (source != null)
                UtilCopyProperties(dest, source);
        }

        //public static TInfo GetSimpleDto<TInfo>(this object entity)
        //    where TInfo : new()
        //{
        //    var info = new TInfo();
        //    UtilCopyProperties(info, entity);
        //    return info;
        //}

        //public static void SetSimpleDto(this object dest, object source)
        //{
        //    if (source != null)
        //        UtilCopyProperties(dest, source);
        //}

        static void UtilCopyProperties(object dest, object src)
        {
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
}
