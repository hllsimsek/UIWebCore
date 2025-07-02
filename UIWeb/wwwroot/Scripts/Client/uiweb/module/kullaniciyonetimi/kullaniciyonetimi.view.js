/// <reference path="../../../uiweb.plugin.js" />

define([
    'text!module/kullaniciyonetimi/kullaniciyonetimi.html',
    'text!module/kullaniciyonetimi/list/kullanicifiltre.html'
], function ($TPL, $Filtre) {

    var _that;

    var KullaniciEpostaModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciEpostaGonder"
    });

    var KullaniciDurumGuncelleModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciDurumGuncelle"
    });


    var VIEWER = Backbone.View.extend({

        el: $("#content"),

        initialize: function (_const) {
            _that = this;

            _that.kullaniciEkleView = null;

            this.toTPL($TPL);
            this.$el.append($TPL);

            _that.yetkimenuid = _const.params.MenuId;
            this.filtrePanel = $("#kullanicifiltreform", this.$el).append($Filtre);
            uiWeb.plugin.contentBindManager.bindallPlugin(this.$el, this.$el);

            $("[name='UserId']", this.filtrePanel).val(uiWeb.user.userid);

            this.kullaniciListesiAc();
        },

        events: {
            "click #formutemizle": "formTemizle",
            "click #listele": "listeYenile",
            "click #yeniekle": "create",
            "click #filtre": "filtre",
            "click .button-sifre": "mailgonder",
            "click .button-onay": "aktifyap",
            "click .button-pasif": "pasifyap",
            "click .button-edit": "edit",
            "click #deneme":"deneme"
        },

        deneme: function () {
            console.log("denememememem")
        },

        aktifyap: function (element) {
            var element = $(element.currentTarget);
            var data = $(element).parents("tr").eq(0).data("data");

            this.kullaniciDurumGuncelle(uiWeb.UserStatusEnum.Aktif, data);
        },

        pasifyap: function (element) {
            var element = $(element.currentTarget);
            var data = $(element).parents("tr").eq(0).data("data");

            this.kullaniciDurumGuncelle(uiWeb.UserStatusEnum.Pasif, data);
        },

        kullaniciDurumGuncelle: function (statuscode, data) {

            var _mesaj = "'Kullanıcıyı aktif etmek istediğinizden emin misiniz?'";

            if (statuscode == uiWeb.UserStatusEnum.Pasif) {
                _mesaj = "Kullanıcıyı pasif durumuna getirmek istediğinizden emin misiniz?";
            }

            $.SmartMessageBox({
                title: "Onay İşlemi",
                content: _mesaj,
                buttons: '[Hayır][Evet]'
            }, function (button, message) {
                if (button === 1) {
                    return false;
                }
                else if (button === 2) {
                    new KullaniciDurumGuncelleModel()
                        .bind("sync", _that.listeYenile, this).fetch({
                            content: _that.$el,
                            isLoading: true,
                            extendValue: {
                                userid: data.userid,
                                firmaid: data.firmaid,
                                statuscode: statuscode
                            }
                        });
                }
            });
        },

        mailgonder: function (element) {
            var element = $(element.currentTarget);
            var data = $(element).parents("tr").eq(0).data("data");

            var BildirimTuru = 0;

            if (data.userid != null) {

                switch (data.statuscode) {
                    case uiWeb.UserStatusEnum.Aktif:
                    case uiWeb.UserStatusEnum.ŞifreBloke:
                    case uiWeb.UserStatusEnum.ŞifreYenileme:
                        BildirimTuru = uiWeb.BildirimTuruEnum.ŞifreSıfırla;

                        break;
                    case uiWeb.UserStatusEnum.DoğrulamaBekleniyor:
                        BildirimTuru = uiWeb.BildirimTuruEnum.HesapDoğrulama;
                        break;
                    default:
                        break;
                }

                if (BildirimTuru != 0) {

                    $.SmartMessageBox({
                        title: "Onay İşlemi",
                        content: "İşlemi yapmak istediğinizden emin misiniz?",
                        buttons: '[Hayır][Evet]'
                    }, function (button, message) {
                        if (button === 1) {
                            return false;
                        }
                        else if (button === 2) {
                            _that.KullaniciEpostaGonder(data.userid, BildirimTuru);
                        }
                    });
                }
            }
        },

        KullaniciEpostaGonder: function (UserId, BildirimTuru) {

            new KullaniciEpostaModel()
                .bind("sync", this.KullaniciEpostaGonderSonuc, _that).fetch({
                    content: _that.$el,
                    isLoading: true,
                    extendValue: {
                        kullaniciuserid: UserId,                        
                        bildirimturid: BildirimTuru
                    }
                });
        },

        KullaniciEpostaGonderSonuc: function () {
            _that.listeYenile();
        },

        filtre: function (element) {
            $("#modal-kullanicifiltre", this.filtrePanel).modalOpen();
        },

        formTemizle: function () {
            uiWeb.manager._contentClear(_that.filtrePanel, _that.filtrePanel);
        },

        listeYenile: function () {
            _that.kullaniciListesi.yenile();
        },

        edit: function (element) {
            var element = element.currentTarget;
            var data = $(element).parents("tr").eq(0).data("data");

            if (data.userid != null) {
                this.kullaniciDetayFormAc(data);
            }
        },

        create: function () {
            this.kullaniciEkleFormAc();
        },

        kullaniciListesiAc: function () {
            var _panel = $("#kullanicilistesi", _that.$el);

            uiWeb.App.master.newChildContext(_panel, "module/kullaniciyonetimi/list/kullanicilistesi.view", function (_kullaniciListesi) {
                _that.kullaniciListesi = new _kullaniciListesi(_panel, _that.filtrePanel, _that).render();
            });
        },

        kullaniciDetayFormAc: function (data) {

            if (_that.kullaniciEkleView == null) {

                var _panel = $("#kullanicieklemeform", _that.$el);

                uiWeb.App.master.newChildContext(_panel, "module/kullaniciyonetimi/form/kullaniciekle.view", function (_kullaniciEkle) {
                    _that.kullaniciEkleView = new _kullaniciEkle(_panel, _that).render({ userid: data.userid, statuscode: data.statuscode });
                })
            }
            else {
                _that.kullaniciEkleView.render({ userid: data.userid, statuscode: data.statuscode });
            }
        },

        kullaniciEkleFormAc: function () {

            if (_that.kullaniciEkleView == null) {
                var _panel = $("#kullanicieklemeform", _that.$el);

                uiWeb.App.master.newChildContext(_panel, "module/kullaniciyonetimi/form/kullaniciekle.view", function (_kullaniciEkle) {
                    _that.kullaniciEkleView = new _kullaniciEkle(_panel, _that).render({ userid: null });
                })
            }
            else {
                _that.kullaniciEkleView.render({ userid: null });
            }
        },

        render: function (params) {

         
            return this;
        }
    });

    return VIEWER;
});
