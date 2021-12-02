using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace ArchivePlanner.Util
{
    public static class ServiceCollectionExtension
    {
        public static void AddConfiguredLiteDb(this ServiceCollection services)
        {
            services.Configure<LiteDbOptions>((s) => s.DbName = "backup");

            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson => new DirectoryInfo(bson));

            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson =>
                    bson.AsString.ToFileSystemEntry());
        }
    }
}
