/// <reference path="../../../../uiweb.plugin.js" />

define([
    'text!module/kullaniciyonetimi/form/kullaniciekle.html'
], function ($TPL) {
    var _that;

    var KullaniciOlusturModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciOlustur"
    });

    var KullaniciGuncelleModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciGuncelle"
    });

    var KullaniciOkuModel = Backbone.Model.extend({
        url: "/Kullanici/KullaniciOku"
    });

    var VIEWER = Backbone.View.extend({
        el: null,

        preinitialize: function (selector, parent) {
            _that = this;
            _that.parent = parent;
            _that.el = selector;
        },

        initialize: function () {
            this.toTPL($TPL);
            this.$el.append($TPL);

            uiWeb.plugin.contentBindManager.bindallPlugin(this.$el);
            $("[name=yetkimenuid]", this.$el).val(_that.parent.yetkimenuid);

            uiWeb.manager.assistant.reload($("#grupid", this.$el), this.$el);
            uiWeb.manager.assistant.reload($("#kullaniciturid", this.$el), this.$el);

        },

        events: {
            "click #btnKullaniciKaydet": "Kaydet"
        },

        KullaniciOku: function () {
            this.model = new KullaniciOkuModel();
            this.model.bind("sync", this.KullaniciOkuSonuc, this);

            this.model.fetch({
                content: _that.$el,
                isLoading: true,
                data: ["userid", "yetkimenuid"]
            });
        },

        KullaniciOkuSonuc: function () {
            this.KullaniciData = this.model.toJSON().data[0];

            
            uiWeb.manager.assistant.setValue(this.KullaniciData, this.$el, this.$el);
        },

        Kaydet: function () {
            var _data = uiWeb.manager.assistant.getValue(["userid", "email", "username"], _that.$el);

            if (_data.userid > 0) {
                if (_data.email != _that.KullaniciData.email) {
                    $.SmartMessageBox({
                        title: "Kullanıcı Eposta Değişikliği",
                        content: "Kullanıcının epostasını değiştirmeniz durumunda yeni eposta ile tekrar doğrulama yapması gerekecektir. İşleme devam edilsin mi?",
                        buttons: '[Hayır][Evet]'
                    }, function (DialogResult) {
                        if (DialogResult === 2) {
                            _that.Guncelle(_data);
                        }
                    });
                }
                else {
                    _that.Guncelle(_data);
                }
            }
            else {
                _that.model = new KullaniciOlusturModel();
                _that.model.bind("sync", _that.KullaniciOlusturSonuc, _that);

                _that.model.fetch({
                    content: _that.$el,
                    isLoading: true,
                    extendValue: { username: _data.username },
                    data: []
                });
            }
        },

        KullaniciGuncelleSonuc: function () {
            _that.parent.listeYenile();
        },

        Guncelle: function (params) {
            _that.model = new KullaniciGuncelleModel();
            _that.model.bind("sync", this.KullaniciGuncelleSonuc, this);

            _that.model.fetch({
                content: _that.$el,
                isLoading: true,
                extendValue: { username: params.username },
                data: []
            });
        },

        KullaniciOlusturSonuc: function () {
            var userid = this.model.toJSON().data.Id;
            $("#userid", this.$el).val(userid);

            if (userid > 0) {
                this.KullaniciData = {
                    userid: userid,
                    email: uiWeb.manager.assistant.getElementValue($("[name=email]", this.$el), this.$el)
                }

                this.parent.listeYenile();
                uiWeb.manager.assistant.areaDisable($(".firmapanel", _that.$el), _that.$el);
            }
        },
        show: function () {
            $("#modal-kullaniciekle", this.$el).modalOpen();
            return this;
        },

        hide: function () {
            $("#modal-kullaniciekle", this.$el).modalClose();
            return this;
        },

        render: function (params) {
            this.show();
            this.params = params;

            $(".usernamepanel", _that.$el).addClass("hide");
            uiWeb.manager.assistant.setAreaScope_Clear(this.$el, this.$el);

            $("[name=userid]", this.$el).val(params.userid);

            if (params.userid != null) {
                this.KullaniciOku();
            }

            return this;
        }
    });

    return VIEWER;
});
