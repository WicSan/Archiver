using Microsoft.Extensions.Options;

namespace ArchivePlanner.Planning
{
    public class PlanningRepository : Repository, IPlanningRepository
    {
        public PlanningRepository(IOptions<LiteDbOptions> options) : base(options)
        {
        }
    }
}
