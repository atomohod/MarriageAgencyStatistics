using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MarriageAgencyStatistics.Applications;
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataTransferModels;
using MarriageAgencyStatistics.DesktopClient.Annotations;
using MarriageAgencyStatistics.Formatters;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private DateTime _choosenDate;
        private bool _reportIsGenerating;
        private bool _areUsersLoaded;
        private string _path;
        private readonly BrideForeverApp _app;
        public RelayCommand GenerateReport => new RelayCommand(Generate, () => IsReportGenerating() && AreUsersLoaded());
        
        private bool IsReportGenerating()
        {
            return !_reportIsGenerating;
        }

        private bool AreUsersLoaded()
        {
            return _areUsersLoaded;
        }

        public string Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Logs { get; set; }
        public ObservableCollection<CheckedListItem<UserViewModel>> Users { get; set; }

        public DateTime ChoosenDate
        {
            get => _choosenDate;
            set
            {
                if (value.Equals(_choosenDate)) return;
                _choosenDate = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel(BrideForeverApp app)
        {
            Logs = new ObservableCollection<string>();
            Users = new ObservableCollection<CheckedListItem<UserViewModel>>();
            Path = ConfigurationManager.AppSettings["path"];
            _app = app;
            ChoosenDate = DateTime.Now;

            LoadUsers();
        }

        private void LoadUsers()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Log("загружаем пользователей...");
                    var users = _app.GetUsers().Result;
                    var selectedUsers = _app.GetSelectedUsers().Result;
                    Log($"получено {users?.Count} пользователей. {selectedUsers?.Count} задействовано");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var user in users)
                        {
                            Users.Add(new CheckedListItem<UserViewModel>(new UserViewModel
                            {
                                Title = user.Title
                            })
                            {
                                IsChecked = selectedUsers.Any(model => model.Title == user.Title)
                            });
                        }

                        _areUsersLoaded = true;
                        GenerateReport.RaiseCanExecuteChanged();
                    });
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception);
                }
            });
        }

        public void Generate()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _reportIsGenerating = true;
                        GenerateReport.RaiseCanExecuteChanged();
                    });

                    var selectedUsers = GetSelectedUsers();

                    Log("получаем бонусы...");
                    var bonus = _app.GetBonuses(ChoosenDate, selectedUsers).Result;
                    Log($"получено {bonus?.Count} строк");

                    Log("статистика онлайн...");
                    var statistics = _app.GetStatistics(ChoosenDate, selectedUsers).Result;
                    Log($"получено {statistics?.Count} строк");

                    Log("отправленные письма...");
                    var sentEmails = _app.GetSentEmails(ChoosenDate, selectedUsers).Result;
                    Log($"получено {sentEmails?.Count} строк");

                    Log("чаты...");
                    var userChatStatistics = _app.GetUserChatStatistics(ChoosenDate, selectedUsers).Result;
                    Log($"получено {userChatStatistics?.Count} строк");
                    
                    List<(User, Bonus, OnlineStatistics, SentEmailStatistics, UserChatStatistic)> result = new List<(User, Bonus, OnlineStatistics, SentEmailStatistics, UserChatStatistic)>();

                    foreach (var user in selectedUsers)
                    {
                        var b = bonus?.FirstOrDefault(model => model.User.Title == user);
                        var s = statistics?.FirstOrDefault(model => model.User.Title == user);
                        var e = sentEmails?.FirstOrDefault(model => model.User.Title == user);
                        var c = userChatStatistics?.FirstOrDefault(model => model.User.Title == user);

                        (User user, Bonus bonus, OnlineStatistics statistics, SentEmailStatistics emails, UserChatStatistic) item = (
                            new User
                            {
                                Name = user
                            },
                            new Bonus
                            {
                                Today = b?.Bonus.Daily ?? -1,
                                LastMonth = b?.Bonus.Monthly ?? -1
                            },
                            new OnlineStatistics
                            {
                                PercentageOnline = s?.Online ?? -1,
                                TotalMinutesOnline = s?.TotalMinutesOnline ?? -1
                            },
                            new SentEmailStatistics
                            {
                                SentEmails = e?.EmailsCount ?? -1
                            },
                            new UserChatStatistic
                            {
                                ChatInvatationsCount =  c?.ChatStatistics.CountSentInvatations ?? -1
                            });

                        result.Add(item);
                    }

                    Log($"Сохраняем в {Path}");
                    BrideForeverExcel.UpdateExcel(result, ChoosenDate, Path);
                    Log("сохранено");

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _reportIsGenerating = false;
                        GenerateReport.RaiseCanExecuteChanged();
                    });
                }
                catch (Exception exception)
                {
                    _logger.Log(LogLevel.Error, exception);
                }
            });
        }

        private string[] GetSelectedUsers()
        {
            return Users
                .Where(item => item.IsChecked)
                .Select(item => item.Item.Title)
                .ToArray();
        }

        public void Log(string log)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Logs.Add(log);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}