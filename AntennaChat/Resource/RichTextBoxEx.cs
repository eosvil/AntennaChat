using Antenna.Framework;
using Antenna.Framework;
using Antenna.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using SDK.AntSdk.AntModels;
using SDK.AntSdk;

namespace AntennaChat.Resource
{
    public class RichTextBoxEx: RichTextBox
    {
        #region 属性
        /// <summary>
        /// 自动添加空格
        /// </summary>
        public bool AutoAddWhiteSpaceAfterTriggered
        {
            get { return (bool)GetValue(AutoAddWhiteSpaceAfterTriggeredProperty); }
            set { SetValue(AutoAddWhiteSpaceAfterTriggeredProperty, value); }
        }
        /// <summary>
        /// 依赖属性 自动添加空格
        /// </summary>
        public static readonly DependencyProperty AutoAddWhiteSpaceAfterTriggeredProperty =
            DependencyProperty.Register("AutoAddWhiteSpaceAfterTriggered", typeof(bool), typeof(RichTextBoxEx), new UIPropertyMetadata(true));
        /// <summary>
        /// 显示文本源
        /// </summary>
        public IList<String> ContentAssistSource
        {
            get { return (IList<String>)GetValue(ContentAssistSourceProperty); }
            set { SetValue(ContentAssistSourceProperty, value); }
        }
        /// <summary>
        /// 依赖属性 显示文本源
        /// </summary>
        public static readonly DependencyProperty ContentAssistSourceProperty =
              DependencyProperty.Register("ContentAssistSource", typeof(IList<String>), typeof(RichTextBoxEx), new UIPropertyMetadata(new List<string>()));
        /// <summary>
        /// 触发提示的关键词
        /// </summary>
        public IList<char> ContentAssistTriggers
        {
            get { return (IList<char>)GetValue(ContentAssistTriggersProperty); }
            set { SetValue(ContentAssistTriggersProperty, value); }
        }
        /// <summary>
        /// 依赖属性 触发提示的关键词
        /// </summary>
        public static readonly DependencyProperty ContentAssistTriggersProperty =
               DependencyProperty.Register("ContentAssistTriggers", typeof(IList<char>), typeof(RichTextBoxEx), new UIPropertyMetadata(new List<char>()));

        private List<AntSdkGroupMember> _richTextSource;

        /// <summary>
        /// 数据源
        /// </summary>
        public List<AntSdkGroupMember> RichTextSource
        {
            get { return _richTextSource; }
            set { _richTextSource = value; }
        }
        /// <summary>
        /// 讨论组ID
        /// </summary>
        private string groupId;
        public string GroupId
        {
            get { return groupId; }
            set { groupId = value; }
        }
        /// <summary>
        /// 是否为阅后即焚
        /// </summary>
        public bool isBurnMode = false;
        #endregion
        private bool IsAssistKeyPressed = false;
        private System.Text.StringBuilder sbLastWords = new System.Text.StringBuilder();
        private ListBox AssistListBox = new ListBox();
        private Popup AssistPopup = new Popup();
        Color talkColor = (Color)ColorConverter.ConvertFromString("#006EFE");
        private Paragraph pTalk;
        /// <summary>
        /// @成员的集合
        /// </summary>
        private List<string> selectList = new List<string>();
        public RichTextBoxEx()
        {
            this.Loaded += new RoutedEventHandler(RichTextBoxEx_Loaded);
            this.Document = new WFdExtend.WFdExtend()
            {
                LineHeight = 2,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                FontFamily = new FontFamily("微软雅黑"),
                FontSize = 13
            };
        }
        private void RichTextBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            //init the assist list box
            if (this.Parent.GetType() != typeof(Grid))
            {
                throw new Exception("this control must be put in Grid control");
            }
            if (AssistPopup.Child != null) return;
            if (ContentAssistTriggers.Count == 0)
            {
                ContentAssistTriggers.Add('@');
            }
            AssistPopup.PopupAnimation = PopupAnimation.Fade;
            AssistPopup.MaxHeight = 100;
            AssistPopup.MinWidth=140;
            AssistPopup.PlacementTarget = this.Parent as Grid;
            AssistPopup.Placement = PlacementMode.Left;
            AssistPopup.StaysOpen = false;
            AssistPopup.IsOpen = false;
            AssistPopup.AllowsTransparency = true;
            AssistPopup.Closed += AssistPopup_Closed;
            //(this.Parent as Grid).Children.Add(AssistListBox);
            Border border = new Border()
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(2),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")),
                Margin = new Thickness(0),
                Background = Brushes.White,
            };
            AssistListBox.MaxHeight = 100;
            AssistListBox.MinWidth = 140;
            AssistListBox.Margin = new Thickness(0);
            AssistListBox.BorderThickness = new Thickness(0);
            AssistListBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            AssistListBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            AssistListBox.SnapsToDevicePixels = true;
            //AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
            AssistListBox.MouseDoubleClick += new MouseButtonEventHandler(AssistListBox_MouseDoubleClick);
            AssistListBox.PreviewKeyDown += new KeyEventHandler(AssistListBox_PreviewKeyDown);
            border.Child = AssistListBox;
            AssistPopup.Child = border;
            
        }

        private void AssistPopup_Closed(object sender, EventArgs e)
        {
            IsAssistKeyPressed = false;
        }

        private void AssistListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if Enter\Tab\Space key is pressed, insert current selected item to richtextbox
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Space)
            {
                InsertAssistWord();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                //Baskspace key is pressed, set focus to richtext box
                if (sbLastWords.Length >= 1)
                {
                    sbLastWords.Remove(sbLastWords.Length - 1, 1);
                    FilterAssistBoxItemsSource();
                }
                else if(sbLastWords.Length==0)
                {
                    AssistPopup.IsOpen = false;
                    IsAssistKeyPressed = false;
                }
                this.Focus();
            }
        }
        private void AssistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertAssistWord();
        }
        private bool InsertAssistWord()
        {
            bool isInserted = false;
            if (AssistListBox.SelectedIndex != -1)
            {
                string selectedString = "";
                if (AssistListBox.SelectedItem.ToString().IndexOf('[') < 0)
                    selectedString = AssistListBox.SelectedItem.ToString();
                else
                    selectedString = AssistListBox.SelectedItem.ToString().Substring(0, AssistListBox.SelectedItem.ToString().IndexOf('[') - 2);
                //if (AutoAddWhiteSpaceAfterTriggered)
                //{
                //    selectedString += " ";
                //}
                this.InsertText(selectedString);
                isInserted = true;
            }
            //AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
            AssistPopup.IsOpen = false;
            sbLastWords.Clear();
            IsAssistKeyPressed = false;
            return isInserted;
        }
        private void InsertText(string text)
        {
            if(isBurnMode) return;
            Focus();
            //删除输入 包括@
            TextPointer p = CaretPosition.GetPositionAtOffset(-sbLastWords.Length - 1);
            if (p != null)
            {
                CaretPosition = p;
            }
            var getGroupMembersUser = RichTextSource.FirstOrDefault(c => c.userNum+c.userName == text);
            text = "@" + text;
            CaretPosition.DeleteTextInRun(sbLastWords.Length+1);
            //pTalk = p.Paragraph;
            //插入选中项  todo
            TextBlock tbLeft = new TextBlock() { Text = text, FontSize = 12, Foreground = new SolidColorBrush(talkColor)};
            if (getGroupMembersUser != null)
                tbLeft.Tag = getGroupMembersUser.userId;
            else if (text.Contains("全体"))
            {
                tbLeft.Tag = groupId;
            }
            TextPointer point = this.Selection.Start;
            InlineUIContainer uiContainer = new InlineUIContainer(tbLeft, point);
            TextPointer nextPoint = uiContainer.ContentEnd;
            this.CaretPosition = nextPoint;
            this.CaretPosition.InsertTextInRun(" ");//加入空格
            //pTalk.Inlines.Add(tbLeft);
            //this.Document.Blocks.Add(pTalk);
            //TextPointer pointer = this.Document.ContentEnd; 
            //if (pointer != null)
            //{
            //    CaretPosition = pointer;
            //}
            //CaretPosition.InsertTextInRun(" ");//加入空格
            //TextPointer pp = CaretPosition.GetPositionAtOffset(1);
            //if (pp != null)
            //{
            //    CaretPosition = pp;
            //}
        }
        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (!IsAssistKeyPressed)
            {
                base.OnPreviewKeyDown(e);
                return;
            }
            ResetAssistListBoxLocation();
            if (e.Key == System.Windows.Input.Key.Back)
            {
                if (sbLastWords.Length > 0)
                {
                    sbLastWords.Remove(sbLastWords.Length - 1, 1);
                    FilterAssistBoxItemsSource();
                }
                else
                {
                    IsAssistKeyPressed = false;
                    sbLastWords.Clear();
                    //AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
                    AssistPopup.IsOpen = false;
                }
            }
            //Enter key pressed, insert the first item to richtextbox
            if ((e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab))
            {
                //if (AssistListBox.Visibility == Visibility.Visible)
                //    AssistListBox.Focus();
                AssistListBox.SelectedIndex = 0;
                if (InsertAssistWord())
                {
                    e.Handled = true;
                }
            }
            if (e.Key == Key.Down)
            {
                AssistListBox.Focus();
            }
            base.OnPreviewKeyDown(e);
        }
        protected override void OnPreviewKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            ResetAssistListBoxLocation();
            base.OnPreviewKeyUp(e);
        }
        private void FilterAssistBoxItemsSource()
        {
            //IEnumerable<string> temp = ContentAssistSource.Where(s => s.ToUpper().StartsWith(sbLastWords.ToString().ToUpper()));
            if (sbLastWords.ToString()=="@")
            {
                sbLastWords.Clear();
            }
            List<string> resultList;
            if (string.IsNullOrEmpty(sbLastWords.ToString()))
            {
                resultList = InitDefaultSource();
            }
            else
            {
                resultList = ResetQueryCondition(sbLastWords.ToString());
            }
            AssistListBox.ItemsSource = resultList;
            AssistListBox.SelectedIndex = 0;
            if (resultList == null || !resultList.Any()||(resultList!=null&&resultList.Count==1&&resultList.Contains("全体成员")))
            {
                //AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
                AssistPopup.IsOpen = false;
                IsAssistKeyPressed = false;
                sbLastWords.Clear();
            }
            else
            {
                if(isBurnMode)
                {
                    AssistPopup.IsOpen = false;
                    IsAssistKeyPressed = false;
                }
                else
                {
                    AssistPopup.IsOpen = true;
                    IsAssistKeyPressed = true;
                }
            }
        }
        /// <summary>
        /// 默认选项
        /// </summary>
        private List<string> InitDefaultSource()
        {
            try
            {
                List<string> resultList = new List<string>();
                //if (RichTextSource.Count > 4)
                //{
                //    for (int i = 0; i < 4; i++)
                //    {
                //        string nameStr = RichTextSource[i].userName + "  [" +
                //                         RichTextSource[i].position + "]";
                //        resultList.Add(nameStr);
                //    }
                //}
                //else
                //{
                    for (int i = 0; i < RichTextSource.Count; i++)
                    {
                        string nameStr = RichTextSource[i].userNum+RichTextSource[i].userName + "  [" +
                                         RichTextSource[i].position + "]";
                        if (RichTextSource[i].userId != AntSdkService.AntSdkCurrentUserInfo.userId)
                            resultList.Add(nameStr);
                    }
                //}
                //GetGroupMembers_User isUserMember =
                // RichTextSource.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
                //if (isUserMember != null && isUserMember.roleLevel == 1)
                //{
                //    resultList.Add("全体成员");
                //}
                resultList.Insert(0, "全体成员");
                return resultList;
            }
            catch(Exception e)
            {
                return null;
            }
        }
        protected override void OnTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (IsAssistKeyPressed == false && e.Text.Length == 1)
            {
                if (ContentAssistTriggers.Contains(char.Parse(e.Text)))
                {
                    ResetAssistListBoxLocation();
                    IsAssistKeyPressed = true;
                    FilterAssistBoxItemsSource();
                    return;
                }
            }
            if (IsAssistKeyPressed)
            {
                sbLastWords.Append(e.Text);
                FilterAssistBoxItemsSource();
            }
        }
        private void ResetAssistListBoxLocation()
        {
            //Rect rect = this.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
            //double left = rect.X >= 20? rect.X : 20;
            //double top = rect.Y >= 20 ? rect.Y + 20 : 20;
            ////left += this.Padding.Left;
            //top += 10;
            //AssistListBox.SetCurrentValue(ListBox.MarginProperty, new Thickness(left, top, 0, 0));
            TextPointer start = this.Selection.Start;
            System.Windows.Rect rect = start.GetCharacterRect(LogicalDirection.Forward);
            Point point = rect.BottomLeft;
            AssistPopup.HorizontalOffset = point.X + 160;
            AssistPopup.VerticalOffset = -(AssistListBox.ActualHeight- point.Y)-25 ;
        }
        #region 查询
        /// <summary>
        /// 查询（暂时支持汉字和首字母查询）
        /// </summary>
        /// <param name="condition"></param>
        private List<string> ResetQueryCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return null;
            condition = condition.ToLower();
            try
            {
                if (DataConverter.InputIsNum(condition))
                {
                    return NumQuery(condition);
                }
                if (DataConverter.InputIsChinese(condition))
                {
                    return ChineseQuery(condition);
                }
                else
                {
                    return SpellQuery(condition);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
        /// <summary>
        /// 按工号查询
        /// </summary>
        /// <param name="condition"></param>
        private List<string> NumQuery(string condition)
        {
            List<AntSdkGroupMember> contactsList = RichTextSource.Where(m=>m.userNum!=null).ToList().Where(c => c.userNum.Contains(condition) && c.userId != AntSdkService.AntSdkCurrentUserInfo.userId).ToList();
            if (!contactsList.Any())
            {
                return null;
            }
            //contactsList = contactsList.OrderBy(c => int.Parse(c.userNum)).ToList();
            List<string> resultList = contactsList.Select(t => t.userNum +t.userName+ "  [" + t.position + "]").ToList();
            //GetGroupMembers_User isUserMember =
            //      RichTextSource.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            //if (isUserMember != null && isUserMember.roleLevel == 1)
            //{ 
            //    resultList.Add("全体成员");
            //}
            //resultList.Insert(0,"全体成员");
            return resultList;

        }
        /// <summary>
        /// 按中文查询
        /// </summary>
        /// <param name="condition"></param>
        private List<string> ChineseQuery(string condition)
        {
            List<AntSdkGroupMember> contactsList = RichTextSource.Where(c => c.userName.Contains(condition) && c.userId != AntSdkService.AntSdkCurrentUserInfo.userId).ToList();//AntSdkService.AntSdkListContactsEntity.contacts.users.Where(c => c.userName.Contains(condition)).ToList();
            if (!contactsList.Any())
            {
                return null;
            }
            List<string> resultList = contactsList.Select(t => t.userNum + t.userName + "  [" + t.position + "]").ToList();
            //GetGroupMembers_User isUserMember =
            //      RichTextSource.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            //if (isUserMember != null && isUserMember.roleLevel == 1)
            //{
            //    resultList.Add("全体成员");
            //}
            //resultList.Insert(0,"全体成员");
            return resultList;
        }

        /// <summary>
        /// 按拼音首字母查询
        /// </summary>
        /// <param name="condition"></param>
        private List<string> SpellQuery(string condition)
        {
            //var resultList = (from c in RichTextSource let pinyinName = 
            //                           DataConverter.GetChineseSpell(c.userName) where pinyinName.Contains(condition) && pinyinName.Substring(0, condition.Length) == condition select c.userNum+ c.userName + "  [" + c.position + "]").ToList();
            //if (resultList.Count == 0) return null;
            string nameNum = string.Empty;
            List<string> resultList=new List<string>();
            if (RichTextSource != null)
            {
                foreach (var user in RichTextSource)
                {
                    var pinyinName = DataConverter.GetChineseSpellList(user.userName); 
                    if (!pinyinName.Any()) continue;
                    foreach (var t in pinyinName)
                    {
                        nameNum = t;
                        if (nameNum.Contains(condition)&&user.userId!= AntSdkService.AntSdkCurrentUserInfo.userId)
                        {
                            resultList.Add(user.userNum+user.userName+ "  [" + user.position + "]");
                            break;
                        }
                    }
                }
            }
            //GetGroupMembers_User isUserMember =
            //    RichTextSource.SingleOrDefault((m => m.userId == AntSdkService.AntSdkCurrentUserInfo.userId));
            //if (isUserMember != null && isUserMember.roleLevel == 1)
            //{
            //    resultList.Add("全体成员");
            //}
            //resultList.Insert(0,"全体成员");
            return resultList;
        }
        #endregion
        #region 公共方法
        /// <summary>
        /// 获取@集合
        /// </summary>
        /// <returns></returns>
        public List<string> GetSelectName()
        {
            selectList.Clear();
            var counts = this.Document.Blocks.OfType<Paragraph>().Select(m => m.Inlines).ToList();
            foreach (var ss in counts)
            {
                var inlines = ss.OfType<Inline>();
                foreach (var inline in inlines)
                {
                    InlineUIContainer iu = inline as InlineUIContainer;
                    if (iu != null)
                    {
                        var textBlock = iu.Child as TextBlock;
                        if (textBlock.Text.IndexOf('@') < 0) continue;
                        selectList.Add(textBlock.Text.Substring(1));
                    }
                }
            }
            return selectList;
        }
        #endregion
    }
}
