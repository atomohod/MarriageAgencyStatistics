using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using MarriageAgencyStatistics.Core.DataProviders;
using MarriageAgencyStatistics.WebAPI.Controllers.BrideForever.Model;

namespace MarriageAgencyStatistics.WebAPI.Controllers.BrideForever
{
    [Route("api/brideforever")]
    public class BrideForeverController : ApiController
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
