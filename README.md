# 使用说明
请搭配NLog.Config使用
在NLog.Config中添加如下节点
  <extensions>
    <add assembly="FredFang.NLog.Targets.MongoDB"/>
  </extensions>
  <targets async="true">
    <target xsi:type="MongoDBTarget"
            name="mongoDBTarget1"
            connectionString="mongodb://localhost:27017/mongoDBTarget1"
            connectionName="mongoDBTarget3"
            connectionStringProviderType="FredFang.NLog.Targets.MongoDB.UnitTest.MongoDBConnectionStringProvider, FredFang.NLog.Targets.MongoDB.UnitTest"
            collectionName="NLog${date:yyMMdd}"
            collectionMaxSize="1073741824"
            collectionMaxItems="100000000"
            includeDefaults="true">
    </target>
  </targets>

name：target名，搭配rules配置使用
connectionString：MongoDB连接字符串
connectionName：搭配connectionStringProviderType使用
connectionStringProviderType：继承IMongoDBConnectionStringProvider接口后将实现类配置在这里，搭配connectionName使用
collectionName：日志Collection名，支持日期序列化
collectionMaxSize：Collection最大容量
collectionMaxSize：Collection最大Item数
includeDefaults：是否包含默认字段（Date、Level、Logger、Message、Exception）

更多配置请见测试源码：https://github.com/fredfang619/FredFang.NLog.Targets.MongoDB/blob/master/src/FredFang.NLog.Targets.MongoDB.UnitTest/NLog.config

Ps.搭配NLog.Web使用更佳哦