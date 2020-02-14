using iMask.Models;
using Line.Messaging;
using Line.Messaging.Webhooks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly CacheService _cacheService;
        public LineBotApp(LineMessagingClient lineMessagingClient,
            CacheService cacheService)
        {
            _messagingClient = lineMessagingClient;
            _cacheService = cacheService;
        }

        protected override async Task OnFollowAsync(FollowEvent ev)
        {
            await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                new List<ISendMessage> {
                    new TextMessage("機器人可查詢附近口罩庫存數量"),
                    new TextMessage("查詢方式: 傳送 LINE 定位"),
                    new TextMessage("資料來源: 健康保險資料開放服務"),
                    new ImageMessage(
                        "https://fysh711426.github.io/iMaskMap/image/mask_info.jpg",
                        "https://fysh711426.github.io/iMaskMap/image/mask_info_preview.jpg",
                        new QuickReply
                        {
                            Items = new List<QuickReplyButtonObject>
                            {
                                new QuickReplyButtonObject(
                                    new LocationTemplateAction("查詢"))
                            }
                        })
                    });
        }

        protected override async Task OnPostbackAsync(PostbackEvent ev)
        {
            //將 data 資料轉成 QueryString
            var query = HttpUtility.ParseQueryString(ev.Postback.Data);

            if (query["type"] == "search")
            {
                var page = int.Parse(query["page"]);
                var skip = (page - 1) * 5;

                var latitude = decimal.Parse(query["latitude"]);
                var longitude = decimal.Parse(query["longitude"]);

                var rankList = GetRankList(
                    latitude, longitude, skip);

                var updateTime = rankList
                    .Where(it => it.properties.updated != "")
                    .Select(it => new DateTime?(DateTime.Parse(it.properties.updated)))
                    .FirstOrDefault();

                var flexMessage = GetFlexMessage(rankList, page,
                    latitude, longitude, updateTime);

                await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                    new List<ISendMessage> {
                        flexMessage
                    });
            }

            if (query["type"] == "map")
            {
                var page = int.Parse(query["page"]);

                var latitude = decimal.Parse(query["latitude"]);
                var longitude = decimal.Parse(query["longitude"]);

                await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                    new List<ISendMessage> {
                        new TextMessage($"https://fysh711426.github.io/iMaskMap/index.html?latitude={latitude}&longitude={longitude}",
                        new QuickReply
                        {
                            Items = new List<QuickReplyButtonObject>
                            {
                                new QuickReplyButtonObject(
                                    new LocationTemplateAction("查詢")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("重新整理",
                                        $"type=search&page={page}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("下一頁",
                                        $"type=search&page={page+1}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("口罩地圖",
                                        $"type=map&page={page}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("疫情地圖",
                                        $"type=COVID&page={page}&latitude={latitude}&longitude={longitude}"))
                            }
                        })
                    });
            }

            if (query["type"] == "COVID")
            {
                var page = int.Parse(query["page"]);

                var latitude = decimal.Parse(query["latitude"]);
                var longitude = decimal.Parse(query["longitude"]);

                await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                    new List<ISendMessage> {
                        new TextMessage($"https://fysh711426.github.io/iCOVID/index.html",
                        new QuickReply
                        {
                            Items = new List<QuickReplyButtonObject>
                            {
                                new QuickReplyButtonObject(
                                    new LocationTemplateAction("查詢")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("重新整理",
                                        $"type=search&page={page}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("下一頁",
                                        $"type=search&page={page+1}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("口罩地圖",
                                        $"type=map&page={page}&latitude={latitude}&longitude={longitude}")),
                                new QuickReplyButtonObject(
                                    new PostbackTemplateAction("疫情地圖",
                                        $"type=COVID&page={page}&latitude={latitude}&longitude={longitude}"))
                            }
                        })
                    });
            }
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            switch (ev.Message)
            {
                case LocationEventMessage locationMessage:
                    {
                        var page = 1;
                        var skip = 0;

                        var rankList = GetRankList(
                            locationMessage.Latitude, 
                            locationMessage.Longitude, skip);

                        var updateTime = rankList
                            .Where(it => it.properties.updated != "")
                            .Select(it => new DateTime?(DateTime.Parse(it.properties.updated)))
                            .FirstOrDefault();

                        var flexMessage = GetFlexMessage(rankList, page,
                            locationMessage.Latitude, locationMessage.Longitude, updateTime);

                        await _messagingClient.ReplyMessageAsync(ev.ReplyToken,
                            new List<ISendMessage> {
                                new TextMessage("部分藥局因採發放號碼牌方式，方便民眾購買口罩，系統目前無法顯示已發送號碼牌數量"),
                                new TextMessage("口罩數量以藥局實際存量為主，線上查詢之數量僅供參考"),
                                flexMessage
                            });
                    }
                    break;
            }
        }

        private FlexMessage GetFlexMessage(List<Feature> rankList, int page, decimal latitude, decimal longitude, DateTime? updateTime)
        {
            var flexMessage = new FlexMessage($"口罩數量 {page}")
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
                                Text = $"口罩數量 {page}",
                                Size = ComponentSize.Lg,
                                Weight = Weight.Bold,
                                Color = "#000000"
                            },
                            new SeparatorComponent()
                        }
                    }
                }
            };

            var boxs = (flexMessage.Contents as BubbleContainer)
                .Body.Contents;

            foreach (var item in rankList)
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
                                            Text = item.properties.name,
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
                                            Text = $"[電話] {item.properties.phone}",
                                            Color = "#928D8B",
                                            Size = ComponentSize.Sm,
                                            Weight = Weight.Bold
                                        },
                                        new TextComponent
                                        {
                                            Text = $"[口罩] 成人: {item.properties.mask_adult}、兒童: {item.properties.mask_child}",
                                            Size = ComponentSize.Sm,
                                            Color = "#000000",
                                            Weight = Weight.Bold
                                        },
                                        new TextComponent
                                        {
                                            Text = $"[地址] {item.properties.address}",
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
                                            Action = new UriTemplateAction("action",
                                                $"https://www.google.com/maps/search/?api=1&query={item.geometry.coordinates[1]},{item.geometry.coordinates[0]}")
                                        }
                                    }
                                }
                            }
                        },
                        new SeparatorComponent()
                    }
                });
            }

            boxs.Add(new FixFlex.TextComponent
            {
                Text = $"更新時間: {updateTime?.ToString("MM/dd HH:mm") ?? ""}",
                Size = ComponentSize.Sm,
                Weight = Weight.Bold,
                Color = "#928D8B",
                Align = Align.Start,
                OffsetTop = "5px"
            });

            flexMessage.QuickReply = new QuickReply
            {
                Items = new List<QuickReplyButtonObject>
                {
                    new QuickReplyButtonObject(
                        new LocationTemplateAction("查詢")),
                    new QuickReplyButtonObject(
                        new PostbackTemplateAction("重新整理",
                            $"type=search&page={page}&latitude={latitude}&longitude={longitude}")),
                    new QuickReplyButtonObject(
                        new PostbackTemplateAction("下一頁", 
                            $"type=search&page={page+1}&latitude={latitude}&longitude={longitude}")),
                    new QuickReplyButtonObject(
                        new PostbackTemplateAction("口罩地圖",
                            $"type=map&page={page}&latitude={latitude}&longitude={longitude}")),
                    new QuickReplyButtonObject(
                        new PostbackTemplateAction("疫情地圖",
                            $"type=COVID&page={page}&latitude={latitude}&longitude={longitude}"))
                }
            };

            return flexMessage;
        }

        private List<Feature> GetRankList(decimal latitude, decimal longitude, int skip)
        {
            var json = _cacheService.GetJson();
            var dataList = json.features;
            var rankList = dataList
                .Select(it => new
                {
                    rank = Haversine(
                        (double)it.geometry.coordinates[1], (double)it.geometry.coordinates[0],
                        (double)latitude, (double)longitude),
                    data = it
                })
                .OrderBy(it => it.rank)
                .Skip(skip).Take(5)
                .Select(it => it.data)
                .ToList();
            return rankList;
        }

        //計算兩點間的距離
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
