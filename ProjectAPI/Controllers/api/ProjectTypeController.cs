using Newtonsoft.Json;
using Project;
using System;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ProjectAPI.Controllers.api
{
    [RoutePrefix("api/Project")]
    public class ProjectTypeController : ApiController
    {

            [HttpPost]
            [Route("ProjectList")]
            public ExpandoObject ProjectList(RequestModel requestModel)
            {
                dynamic response = new ExpandoObject();
                try
                {
                    WebaikonErpEntities dbContext = new WebaikonErpEntities();
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ProjectType model = JsonConvert.DeserializeObject<ProjectType>(decryptData);

                    var list = (from d1 in dbContext.ProjectTypes
                                where (model.ProjectTypeId == d1.ProjectTypeId || model.ProjectTypeId == 0)
                                && (model.Status == d1.Status || model.Status == 0)
                                orderby d1.ProjectTypeName
                                select new
                                {
                                    d1.ProjectTypeId,
                                    d1.ProjectTypeName,
                                    d1.ProjectDescription,
                                    d1.Status,
                                }).ToList();

                    response.ProjectList = list;
                    response.Message = ConstantData.SuccessMessage;
                }
                catch (Exception ex)
                {
                    response.Message = ex.Message;
                }
                return response;
            }

            [HttpPost]
            [Route("saveProject")]
            public ExpandoObject SaveProject(RequestModel requestModel)
            {
                dynamic response = new ExpandoObject();
            try
            {
                WebaikonErpEntities dbContext = new WebaikonErpEntities();
                string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                ProjectType model = JsonConvert.DeserializeObject<ProjectType>(decryptData);

                ProjectType Project;

                // ================= UPDATE =================
                if (model.ProjectTypeId > 0)
                {
                    Project = dbContext.ProjectTypes.FirstOrDefault(x => x.ProjectTypeId == model.ProjectTypeId);

                    if (Project == null)
                    {
                        response.Message = "Project not found";
                        return response;
                    }

                    Project.ProjectTypeName = model.ProjectTypeName;
                    Project.ProjectDescription = model.ProjectDescription;
                    Project.Status = model.Status;
                }
                // ================= INSERT =================
                else
                {
                    Project = new ProjectType
                    {

                        ProjectTypeName = model.ProjectTypeName,
                        ProjectDescription = model.ProjectDescription,
                        Status = model.Status,
                    };
                }

                dbContext.ProjectTypes.Add(Project);
            

                    dbContext.SaveChanges();
                    response.Message = ConstantData.SuccessMessage;
                    response.ProjectTypeId = Project.ProjectTypeId;

                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("IX"))
                    {
                        response.Message = "This Project already exists";
                    }
                    else
                    {
                        response.Message = ex.Message;
                    }
                }

                return response;
            }

            [HttpPost]
            [Route("deleteProject")]
            public ExpandoObject DeleteProject(RequestModel requestModel)
            {
                dynamic response = new ExpandoObject();
                try
                {
                    WebaikonErpEntities dbContext = new WebaikonErpEntities();
                    string AppKey = HttpContext.Current.Request.Headers["AppKey"];
                    AppData.CheckAppKey(dbContext, AppKey, (byte)KeyFor.Admin);
                    var decryptData = CryptoJs.Decrypt(requestModel.request, CryptoJs.key, CryptoJs.iv);
                    ProjectType model = JsonConvert.DeserializeObject<ProjectType>(decryptData);

                    var Project = dbContext.ProjectTypes.Where(x => x.ProjectTypeId == model.ProjectTypeId).First();
                    dbContext.ProjectTypes.Remove(Project);
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
