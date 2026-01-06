using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace ExpenseAPI.Controllers.api
{
    [RoutePrefix("api/Expense")]
    public class ExpenseController : ApiController
    {
        [HttpPost]
        [Route("ExpenseList")]
        public ExpandoObject ExpenseList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Expense model = JsonConvert.DeserializeObject<Expense>(decryptData);

                var list = (from d1 in dbContext.Expenses
                            where (model.ExpenseId == d1.ExpenseId || model.ExpenseId == 0)
                            orderby d1.Amount
                            select new
                            {
                                d1.ExpenseId,
                                d1.ExpenseHeadId,
                                d1.ExpenseHead.ExpenseHeadName,
                                d1.Description,
                                d1.ExpenseDate,
                                d1.PaymentMode,
                                d1.Amount,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.ExpenseList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveExpense")]
        public ExpandoObject SaveExpense(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Expense model = JsonConvert.DeserializeObject<Expense>(decryptData);

                Expense Expense;

                // ================= UPDATE =================
                if (model.ExpenseId > 0)
                {
                    Expense = dbContext.Expenses.FirstOrDefault(x => x.ExpenseId == model.ExpenseId);

                    if (Expense == null)
                    {
                        response.Message = "Expense not found";
                        return response;
                    }

                    Expense.ExpenseHeadId = model.ExpenseHeadId;
                    Expense.Description = model.Description;
                    Expense.ExpenseDate = model.ExpenseDate;
                    Expense.PaymentMode = model.PaymentMode;
                    Expense.Amount = model.Amount;
                    Expense.UpdatedBy = model.CreatedBy; // logged-in staff
                    Expense.UpdatedOn = DateTime.Now;
                }
                // ================= INSERT =================
                else
                {
                    Expense = model;
                    if(Expense.ExpenseId == 0)
                    {
                        Expense.CreatedOn = DateTime.Now;

                        Expense.UpdatedBy = null;
                        Expense.UpdatedOn = null;

                        dbContext.Expenses.Add(Expense);

                    }
                }
                        dbContext.SaveChanges();
                        response.Message = ConstantData.SuccessMessage;



                //response.ExpenseId = Expense.ExpenseId;

            }
            catch (Exception ex)
            {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                {
                    response.Message = "This Expense already exists";
                }
                else
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("deleteExpense")]
        public ExpandoObject DeleteExpense(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Expense model = JsonConvert.DeserializeObject<Expense>(decryptData);

                var Expense = dbContext.Expenses.Where(x => x.ExpenseId == model.ExpenseId).First();
                dbContext.Expenses.Remove(Expense);
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
