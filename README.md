<p>
	# 使用说明<br />
请搭配NLog.Config使用<br />
在NLog.Config中添加如下节点<br />
&nbsp; &lt;extensions&gt;<br />
&nbsp; &nbsp; &lt;add assembly="FredFang.NLog.Targets.MongoDB"/&gt;<br />
&nbsp; &lt;/extensions&gt;<br />
&nbsp; &lt;targets async="true"&gt;<br />
&nbsp; &nbsp; &lt;target xsi:type="MongoDBTarget"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; name="mongoDBTarget1"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; connectionString="mongodb://localhost:27017/mongoDBTarget1"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; connectionName="mongoDBTarget3"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; connectionStringProviderType="FredFang.NLog.Targets.MongoDB.UnitTest.MongoDBConnectionStringProvider, FredFang.NLog.Targets.MongoDB.UnitTest"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; collectionName="NLog${date:yyMMdd}"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; collectionMaxSize="1073741824"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; collectionMaxItems="100000000"<br />
&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; includeDefaults="true"&gt;<br />
&nbsp; &nbsp; &lt;/target&gt;<br />
&nbsp; &lt;/targets&gt;<br />
<br />
name：target名，搭配rules配置使用<br />
connectionString：MongoDB连接字符串<br />
connectionName：搭配connectionStringProviderType使用<br />
connectionStringProviderType：继承IMongoDBConnectionStringProvider接口后将实现类配置在这里，搭配connectionName使用<br />
collectionName：日志Collection名，支持日期序列化<br />
collectionMaxSize：Collection最大容量<br />
collectionMaxSize：Collection最大Item数<br />
includeDefaults：是否包含默认字段（Date、Level、Logger、Message、Exception）<br />
<br />
更多配置请见测试源码：https://github.com/fredfang619/FredFang.NLog.Targets.MongoDB/blob/master/src/FredFang.NLog.Targets.MongoDB.UnitTest/NLog.config<br />
<br />
Ps.搭配NLog.Web使用更佳哦<br />
	<div>
		<br />
	</div>
</p>
<p>
	<br />
</p>