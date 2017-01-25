using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredFang.NLog.Targets.MongoDB
{
    public interface IMongoDBConnectionStringProvider
    {
        string GetConnectionString(string name);
    }
}
