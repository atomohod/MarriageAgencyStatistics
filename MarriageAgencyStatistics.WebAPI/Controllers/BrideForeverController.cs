using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarriageAgencyStatistics.Core.Clients.BrideForever;
using MarriageAgencyStatistics.Core.DataProviders.BrideForever;
using MarriageAgencyStatistics.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarriageAgencyStatistics.WebAPI.Controllers
{
    [Route("api/[controller]")]
    public class BrideForeverController : Controller
    {
        private readonly BrideForeverDataProvider _dataProvider;

        public BrideForeverController(BrideForeverDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        // GET api/values
        [HttpGet]
        public async Task<IEnumerable<UserBonusModel>> GetUserBonus()
        {
            var users = await _dataProvider.GetUsers();

            var result = new List<UserBonusModel>();

            foreach (var user in users)
            {
                var userBonus = await _dataProvider.GetUserBonus(user);

                //TODO user automapper
                result.Add(new UserBonusModel
                {
                    User = new UserModel
                    {
                        Title = user.Name
                    },

                    Bonus = new BonusModel
                    {
                        Daily = userBonus.Today,
                        Monthly = userBonus.LastMonth
                    }
                });
            }

            return result;
        }
    }
}
