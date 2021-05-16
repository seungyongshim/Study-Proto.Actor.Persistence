using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Persistence;
using Proto.Persistence.Sqlite;

namespace SampleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = 
            Host.CreateDefaultBuilder()
                .ConfigureServices(service =>
                {
                    service.AddSingleton<IProvider>(new SqliteProvider(new SqliteConnectionStringBuilder { DataSource = "states.db" }));

                })
                .UseProtoActor(_ => _, root =>
                {
                    var dtoActor = root.SpawnNamed(root.PropsFactory<DtoPersistenceActor>()
                                                       .Create("dto1"), "dto1");

                    root.Send(dtoActor, new SampleDto
                    {
                        Number = RandomNumberGenerator.GetInt32(100)
                    });


                });

            await host.RunConsoleAsync();
        }
    }
}
