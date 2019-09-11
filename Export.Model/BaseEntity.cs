using Export.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Model
{
    /// <summary>
    /// 实体基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseEntity<P> : IBaseEntity<P>
    {
        /// <summary>
        /// 主键
        /// </summary>
        [TableColumn(Name = "uuid", ColumnType = TableColumnType.PrimaryKey, Comments = "主键", DefaultValue = "newid()",TableColumnProcess = "Export.Model,UuidColumnProcess")]
        public virtual P Uuid { get; set; }

        /// <summary>
        /// 赋值前
        /// </summary>
        public virtual object Before(object fieldValue) { return fieldValue; }

        /// <summary>
        /// 赋值后
        /// </summary>
        public virtual object After(object fieldValue) { return fieldValue; }
    }

    /// <summary>
    /// 基类接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBaseEntity<P> : IEntityProcess
    {
        /// <summary>
        /// 主键
        /// </summary>
        P Uuid { get; set; }
    }

    /// <summary>
    /// 创建人接口
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <typeparam name="P"></typeparam>
    public interface ICreateUserEntity<C, P> : IBaseEntity<P>
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        C CreateUser { get; set; }
    }

    /// <summary>
    /// 修改人接口
    /// </summary>
    /// <typeparam name="C"></typeparam>
    /// <typeparam name="P"></typeparam>
    public interface IUpdateUserEntity<C, P> : ICreateUserEntity<C, P>
    {
        /// <summary>
        /// 修改时间
        /// </summary>
        DateTime UpdateTime { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        P UpdateUser { get; set; }
    }

    /// <summary>
    /// 实体处理
    /// </summary>
    public interface IEntityProcess
    {
        /// <summary>
        /// 赋值前
        /// </summary>
        object Before(object fieldValue);

        /// <summary>
        /// 赋值后
        /// </summary>
        object After(object fieldValue);
    }
}
