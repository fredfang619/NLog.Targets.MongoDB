using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredFang.NLog.Targets.MongoDB.UnitTest
{
    class MongoDBConnectionStringProvider : IMongoDBConnectionStringProvider
    {
        public string GetConnectionString(string name)
        {
            return string.Format("mongodb://10.1.249.175:27017/{0}", name);
        }
    }
}
