using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SDK.Service;
using SDK.Service.Models;

namespace SDK.AntSdk.AntModels
{
    public class AntSdkReceivedUserMsg
    {
        //[0][0] messageType 消息类型 String 2100：用户离线，2101：用户在线，2102：用户离开，2103：用户忙碌，2109：用户账号被停用,2111:用户修改密码,踢出登录(多终端同时在线),2112: 用户被踢出登陆录
        //[1][0] userId 用户ID String 表示是用户登录或者退出的，终端收到这个消息后，可能需要在本地修改该用户的在线状态。 

        /// <summary>
        /// 用户通知信息基类
        /// </summary>
        public class UserBase : AntSdkMsBase
        {
            /// <summary>
            /// 用户ID
            /// </summary>
            public string userId { get; set; } = string.Empty;
        }

        public class State : UserBase
        {
            public string attr { get; set; } = string.Empty;
        }

        /// <summary>
        /// MQTT收到用户信息变更
        /// </summary>
        public class Modify : UserBase
        {
            /// <summary>
            /// 用户信息变更通知
            /// </summary>
            public Modify_content attr { get; set; }
        }

        /// <summary>
        /// 用户信息变更数据
        /// </summary>
        public class Modify_content
        {
            /// <summary>
            /// 职位
            /// </summary>
            public string position { get; set; } = string.Empty;
            /// <summary>
            /// 头像eg http://www.baidu.com
            /// </summary>
            public string picture { get; set; } = string.Empty;

            /// <summary>
            /// 个性签名
            /// </summary>
            public string signature { get; set; } = string.Empty;

            /// <summary>
            /// 性别
            /// </summary>
            public string sex { get; set; } = string.Empty;

            /// <summary>
            /// 部门ID
            /// </summary>
            public string departmentId { get; set; } = string.Empty;

            /// <summary>
            /// 人员名称
            /// </summary>
            public string userName { get; set; } = string.Empty;

            /// <summary>
            /// 工号
            /// </summary>
            public string userNum { get; set; } = string.Empty;

            /// <summary>
            /// 电话号码
            /// </summary>
            public string phone { get; set; } = string.Empty;
        }

        /// <summary>
        /// 方法说明：获取触角SDK接收到的用户信息
        /// </summary>
        /// <param name="sdkuserMsg">平台SDK接到的用户信息</param>
        /// <returns>转换的触角SDK用户信息</returns>
        internal static UserBase GetAntSdkReceiveUserMsg(MsSdkUserBase sdkuserMsg)
        {
            try
            {
                var sdkreceivemsgtypeValue = (long) sdkuserMsg.MsgType;
                var antsdkreceivemsgType = (AntSdkMsgType) sdkreceivemsgtypeValue;
                var sdkuserState = sdkuserMsg as MsUserStateChange;
                if (sdkuserState != null)
                {
                    var antsdkuserState = new State
                    {
                        MsgType = antsdkreceivemsgType,
                        userId = sdkuserState.userId,
                        attr = sdkuserState.attr
                    };
                    return antsdkuserState;
                }
                var sdkuserUpdate = sdkuserMsg as MsUserInfoModify;
                if (sdkuserUpdate != null)
                {
                    var antsdkuserUpdate = new Modify
                    {
                        MsgType = antsdkreceivemsgType,
                        userId = sdkuserUpdate.userId,
                        attr = new Modify_content
                        {
                            departmentId = sdkuserUpdate.attr?.departmentId,
                            position = sdkuserUpdate.attr?.position,
                            phone = sdkuserUpdate.attr?.phone,
                            picture = sdkuserUpdate.attr?.picture,
                            signature = sdkuserUpdate.attr?.signature,
                            sex = sdkuserUpdate.attr?.sex,
                            userName = sdkuserUpdate.attr?.userName,
                            userNum = sdkuserUpdate.attr?.userNum
                        }
                    };
                    return antsdkuserUpdate;
                }
                //返回
                return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(
                    $"[AntSdkUserMsg.GetAntSdkReceiveUserMsg]:{Environment.NewLine}{ex.Message}{ex.StackTrace}");
                return null;
            }
        }
    }
}
