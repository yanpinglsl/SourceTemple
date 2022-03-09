using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Zhaoxi.MicroService.Framework.HttpApiExtend;
using Zhaoxi.MicroService.Interface;

namespace Zhaoxi.MicroService.Client.Controllers
{
    /// <summary>
    /// dotnet run --urls=http://*:5177
    /// </summary>
    public class TestController : Controller
    {
        #region Identity
        private readonly ILogger<TestController> _logger;
        private readonly IUserService _iUserService;
        private readonly IHttpAPIInvoker _iHttpAPIInvoker;
        private static int iIndex = 0;
        public TestController(ILogger<TestController> logger, IUserService userService
            , IHttpAPIInvoker httpAPIInvoker)
        {
            this._logger = logger;
            this._iUserService = userService;
            this._iHttpAPIInvoker = httpAPIInvoker;
        }
        #endregion

        /// <summary>
        /// http://localhost:5177/Test/Index
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            Console.WriteLine($"This is {nameof(TestController)}.{nameof(Index)}");

            #region 单体架构
            base.ViewBag.Users = this._iUserService.UserAll();//三层架构
            #endregion

            //#region 单个分布式
            ////////等同于把  自己连数据库获取数据  换成 调用WebAPI获取数据
            ////string url = "http://localhost:5726/api/users/all";//5727  5728  5729
            ////string content = this._iHttpAPIInvoker.InvokeApi(url); //this.InvokeApi(url);
            ////base.ViewBag.Users = JsonConvert.DeserializeObject<IEnumerable<User>>(content);
            ////Console.WriteLine($"This is {url} Invoke");
            //#endregion

            //#region Nginx
            ////string url = "http://localhost:8080/api/users/all";//来个Nginx地址，负责转发
            ////string content = this._iHttpAPIInvoker.InvokeApi(url); //this.InvokeApi(url);
            ////base.ViewBag.Users = JsonConvert.DeserializeObject<IEnumerable<User>>(content);
            ////Console.WriteLine($"This is {url} Invoke");
            //#endregion

            //#region Consul
            ////基于Consul去获取地址信息---只有IP:Port--然后调用
            //string url = "http://ZhaoxiUserService/api/users/all";
            //ConsulClient client = new ConsulClient(c =>
            //{
            //    c.Address = new Uri("http://localhost:8500/");
            //    c.Datacenter = "dc1";
            //});//找到consul--像DNS
            //var response = client.Agent.Services().Result.Response;//获取Consul全部服务清单

            //Uri uri = new Uri(url);
            //string groupName = uri.Host;
            //AgentService agentService = null;
            //var dictionary = response.Where(s => s.Value.Service.Equals(groupName, StringComparison.OrdinalIgnoreCase)).ToArray();
            //{
            //    //agentService = dictionary[0].Value;//写死第一个 
            //}
            //{
            //    ////轮询
            //    //agentService = dictionary[iIndex++ % dictionary.Length].Value;
            //}
            //{
            //    ////随机策略--也就是平均一下
            //    //agentService = dictionary[new Random(iIndex++).Next(0, dictionary.Length)].Value;
            //}
            //{
            //    //权重策略---能为不同的进程指定不同的权重值，根据权重值分配请求数
            //    List<KeyValuePair<string, AgentService>> pairsList = new List<KeyValuePair<string, AgentService>>();
            //    foreach (var pair in dictionary)
            //    {
            //        int count = int.Parse(pair.Value.Tags?[0]!);//1  10   30
            //        for (int i = 0; i < count; i++)
            //        {
            //            pairsList.Add(pair);
            //        }
            //    }
            //    //41个  
            //    agentService = pairsList.ToArray()[new Random(iIndex++).Next(0, pairsList.Count())].Value;
            //}

            //url = $"{uri.Scheme}://{agentService.Address}:{agentService.Port}{uri.PathAndQuery}";
            //string content = this._iHttpAPIInvoker.InvokeApi(url); //InvokeApi(url);
            //base.ViewBag.Users = JsonConvert.DeserializeObject<IEnumerable<User>>(content);
            //Console.WriteLine($"This is {url} Invoke");
            //#endregion

            return View();
        }

        private string InvokeApi(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpRequestMessage message = new HttpRequestMessage();
                message.Method = HttpMethod.Get;
                message.RequestUri = new Uri(url);
                var result = httpClient.SendAsync(message).Result;
                string content = result.Content.ReadAsStringAsync().Result;
                return content;
            }
        }
    }
}
