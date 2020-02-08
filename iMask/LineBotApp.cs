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
using System.Web;

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
                "機器人可查詢附近口罩庫存數量",
                "查詢方式: 傳送 LINE 定位",
                "資料來源: 健康保險資料開放服務",
                "出門前記得帶健保卡!!");
        }

        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            //將 data 資料轉成 QueryString
            var query = HttpUtility.ParseQueryString(ev.Postback.Data);

            if (query["type"] == "location")
            {
                var amount = await _db.Amounts.Where(it => it.Code == query["code"])
                    .FirstOrDefaultAsync();
                await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                    new List<ISendMessage> {
                    new LocationMessage(amount.Name, amount.Address, amount.Latitude, amount.Longitude) });
            }
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message)
            {
                case LocationEventMessage locationMessage:
                    {
                        var amountList = await _db.Amounts
                            .AsNoTracking()
                            .ToListAsync();
                        var rankList = amountList
                            .Select(it => new
                            {
                                rank = Haversine(
                                    (double)it.Latitude, (double)it.Longitude,
                                    (double)locationMessage.Latitude, (double)locationMessage.Longitude),
                                amount = it
                            })
                            .OrderBy(it => it.rank);

                        FlexMessage create(IEnumerable<Amount> amounts)
                        {
                            var flexMessage = new FlexMessage("口罩庫存")
                            {
                                Contents = new BubbleContainer
                                {
                                    Body = new BoxComponent
                                    {
                                        Layout = BoxLayout.Vertical,
                                        Spacing = Spacing.Md,
                                        Contents = new List<IFlexComponent>
                                    {
                                        new TextComponent
                                        {
                                            Text = "口罩庫存",
                                            Size = ComponentSize.Lg,
                                            Weight = Weight.Bold,
                                            Color = "#000000"
                                        },
                                        new SeparatorComponent
                                        {

                                        }
                                    }
                                    }
                                }
                            };

                            var boxs = (flexMessage.Contents as BubbleContainer)
                                .Body.Contents;

                            foreach (var item in amounts)
                            {
                                boxs.Add(new FixFlex.BoxComponent
                                {
                                    Layout = BoxLayout.Vertical,
                                    Spacing = Spacing.Sm,
                                    OffsetStart = "-2px",
                                    Contents = new List<IFlexComponent>
                                {
                                    new BoxComponent
                                    {
                                        Layout = BoxLayout.Horizontal,
                                        Contents = new List<IFlexComponent>
                                        {
                                            new FixFlex.BoxComponent
                                            {
                                                Layout = BoxLayout.Vertical,
                                                PaddingStart = "5px",
                                                Spacing = Spacing.Xs,
                                                Contents = new List<IFlexComponent>
                                                {
                                                    new TextComponent
                                                    {
                                                        Text = item.Name,
                                                        Weight = Weight.Bold,
                                                        Margin = Spacing.Sm,
                                                        Flex = 0,
                                                        Wrap = true,
                                                        MaxLines = 2,
                                                        Size = ComponentSize.Md,
                                                        Color = "#000000"
                                                    },
                                                    new TextComponent
                                                    {
                                                        Text = $"[電話] {item.Phone}",
                                                        Color = "#928D8B",
                                                        Size = ComponentSize.Sm,
                                                        Weight = Weight.Bold
                                                    },
                                                    new TextComponent
                                                    {
                                                        Text = $"[庫存] 成人: {item.AdultAmount?.ToString() ?? "未知"}、兒童: 50{item.ChildAmount?.ToString() ?? "未知"}",
                                                        Size = ComponentSize.Sm,
                                                        Color = "#000000",
                                                        Weight = Weight.Bold
                                                    },
                                                    new TextComponent
                                                    {
                                                        Text = $"[地址] {item.Address}",
                                                        Size = ComponentSize.Sm,
                                                        Color = "#928D8B",
                                                        MaxLines = 2,
                                                        Wrap = true,
                                                        Weight = Weight.Bold
                                                    }
                                                }
                                            },
                                            new FixFlex.BoxComponent
                                            {
                                                Layout = BoxLayout.Vertical,
                                                BackgroundColor = "#905C44",
                                                CornerRadius = "3px",
                                                Margin = Spacing.Sm,
                                                Width = "40px",
                                                Height = "35px",
                                                Flex = 0,
                                                OffsetEnd = "-0px",
                                                OffsetTop = "30px",
                                                Contents = new List<IFlexComponent>
                                                {
                                                    new FixFlex.TextComponent
                                                    {
                                                        Text = "地圖",
                                                        Flex = 0,
                                                        Align = Align.Center,
                                                        Size = ComponentSize.Sm,
                                                        Color = "#ffffff",
                                                        OffsetTop = "8px",
                                                        Weight = Weight.Bold,
                                                        Action = new PostbackTemplateAction("action", $"type=location&code={item.Code}")
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new SeparatorComponent
                                    {

                                    }
                                }
                                });
                            }

                            return flexMessage;
                        }

                        var flexMessage1 = create(rankList.Take(5).Select(it => it.amount));
                        var flexMessage2 = create(rankList.Skip(5).Take(5).Select(it => it.amount));

                        await _messagingClient.ReplyMessageAsync(ev.ReplyToken, 
                            new List<ISendMessage> { flexMessage1, flexMessage2,
                            new TextMessage("資料來源: 健康保險資料開放服務") });
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
                    Math.Cos(rad(lat1)) * Math.Cos(rad(lat2)) *
                    Math.Sin(dLong / 2) * Math.Sin(dLong / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;

            return d;
        }
    }
}
