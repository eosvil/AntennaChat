using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Antenna.Framework;
using Antenna.Model;
using AntennaChat.Views;
using SDK.AntSdk;
using SDK.AntSdk.AntModels;

namespace AntennaChat.ViewModel.Contacts
{
    public class GroupPublicFunction
    {
        public static void UpdateGroupPicture(object localPicture)
        {
            string[] ThreadParams = localPicture as string[];
            string pictureAddress = ImageHandle.UploadPicture(ThreadParams[1]);
            AntSdkUpdateGroupInput updateInput = new AntSdkUpdateGroupInput();
            updateInput.userId = AntSdkService.AntSdkLoginOutput.userId;
            updateInput.groupId = ThreadParams[0];
            updateInput.groupName = "";
            updateInput.groupPicture = pictureAddress;
            var errCode = 0;
            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            AntSdkService.UpdateGroup(updateInput, ref errCode, ref errMsg);
            //(new HttpService()).UpdateGroup(updateInput, ref updateOut, ref errMsg);
        }

        public static List<AntSdkGroupMember> GetMembers(string groupId)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            GetGroupMembersInput input = new GetGroupMembersInput();
            input.token = AntSdkService.AntSdkLoginOutput.token;
            input.version = GlobalVariable.Version;
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.groupId = groupId;
            GetGroupMembersOutput output = new GetGroupMembersOutput();
            var errCode = 0;
            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //if ((new HttpService()).GetGroupMembers(input, ref output, ref errMsg))
            //{
            //    Members = output.users;

            //    this.GroupName = string.Format("{0}", GroupInfo.groupName);
            //    GroupMemberCount = string.Format("({0}人)", Members == null ? 0 : Members.Count());
            //    GetGroupMembers_User tempUser = Members.Find(c => c.roleLevel == (int)GlobalVariable.GroupRoleLevel.Admin);
            //    if (tempUser != null && AntSdkService.AntSdkLoginOutput.userId == tempUser.userId)
            //    {
            //        DeleteGroupVisibility = Visibility.Visible;
            //        GroupClassifyName = "我管理的";
            //        GroupClassify = 1;
            //    }
            //    else
            //    {
            //        GroupClassifyName = "我加入的";
            //        GroupClassify = 2;
            //    }
            //}
            //DONE:AntSdk_Modify
            var groupMembers = AntSdkService.GetGroupMembers(AntSdkService.AntSdkLoginOutput.userId,
                groupId, ref errCode, ref errMsg);
            if (groupMembers != null && groupMembers.Length > 0)
            {
                return groupMembers.ToList();
            }
            //Log输出
            if (!string.IsNullOrEmpty(errMsg))
            {

            }
            stopWatch.Stop();
            LogHelper.WriteDebug(string.Format("[GroupInfoViewModel_GetMembers({0}毫秒)]",
                stopWatch.Elapsed.TotalMilliseconds));
            return new List<AntSdkGroupMember>();
        }

        /// <summary>
        /// 查询用户信息
        /// </summary>
        /// <param name="id"></param>
        public static AntSdkUserInfo QueryUserInfo(string id)
        {
            var isExist = AntSdkService.AntSdkListContactsEntity.users.FirstOrDefault(v => v.userId == id);
            if (isExist == null) return null;
            var errCode = 0;
            var errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var user = AntSdkService.AntSdkGetUserInfo(id, ref errCode, ref errMsg);
            return user ?? null;
        }
        /// <summary>
        /// 退出讨论组
        /// </summary>
        public static bool ExitGroup(string groupId, string groupName, List<AntSdkGroupMember> Members)
        {

            ExitGroupInput input = new ExitGroupInput();
            input.groupId = groupId;
            input.token = AntSdkService.AntSdkLoginOutput.token;
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.version = GlobalVariable.Version;

            BaseOutput output = new BaseOutput();
            var errCode = 0;
            string errMsg = string.Empty;
            //TODO:AntSdk_Modify
            //DONE:AntSdk_Modify
            var isResult = AntSdkService.GroupExitor(AntSdkService.AntSdkLoginOutput.userId, groupId, ref errCode, ref errMsg);
            if (isResult)
            {
                string[] ThreadParams = new string[3];
                ThreadParams[0] = groupId;
                ThreadParams[1] = ImageHandle.GetGroupPicture(Members.Where(c => c.userId != AntSdkService.AntSdkLoginOutput.userId).Select(c => c.picture).ToList());
                ThreadParams[2] = string.IsNullOrEmpty(groupName) ? "" : groupName;
                Thread UpdateGroupPictureThread = new Thread(UpdateGroupPicture);
                UpdateGroupPictureThread.Start(ThreadParams);

                //OnDropOutGroupEvent(this);
            }
            else
            {
                MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            }
            return isResult;

        }

        /// <summary>
        /// 解散讨论组
        /// </summary>
        public static void DismissGroup(string groupId)
        {
            ExitGroupInput input = new ExitGroupInput();
            input.groupId = groupId;
            input.token = AntSdkService.AntSdkLoginOutput.token;
            input.userId = AntSdkService.AntSdkLoginOutput.userId;
            input.version = GlobalVariable.Version;
            BaseOutput output = new BaseOutput();
            var errCode = 0;
            var errMsg = string.Empty;
            var isResult = AntSdkService.DissolveGroup(AntSdkService.AntSdkLoginOutput.userId, groupId, ref errCode, ref errMsg);
            if (isResult)
            {
                //OnDropOutGroupEvent(this);
            }
            else
            {
                MessageBoxWindow.Show(errMsg, GlobalVariable.WarnOrSuccess.Warn);
            }
        }

    }
}
