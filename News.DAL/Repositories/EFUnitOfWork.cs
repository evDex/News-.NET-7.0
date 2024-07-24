using Microsoft.Data.SqlClient;
using News.DAL.Dapper;
using News.DAL.Entities;
using News.DAL.Interfaces;
using System;
using System.Data;

namespace News.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private ArticleRepository articleRepository;
        private CommentRepository commentRepository;
        private FileRepository fileRepository;
        private HashTagRepository hashTagRepository;
        private RoleRepository roleRepository;
        private UserRepository userRepository;

        private SqlConnection sqlConnection;

        private bool disposed = false;
        public EFUnitOfWork()
        {
            sqlConnection = NewsEntities.Connection();
            NewsDbInitialize.InitData();
        }
        public bool ConnectionOpen()
        {
            if(sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection.Open();
                return true;
            }
            return false;
        }
        public bool ConnectionClose()
        {
            if (sqlConnection.State == ConnectionState.Open)
            {
                sqlConnection.Close();
                return true;
            }
            return false;
        }
        public ArticleRepository Articles
        {
            get
            {
                if (articleRepository == null)
                    articleRepository = new ArticleRepository(sqlConnection);
                return articleRepository;
            }
        }
        public CommentRepository Comments
        {
            get
            {
                if (commentRepository == null)
                    commentRepository = new CommentRepository(sqlConnection);
                return commentRepository;
            }
        }
        public IGenericRepository<FileData> Files
        {
            get
            {
                if (fileRepository == null)
                    fileRepository = new FileRepository(sqlConnection);
                return fileRepository;
            }
        }
        public IGenericRepository<HashTag> HashTags
        {
            get
            {
                if (hashTagRepository == null)
                    hashTagRepository = new HashTagRepository(sqlConnection);
                return hashTagRepository;
            }
        }
        public RoleRepository Roles
        {
            get
            {
                if (roleRepository == null)
                    roleRepository = new RoleRepository(sqlConnection);
                return roleRepository;
            }
        }
        public UserRepository Users
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(sqlConnection);
                return userRepository;
            }
        }
        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    articleRepository?.Dispose();
                    commentRepository?.Dispose();
                    fileRepository?.Dispose();
                    hashTagRepository?.Dispose();
                    roleRepository?.Dispose();
                    userRepository?.Dispose();
                    sqlConnection?.Dispose();
                }
                this.disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
