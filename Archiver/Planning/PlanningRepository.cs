using Archiver.Planning.Database;
using Archiver.Planning.Model;
using Archiver.Util;
using Microsoft.Extensions.Options;

namespace Archiver.Planning
{
    public class PlanningRepository : Repository<BackupPlan>
    {
        public PlanningRepository(JsonDatabase database) : base(database)
        {
        }
    }
}
