using iMask.EF;
using iMask.EF.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace iMask
{
    public class LineBotApp : WebhookApplication
    {
        private readonly LineMessagingClient _messagingClient;
        private readonly CoreDbContext _db;
        public LineBotApp(LineMessagingClient lineMessagingClient, CoreDbContext db)
        {
            _messagingClient = lineMessagingClient;
            _db = db;
        }

        protected override async Task OnFollowAsync(FollowEvent ev)
        {
            await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                "使用 LINE 定位，查詢附近口罩庫存數量",
                "資料來源，健康保險資料開放服務",
                "記得要帶健保卡才能購買!!");
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message)
            {
                case LocationEventMessage locationMessage:
                    {
                        var shopList = await _db.Shops.ToListAsync();
                        var rankList = shopList
                            .Select(it => new
                            {
                                rank = Haversine(
                                    (double)it.Latitude, (double)it.Longitude,
                                    (double)locationMessage.Latitude, (double)locationMessage.Longitude),
                                shop = it
                            })
                            .OrderBy(it => it.rank)
                            .Take(5);

                        var shopIds = rankList.Select(it => it.shop.Id).ToArray();
                        var amountDictionary = await _db.Amounts
                            .Where(it => shopIds.Contains(it.ShopId) && it.IsEnable == 1)
                            .ToDictionaryAsync(it => it.ShopId);

                        var messages = new List<ISendMessage>();
                        foreach (var item in rankList)
                        {
                            var amount = null as Amount;
                            amountDictionary.TryGetValue(item.shop.Id, out amount);
                            messages.Add(new TextMessage(
                                $"{item.shop.Name}\n" +
                                $"{item.shop.Phone}\n" +
                                $"{item.shop.Address}\n" +
                                $"成人口罩: {amount?.AdultAmount.ToString() ?? "未知"}\n" +
                                $"兒童口罩: {amount?.ChildAmount.ToString() ?? "未知"}"));
                        }
                        await _messagingClient.ReplyMessageAsync(ev.ReplyToken, messages);
                    }
                    break;
            }
        }

        //計算兩點座標間的距離
        private double Haversine(double lat1, double long1, double lat2, double long2)
        {
            var R = 6371;

            double rad(double x)
            {
                return x * Math.PI / 180;
            }

            var dLat = rad(lat2 - lat1);
            var dLong = rad(long2 - long1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(rad(lat1)) * Math.Cos(rad(lat2))
                    * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            return d;
        }
    }
}
