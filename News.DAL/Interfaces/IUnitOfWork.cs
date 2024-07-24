using News.DAL.Entities;
using News.DAL.Repositories;
using System;

namespace News.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ArticleRepository Articles { get; }
        CommentRepository Comments { get; }
        IGenericRepository<FileData> Files { get; }
        IGenericRepository<HashTag> HashTags { get; }
        UserRepository Users { get; }
        RoleRepository Roles { get; }
        bool ConnectionOpen();
        bool ConnectionClose();
    }
}
