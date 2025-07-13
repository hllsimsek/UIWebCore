using Core.Models;
using CoreApplication.Abstract;
using CoreHelper.Helper;
using Microsoft.AspNetCore.Mvc;
using ProjeModelsAPP;
using ProjeModelsAPP.Models.Kullanici.ReturnModel;
using UIAPIApplication.Abstract;

namespace UIAPI
{
    public class KullaniciController : UIApiController
    {
        private readonly IKullaniciService kullaniciService;

        public KullaniciController(ISistemService sistem, IKullaniciService kullaniciServiceManager) : base(sistem)
        {
            kullaniciService = kullaniciServiceManager;
            kullaniciService.SistemS = SistemS;
            kullaniciService.SistemY = SistemY;
        }


        [HttpPost]
        public GetClientUserInfoReturnModel GetClientUserInfo([FromBody] KullaniciOkuPostModel postModel)
        {
            kullaniciService.SistemY.RouteData = ControllerContext.RouteData;
          
            postModel.userid = GetUserId;
            var _result = kullaniciService.GetClientUserInfo(postModel, GetIsAdmin);
          
            return _result;
        }

        [HttpPost]
        public UIContract KullaniciListesi([FromBody] KullaniciListePostModel postModel)
        {
            var _list = UIDataModelP().ResultToObject(postModel);

            var _result = new DataGridReturnModel
            {
                recordsTotal = _list.totalrecordcount,
                recordsFiltered = _list.totalrecordcount,
                data = _list.Data
            };

            UIContract.data = _result;

            UIContract.status = UIOperationStatus.Information;
            return UIContract;
        }

        [HttpPost]
        public UIContract KullaniciOku([FromBody] KullaniciOkuPostModel postModel)
        {
            postModel.userid = postModel.detayuserid;    

            UIContract.data = UIDataModelP().ResultToDataTable(postModel);
            UIContract.status = UIOperationStatus.Information;
            return UIContract;
        }

        [HttpPost]
        public UIContractID KullaniciOlustur([FromBody] KullaniciOlusturPostModel postModel)
        {
            kullaniciService.SistemY.RouteData = ControllerContext.RouteData;
            return kullaniciService.KullaniciOlustur(postModel);
        }

        [HttpPost]
        public UIContract KullaniciGuncelle([FromBody] KullaniciGuncellePostModel postModel)
        {
            kullaniciService.SistemY.RouteData = ControllerContext.RouteData;
            return kullaniciService.KullaniciGuncelle(UIContract, postModel);
        }

        [HttpPost]
        public UIContract KullaniciDurumGuncelle([FromBody] KullaniciDurumGuncellePostModel postModel)
        {
            var _result = UIDataModelP().ResultToDataTable(postModel);
            UIContract.DBOperasyonSonucBelgesiIsle(_result);

            return UIContract;
        }

        [HttpPost]
        public UIContract KullaniciEpostaGonder([FromBody] KullaniciEmailPostModel postModel)
        {
            kullaniciService.SistemY.RouteData = ControllerContext.RouteData;
            return kullaniciService.KullaniciEpostaGonder(postModel);
        }

    }
}