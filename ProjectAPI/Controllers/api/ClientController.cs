using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Client")]
    public class ClientController : ApiController
    {

        [HttpPost]
        [Route("ClientList")]
        public ExpandoObject ClientList(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Client model = JsonConvert.DeserializeObject<Client>(decryptData);

                var list = (from d1 in dbContext.Clients
                            where (model.ClientId == d1.ClientId || model.ClientId == 0)
                            && (model.Status == d1.Status || model.Status == 0)
                            orderby d1.ClientCompanyName
                            select new
                            {
                                d1.ClientId,
                                d1.ClientCompanyName,
                                d1.ContactPersonName,
                                d1.MobileNo,
                                d1.AlternateMobileNo,
                                d1.Email,
                                d1.GSTNo,
                                d1.ClientFullAddress,
                                d1.StateId,
                                d1.State.StateName,
                                d1.StateCode,
                                d1.Status,
                                d1.CreatedBy,
                                d1.CreatedOn,
                                d1.UpdatedBy,
                                d1.UpdatedOn,
                            }).ToList();

                response.ClientList = list;
                response.Message = ConstantData.SuccessMessage;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        [HttpPost]
        [Route("saveClient")]
        public ExpandoObject SaveClient(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Client model = JsonConvert.DeserializeObject<Client>(decryptData);

                Client client;

                // ================= UPDATE =================
                if (model.ClientId > 0)
                {
                    client = dbContext.Clients
                                      .FirstOrDefault(x => x.ClientId == model.ClientId);

                    if (client == null)
                    {
                        response.Message = "Client not found";
                        return response;
                    }

                    client.ClientCompanyName = model.ClientCompanyName;
                    client.ContactPersonName = model.ContactPersonName;
                    client.MobileNo = model.MobileNo;
                    client.AlternateMobileNo = model.AlternateMobileNo;
                    client.Email = model.Email;
                    client.GSTNo = model.GSTNo;
                    client.ClientFullAddress = model.ClientFullAddress;
                    client.StateId = model.StateId;
                    client.StateCode = model.StateCode;
                    client.Status = model.Status;

                    client.UpdatedBy = model.UpdatedBy;
                    client.UpdatedOn = DateTime.Now;
                }
                // ================= INSERT =================
                else
                {
                    client = new Client
                    {
                        ClientCompanyName = model.ClientCompanyName,
                        ContactPersonName = model.ContactPersonName,
                        MobileNo = model.MobileNo,
                        AlternateMobileNo = model.AlternateMobileNo,
                        Email = model.Email,
                        GSTNo = model.GSTNo,
                        ClientFullAddress = model.ClientFullAddress,
                        StateId = model.StateId,
                        StateCode = model.StateCode,
                        Status = model.Status,

                        CreatedBy = model.CreatedBy,
                        CreatedOn = DateTime.Now
                    };

                    dbContext.Clients.Add(client);
                }

                dbContext.SaveChanges();
                response.Message = ConstantData.SuccessMessage;
                response.ClientId = client.ClientId;
            
            }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                    {
                        response.Message = "This client already exists";
                    }
                    else
                    {
                        response.Message = ex.Message;
                    }
                }

                return response;
        }

        [HttpPost]
        [Route("deleteClient")]
        public ExpandoObject DeleteClient(RequestModel requestModel)
        {
            dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                Client model = JsonConvert.DeserializeObject<Client>(decryptData);

                var Client = dbContext.Clients.Where(x => x.ClientId == model.ClientId).First();
                dbContext.Clients.Remove(Client);
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
