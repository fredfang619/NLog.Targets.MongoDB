using NLog.Config;
using NLog.Layouts;
using System.ComponentModel;

namespace FredFang.NLog.Targets.MongoDB
{
    /// <summary>
    /// MongoDB Field
    /// </summary>
    [NLogConfigurationItem]
    public sealed class MongoDBField
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public MongoDBField()
            : this(null, null, "String")
        {
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name">field名</param>
        /// <param name="layout">layout</param>
        public MongoDBField(string name, Layout layout)
            : this(name, layout, "String")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDBField" /> class.
        /// </summary>
        /// <param name="name">field名</param>
        /// <param name="layout">layout</param>
        /// <param name="bsonType">bson格式</param>
        public MongoDBField(string name, Layout layout, string bsonType)
        {
            Name = name;
            Layout = layout;
            BsonType = bsonType ?? "String";
        }

        /// <summary>
        /// field名
        /// </summary>
        [RequiredParameter]
        public string Name { get; set; }

        /// <summary>
        /// layout
        /// </summary>
        [RequiredParameter]
        public Layout Layout { get; set; }

        /// <summary>
        /// bson格式
        /// </summary>
        [DefaultValue("String")]
        public string BsonType { get; set; }
    }
}
