using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System.Threading;

namespace NLog.Targets.MongoDB.UnitTest
{
    [TestClass]
    public class MongoDBTargetTest
    {

        [TestMethod]
        public void LogTest()
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Trace("Trace Test");
            logger.Debug("Debug Test");
            logger.Warn("Warn Test");
            try
            {
                throw new Exception("error accurs");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error Test");
            }
            try
            {
                var a = 1;
                var i = a / 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Fatal Test");
            }

            Thread.Sleep(5000);// 开启NLog的异步后不可立即结束进程
        }
    }
}
