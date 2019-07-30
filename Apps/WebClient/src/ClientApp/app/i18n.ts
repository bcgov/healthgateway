import Vue from 'vue';
import VueI18n from 'vue-i18n';

Vue.use(VueI18n);

type NewType = any;

interface Messages {
    [key:string]: NewType;
}

function loadLocaleMessages() {
    const locales = require.context('@/assets/locales', true, /[A-Za-z0-9-_,\s]+\.json$/i);
    const  messages: Messages = {};

    locales.keys().forEach(key => {
      //console.debug("found keys");
      const matched = key.match(/([A-Za-z0-9-_]+)\./i);
      if (matched && matched.length > 1) {

        const locale = matched[1];
        messages[locale] = locales(key);
      }
    })
    return messages;
  }

  export default new VueI18n({
    //locale: process.env.VUE_APP_I18N_LOCALE || 'en',
    //fallbackLocale: process.env.VUE_APP_I18N_FALLBACK_LOCALE || 'en',
    locale: 'en',
    fallbackLocale: 'en',
    messages: loadLocaleMessages()
  })