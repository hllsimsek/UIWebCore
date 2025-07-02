using CoreFront;
using CoreHelper.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjeModels;
using static Core.Models.CoreEnums;
using static ProjeCoreModel.Enums;

namespace UIWeb
{
    public class KullaniciController : UIWebController
    {
        [HttpPost]
        [Authentication(IsCsrfKontrol = true)]
        public object GetClientUserInfo()
        {
            KullaniciOkuPostModel postModel = new()
            {
                userid = GetUserId
            };

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }

        [HttpPost]
        [Authorization(Action = EntitiyAction.Listele, Menu = EntitiyMenu.KullaniciListesi, isCsrfKontrol = true)]
        public object KullaniciListesi(KullaniciListePostModel postModel)
        {
            postModel.userid = GetUserId;

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }

        [HttpPost]
        [Authorization(Action = EntitiyAction.Güncelle, Menu = EntitiyMenu.KullaniciListesi)]
        public object KullaniciDurumGuncelle(KullaniciDurumGuncellePostModel postModel)
        {
            postModel.loguserid = GetUserId;

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }

        [HttpPost]
        [Authorization(Menu = EntitiyMenu.KullaniciListesi, Action = EntitiyAction.Listele)]
        public object KullaniciOku(KullaniciOkuPostModel postModel)
        {
            postModel.detayuserid = postModel.userid;
            postModel.userid = GetUserId;

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }

        [HttpPost]
        [Authorization(Action = EntitiyAction.YeniEkle, Menu = EntitiyMenu.KullaniciListesi)]
        public object KullaniciOlustur(KullaniciOlusturPostModel postModel)
        {
            UIContractID _UIContract;

            postModel.loguserid = GetUserId;
            postModel.url = CoreFront.Helper.Utility.DomainName(HttpContext);

            _UIContract = UIProxy.PostAsJson<UIContractID>(ControllerContext, postModel, UITokenBilgisi);

            return Json(_UIContract);
        }

        [HttpPost]
        [Authorization(Action = EntitiyAction.Güncelle, Menu = EntitiyMenu.KullaniciListesi)]
        public object KullaniciGuncelle(KullaniciGuncellePostModel postModel)
        {
            postModel.loguserid = GetUserId;
            postModel.url = CoreFront.Helper.Utility.DomainName(HttpContext);

            UIContract _UIContract = UIProxy.PostAsJson<UIContract>(ControllerContext, postModel, UITokenBilgisi);

            return Json(_UIContract);
        }


        [HttpPost]
        [Authentication(IsCsrfKontrol = true)]
        [EnableRateLimiting("1_d_3")]
        public object KullaniciEpostaGonder(KullaniciEmailPostModel postModel)
        {
            postModel.userid = GetUserId;
            postModel.url = CoreProxy.Utility.DomainName(ControllerContext.HttpContext);

            return UIProxy.PostAsJson(ControllerContext, postModel, UITokenBilgisi);
        }
    }
}