using Autofac;
using News.DAL.Interfaces;
using News.DAL.Repositories;

namespace News.BLL.Infrastructure
{
    public class AutofacConfig
    {
        public static void ConfigureContainer(ContainerBuilder builder)
        {
            // регистрируем споставление типов
            builder.RegisterType<EFUnitOfWork>().As<IUnitOfWork>();
        }
    }
}
