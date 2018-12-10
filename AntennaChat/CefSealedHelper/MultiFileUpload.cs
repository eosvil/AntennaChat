using Antenna.Framework;
using Antenna.Model;
using Antenna.Model.MultiUpload;
using Antenna.Model.PictureAndTextMix;
using AntennaChat.ViewModel.Talk;
using CefSharp;
using Newtonsoft.Json;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;
using SDK.AntSdk.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Antenna.Model.ModelEnum;
using static SDK.AntSdk.AntModels.AntSdkChatMsg;

namespace AntennaChat.CefSealedHelper
{
    public class MultiFileUpload
    {
        //多文件上传方法
        public async void MultiFileHttpClientUpLoad(AntSdkMsgType antSdkMsgType, CurrentChatDto currentChat, MessageStateArg arg, MixMsg mixMsg, List<MixMessageObjDto> obj)
        {
            try
            {
                var url = AntSdkService.AntSdkConfigInfo.AntSdkMultiFileUpload;
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("user-agent", "User-Agent    Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; Touch; MALNJS; rv:11.0) like Gecko");
                HttpResponseMessage response;
                MultipartFormDataContent mulContent = new MultipartFormDataContent();
                foreach (var list in mixMsg.TagDto)
                {
                    FileStream fs = new FileStream(@list.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    HttpContent fileContent = new StreamContent(fs);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                    int pos = list.Path.LastIndexOf("/");
                    if (pos == -1)
                    {
                        pos = list.Path.LastIndexOf("\\");
                    }
                    string fileName = list.Path.Substring(pos + 1);

                    mulContent.Add(fileContent, list.PreGuid, fileName);
                }
                string result = "";
                if (mixMsg.TagDto.Count() > 0)
                {
                    response = await client.PostAsync(new Uri(url), mulContent);
                    //var code = response.EnsureSuccessStatusCode();

                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    result = JsonConvert.SerializeObject(new BaseMultiDto() { errorCode = "0", errorMsg = "", data = null });
                }
                if (!string.IsNullOrEmpty(result))
                {
                    BaseMultiDto baseMultiDto = JsonConvert.DeserializeObject<BaseMultiDto>(result);
                    if (baseMultiDto.errorCode == "0")
                    {
                        string JsonContent = "";
                        string mixError = "";
                        bool isResult = false;
                        int i = 0;
                        if (mixMsg.TagDto.Count() > 0)
                        {
                            foreach (var list in obj)
                            {
                                //var type = list as MixMessageBase;
                                switch (list.type)
                                {
                                    case "1002":
                                        //var picDto = list as MixMessageDto;
                                        PictureDto pic = JsonConvert.DeserializeObject<PictureDto>(list.content?.ToString());
                                        list.content = JsonConvert.SerializeObject(new PictureDto() { width = "50", height = "50", picUrl = baseMultiDto.data[i].dowmnloadUrl });
                                        i++;
                                        break;
                                }
                            }
                        }

                        #region New
                        List<MixMessage_content> ListMixMsg = new List<MixMessage_content>();
                        AntSdkChatMsg.MixMessage mixImageText = new AntSdkChatMsg.MixMessage();

                        foreach (var list in obj)
                        {
                            MixMessage_content message_Content = new MixMessage_content();
                            //var type = list as MixMessageBase;
                            switch (list.type)
                            {
                                case "1001":
                                    //var text = list as MixMessageDto;
                                    message_Content.type = list.type;
                                    message_Content.content = list.content;
                                    break;
                                case "1002":
                                    //var picDto = list as MixMessageDto;
                                    message_Content.type = list.type;
                                    message_Content.content = list.content;
                                    break;
                                case "1008":
                                    //var at = list as MixMessageObjDto;
                                    if (currentChat.isOnceSend)
                                    {
                                        message_Content.type = list.type;
                                        message_Content.content = list.content;
                                    }
                                    else
                                    {
                                        List<object> listObj = new List<object>();
                                        message_Content.type = list.type;
                                        listObj.Add(list.content);
                                        message_Content.content = listObj;
                                    }
                                    break;
                                //换行
                                case "0000":
                                    message_Content.type = list.type;
                                    message_Content.content = "";
                                    break;
                            }
                            ListMixMsg.Add(message_Content);
                        }
                        mixImageText.content = ListMixMsg; ;
                        mixImageText.MsgType = AntSdkMsgType.ChatMsgMixMessage;
                        mixImageText.messageId = currentChat.messageId;
                        mixImageText.flag = 0;
                        mixImageText.sendUserId = currentChat.sendUserId;
                        mixImageText.sessionId = currentChat.sessionId;
                        mixImageText.targetId = currentChat.targetId;
                        mixImageText.chatType = (int)currentChat.type;
                        mixImageText.os = (int)GlobalVariable.OSType.PC;
                        if (currentChat.type == AntSdkchatType.Point)
                        {
                            //消息监控
                            var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == arg.MessageId);
                            if (IsHave != null)
                            {
                                SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                            }
                            if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(arg.MessageId))
                            {
                                SendMsgListPointMonitor.MsgIdAndImgSendingId[arg.MessageId] = arg.SendIngId;
                            }
                            else
                            {
                                SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(arg.MessageId, arg.SendIngId);
                            }
                            SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                        }
                        else
                        {
                            //消息监控
                            var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == arg.MessageId);
                            if (IsHave != null)
                            {
                                SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                            }
                            if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(arg.MessageId))
                            {
                                SendMsgListMonitor.MsgIdAndImgSendingId[arg.MessageId] = arg.SendIngId;
                            }
                            else
                            {
                                SendMsgListMonitor.MsgIdAndImgSendingId.Add(arg.MessageId, arg.SendIngId);
                            }
                            SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                        }
                        //发送
                        if (currentChat.isOnceSend)
                        {
                            isResult = AntSdkService.SdkRePublishChatMsg(mixImageText, ref mixError);
                        }
                        else
                        {
                            isResult = AntSdkService.SdkPublishChatMsg(mixImageText, ref mixError);
                            //if (mixMsg.TagDto.Count() == 0)
                            //{
                            //    #region 滚动条置底
                            //    arg.WebBrowser.GetMainFrame().ExecuteJavaScriptAsync("setscross();");
                            //    #endregion
                            //}
                        }

                        #endregion

                        #region old
                        //if (antSdkMsgType == AntSdkMsgType.ChatMsgMixImageText)
                        //{
                        //AntSdkChatMsg.MixImageText mixImageText = new AntSdkChatMsg.MixImageText();
                        //List<AntSdkChatMsg.MixImageText_content> imageText_Content = new List<AntSdkChatMsg.MixImageText_content>();
                        //mixImageText.messageId = currentChat.messageId;
                        //foreach (var lists in picAndTxtMix)
                        //{
                        //    if (lists.type == PictureAndTextMixEnum.Image)
                        //    {
                        //        imageText_Content.Add(new AntSdkChatMsg.MixImageText_content()
                        //        {
                        //            type = (Convert.ToInt32(lists.type).ToString()) == "0" ? "0000" : Convert.ToInt32(lists.type).ToString(),
                        //            content = JsonConvert.SerializeObject(new
                        //            PictureAndTextMixContentDto
                        //            { picUrl = lists.content })
                        //        });
                        //    }
                        //    else
                        //    {
                        //        imageText_Content.Add(new AntSdkChatMsg.MixImageText_content()
                        //        {
                        //            type = (Convert.ToInt32(lists.type).ToString()) == "0" ? "0000" : Convert.ToInt32(lists.type).ToString(),
                        //            content = lists.content
                        //        });
                        //    }
                        //}
                        //JsonContent = JsonConvert.SerializeObject(imageText_Content);
                        //mixImageText.content = imageText_Content;
                        //mixImageText.MsgType = AntSdkMsgType.ChatMsgMixImageText;
                        //mixImageText.flag = 0;
                        //mixImageText.sendUserId = currentChat.sendUserId;
                        //mixImageText.sessionId = currentChat.sessionId;
                        //mixImageText.targetId = currentChat.targetId;
                        //mixImageText.chatType = (int)currentChat.type;
                        //mixImageText.os = (int)GlobalVariable.OSType.PC;
                        //    if (currentChat.type == AntSdkchatType.Point)
                        //    {
                        //        //消息监控
                        //        var IsHave = SendMsgListPointMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == arg.MessageId);
                        //        if (IsHave != null)
                        //        {
                        //            SendMsgListPointMonitor.MessageStateMonitorList.Remove(IsHave);
                        //        }
                        //        if (SendMsgListPointMonitor.MsgIdAndImgSendingId.ContainsKey(arg.MessageId))
                        //        {
                        //            SendMsgListPointMonitor.MsgIdAndImgSendingId[arg.MessageId] = arg.SendIngId;
                        //        }
                        //        else
                        //        {
                        //            SendMsgListPointMonitor.MsgIdAndImgSendingId.Add(arg.MessageId, arg.SendIngId);
                        //        }
                        //        SendMsgListPointMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                        //    }
                        //    else
                        //    {
                        //        //消息监控
                        //        var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == arg.MessageId);
                        //        if (IsHave != null)
                        //        {
                        //            SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                        //        }
                        //        if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(arg.MessageId))
                        //        {
                        //            SendMsgListMonitor.MsgIdAndImgSendingId[arg.MessageId] = arg.SendIngId;
                        //        }
                        //        else
                        //        {
                        //            SendMsgListMonitor.MsgIdAndImgSendingId.Add(arg.MessageId, arg.SendIngId);
                        //        }
                        //        SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                        //    }
                        //    //发送
                        //    if (currentChat.isOnceSend)
                        //    {
                        //        isResult = AntSdkService.SdkRePublishChatMsg(mixImageText, ref mixError);
                        //    }
                        //    else
                        //    {
                        //        isResult = AntSdkService.SdkPublishChatMsg(mixImageText, ref mixError);
                        //        #region 滚动条置底
                        //        StringBuilder sbEnd = new StringBuilder();
                        //        sbEnd.AppendLine("setscross();");
                        //        arg.WebBrowser.ExecuteScriptAsync(sbEnd.ToString());
                        //        #endregion
                        //    }
                        //}
                        //else
                        //{
                        //    At at = new At();
                        //    List<At_content> atContent = new List<At_content>();
                        //    at.messageId = currentChat.messageId;
                        //    foreach (var lists in picAndTxtMix)
                        //    {
                        //        switch (lists.type)
                        //        {
                        //            case PictureAndTextMixEnum.Text:
                        //                atContent.Add(new At_content()
                        //                {
                        //                    type = Convert.ToInt32(lists.type).ToString(),
                        //                    content = lists.content
                        //                });
                        //                break;
                        //            case PictureAndTextMixEnum.Image:
                        //                atContent.Add(new At_content()
                        //                {
                        //                    type = Convert.ToInt32(lists.type).ToString(),
                        //                    content = JsonConvert.SerializeObject(new
                        //                PictureAndTextMixContentDto
                        //                    { picUrl = lists.content })
                        //                });
                        //                break;
                        //            case PictureAndTextMixEnum.LineBreak:
                        //                atContent.Add(new At_content()
                        //                {
                        //                    type = Convert.ToInt32(lists.type).ToString() + "000",
                        //                    content = lists.content
                        //                });
                        //                break;
                        //            case PictureAndTextMixEnum.AtAll:
                        //                atContent.Add(new At_content()
                        //                {
                        //                    type = Convert.ToInt32(lists.type).ToString(),
                        //                });
                        //                break;
                        //            case PictureAndTextMixEnum.AtPerson:
                        //                var person = lists.obj as AntSdkChatMsg.contentAtOrdinary;
                        //                if (person == null)
                        //                {
                        //                    atContent.Add(new At_content()
                        //                    {
                        //                        type = Convert.ToInt32(lists.type).ToString(),
                        //                        ids = lists.ids,
                        //                        names = lists.names
                        //                    });
                        //                }
                        //                else
                        //                {
                        //                    atContent.Add(new At_content()
                        //                    {
                        //                        type = Convert.ToInt32(lists.type).ToString(),
                        //                        ids = person.ids,
                        //                        names = person.names
                        //                    });
                        //                }
                        //                break;
                        //        }
                        //    }
                        //    JsonContent = JsonConvert.SerializeObject(atContent);
                        //    at.sourceContent = JsonContent;
                        //    at.MsgType = AntSdkMsgType.ChatMsgAt;
                        //    at.flag = 0;
                        //    at.sendUserId = currentChat.sendUserId;
                        //    at.sessionId = currentChat.sessionId;
                        //    at.targetId = currentChat.targetId;
                        //    at.chatType = (int)currentChat.type;
                        //    at.os = (int)GlobalVariable.OSType.PC;
                        //    //消息监控
                        //    var IsHave = SendMsgListMonitor.MessageStateMonitorList.SingleOrDefault(m => m.dispatcherTimer.arg.MessageId == arg.MessageId);
                        //    if (IsHave != null)
                        //    {
                        //        SendMsgListMonitor.MessageStateMonitorList.Remove(IsHave);
                        //    }
                        //    if (SendMsgListMonitor.MsgIdAndImgSendingId.ContainsKey(arg.MessageId))
                        //    {
                        //        SendMsgListMonitor.MsgIdAndImgSendingId[arg.MessageId] = arg.SendIngId;
                        //    }
                        //    else
                        //    {
                        //        SendMsgListMonitor.MsgIdAndImgSendingId.Add(arg.MessageId, arg.SendIngId);
                        //    }
                        //    SendMsgListMonitor.MessageStateMonitorList.Add(new SendMsgStateMonitor(arg));
                        //    //发送
                        //    if (currentChat.isOnceSend)
                        //    {
                        //        isResult = AntSdkService.SdkRePublishChatMsg(at, ref mixError);
                        //    }
                        //    else
                        //    {
                        //        isResult = AntSdkService.SdkPublishChatMsg(at, ref mixError);
                        //        #region 滚动条置底
                        //        StringBuilder sbEnd = new StringBuilder();
                        //        sbEnd.AppendLine("setscross();");
                        //        arg.WebBrowser.ExecuteScriptAsync(sbEnd.ToString());
                        //        #endregion
                        //    }
                        //}
                        #endregion
                        if (isResult == true)
                        {
                            //更新数据库
                            if (currentChat.type == AntSdkchatType.Point)
                            {
                                //单聊
                                T_Chat_MessageDAL t_chat = new T_Chat_MessageDAL();
                                t_chat.UpdateContent(currentChat.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, JsonConvert.SerializeObject(ListMixMsg));
                            }
                            else
                            {
                                //群聊
                                T_Chat_Message_GroupDAL t_chatGroup = new T_Chat_Message_GroupDAL();
                                t_chatGroup.UpdateContent(currentChat.messageId, AntSdkService.AntSdkConfigInfo.AntSdkCompanyCode, AntSdkService.AntSdkCurrentUserInfo.userId, JsonConvert.SerializeObject(ListMixMsg));
                            }
                        }
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                //隐藏发送状态
                PublicTalkMothed.HiddenMsgDiv(arg.WebBrowser, arg.SendIngId);
                //显示重发按钮
                PublicTalkMothed.VisibleMsgDiv(arg.WebBrowser, arg.RepeatId);
            }
        }
    }
}
