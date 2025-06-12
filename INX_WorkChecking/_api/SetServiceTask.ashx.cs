using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pin.Page;
using CYCloud.Service;

namespace WebApp._api
{
    /// <summary>
    /// SetServiceTask 的摘要描述
    /// </summary>
    public class SetServiceTask : pin.Page.BaseHandler
    {
        protected override void DoHandler(HttpContext context)
        {
            var oData = DeserializeObject<Task>(context.Request.Params[0]);
            if (oResult.Success)
            {
                oData.Token.Result.Reset();

                if (ValueService.Tokens.CheckExist(oData.Token.Guid))
                {
                    ValueService.Tasks.Add(oData);
                }
                else
                    oData.Token.Result.Error("尚未取得認證或認證已過期");
            }
            else
                oData.Token.Result.Error("資料格式錯誤");

            context.Response.Write(SerializeObject(oData));
        }

        protected override BaseHandlerOption SetBaseOption()
        {
            return new BaseHandlerOption() { Session = false, NoCache = true };
        }
    }
}