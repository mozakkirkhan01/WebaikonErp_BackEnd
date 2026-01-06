using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/ClientRenewal")]
    public class ClientRenewalController : ApiController
    {
        [HttpPost]
        [Route("ClientRenewalList")]
        public ExpandoObject ClientRenewalList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientRenewal model = JsonConvert.DeserializeObject<ClientRenewal>(decryptData);

                var list = (from d1 in dbContext.ClientRenewals
                            where (model.ClientRenewalId == d1.ClientRenewalId || model.ClientRenewalId == 0)
                            select new
                            {
                                d1.ClientRenewalId,
                                d1.ClientId,
                                d1.Client.ClientCompanyName,
                                d1.ProjectTypeId,
                                d1.ProjectType.ProjectTypeName,
                                d1.ProjectName,
                                d1.Description,
                                d1.RenewalDate,
                                d1.RenewalAmount,
                                d1.RenewalStatus,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.ClientRenewalList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveClientRenewal")]
        public ExpandoObject SaveClientRenewal(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientRenewal model = JsonConvert.DeserializeObject<ClientRenewal>(decryptData);

                ClientRenewal ClientRenewal;

                // ================= UPDATE =================
                if (model.ClientRenewalId > 0)
                {
                    ClientRenewal = dbContext.ClientRenewals.FirstOrDefault(x => x.ClientRenewalId == model.ClientRenewalId);

                    if (ClientRenewal == null)
                    {
                        response.Message = "ClientRenewal not found";
                        return response;
                    }

                    ClientRenewal.ClientRenewalId = model.ClientRenewalId;
                    ClientRenewal.ClientId = model.ClientId;
                    ClientRenewal.ProjectTypeId = model.ProjectTypeId;
                    ClientRenewal.ProjectName = model.ProjectName;
                    ClientRenewal.Description = model.Description;
                    ClientRenewal.RenewalDate = model.RenewalDate;
                    ClientRenewal.RenewalAmount = model.RenewalAmount;
                    ClientRenewal.RenewalStatus = model.RenewalStatus;
                    ClientRenewal.UpdatedBy = model.CreatedBy; // logged-in staff
                    ClientRenewal.UpdatedOn = DateTime.Now;
                }
                // ================= INSERT =================
                else
                {
                    ClientRenewal = model;
                    if (ClientRenewal.ClientRenewalId == 0)
                    {
                        ClientRenewal.CreatedOn = DateTime.Now;

                        ClientRenewal.UpdatedBy = null;
                        ClientRenewal.UpdatedOn = null;

                        dbContext.ClientRenewals.Add(ClientRenewal);

                    }
                }
                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;



                //response.ClientRenewalId = ClientRenewal.ClientRenewalId;

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                {
                    response.Message = "This ClientRenewal already exists";
                }
                else
                {
                    response.Message = ex.Message;
                }
            }

            return response;
        }

        [HttpPost]
        [Route("deleteClientRenewal")]
        public ExpandoObject DeleteClientRenewal(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ClientRenewal model = JsonConvert.DeserializeObject<ClientRenewal>(decryptData);

                var ClientRenewal = dbContext.ClientRenewals.Where(x => x.ClientRenewalId == model.ClientRenewalId).First();
                dbContext.ClientRenewals.Remove(ClientRenewal);
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
