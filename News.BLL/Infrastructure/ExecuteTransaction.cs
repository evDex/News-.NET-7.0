using News.BLL.DTO;
using News.DAL.Entities;
using News.DAL.Interfaces;
using News.Infrastructure.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News.BLL.Infrastructure
{
    public class ExecuteTransaction
    {
        IUnitOfWork Database { get; set; }
        public ExecuteTransaction(IUnitOfWork uow)
        {
            Database = uow;
        }
        public async Task<BaseResponse<TOne>> ExecuteTransationAsync<TOne>(Func<BaseResponse<TOne>, Task<BaseResponse<TOne>>> operation, BaseResponse<TOne> response) 
        {
            try
            {
                var baseResponse = new BaseResponse<TOne>();

                Database.ConnectionOpen();
                await operation(response);
                Database.ConnectionClose();
                return baseResponse;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new BaseResponse<TOne>();
            }
        }
        public virtual async Task<BaseResponse<TTwo>> ExecuteTransationAsync<TOne, TTwo>(Func<TOne, BaseResponse<TTwo>, Task<BaseResponse<TTwo>>> operation, TOne model,BaseResponse<TTwo> response) 
        {
            try
            {
                Database.ConnectionOpen();
                await operation(model, response);
                Database.ConnectionClose();
                return response;
            }
            catch (Exception ex)
            {
                StaticLogger.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, " | " + ex.Message);
                return new BaseResponse<TTwo>();
            }
        }
    }
}
