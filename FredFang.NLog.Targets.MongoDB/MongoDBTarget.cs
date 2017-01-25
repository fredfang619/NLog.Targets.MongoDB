using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using MongoDB.Bson;
using MongoDB.Driver;
using NLog;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FredFang.NLog.Targets.MongoDB
{
    /// <summary>
    /// 继承NLog.Targets.Target以实现记录日志到MongoDB
    /// </summary>
    [Target("MongoDBTarget")]
    public class MongoDBTarget : Target
    {
        private static readonly ConcurrentDictionary<string, IMongoCollection<BsonDocument>> CollectionCache = new ConcurrentDictionary<string, IMongoCollection<BsonDocument>>();
        private static IMongoDBConnectionStringProvider ConnectionStringProvider;

        /// <summary>
        /// 构造初始对象
        /// </summary>
        public MongoDBTarget()
        {
            Fields = new List<MongoDBField>();
            IncludeDefaults = true;
        }

        /// <summary>
        /// MongoDB Fields
        /// </summary>
        [ArrayParameter(typeof(MongoDBField), "field")]
        public IList<MongoDBField> Fields { get; private set; }

        /// <summary>
        /// MongoDB连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// MongoDB连接字符串名，与ConnectionStringProviderType组合使用
        /// </summary>
        public string ConnectionName { get; set; }
        
        /// <summary>
        /// MongoDB连接字符串提供方类型
        /// </summary>
        public string ConnectionStringProviderType { get; set; }

        /// <summary>
        /// 是否包含默认Fields
        /// </summary>
        public bool IncludeDefaults { get; set; }

        /// <summary>
        /// MongoDB Collection名
        /// </summary>
        public string CollectionName { get; set; }

        /// <summary>
        /// MongoDB Collection最大容量
        /// </summary>
        public long? CollectionMaxSize { get; set; }

        /// <summary>
        /// MongoDB Collection最大Item数
        /// </summary>
        public long? CollectionMaxItems { get; set; }

        /// <summary>
        /// 初始化Target配置
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if(!string.IsNullOrWhiteSpace(ConnectionStringProviderType))
            {
                try
                {
                    Type type = Type.GetType(ConnectionStringProviderType, true, true);
                    ConnectionStringProvider = (IMongoDBConnectionStringProvider)Activator.CreateInstance(type);
                }
                catch (TypeLoadException ex)
                {
                    throw new NLogConfigurationException(string.Format("ConnectionStringProvider初始化失败", ex));
                }
                ConnectionString = ConnectionStringProvider.GetConnectionString(ConnectionName);
            }
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new NLogConfigurationException("MongoDB连接字符串不可为空");
            }
        }

        /// <summary>
        /// 写入MongoDB
        /// </summary>
        /// <param name="logEvents"></param>
        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            if (logEvents.Length == 0)
            {
                return;
            }

            try
            {
                var documents = logEvents.Select(e => CreateDocument(e.LogEvent));
                var collection = GetCollection();
                collection.InsertMany(documents);

                foreach (var ev in logEvents)
                {
                    ev.Continuation(null);
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Error("写入MongoDB时失败：{0}", ex);
                foreach (var ev in logEvents)
                {
                    ev.Continuation(ex);
                }
            }
        }

        /// <summary>
        /// 写入MongoDB
        /// </summary>
        /// <param name="logEvent"></param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var document = CreateDocument(logEvent);
                var collection = GetCollection();
                collection.InsertOne(document);
            }
            catch (Exception ex)
            {
                InternalLogger.Error("写入MongoDB时失败：{0}", ex);
            }
        }

        private BsonDocument CreateDocument(LogEventInfo logEvent)
        {
            var document = new BsonDocument();
            if (IncludeDefaults || Fields.Count == 0)
            {
                AddDefaultCollections(logEvent, ref document);
            }

            // 添加其他自定义列
            foreach (var field in Fields)
            {
                var value = GenerateBsonValue(field, logEvent);
                if (value != null)
                {
                    document[field.Name] = value;
                }
            }

            return document;
        }

        private void AddDefaultCollections(LogEventInfo logEvent, ref BsonDocument document)
        {
            document.Add("Date", new BsonDateTime(logEvent.TimeStamp));

            if (logEvent.Level != null)
            {
                document.Add("Level", new BsonString(logEvent.Level.Name));
            }

            if (logEvent.LoggerName != null)
            {
                document.Add("Logger", new BsonString(logEvent.LoggerName));
            }

            if (logEvent.FormattedMessage != null)
            {
                document.Add("Message", new BsonString(logEvent.FormattedMessage));
            }

            if (logEvent.Exception != null)
            {
                document.Add("Exception", GenerateException(logEvent.Exception));
            }
        }

        private BsonValue GenerateException(Exception exception)
        {
            if (exception == null)
            {
                return BsonNull.Value;
            }

            var document = new BsonDocument();
            document.Add("Message", new BsonString(exception.Message));
            document.Add("BaseMessage", new BsonString(exception.GetBaseException().Message));
            document.Add("Text", new BsonString(exception.ToString()));
            document.Add("Type", new BsonString(exception.GetType().ToString()));
            document.Add("Source", new BsonString(exception.Source));
            MethodBase method = exception.TargetSite;
            if (method != null)
            {
                AssemblyName assembly = method.Module.Assembly.GetName();
                document.Add("MethodName", new BsonString(method.Name));
                document.Add("AssemblyName", new BsonString(assembly.Name));
                document.Add("AssemblyVersion", new BsonString(assembly.Version.ToString()));
            }

            return document;
        }

        private BsonValue GenerateBsonValue(MongoDBField field, LogEventInfo logEvent)
        {
            var value = field.Layout.Render(logEvent);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim();

            if (string.IsNullOrEmpty(field.BsonType)
                || string.Equals(field.BsonType, "String", StringComparison.OrdinalIgnoreCase))
            {
                return new BsonString(value);
            }

            BsonValue bsonValue;
            if (string.Equals(field.BsonType, "Boolean", StringComparison.OrdinalIgnoreCase)
                && ToBoolean(value, out bsonValue))
            {
                return bsonValue;
            }

            if (string.Equals(field.BsonType, "DateTime", StringComparison.OrdinalIgnoreCase)
                && ToDateTime(value, out bsonValue))
            {
                return bsonValue;
            }

            if (string.Equals(field.BsonType, "Double", StringComparison.OrdinalIgnoreCase)
                && ToDouble(value, out bsonValue))
            {
                return bsonValue;
            }

            if (string.Equals(field.BsonType, "Int32", StringComparison.OrdinalIgnoreCase)
                && ToInt32(value, out bsonValue))
            {
                return bsonValue;
            }

            if (string.Equals(field.BsonType, "Int64", StringComparison.OrdinalIgnoreCase)
                && ToInt64(value, out bsonValue))
            {
                return bsonValue;
            }

            return new BsonString(value);
        }

        #region Bson类型转换
        private bool ToBoolean(string value, out BsonValue bsonValue)
        {
            bool result;
            if (bool.TryParse(value, out result))
            {
                bsonValue = new BsonBoolean(result);
                return true;
            }
            else
            {
                bsonValue = null;
                return false;
            }
        }

        private bool ToDateTime(string value, out BsonValue bsonValue)
        {
            DateTime result;
            if (DateTime.TryParse(value, out result))
            {
                bsonValue = new BsonDateTime(result);
                return true;
            }
            else
            {
                bsonValue = null;
                return false;
            }
        }

        private bool ToDouble(string value, out BsonValue bsonValue)
        {
            double result;
            if (double.TryParse(value, out result))
            {
                bsonValue = new BsonDouble(result);
                return true;
            }
            else
            {
                bsonValue = null;
                return false;
            }

        }

        private bool ToInt32(string value, out BsonValue bsonValue)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                bsonValue = new BsonInt32(result);
                return true;
            }
            else
            {
                bsonValue = null;
                return false;
            }
        }

        private bool ToInt64(string value, out BsonValue bsonValue)
        {
            long result;
            if (long.TryParse(value, out result))
            {
                bsonValue = new BsonInt64(result);
                return true;
            }
            else
            {
                bsonValue = null;
                return false;
            }
        }
        #endregion

        private IMongoCollection<BsonDocument> GetCollection()
        {
            string collectionName = FormatCollectionName(CollectionName) ?? "Log";
            string cacheKey = string.Format("ck_{0}_{1}",
                ConnectionString ?? string.Empty,
                collectionName ?? string.Empty);

            return CollectionCache.GetOrAdd(cacheKey, p =>
            {
                InternalLogger.Info("刷新Collection");
                var mongoUrl = new MongoUrl(ConnectionString);
                var client = new MongoClient(mongoUrl);

                var databaseName = mongoUrl.DatabaseName ?? "NLog";
                var database = client.GetDatabase(databaseName);

                if (!CollectionMaxSize.HasValue || CollectionExists(database, collectionName))
                    return database.GetCollection<BsonDocument>(collectionName);

                var options = new CreateCollectionOptions
                {
                    Capped = true,
                    MaxSize = CollectionMaxSize,
                    MaxDocuments = CollectionMaxItems
                };

                database.CreateCollection(collectionName, options);

                return database.GetCollection<BsonDocument>(collectionName);
            });
        }

        private static bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };

            return database.ListCollections(options).ToEnumerable().Any();
        }

        private static Regex collectionNameRegex = new Regex("\\${date:(.+)}");
        private string FormatCollectionName(string collectionName)
        {
            string result = null;
            if (collectionNameRegex.IsMatch(collectionName))
            {
                MatchEvaluator evaluator = (Match match) => System.DateTime.Now.ToString(match.Groups[1].Value);
                result = collectionNameRegex.Replace(collectionName, evaluator);
            }
            return result;
        }
    }
}