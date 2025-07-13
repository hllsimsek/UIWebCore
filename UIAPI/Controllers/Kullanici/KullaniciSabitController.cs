using CoreApplication.Abstract;
using CoreHelper.Helper;
using Microsoft.AspNetCore.Mvc;
using ProjeModelsAPP;

namespace UIAPI
{
    public class KullaniciSabitController(ISistemService sistem) : UIApiController(sistem)
    {
        [HttpPost]
        public UIContract KullaniciTurListesi([FromBody] KullaniciTurListesiPostModel postModel)
        {
            UIContract.data = UIDataModelP().ResultToDataTable(postModel);
            UIContract.status = UIOperationStatus.Information;

            return UIContract;
        }


        [HttpPost]
        public UIContract KullaniciStatuListesi([FromBody] UserStatusPostModel postModel)
        {
            UIContract.data = UIDataModelP().ResultToDataTable(postModel);
            UIContract.status = UIOperationStatus.Information;

            return UIContract;
        }
    }
}
