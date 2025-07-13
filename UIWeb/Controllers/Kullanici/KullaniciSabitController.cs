using CoreFront;
using Microsoft.AspNetCore.Mvc;
using ProjeModels;

namespace UIWeb
{
    public class KullaniciSabitController : UIWebController
    {
        [HttpPost]
        [Authentication]
        public object KullaniciTurListesi(KullaniciTurListesiPostModel postModel)
        {           
            postModel.userid = GetUserId;
            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }

        [HttpPost]
        [Authentication]
        public object KullaniciStatuListesi(UserStatusPostModel postModel)
        {
            postModel.userid = GetUserId;

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }
    }
}