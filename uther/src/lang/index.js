import { createI18n } from 'vue-i18n'
import elementEnLocale from 'element-plus/es/locale/lang/en'
import elementZhLocale from 'element-plus/es/locale/lang/zh-cn'
import enLocale from './en'
import zhLocale from './zh'
const Avue = window.AVUE || { locale: { en: {}, zh: {} } }
const messages = {
  en: {
    ...enLocale,
    ...elementEnLocale,
    ...Avue.locale.en,
  },
  zh: {
    ...zhLocale,
    ...elementZhLocale,
    ...Avue.locale.zh,
  }
}

const i18n = createI18n({
  legacy: true,
  globalInjection: true,
  locale: 'zh',// getStore({ name: 'language' }) || 'zh',
  messages
})

export default i18n
