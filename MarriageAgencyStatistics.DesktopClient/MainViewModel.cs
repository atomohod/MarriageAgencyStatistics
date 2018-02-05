using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
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
using MarriageAgencyStatistics.Common;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataTransferModels;
using MarriageAgencyStatistics.DesktopClient.Annotations;
using MarriageAgencyStatistics.Formatters;
using Newtonsoft.Json;
using RestSharp;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RestClient _client;
        private DateTime _choosenDate;
        private bool _reportIsGenerating;
        private bool _areUsersLoaded;
        private string _path;
        public RelayCommand GenerateReport => new RelayCommand(Generate, () => IsReportGenerating() && AreUsersLoaded());
        public RelayCommand SaveUsersCommand => new RelayCommand(SaveUsers);

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

        public MainViewModel(RestClient client)
        {
            _client = client;

            Logs = new ObservableCollection<string>();
            Users = new ObservableCollection<CheckedListItem<UserViewModel>>();
            Path = ConfigurationManager.AppSettings["path"];
            ChoosenDate = DateTime.Now;

            LoadUsers();
        }

        private void LoadUsers()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Log("загружаем пользователей...");
                var users = _client.Get<List<UserModel>>(new RestRequest($"users")).Data;
                var selectedUsers = _client.Get<List<UserModel>>(new RestRequest($"selectedusers")).Data;
                Log("готово!");

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
            });
        }

        private void SaveUsers()
        {
            var selectedUsers = GetSelectedUsers();

            var restRequest = new RestRequest($"selectedusers", Method.POST);
            restRequest.AddHeader("content-type", "application/json");
            restRequest.AddParameter("application/json", JsonConvert.SerializeObject(selectedUsers.Select(item => item.Item.Title).ToArray()), ParameterType.RequestBody);

            _client.Post(restRequest);
        }

        public void Generate()
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _reportIsGenerating = true;
                    GenerateReport.RaiseCanExecuteChanged();
                });

                var selectedUsers = GetSelectedUsers();
                var selectedUsersString = GetSelectedUsersString(selectedUsers);

                Log("получаем бонусы...");
                var bonus = _client.Get<List<UserBonusModel>>(new RestRequest($"bonus?date={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}{selectedUsersString}")).Data;
                Log("готово!");

                Log("считаем статистику онлайн...");
                var statistics = _client.Get<List<UserOnlineStatisticsModel>>(new RestRequest($"statistic?date={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}{selectedUsersString}")).Data;
                Log("готово!");

                Log("считаем отправленные письма...");
                var sentEmails = _client.Get<List<UserSentEmailsStatisticsModel>>(new RestRequest($"sentemails?dateFrom={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}&dateTo={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}{selectedUsersString}")).Data;
                Log("готово!");

                List<(User, Bonus, OnlineStatistics, SentEmailStatistics)> result = new List<(User, Bonus, OnlineStatistics, SentEmailStatistics)>();

                foreach (var user in selectedUsers)
                {
                    var b = bonus.FirstOrDefault(model => model.User.Title == user.Item.Title);
                    var s = statistics.FirstOrDefault(model => model.User.Title == user.Item.Title);
                    var e = sentEmails.FirstOrDefault(model => model.User.Title == user.Item.Title);

                    (User user, Bonus bonus, OnlineStatistics statistics, SentEmailStatistics emails) item = (
                        new User
                        {
                            Name = user.Item.Title
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
                        });

                    result.Add(item);
                }

                Log($"Сохраняем в {Path}");
                BrideForeverExcel.UpdateExcel(result, ChoosenDate, Path);
                Log("готово!");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _reportIsGenerating = false;
                    GenerateReport.RaiseCanExecuteChanged();
                });

            });
        }

        private static string GetSelectedUsersString(List<CheckedListItem<UserViewModel>> selectedUsers)
        {
            var selectedUsersString = selectedUsers
                .Select(item => $"&userNames={item.Item.Title}")
                .Aggregate((a, b) => $"{a}{b}");
            return selectedUsersString;
        }

        private List<CheckedListItem<UserViewModel>> GetSelectedUsers()
        {
            var selectedUsers = Users
                .Where(item => item.IsChecked)
                .ToList();
            return selectedUsers;
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