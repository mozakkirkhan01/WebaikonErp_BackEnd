using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/ClientPayment")]
    public class ClientPaymentPaymentController : ApiController
    {
        [HttpPost]
        [Route("ClientPaymentList")]
        public ExpandoObject ClientPaymentList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientPayment model = JsonConvert.DeserializeObject<ClientPayment>(decryptData);

                var list = (from d1 in dbContext.ClientPayments
                            where (model.ClientPaymentId == d1.ClientPaymentId || model.ClientPaymentId == 0)
                            orderby d1.ClientPaymentId
                            select new
                            {
                                d1.Client.ClientCompanyName,
                                d1.ProjectType.ProjectTypeName,
                                d1.Description,
                                d1.Amount,
                                d1.PaymentDate,
                                d1.PaymentMode,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.ClientPaymentList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveClientPayment")]
        public ExpandoObject SaveClientPayment(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientPayment model = JsonConvert.DeserializeObject<ClientPayment>(decryptData);

                ClientPayment ClientPayment;

                // ================= UPDATE =================
                if (model.ClientPaymentId > 0)
                {
                    ClientPayment = dbContext.ClientPayments
                                      .FirstOrDefault(x => x.ClientPaymentId == model.ClientPaymentId);

                    if (ClientPayment == null)
                    {
                        response.Message = "ClientPayment not found";
                        return response;
                    }

                    ClientPayment.ClientId = model.ClientId;
                    ClientPayment.ProjectTypeId = model.ProjectTypeId;
                    ClientPayment.Description = model.Description;
                    ClientPayment.Amount = model.Amount;
                    ClientPayment.PaymentDate = model.PaymentDate;
                    ClientPayment.PaymentMode = model.PaymentMode;

                    ClientPayment.UpdatedBy = model.UpdatedBy;
                    ClientPayment.UpdatedOn = DateTime.Now;
                }
                // ================= INSERT =================
                else
                {
                    ClientPayment = model;
                    ClientPayment.CreatedOn = DateTime.Now;
                    //ClientPayment = new ClientPayment
                    //{
                    //    ClientId = model.ClientId,
                    //    ProjectTypeId = model.ProjectTypeId,
                    //    Description = model.Description,
                    //    Amount = model.Amount,
                    //    PaymentDate = model.PaymentDate,
                    //    PaymentMode = model.PaymentMode,

                    //    CreatedBy = 1,
                    //    CreatedOn = DateTime.Now
                    //};

                }
                if (ClientPayment.ClientPaymentId == 0)
                {
                    dbContext.ClientPayments.Add(ClientPayment);
                    dbContext.SaveChanges();
                    response.Message = ConstantData.SuccessMessage;
                }

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                {
                    response.Message = "This ClientPayment already exists";
                }
                else
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("deleteClientPayment")]
        public ExpandoObject DeleteClientPayment(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientPayment model = JsonConvert.DeserializeObject<ClientPayment>(decryptData);

                var ClientPayment = dbContext.ClientPayments.Where(x => x.ClientPaymentId == model.ClientPaymentId).First();
                dbContext.ClientPayments.Remove(ClientPayment);
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
