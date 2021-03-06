using ArchivePlanner.Planning.Database;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Microsoft.Extensions.Options;

namespace ArchivePlanner.Planning
{
    public class PlanningRepository : Repository<BackupPlan>
    {
        public PlanningRepository(JsonDatabase database) : base(database)
        {
        }
    }
}
