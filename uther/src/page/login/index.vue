<template>
  <div ref="login" @keyup.enter.native="handleLogin" class="login-content">
    <header>
      <div class="showTime">
        <span class="date">
          <span>{{ nowTime }}</span>
        </span>
      </div>
      <div class="title-content">
        <el-image src="/img/bg/icon.png" fit="cover" />
        <h2>{{ websiteTitleName }}</h2>
      </div>
    </header>
    <div ref="form" class="form">
      <span class="title">{{ $t('login.title') }}</span>
      <userLogin style="width: 78%" />
    </div>
    <!-- 底部信息 -->
    <div class="login-container-conpyright">
      <div class="login-container-conpyright-text">{{ $t('login.allRightsReserved') }} v1.8.4</div>
    </div>
  </div>
</template>
<script>
import userLogin from './userlogin'
import codeLogin from './codelogin'
import thirdLogin from './thirdlogin'
import { mapGetters } from 'vuex'
import { validatenull } from '@/util/validate'
import topLang from '@/page/index/top/top-lang'
import topColor from '@/page/index/top/top-color'
import { getQueryString, getTopUrl } from '@/util/util'
import { setStore } from '@/util/store'
// import { screenSize } from '@/util/transfrom'
import dayjs from 'dayjs'

export default {
  name: 'login',
  components: {
    userLogin,
    codeLogin,
    thirdLogin,
    topLang,
    topColor
  },
  data() {
    return {
      nowTime: '',
      activeName: 'user',
      socialForm: {
        tenantId: '000000',
        source: '',
        code: '',
        state: ''
      },
      websiteTitleName: ''
    }
  },
  watch: {
    $route() {
      this.handleLogin()
    }
  },
  created() {
    this.handleLogin()
  },
  mounted() {
    // screenSize(this.$refs.form)+
    setInterval(() => {
      this.nowTime =
        dayjs().format('YYYY') +
        '年' +
        dayjs().format('MM') +
        '月' +
        dayjs().format('DD') +
        '日 ' +
        dayjs().format('HH:mm:ss')
    }, 1000)

    window.addEventListener('setLocalItem', () => {
      const websiteCofigTemp = JSON.parse(localStorage.getItem('websiteCofigTemp')) || {}
      this.websiteTitleName = websiteCofigTemp?.websiteTitleName || ''

      document.title = this.websiteTitleName
    })
  },
  computed: {
    ...mapGetters(['website', 'tagWel'])
  },
  props: [],
  methods: {
    handleLogin() {
      const topUrl = getTopUrl()
      const redirectUrl = '/oauth/redirect/'
      this.socialForm.source = getQueryString('source')
      this.socialForm.code = getQueryString('code')
      this.socialForm.state = getQueryString('state')
      if (validatenull(this.socialForm.source) && topUrl.includes(redirectUrl)) {
        let source = topUrl.split('?')[0]
        source = source.split(redirectUrl)[1]
        this.socialForm.source = source
      }
      if (
        !validatenull(this.socialForm.source) &&
        !validatenull(this.socialForm.code) &&
        !validatenull(this.socialForm.state)
      ) {
        const loading = this.$loading({
          lock: true,
          text: '第三方系统登录中,请稍后。。。',
          spinner: 'el-icon-loading'
        })
        this.$store
          .dispatch('LoginBySocial', this.socialForm)
          .then(() => {
            window.location.href = topUrl.split(redirectUrl)[0]
            this.$router.push({ path: this.tagWel.value })
            loading.close()
          })
          .catch(() => {
            loading.close()
          })
      }
    }
  }
}
</script>

<style lang="less" scope>
.login-content {
  .login-container-conpyright {
    width: 100%;
    text-align: center;
    color: #999;
    font-size: 14px;
    position: absolute;
    bottom: 10px;
  }

  background-image: url('/img/login/bg.jpg');
  background-size: 100%;
  background-position: center;
  background-repeat: no-repeat;
  width: 100%;
  height: 100%;

  header {
    position: absolute;
    height: 7vh;
    background-image: url('/img/WindTurbine/background/titleBg.png');
    background-position: center;
    background-repeat: no-repeat;
    width: 100%;
    z-index: 12;
    margin-top: -2px;

    .title-content {
      display: flex;
      flex-direction: row;
      justify-content: center;
      align-items: center;

      h2 {
        color: #ffffff;
        font-size: 2.8vh;
        line-height: 7vh;
        letter-spacing: 3px;
        margin-left: 10px;
      }

      :deep(.el-image){
        width: 24px;
        height: 27px;
      }
    }

    .showTime {
      position: absolute;
      left: 14.65625vh;
      top: 1.75vh;
      color: #fff;
      display: -webkit-box;
      display: -ms-flexbox;
      display: flex;

      .date {
        span {
          display: block;

          &:nth-child(1) {
            font-size: 1.575vh;
            text-align: right;
            line-height: 2.625vh;
          }

          &:nth-child(2) {
            font-size: 0.245vh;
          }
        }
      }
    }
  }

  .form {
    all: initial;
    background-image: url('/img/login/formBg.png');
    background-size: 100% 100%;
    width: 520px;
    height: 500px;
    position: relative;
    left: 55vw;
    top: 28vh;
    // padding: 3.5%;
    display: flex;
    flex-direction: column;
    align-items: center;

    .title {
      font-size: 32px;
      font-weight: bold;
      color: #dcdfe6;
      letter-spacing: 14px;
      margin-top: 16%;
      margin-bottom: 5%;
    }
  }
}
</style>
