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
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.Core.Services;
using MarriageAgencyStatistics.DataTransferModels;
using MarriageAgencyStatistics.DesktopClient.Annotations;
using MarriageAgencyStatistics.Formatters;
using RestSharp;

namespace MarriageAgencyStatistics.DesktopClient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RestClient _client;
        private DateTime _choosenDate;
        private bool _reportIsGenerating = true;
        private string _path;
        public RelayCommand GenerateReport => new RelayCommand(Generate, ReportGenerating);

        private bool ReportGenerating()
        {
            return _reportIsGenerating;
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

            Path = ConfigurationManager.AppSettings["path"];
            ChoosenDate = DateTime.Now;
        }

        public void Generate()
        {
            _reportIsGenerating = true;
            GenerateReport.RaiseCanExecuteChanged();

            ThreadPool.QueueUserWorkItem(state =>
            {
                Log("получаем список пользователей...");
                var users = _client.Get<List<UserModel>>(new RestRequest($"users")).Data;
                Log("готово!");

                Log("получаем бонусы...");
                var bonus = _client.Get<List<UserBonusModel>>(new RestRequest($"bonus?date={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}")).Data;
                Log("готово!");

                Log("считаем статистику онлайн...");
                var statistics = _client.Get<List<UserOnlineStatisticsModel>>(new RestRequest($"statistic?date={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}")).Data;
                Log("готово!");

                Log("считаем отправленные письма...");
                var sentEmails = _client.Get<List<UserSentEmailsStatisticsModel>>(new RestRequest($"sentemails?dateFrom={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}&dateTo={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}")).Data;
                Log("готово!");


                List<(User, Bonus, OnlineStatistics, SentEmailStatistics)> result = new List<(User, Bonus, OnlineStatistics, SentEmailStatistics)>();

                foreach (var user in users)
                {
                    var b = bonus.FirstOrDefault(model => model.User.Title == user.Title);
                    var s = statistics.FirstOrDefault(model => model.User.Title == user.Title);
                    var e = sentEmails.FirstOrDefault(model => model.User.Title == user.Title);

                    (User user, Bonus bonus, OnlineStatistics statistics, SentEmailStatistics emails) item = (
                        new User
                        {
                            Name = user.Title
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

            });

            _reportIsGenerating = false;
            GenerateReport.RaiseCanExecuteChanged();
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
