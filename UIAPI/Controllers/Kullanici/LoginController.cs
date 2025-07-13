using CoreApplication.Abstract;
using CoreHelper.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProjeModelsAPP.CoreModels;
using ProjeModelsAPP.Models.Kullanici.ReturnModel;
using UIAPIApplication.Abstract;

namespace UIAPI
{
    public class LoginController : UIApiController
    {
        readonly ILoginService loginService;
        public LoginController(ISistemService sistem, ILoginService loginServiceManager) : base(sistem)
        {
            loginService = loginServiceManager;
            loginService.SistemS = SistemS;
            loginService.SistemY = SistemY;
        }

        [HttpPost]
        public KullaniciGirisiData KullaniciGirisi([FromBody] SP_KullaniciGirisUserNamePostModel postModel)
        {
            loginService.SistemY.RouteData = ControllerContext.RouteData;
            KullaniciGirisiData _return = loginService.KullaniciGirisi(postModel);

            return _return;
        }

        [HttpPost]
        public KullaniciHesabiDogrulaData KullaniciHesapDogrula([FromBody] KullaniciHesapDogrulaPostModel postModel)
        {
            loginService.SistemY.RouteData = ControllerContext.RouteData;
            KullaniciHesabiDogrulaData _return = loginService.KullaniciHesapDogrula(postModel);

            return _return;
        }

        [HttpPost]
        public UIContract SifreDegistir([FromBody] SP_KullaniciSifreDegistirPostModel postModel)
        {
            loginService.SistemY.RouteData = ControllerContext.RouteData;
            return loginService.SifreDegistir(postModel);
        }

    }
}