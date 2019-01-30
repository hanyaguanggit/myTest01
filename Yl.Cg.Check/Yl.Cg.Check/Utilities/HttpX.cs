using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Yl.Cg.Model.Args;
using Yl.Cg.Model.Args.Auth;
using Yl.Ticket5.Common40.Net;
using Yl.Ticket5.Common40.Utilities;

namespace YL.Check.Utilities
{
    public class HttpX
    {
        public static readonly HttpX Instance = new HttpX();

        public void Get<T>(string url, string paras, bool exclusive, DataUploaderCompleted<T> onComplated, Stream requestStream = null)
        {
            HttpTransfer.Instance.Get(url, paras, exclusive, onComplated, requestStream);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="route"></param>
        /// <param name="paras"></param>
        /// <param name="exclusive"></param>
        /// <param name="onComplated"></param>
        /// <param name="requestStream"></param>
        public void Post<T>(string route, string paras, bool exclusive, DataUploaderCompleted<T> onComplated, Stream requestStream = null)
        {
            if (requestStream == null)
            {
                requestStream = new MemoryStream();
            }
            AuthInfoArg info = new AuthInfoArg()
            {
                CreateTime = DateTime.Now.Ticks,
                LoginUserId = Config.Instance.LoginUser.Id,
            };
            string url = System.IO.Path.Combine(
                Config.Instance.InterfacePath, route,
                string.Format("?auth={0}", TextureHelper.Encrypto(Yl.Ticket5.Common40.Utilities.TextureHelper.ToJson(info), "ylcg_TAT").Replace("+", "{plus}")));
            DataUploaderCompleted<T> action = new DataUploaderCompleted<T>(arg =>
            {
                onComplated(arg);
                requestStream.Dispose();
            });
            HttpTransfer.Instance.Post<T>(url, paras, exclusive, onComplated, requestStream);
        }
    }
}