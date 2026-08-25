using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BlockChain.Models;

namespace BlockChain.Services
{
    public class P2PService
    {
        private readonly BlockChainService _blockChainService;
        private readonly int _port;
        private HttpListener _listener;
        private static readonly HttpClient _httpClient = new HttpClient();

        public P2PService(BlockChainService blockChainService, int port)
        {
            _blockChainService = blockChainService;
            _port = port;
        }

        public void StartServer()
        {
            Task.Run(() =>
            {
                try
                {
                    _listener = new HttpListener();
                    _listener.Prefixes.Add($"http://localhost:{_port}/");
                    _listener.Start();

                    while (_listener.IsListening)
                    {
                        var context = _listener.GetContext();
                        ProcessRequest(context);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[Помилка P2P сервера]: {ex.Message}");
                }
            });
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/transaction")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        var transaction = JsonConvert.DeserializeObject<Transaction>(json);

                        if (_blockChainService.AddTransaction(transaction))
                        {
                            response.StatusCode = (int)HttpStatusCode.OK;
                            WriteResponse(response, "Транзакцію додано в Mempool");
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            WriteResponse(response, "Транзакція недійсна або вже існує");
                        }
                    }
                }
                else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/block")
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        var block = JsonConvert.DeserializeObject<Block>(json);

                        string result = _blockChainService.AddReceivedBlock(block);
                        if (result == "OK")
                        {
                            response.StatusCode = (int)HttpStatusCode.OK;
                            WriteResponse(response, "Блок успішно прийнято");
                        }
                        else
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            WriteResponse(response, $"Блок відхилено: {result}");
                        }
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/chain")
                {
                    string json = JsonConvert.SerializeObject(_blockChainService.Chain);
                    response.StatusCode = (int)HttpStatusCode.OK;
                    WriteResponse(response, json);
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    WriteResponse(response, "Не знайдено");
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                WriteResponse(response, ex.Message);
            }
        }

        private void WriteResponse(HttpListenerResponse response, string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        public async Task<bool> SendTransactionAsync(int targetPort, Transaction transaction)
        {
            try
            {
                string json = JsonConvert.SerializeObject(transaction);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"http://localhost:{targetPort}/transaction", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<string> SendBlockAsync(int targetPort, Block block)
        {
            try
            {
                string json = JsonConvert.SerializeObject(block);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"http://localhost:{targetPort}/block", content);
                string responseText = await response.Content.ReadAsStringAsync();
                return responseText;
            }
            catch (Exception ex)
            {
                return $"Помилка з'єднання: {ex.Message}";
            }
        }

        public async Task<string> SyncChainAsync(int targetPort)
        {
            try
            {
                var response = await _httpClient.GetAsync($"http://localhost:{targetPort}/chain");
                if (!response.IsSuccessStatusCode) return "Не вдалося отримати цепь з ноди.";

                string json = await response.Content.ReadAsStringAsync();
                var incomingChain = JsonConvert.DeserializeObject<List<Block>>(json);

                bool replaced = _blockChainService.ReplaceChain(incomingChain);
                if (replaced) return "Ланцюг успішно синхронізовано та оновлено!";
                return "Синхронізацію відхилено (отриманий ланцюг коротший або невалідний).";
            }
            catch (Exception ex)
            {
                return $"Помилка синхронізації: {ex.Message}";
            }
        }
    }
}