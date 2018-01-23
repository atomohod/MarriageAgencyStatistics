using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MarriageAgencyStatistics.Core.DataProviders;
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
        public ICommand GenerateReport => new RelayCommand(Generate);

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
            ChoosenDate = DateTime.Now;
        }

        public void Generate()
        {
            var users = _client.Get<List<UserModel>>(new RestRequest($"users")).Data;

            var bonus = _client.Get<List<UserBonusModel>>(new RestRequest($"bonus")).Data;

            var statistics = _client.Get<List<UserOnlineStatisticsModel>>(new RestRequest($"statistic?date={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}")).Data;

            var sentEmails = _client.Get<List<UserSentEmailsStatisticsModel>>(new RestRequest($"sentemails?dateFrom={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}&dateTo={ChoosenDate.Month}%2F{ChoosenDate.Day}%2F{ChoosenDate.Year}")).Data;

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
                        Today = b.Bonus.Daily,
                        LastMonth = b.Bonus.Monthly
                    },
                    new OnlineStatistics
                    {
                        Online = s.Online
                    },
                    new SentEmailStatistics
                    {
                        SentEmails = e.EmailsCount
                    });

                result.Add(item);
            }

            BrideForeverExcel.UpdateExcel(result, @"E:\BrideForever.xls");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
