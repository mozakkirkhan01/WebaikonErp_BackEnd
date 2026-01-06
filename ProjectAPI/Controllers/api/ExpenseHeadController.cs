using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace ExpenseHeadAPI.Controllers.api
{
    [RoutePrefix("api/ExpenseHead")]
    public class ExpenseHeadController : ApiController
    {
        [HttpPost]
        [Route("ExpenseHeadList")]
        public ExpandoObject ExpenseHeadList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ExpenseHead model = JsonConvert.DeserializeObject<ExpenseHead>(decryptData);

                var list = (from d1 in dbContext.ExpenseHeads
                            where (model.ExpenseHeadId == d1.ExpenseHeadId || model.ExpenseHeadId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.ExpenseHeadName
                            select new
                            {
                                d1.ExpenseHeadId,
                                d1.ExpenseHeadName,
                                d1.Status,
                            }).ToList();

                response.ExpenseHeadList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveExpenseHead")]
        public ExpandoObject SaveExpenseHead(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ExpenseHead model = JsonConvert.DeserializeObject<ExpenseHead>(decryptData);

                ExpenseHead ExpenseHead;

                // ================= UPDATE =================
                if (model.ExpenseHeadId > 0)
                {
                    ExpenseHead = dbContext.ExpenseHeads.FirstOrDefault(x => x.ExpenseHeadId == model.ExpenseHeadId);

                    if (ExpenseHead == null)
                    {
                        response.Message = "ExpenseHead not found";
                        return response;
                    }

                    ExpenseHead.ExpenseHeadName = model.ExpenseHeadName;
                    ExpenseHead.Status = model.Status;
                }
                // ================= INSERT =================
                else
                {
                    ExpenseHead = new ExpenseHead
                    {

                        ExpenseHeadName = model.ExpenseHeadName,
                        Status = model.Status,
                    };
                }

                dbContext.ExpenseHeads.Add(ExpenseHead);


                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
                response.ExpenseHeadId = ExpenseHead.ExpenseHeadId;

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                {
                    response.Message = "This ExpenseHead already exists";
                }
                else
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("deleteExpenseHead")]
        public ExpandoObject DeleteExpenseHead(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ExpenseHead model = JsonConvert.DeserializeObject<ExpenseHead>(decryptData);

                var ExpenseHead = dbContext.ExpenseHeads.Where(x => x.ExpenseHeadId == model.ExpenseHeadId).First();
                dbContext.ExpenseHeads.Remove(ExpenseHead);
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("FK"))
                    response.Message = "This record is in use.so can't delete.";
                else
                    response.Message = ex.Message;
            }
            return response;
        }

    }
}
