using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pin.Page;
using CYCloud.Service;

namespace WebApp._api
{
    /// <summary>
    /// GetToken 的摘要描述
    /// </summary>
    public class GetServiceToken : pin.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            var oData = DeserializeObject<Token>(context.Request.Params[0]);
            if (oResult.Success)
            {
                if (!ValueService.Auths.Check(oData))
                    oData.Result.Error("認證失敗");
                else if (!ValueService.Tokens.CheckDuplicate(oData.ID))
                    oData.Result.Error("重複認證");
                else
                {
                    oData.Guid = Guid.NewGuid().ToString();
                    oData.ExpireDate = DateTime.Now.AddMinutes(5);
                }
            }
            else
                oData.Result.Error("資料格式錯誤");

            context.Response.Write(SerializeObject(oData));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = false, NoCache = true };
        }
    }
}