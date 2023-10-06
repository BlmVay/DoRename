using do9Rename.Core;
using do9Rename.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace do9Rename.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public const string Substract = "SUBSTRACT";     //截取功能
        public const string Append = "APPEND";           //插入功能
        public const string Replace = "REPLACE";         //替换功能
        public const string Nothing = "NOTHING";         //啥也不干


        #region 私有变量

        private static readonly char[] InvalidChars = @"\/:*?""<>|".ToCharArray();

        private readonly ObservableOrderStack<IRenameCommand> _operations;
        private readonly ObservableOrderStack<IRenameCommand> _undos;
        private readonly IModifyExtCommand _removeExt;
        private readonly IModifyExtCommand _appendExt;

        private string _msg;
        private int _selectedIndex;
        private bool _withExt;
        private bool _isUsingRegex;

        private int _startNumber;

        private int _numberCount;

        private int _numberPosition;

        private int _subSkip;
        private int _subTake;
        private bool _subHeadFirst;

        private int _appendSkip;
        private string _appendText;
        private bool _appendHeadFirst;

        private string _replaceOld;
        private string _replaceNew;

        #endregion

        #region 界面绑定属性

        /// <summary>
        /// 影响扩展名否
        /// </summary>
        public bool WithExtension
        {
            get => _withExt;
            set => Set(ref _withExt, value);
        }

        /// <summary>
        /// 是否在替换时使用正则1
        /// </summary>
        public bool IsUsingRegex
        {
            get => _isUsingRegex;
            set => Set(ref _isUsingRegex, value);
        }

        public int NumberPosition
        {
            set => Set(ref _numberPosition, value);
            get => _numberPosition;
        }

        public int StartNumber
        {
            get => _startNumber;
            set => Set(ref _startNumber, value);
        }

        public int NumberCount
        {
            get { return _numberCount; }
            set { Set(ref _numberCount, value); }
        }

        /// <summary>
        /// 界面显示的消息
        /// </summary>
        public string MessageText
        {
            get => _msg;
            set => Set(ref _msg, value);
        }

        /// <summary>
        /// 文件列表选中项的索引
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;
            set => Set(ref _selectedIndex, value);
        }

        /// <summary>
        /// 原文件名
        /// </summary>
        public ObservableCollection<FileInfo> OldNames { get; }

        /// <summary>
        /// 新文件名
        /// </summary>
        public ObservableCollection<string> NewNames { get; }

        //截取功能的参数

        /// <summary>
        /// 跳过的字符数
        /// </summary>
        public string SubSkip
        {
            get => $"{_subSkip}";
            set
            {
                if (!int.TryParse(value, out var temp))
                {
                    MessageText = "不合法的输入";
                    return;
                }
                Set(ref _subSkip, temp);
            }
        }

        /// <summary>
        /// 获取的字符数
        /// </summary>
        public string SubTake
        {
            get => $"{_subTake}";
            set
            {
                if (int.TryParse(value, out var temp))
                {
                    if (temp >= 0)
                    {
                        Set(ref _subTake, temp);
                        return;
                    }
                }
                MessageText = "不合法输入";
            }
        }
            
        /// <summary>
        /// 是否从前方开始计数
        /// </summary>
        public bool SubHeadFirst
        {
            get => _subHeadFirst;
            set => Set(ref _subHeadFirst, value);
        }

        //插入功能参数

        /// <summary>
        /// 插入位置
        /// </summary>
        public string AppendSkip
        {
            get => $"{_appendSkip}";
            set
            {
                if (int.TryParse(value, out var temp))
                {
                    if (temp >= 0)
                    {
                        Set(ref _appendSkip, temp);
                        return;
                    }
                }
                MessageText = "不合法的输入";
            }
        }

        /// <summary>
        /// 插入内容
        /// </summary>
        public string AppendText
        {
            get => _appendText;
            set => Set(ref _appendText, value);
        }

        /// <summary>
        /// 插入方向
        /// </summary>
        public bool AppendHeadFirst
        {
            get => _appendHeadFirst;
            set => Set(ref _appendHeadFirst, value);
        }

        //替换功能参数

        /// <summary>
        /// 要替换的内容
        /// </summary>
        public string ReplaceOld
        {
            get => _replaceOld;
            set => Set(ref _replaceOld, value);
        }

        /// <summary>
        /// 替换为的内容
        /// </summary>
        public string ReplaceNew
        {
            get => _replaceNew;
            set => Set(ref _replaceNew, value);
        }

        #endregion

        #region 界面绑定命令

        public ICommand AddOperationCommand { get; set; }
        public ICommand ExecuteRenameCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ClearCommand { get; set; }
        public ICommand DeleteOneCommand { get; set; }

        public ICommand InvertCommand { get; set; }

        public ICommand UndoCommand { get; set; }
        public ICommand RedoCommand { get; set; }
        public ICommand SelectFolderCommand { get; set; }
        public ICommand AddFileCommand { get; set; }
        public ICommand ReferenceCommand { get; set; }
        public ICommand CheckExtCommand { get; set; }
        public ICommand ClearOperationCommand { get; set; }

        public ICommand AddNumberSuffix { get; set; }

        #endregion

        public MainViewModel()
        {
            // 初始化内部私有变量
            _operations = new ObservableOrderStack<IRenameCommand>();
            _undos = new ObservableOrderStack<IRenameCommand>();
            _removeExt = new RemoveExtCommand();
            _appendExt = new AppendExtCommand();

            // 初始化Command对象
            RegisterCommands();

            // 初始化绑定属性

            // 全局属性
            OldNames = new ObservableCollection<FileInfo>();
            NewNames = new ObservableCollection<string>();
            SelectedIndex = -1;
            WithExtension = false;

            OldNames.CollectionChanged += (s, e) => UpdateNewName();

            // 截取功能属性
            SubSkip = "0";
            SubTake = "99";
            SubHeadFirst = true;

            // 插入功能属性
            AppendSkip = "0";
            AppendText = "";
            AppendHeadFirst = true;

            // 替换功能属性
            ReplaceOld = "a";
            ReplaceNew = "b";

            // 完毕
            MessageText = "就绪";
#if DEBUG
            Console.WriteLine("Init success.");
#endif
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        private void RegisterCommands()
        {
            AddOperationCommand = new RelayCommand<string>(AddOperation);
            ExecuteRenameCommand = new RelayCommand(ExecuteRename);
            RefreshCommand = new RelayCommand(UpdateOldName);
            ClearCommand = new RelayCommand(ClearAll);
            SelectFolderCommand = new RelayCommand(AddDirectory);
            AddFileCommand = new RelayCommand(AddFiles);
            CheckExtCommand = new RelayCommand(UpdateNewName);
            ClearOperationCommand = new RelayCommand(() => { _operations.Clear(); _undos.Clear(); UpdateNewName(); });
            ReferenceCommand = new RelayCommand(() => MessageBox.Show("请将问题原因发送到邮箱：do9core@outlook.com", "提示"));
            DeleteOneCommand = new RelayCommand(DeleteSelected, () => SelectedIndex >= 0);
            InvertCommand = new RelayCommand(InvertSelect);
            UndoCommand = new RelayCommand(Undo, () => _operations.Count > 0);
            RedoCommand = new RelayCommand(Redo, () => _undos.Count > 0);
            AddNumberSuffix = new RelayCommand(() =>
            {
                Console.WriteLine(StartNumber);
                Console.WriteLine(NumberCount);
                var sub = new AddNumberSuffixCommand(NumberCount,NumberPosition);
                _operations.Push(sub);
                MessageText = $"追加数字操作 - {sub}";
                UpdateNewName();
            });
        }

        /// <summary>
        /// 向文件列表添加选中的文件
        /// </summary>
        private void AddFiles()
        {
            var dialog = new CommonOpenFileDialog
            {
                Multiselect = true,
                Title = "选择一个或多个文件"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageText = "没有选择文件";
                return;
            }

#if DEBUG
            Console.WriteLine("Added files:");
#endif
            var count = dialog.FileNames.Count();
            foreach (var fName in dialog.FileNames)
            {
#if DEBUG
                Console.WriteLine(fName);
#endif
                OldNames.Add(new FileInfo(fName));
            }

            MessageText = count > 1 ? $"已添加{count}个文件" : $"已添加文件{dialog.FileName}";
        }

        /// <summary>
        /// 向文件列表添加选中目录下的所有文件
        /// </summary>
        private void AddDirectory()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "选择一个文件夹"
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageText = "没有加载目录";
                return;
            }

            var dir = new DirectoryInfo(dialog.FileName);
            if (!dir.Exists)
            {
                MessageText = "目录不存在";
                return;
            }

#if DEBUG
            Console.WriteLine("Add dir: {0}", dir.FullName);
#endif

            foreach (var fInf in dir.GetFiles())
            {
                OldNames.Add(fInf);
            }

            MessageText = $"已添加目录{dir.FullName}";
        }

        /// <summary>
        /// 获取操作后的新文件名
        /// </summary>
        /// <param name="fInf">文件信息</param>
        /// <returns>新文件名</returns>
        private string GetNewName(FileSystemInfo fInf, int index)
        {
            _removeExt.Extension = fInf.Extension;
            _appendExt.Extension = fInf.Extension;
            Console.WriteLine("------" + _removeExt.Execute(fInf.Name, index));
            var result = WithExtension ? fInf.Name : _removeExt.Execute(fInf.Name, index);
            foreach (var operation in _operations)
            {
                result = operation.Execute(result, index);
            }

            result = WithExtension ? result : _appendExt.Execute(result, index);
            return result;
        }

        /// <summary>
        /// 更新列表显示的新文件名
        /// </summary>
        private void UpdateNewName()
        {
            NewNames.Clear();
            try
            {
                int index = 0;
                foreach (var fInf in OldNames)
                {
                    NewNames.Add(GetNewName(fInf, StartNumber + index++));
                }
            }
            catch (ArgumentException e)
            {
                MessageText = e.Message;
            }
        }

        /// <summary>
        /// 刷新文件列表的旧文件名
        /// </summary>
        private void UpdateOldName()
        {
            var tempList = OldNames.ToList();
            OldNames.Clear();

            foreach (var fInf in tempList)
            {
                fInf.Refresh();
                OldNames.Add(fInf);
            }
            MessageText = "刷新完毕";
        }

        /// <summary>
        /// 向操作栈中追加一项
        /// </summary>
        /// <param name="operation">操作类型</param>
        private void AddOperation(string operation)
        {
            switch (operation)
            {
                case Substract:
                    var sub = new SubstractCommand(_subSkip, _subTake, _subHeadFirst);
                    _operations.Push(sub);
                    MessageText = $"追加操作 - {sub}";
                    break;
                case Append:
                    var append = new AppendCommand(_appendSkip, _appendText, _appendHeadFirst);
                    _operations.Push(append);
                    MessageText = $"追加操作 - {append}";
                    break;
                case Replace:
                    var replace = new ReplaceCommand(_replaceOld, _replaceNew, _isUsingRegex);
                    _operations.Push(replace);
                    MessageText = $"追加操作 - {replace}";
                    break;
                case Nothing:
                    _operations.Push(new DoNothingCommand());
                    break;
                default:
                    throw new ArgumentException();
            }
            UpdateNewName();
        }

        /// <summary>
        /// 执行命名，体现在文件上
        /// </summary>
        private void ExecuteRename()
        {
            var error = 0;
            for (var i = 0; i < OldNames.Count; ++i)
            {
                if (!IsValidFileName(NewNames[i]))
                {
                    ++error;
                    continue;
                }
                Console.WriteLine(NewNames[i]);
                var dir = OldNames[i].Directory;
                OldNames[i].MoveTo($"{dir?.FullName}/{NewNames[i]}");
            }
            UpdateOldName();
            UpdateNewName();

            _operations.Clear();
            _undos.Clear();

            MessageText = error > 0 ? $"{error}个文件重命名失败，已跳过" : "重命名成功完成";
        }

        /// <summary>
        /// 检查文件名中是否有不合法字符
        /// </summary>
        /// <param name="fName">文件名</param>
        /// <returns>是否合法</returns>
        private static bool IsValidFileName(string fName)
        {
            return !InvalidChars.Any(fName.Contains);
        }

        /// <summary>
        /// 清空文件列表和操作栈
        /// </summary>
        private void ClearAll()
        {
            NewNames.Clear();
            OldNames.Clear();
            _operations.Clear();
            _undos.Clear();

            MessageText = "成功清除列表";
        }

        /// <summary>
        /// 从文件列表中移除选中项
        /// </summary>
        private void DeleteSelected()
        {
            var tempIndex = SelectedIndex;
            var tempName = OldNames[tempIndex].FullName;

            OldNames.RemoveAt(SelectedIndex);
            NewNames.RemoveAt(tempIndex);

            MessageText = $"已移除{tempName}";
        }


        private void InvertSelect()
        {
        }


        /// <summary>
        /// 撤销操作
        /// </summary>
        private void Undo()
        {
            var lastOpe = _operations.Pop();
            _undos.Push(lastOpe);
            UpdateNewName();

            MessageText = $"已撤销操作 - {lastOpe}";
        }

        /// <summary>
        /// 重做操作
        /// </summary>
        private void Redo()
        {
            var ope = _undos.Pop();
            _operations.Push(ope);
            UpdateNewName();

            MessageText = $"已重做操作 - {ope}";
        }
    }
}