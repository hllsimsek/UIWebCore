using CoreFront;
using CoreHelper.Helper;
using Microsoft.AspNetCore.Mvc;
using ProjeModels;
using ProjeCoreModel;
using ProjeModels.CoreModels;
using Microsoft.AspNetCore.RateLimiting;

namespace UIWeb
{
    public class LoginController : UIBaseController
    {
        IConfiguration Configuration;
        public LoginController(IConfiguration configuration) => Configuration = configuration;

        public static readonly object LockObject = new();

        [HttpPost]
        [EnableRateLimiting("3_sn_1")]
        public JsonResult KullaniciGirisi(SP_KullaniciGirisUserNamePostModel postModel)
        {
            lock (LockObject)
            {
                KullaniciGirisiData result;

                KullaniciGirisReturnModel uIContract = new()
                {
                    message = new UIMessage(),
                    status = UIOperationStatus.Authentication
                };

                int _hataliGiris = LoginIslemleri.GetHataliGiris(HttpContext) + 1;
                LoginIslemleri.SetHataliGiris(HttpContext, _hataliGiris);

                if (_hataliGiris > LoginIslemleri.HataliGirisSayisi)
                {
                    uIContract.IsSessionTimeOut = true;
                    uIContract.IsCapVisible = true;

                    uIContract.status = UIOperationStatus.Warning;

                    long _timeout = LoginIslemleri.GetSessionTimeoutTime(HttpContext);

                    if (_timeout == 0 || _timeout > DateTime.Now.Ticks)
                    {
                        if (_timeout == 0)
                        {
                            LoginIslemleri.SetSessionTimeoutTime(HttpContext, DateTime.Now.AddMinutes(5));
                        }

                        _timeout = LoginIslemleri.GetSessionTimeoutTime(HttpContext);

                        string _mesaj = string.Format("Çok fazla giriş denemesi yaptığınız için {0} dakika sonra işlem yapabilirsiniz.", (new DateTime(_timeout) - DateTime.Now).Minutes + 1);

                        uIContract.message.Set(uIContract.status, _mesaj);

                        return Json(uIContract);
                    }
                }

                Guid _guid = Guid.NewGuid();
                Response.SetCookieCSRFToken(_guid);
                LoginIslemleri.SetSessionCSRFToken(HttpContext, _guid);              

                postModel.ipadres = CoreFront.Helper.Utility.GetClientIp(HttpContext);
                postModel.machinename = CoreFront.Helper.Utility.GetMachineName;
                postModel.ismobiledevice = CoreFront.Helper.Utility.IsMobileDevice(HttpContext);
                postModel.domain = CoreFront.Helper.Utility.DomainName(HttpContext);

                result = UIProxy.PostAsJson<KullaniciGirisiData>(ControllerContext, postModel, null);

                if (result.status == UIOperationStatus.Information)
                {
                    LoginIslemleri.SetHataliGiris(HttpContext, 0);
                    LoginIslemleri.SetSessionTimeoutTime(HttpContext, DateTime.Now);

                    Response.ClientTokenOlustur(string.Format("{0}", result.OturumAnahtari));
                }

                uIContract.status = result.status;
                uIContract.data = result.data;
                uIContract.message = result.message;

                if (result.IsError)
                {
                    if (_hataliGiris > 1)
                    {
                        uIContract.IsCapVisible = true;
                    }

                    if (_hataliGiris > LoginIslemleri.HataliGirisSayisi)
                    {
                        LoginIslemleri.SetSessionTimeoutTime(HttpContext, DateTime.Now.AddMinutes(5));
                        uIContract.IsSessionTimeOut = true;
                        uIContract.SessionTimeOut = LoginIslemleri.GetSessionTimeoutTime(HttpContext);
                    }
                }

                return Json(uIContract);
            }
        }

        [HttpPost]
        public JsonResult SifremiUnuttumGiris(SifremiUnuttumPostModel postModel)
        {
            UIContract UIContract;         

            KullaniciEmailPostModel postModelEposta = new()
            {
                kullaniciadi = postModel.eposta,
                url = CoreFront.Helper.Utility.DomainName(HttpContext),
                bildirimturid = Enums.BildirimTuruEnum.ŞifremiUnuttum
            };

            UIContract = UIProxy.PostAsJson<UIContract>(ControllerContext, "Kullanici", "KullaniciEpostaGonder", postModelEposta, null);

            //LoginIslemleri.ClientCSRFAnahtariOlustur();

            return Json(UIContract);
        }

        [HttpPost]
        public JsonResult SifreDegistir(SP_KullaniciSifreDegistirPostModel postModel)
        {
            UIContract UIContract;          

            if (!LoginIslemleri.IsValidPassword(string.Format("{0}", postModel.sifre)))
            {
                LoginIslemleri.SetRandomString(HttpContext);

                UIContract = new UIContract();
                UIContract.message.Set(UIOperationStatus.Authentication, "Şifreniz en az bir büyük harf , sayı ve en az 8 karakterden oluşması gerekmektedir.. Lütfen gözden geçirerek yeniden deneyiniz.");

                return Json(UIContract);
            }

            if (postModel.sifre != postModel.sifretekrar)
            {
                LoginIslemleri.SetRandomString(HttpContext);

                UIContract = new UIContract();
                UIContract.message.Set(UIOperationStatus.Authentication, "Şifreler eşleşmiyor, lütfen kontrol ediniz.");

                return Json(UIContract);
            }

            LoginIslemleri.SetRandomString(HttpContext);

            KullaniciHesabiDogrulaData _model = UIProxy.PostAsJson<KullaniciHesabiDogrulaData>(ControllerContext, "Login", "KullaniciHesapDogrula", new KullaniciHesapDogrulaPostModel()
            {
                eposta = postModel.eposta,
                sifredegistirmeanahtari = postModel.sifredegistirmeanahtari
            }, null);

            if (_model != null)
            {
                if (_model.sonuc)
                {
                    postModel.userid = _model.userid;
                    UIContract = UIProxy.PostAsJson<UIContract>(ControllerContext, postModel, null);
                }
                else
                {
                    UIContract = _model;
                }
            }
            else
            {
                UIContract = new UIContract();
                UIContract.message.Set(UIOperationStatus.Exception, "İşlem sırasında bir hata oluştu. Hata incelenmek üzere kayıt altına alındı");
            }

            return Json(UIContract);
        }

        public ActionResult HesapDogrula()
        {
            Response.Cookies.Delete("clienttoken");

            @ViewBag.Title = "Hesap Dogrula";

            if (!Guid.TryParse(string.Format("{0}", RouteData.Values["Key"]), out Guid result))
            {
                return RedirectToAction("Error", "Home", new { type = 1 });
            }

            if (!Guid.TryParse(string.Format("{0}", RouteData.Values["Type"]), out result))
            {
                return RedirectToAction("Error", "Home", new { type = 1 });
            }

            string Key = string.Format("{0}", RouteData.Values["Key"]);
            string Type = string.Format("{0}", RouteData.Values["Type"]);

            byte _EmailTurId;

            if (Type == "8079a5cf-297c-4d5a-8339-972b25c2f251") // Hesap Doğrulama
            {
                _EmailTurId = Enums.BildirimTuruEnum.HesapDoğrulama;
            }
            else if (Type == "5ec50f3d-703c-4587-8637-0775ac9c120b") // Şifre Değişikliği
            {
                _EmailTurId = Enums.BildirimTuruEnum.ŞifremiUnuttum;
            }
            else if (Type == "5ec50f3d-203c-9774-3654-0775ac9c120b") // Şifre Değişikliği
            {
                _EmailTurId = Enums.BildirimTuruEnum.ŞifreSıfırla;
            }
            else
            {
                return RedirectToAction("Error", "Home", new { type = 1 });
            }

            KullaniciHesabiDogrulaData _model = UIProxy.PostAsJson<KullaniciHesabiDogrulaData>(ControllerContext, "Login", "KullaniciHesapDogrula", new KullaniciHesapDogrulaPostModel()
            {
                dogrulamakey = Key,
                type = _EmailTurId
            }, null);

            if (_model != null)
            {
                if (_model.sonuc)// Doğrulama başarılı
                {
                    ViewBag.SifreDegistirmeAnahtari = _model.sifredegistirmeanahtari;
                    ViewBag.Eposta = _model.eposta;
                }
                else
                {
                    return RedirectToAction("Error", "Home", new { type = _model.sonucdurumkodu });
                }
            }

            return View();
        }

        public void Cikis()
        {
            Response.Cookies.Delete("clienttoken");

            Response.Redirect(string.Concat("/", Utility.UrlPath));
        }
    }
}
