<template>
  <el-form
    class="login-form"
    status-icon
    :rules="loginRules"
    ref="loginForm"
    :model="loginForm"
    label-width="0"
  >
    <!-- <el-form-item v-if="tenantMode" prop="tenantId">
      <el-input
        size="small"
        @keyup.enter.native="handleLogin"
        v-model="loginForm.tenantId"
        auto-complete="off"
        :placeholder="$t('login.tenantId')"
      >
        <i slot="prefix" class="icon-quanxian" />
      </el-input>
    </el-form-item> -->
    <el-form-item prop="username">
      <el-input
        size="small"
        @keyup.enter.native="handleLogin"
        v-model="loginForm.username"
        auto-complete="off"
        :placeholder="$t('login.username')"
        class="username"
      >
        <!-- <i slot="prefix" class="icon-yonghu" /> -->
        <el-image slot="prefix" src="/img/login/user.png" fit="cover" />
      </el-input>
    </el-form-item>
    <el-form-item prop="password">
      <el-input
        size="small"
        @keyup.enter.native="handleLogin"
        :type="passwordType"
        v-model="loginForm.password"
        auto-complete="off"
        :placeholder="$t('login.password')"
        class="password"
      >
        <i class="el-icon-view el-input__icon" slot="suffix" @click="showPassword" />
        <el-image slot="prefix" src="/img/login/Lock.png" fit="cover" @click="showPassword" />
      </el-input>
    </el-form-item>
    <el-form-item v-if="website.captchaMode" prop="code" style="margin-bottom: 0px">
      <div style="display: flex; flex-direction: row; justify-content: space-between">
        <div style="width: 56%">
          <el-input
            size="small"
            @keyup.enter.native="handleLogin"
            v-model="loginForm.code"
            auto-complete="off"
            :placeholder="$t('login.code')"
            class="loginKeys"
          >
            <el-image slot="prefix" src="/img/login/Keys.png" fit="cover" />
          </el-input>
        </div>
        <div style="width: 31%">
          <div class="login-code">
            <img :src="loginForm.image" class="login-code-img" @click="refreshCode" />
          </div>
        </div>
      </div>
    </el-form-item>
    <el-form-item>
      <el-checkbox v-model="loginForm.remember">记住密码</el-checkbox>
    </el-form-item>
    <el-form-item style="margin-top: 40px">
      <el-button
        type="primary"
        size="small"
        @click.native.prevent="handleLogin"
        class="login-submit"
        >{{ $t('login.submit') }}
      </el-button>
    </el-form-item>
    <el-dialog title="用户信息选择" append-to-body :visible.sync="userBox" width="350px">
      <avue-form :option="userOption" v-model="userForm" @submit="submitLogin" />
    </el-dialog>
  </el-form>
</template>

<script>
import { mapGetters } from 'vuex'
import { info } from '@/api/system/tenant'
import { getCaptcha, getHomeRoute } from '@/api/user'
import { getTopUrl } from '@/util/util'
import website from '@/config/website'

const Base64 = require('js-base64').Base64

export default {
  name: 'userlogin',
  data() {
    return {
      tenantMode: this.website.tenantMode,
      loginForm: {
        //租户ID
        tenantId: '000000',
        //部门ID
        deptId: '',
        //角色ID
        roleId: '',
        //用户名
        username: '',
        //密码
        password: '',
        //账号类型
        type: 'account',
        //验证码的值
        code: '',
        //验证码的索引
        key: '',
        //预加载白色背景
        image: 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7',
        remember: false
      },
      loginRules: {
        tenantId: [{ required: false, message: '请输入租户ID', trigger: 'blur' }],
        username: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
        password: [
          { required: true, message: '请输入密码', trigger: 'blur' },
          { min: 1, message: '密码长度最少为6位', trigger: 'blur' }
        ]
      },
      passwordType: 'password',
      userBox: false,
      userForm: {
        deptId: '',
        roleId: ''
      },
      userOption: {
        labelWidth: 70,
        submitBtn: true,
        emptyBtn: false,
        submitText: '登录',
        column: [
          {
            label: '部门',
            prop: 'deptId',
            type: 'select',
            props: {
              label: 'deptName',
              value: 'id'
            },
            dicUrl: '/api/blade-system/dept/select',
            span: 24,
            display: false,
            rules: [
              {
                required: true,
                message: '请选择部门',
                trigger: 'blur'
              }
            ]
          },
          {
            label: '角色',
            prop: 'roleId',
            type: 'select',
            props: {
              label: 'roleName',
              value: 'id'
            },
            dicUrl: '/api/blade-system/role/select',
            span: 24,
            display: false,
            rules: [
              {
                required: true,
                message: '请选择角色',
                trigger: 'blur'
              }
            ]
          }
        ]
      }
    }
  },
  created() {
    this.getTenant()
    this.refreshCode()
    let username = this.getCookie('username')
    let password = Base64.decode(this.getCookie('password'))
    if (username) {
      this.loginForm.username = username
      this.loginForm.password = password
      this.loginForm.remember = true
      website.captchaMode = false
    }
  },
  mounted() {},
  watch: {
    'loginForm.deptId'() {
      const column = this.findObject(this.userOption.column, 'deptId')
      if (this.loginForm.deptId.includes(',')) {
        column.dicUrl = `/api/blade-system/dept/select?deptId=${this.loginForm.deptId}`
        column.display = true
      } else {
        column.dicUrl = ''
      }
    },
    'loginForm.roleId'() {
      const column = this.findObject(this.userOption.column, 'roleId')
      if (this.loginForm.roleId.includes(',')) {
        column.dicUrl = `/api/blade-system/role/select?roleId=${this.loginForm.roleId}`
        column.display = true
      } else {
        column.dicUrl = ''
      }
    }
  },
  computed: {
    ...mapGetters(['tagWel', 'userInfo'])
  },
  props: [],
  methods: {
    refreshCode() {
      if (website.captchaMode) {
        getCaptcha().then(res => {
          const data = res.data
          this.loginForm.key = data.key
          this.loginForm.image = data.image
        })
      }
    },
    showPassword() {
      this.passwordType === '' ? (this.passwordType = 'password') : (this.passwordType = '')
    },
    submitLogin(form, done) {
      if (form.deptId !== '') {
        this.loginForm.deptId = form.deptId
      }
      if (form.roleId !== '') {
        this.loginForm.roleId = form.roleId
      }
      this.handleLogin()
      done()
    },
    handleLogin() {
      this.$refs.loginForm.validate(valid => {
        if (valid) {
          const loading = this.$loading({
            lock: true,
            text: '登录中,请稍后。。。',
            spinner: 'el-icon-loading'
          })
          this.$store
            .dispatch('LoginByUsername', this.loginForm)
            .then(() => {
              if (this.website.switchMode) {
                const deptId = this.userInfo.dept_id
                const roleId = this.userInfo.role_id
                if (deptId.includes(',') || roleId.includes(',')) {
                  this.loginForm.deptId = deptId
                  this.loginForm.roleId = roleId
                  this.userBox = true
                  this.$store.dispatch('LogOut').then(() => {
                    loading.close()
                  })
                  return false
                }
              }
              // let routerName = this.tagWel.value
              this.setUserInfo()
              this.$store.dispatch('GetDeptTree', this.userInfo.user_id).then(res => {
                getHomeRoute().then(res => {
                  if (res.data.success) {
                    this.$router.push({ path: res.data.data })
                  } else {
                    this.$router.push({ path: '/' })
                  }
                })
                loading.close()
              })
            })
            .catch(() => {
              loading.close()
              this.refreshCode()
            })
        }
      })
    },
    getTenant() {
      let domain = getTopUrl()
      // 临时指定域名，方便测试
      //domain = "https://bladex.vip";
      info(domain).then(res => {
        const data = res.data
        this.setLocalstorageItem('websiteCofigTemp', JSON.stringify(data.data))
        if (data.success && data.data.tenantId) {
          this.tenantMode = false
          this.loginForm.tenantId = data.data.tenantId
          this.$parent.$refs.login.style.backgroundImage = `url(${data.data.backgroundUrl})`
        }
      })
    },
    setUserInfo() {
      if (this.loginForm.remember) {
        this.setCookie('username', this.loginForm.username)
        // base64加密密码
        let passWord = Base64.encode(this.loginForm.password)
        this.setCookie('password', passWord)
      } else {
        this.setCookie('username', '')
        this.setCookie('password', '')
        // website.captchaMode = true
      }
    },
    getCookie(key) {
      if (document.cookie.length > 0) {
        let start = document.cookie.indexOf(key + '=')
        if (start !== -1) {
          start = start + key.length + 1
          let end = document.cookie.indexOf(';', start)
          if (end === -1) end = document.cookie.length
          return unescape(document.cookie.substring(start, end))
        }
      }
      return ''
    },
    setCookie(cName, value, expiredays) {
      let exdate = new Date()
      exdate.setDate(exdate.getDate() + expiredays)
      document.cookie =
        cName +
        '=' +
        decodeURIComponent(value) +
        (expiredays == null ? '' : ';expires=' + exdate.toGMTString())
    }
  }
}
</script>

<style lang="less" scoped>
.login-submit {
  font-weight: bold;
  letter-spacing: 6px;
}
::v-deep .el-input__inner,
.el-input__inner:hover {
  color: #dcdfe6;
  border-radius: 0 4px 4px 0;
  border-color: rgba(14, 91, 109, 1);
  -webkit-box-shadow: 1px 1px 15px rgb(14, 91, 109, 1) inset;
  box-shadow: 1px 1px 15px rgb(14, 91, 109, 1) inset;
  background: rgba(14, 91, 109, 1) !important;
}
::v-deep .el-input--small .el-input__inner {
  height: 47px;
  line-height: 47px;
  padding-left: 40px;
  font-size: 18px;
}
::v-deep .el-form-item {
  margin-bottom: 20px;
}
::v-deep .el-button--small {
  width: 100%;
  height: 45px;
}
::v-deep .el-button--primary {
  background-color: rgba(14, 91, 109, 1);
  border-color: rgba(14, 91, 109, 1);
  &:hover {
    background-color: rgba(14, 91, 109, 1);
  }
}
::v-deep .el-button--small {
  color: #dcdfe6;
  font-size: 18px;
}
.username {
  ::v-deep .el-input__prefix {
    padding: 3% 0;
  }
}
.password {
  ::v-deep .el-input__prefix {
    padding: 3% 0;
  }
}
.loginKeys {
  ::v-deep .el-input__prefix {
    padding: 5% 0;
  }
}
::v-deep .el-form-item__content {
  line-height: 29px;
  padding: 0 10px;
  .el-image__inner {
    padding: 0 10px;
  }
}
::v-deep .el-checkbox__inner {
  border-radius: 0px;
  background-color: rgba(14, 91, 109, 1);
  width: 20px;
  height: 20px;
  border: none;
}
::v-deep .el-checkbox__inner::after {
  height: 14px;
  left: 8px;
}
::v-deep .el-checkbox__input.is-checked .el-checkbox__inner,
.el-checkbox__input.is-indeterminate .el-checkbox__inner {
  background-color: rgba(14, 91, 109, 1);
  border-color: rgba(14, 91, 109, 1);
}
</style>
