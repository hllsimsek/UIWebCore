/// <reference path="../../../../uiweb.plugin.js" />
define([
    'text!module/kullaniciyonetimi/list/kullanicilistesi.html',
], function ($TPL) {

    var _that;

    var DeleteModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciSil"
    });

    var VIEWER = Backbone.View.extend({

        el: null,

        preinitialize: function (_context, filtrePanel, parent) {
            _that = this;

            this.el = _context;
            this.parent = parent;
            this.filtrePanel = filtrePanel;
        },

        initialize: function () {

            this.toTPL($TPL);
            this.$el.append($TPL);

            this.Grid = $("#dtKullaniciListesi", this.$el);

            // Plugin Bind
            uiWeb.plugin.contentBindManager.bindallPlugin(this.$el);
           
        },

        events: {
            "click .dtKullaniciListesi.button-delete": "sil",
            "dtKullaniciListesi.gridCompleted #dtKullaniciListesi": "gridOnCompleted"
        },

        gridOnCompleted: function () {
            var data = null,
                _tdSifre = null,
                _tdDurum = null;

            $.each($("tr", this.Grid), function (i, e) {

                e = $(e);
                data = e.data().data;

                if (data != null) {

                    _tdSifre = $("td.td-sifre", e);
                    _tdDurum = $("td.td-durum", e);

                    if (data.ldapuser == true) {
                        _tdSifre.removeClass("button-sifre");
                        $("i", _tdSifre).removeClass("icon-sifre");
                    }

                    switch (data.statuscode) {
                        case uiWeb.UserStatusEnum.Aktif:
                        case uiWeb.UserStatusEnum.ŞifreBloke:
                        case uiWeb.UserStatusEnum.ŞifreYenileme:
                            if (data.ldapuser == false)
                                $("i", _tdSifre).attr("title", "Şifre Sıfırlama E-Posta'sı Gönder");

                            break;
                        case uiWeb.UserStatusEnum.DoğrulamaBekleniyor:

                            $("i", _tdSifre).attr("title", "Doğrulama E-Posta'sı Gönder");
                            $("i", _tdSifre).removeClass("icon-sifre").addClass("icon-mail");

                            break;
                        case uiWeb.UserStatusEnum.Pasif:
                            _tdSifre.removeClass("button-sifre");
                            _tdDurum.removeClass("button-pasif").addClass("button-onay");

                            $("i", _tdDurum).attr("title", "Aktif Yap");
                            $("i", _tdDurum).removeClass("icon-pasif").addClass("icon-onayla");

                            break;
                        default:
                            _tdSifre.removeClass("button-sifre");
                            break;
                    }
                }
            });

        },

        sil: function (element) {

            var element = $(element.currentTarget);
            var data = $(element).parents("tr").eq(0).data("data");

            if (data.Id != null) { // Delete

                this.model = new DeleteModel();
                this.model.bind("sync", this.resultDelete, this);

                this.model.fetch({
                    content: this.$el,
                    eventType: "D",
                    extendValue: { id: data.id }
                });
            }
        },

        yenile: function () {
            uiWeb.manager.assistant.reload(this.Grid, this.filtrePanel);
        },


        temizle: function () {
            uiWeb.manager._contentClear(this.parent.el, this.filtrePanel);
        },

        resultDelete: function () {
            this.yenile();
        },

        show: function () {
            $("#modal-kullanicifiltre", this.filtrePanel).modalOpen();
            return this;
        },

        hide: function () {
            $("#modal-kullanicifiltre", this.filtrePanel).modalClose();
            return this;
        },

        render: function (params) {

            this.yenile();
            return this;
        }
    });

    return VIEWER;
});
