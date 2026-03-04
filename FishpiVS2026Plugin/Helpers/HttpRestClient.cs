using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FishpiVS2026Plugin.Helpers
{
	public class HttpRestClient : IDisposable
	{
		private readonly RestClient _client;
		private bool _disposed = false;

		public HttpRestClient(string baseUrl)
		{
			if (string.IsNullOrWhiteSpace(baseUrl))
				throw new ArgumentNullException(nameof(baseUrl), "基础地址不能为空");

			// 配置 RestClient 选项
			var options = new RestClientOptions(baseUrl)
			{
				Timeout = TimeSpan.FromSeconds(10),
				ThrowOnAnyError = false,
				UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            };

			_client = new RestClient(options);
        }

		/// <summary>
		/// 添加全局请求头（所有请求都会携带）
		/// </summary>
		/// <param name="key">请求头键</param>
		/// <param name="value">请求头值</param>
		public void AddDefaultHeader(string key, string value)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key), "请求头键不能为空");

			_client.AddDefaultHeader(key, value);
		}

		/// <summary>
		/// GET 请求（同步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径（如：/api/user）</param>
		/// <param name="parameters">请求参数（可选）</param>
		/// <returns>响应结果</returns>
		public RestResponse<T> Get<T>(string resource, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Get, parameters);
			return _client.Execute<T>(request);
		}

		/// <summary>
		/// GET 请求（异步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径</param>
		/// <param name="parameters">请求参数</param>
		/// <returns>响应结果</returns>
		public async Task<RestResponse<T>> GetAsync<T>(string resource, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Get, parameters);
			return await _client.ExecuteAsync<T>(request);
		}

		/// <summary>
		/// POST 请求（同步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径</param>
		/// <param name="body">请求体数据</param>
		/// <param name="parameters">URL 参数</param>
		/// <returns>响应结果</returns>
		public RestResponse<T> Post<T>(string resource, object body = null, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Post, parameters);
			if (body != null)
				request.AddJsonBody(body); // 自动序列化为 JSON
			return _client.Execute<T>(request);
		}

		/// <summary>
		/// POST 请求（异步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径</param>
		/// <param name="body">请求体数据</param>
		/// <param name="parameters">URL 参数</param>
		/// <returns>响应结果</returns>
		public async Task<RestResponse<T>> PostAsync<T>(string resource, object body = null, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Post, parameters);
			if (body != null)
				request.AddJsonBody(body);
			return await _client.ExecuteAsync<T>(request);
		}

		/// <summary>
		/// PUT 请求（异步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径</param>
		/// <param name="body">请求体数据</param>
		/// <param name="parameters">URL 参数</param>
		/// <returns>响应结果</returns>
		public async Task<RestResponse<T>> PutAsync<T>(string resource, object body = null, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Put, parameters);
			if (body != null)
				request.AddJsonBody(body);
			return await _client.ExecuteAsync<T>(request);
		}

		/// <summary>
		/// DELETE 请求（异步）
		/// </summary>
		/// <typeparam name="T">响应数据类型</typeparam>
		/// <param name="resource">接口路径</param>
		/// <param name="parameters">URL 参数</param>
		/// <returns>响应结果</returns>
		public async Task<RestResponse<T>> DeleteAsync<T>(string resource, params Parameter[] parameters) where T : class, new()
		{
			var request = CreateRequest(resource, Method.Delete, parameters);
			return await _client.ExecuteAsync<T>(request);
		}

		/// <summary>
		/// 创建请求对象（封装通用配置）
		/// </summary>
		/// <param name="resource">接口路径</param>
		/// <param name="method">请求方法</param>
		/// <param name="parameters">请求参数</param>
		/// <returns>RestRequest 对象</returns>
		private RestRequest CreateRequest(string resource, Method method, params Parameter[] parameters)
		{
			if (string.IsNullOrWhiteSpace(resource))
				throw new ArgumentNullException(nameof(resource), "接口路径不能为空");

			var request = new RestRequest(resource, method)
			{
				// 自动处理 JSON 序列化/反序列化
				RequestFormat = DataFormat.Json
			};

			// 添加 URL 参数
			if (parameters != null && parameters.Length > 0)
			{
				foreach (var param in parameters)
				{
					request.AddParameter(param);
				}
			}

			return request;
		}

		/// <summary>
		/// 检查响应是否成功
		/// </summary>
		/// <param name="response">响应对象</param>
		/// <returns>是否成功</returns>
		public bool IsResponseSuccess(RestResponse response)
		{
			return response != null
				   && response.StatusCode == HttpStatusCode.OK
				   && response.IsSuccessful;
		}

		#region 释放资源
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				// 释放托管资源
				_client?.Dispose();
			}

			_disposed = true;
		}

		~HttpRestClient()
		{
			Dispose(false);
		}
		#endregion
	}
}
