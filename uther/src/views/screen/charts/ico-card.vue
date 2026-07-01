<template>
  <div class="content-box" ref="UI">
    <!-- <el-image :src="imgUrl" fit="cover" style="width: 25px; height: 25px" /> -->
    <i :class="['icon', 'local', getClass(this.index)]" style="color: #abe1ff"></i>
    <div>
      <span>
        {{ title }}
      </span>
      <span>
        {{ percentage }}
      </span>
    </div>
    <div>
      <span>较昨日</span>
      <div>
        <span>
          {{ num }}
        </span>
      </div>
    </div>
  </div>
</template>

<script>
import { entityPartEnum } from '@/util/constant'
export default {
  props: {
    title: {
      type: String,
      default: '叶片'
    },
    percentage: {
      type: String,
      default: '50.25%'
    },
    isUp: {
      type: Boolean,
      default: true
    },
    num: {
      type: String,
      default: '2%,23'
    },
    index: {
      type: String,
      default: ''
    },
    bgColor: {
      type: String,
      default: 'rgba(255, 255, 255, 0.1)'
    }
  },
  data() {
    return {
      /*  entityPartEnum: {
        GBX: '齿轮箱',
        GEN: '发电机',
        MSS: '主轴承',
        BLA: '叶片',
        FLG: '法兰',
        TOW: '塔筒',
        NAC: '机舱',
        ROT: '风轮',
        TOWFDA: '塔基',
        STL: '钢索',
        SSD: '钢索',
        TOWTOP: '塔顶',
        BJZ: '变桨轴承',
        OFE: '变桨轴承',
        PBG: '变桨轴承',
        MS: '主轴',
        YBG: '偏航轴承'
      } */
    }
  },
  computed: {
    /*handleIcon() {
      const objEum = {
        true: 'el-icon-caret-top',
        false: 'el-icon-caret-bottom',
        ping: 'el-icon-caret-right'
      }
      return objEum[this.isUp]
    }
     imgUrl() {
      console.log('图片index', this.title, this.index)
      return `/img/screen/blueDevicePart/${this.index}.png`
    } */
  },
  mounted() {
    this.setUI()
  },
  watch: {
    bgColor: {
      handler(val) {
        this.$refs.UI.style.setProperty('--bgColor', val)
      }
    }
  },
  methods: {
    setUI() {
      const objEum = {
        true: '#fff',
        false: '#fff',
        ping: '#FFFFFF'
      }
      this.$refs.UI.style.setProperty('--fontColorUp', objEum[this.isUp])
    },
    getClass(data) {
      console.log('图片index', data, this.title, this.index)
      if (!data) {
        return `local-location`
      }
      if (data == 'SSD' || data == 'STL') {
        return `local-SSD`
      } else if (data == 'PBG' || data == 'BJZ' || data == 'OFE') {
        return `local-OFE`
      } else if (data in entityPartEnum) {
        return `local-${data}`
      } else {
        return `local-mark`
      }
    }
  }
}
</script>

<style lang="less" scoped>
.content-box {
  --bgColor: rgba(255, 255, 255, 0.1);
  --fontColorUp: #ff321b;
  display: flex;
  flex-direction: row;
  align-items: center;
  position: relative;
  // left: 42px;
  width: 208px;
  justify-content: space-around;
  background-color: var(--bgColor);
  padding: 5px;
  border-radius: 5px;
  div:nth-child(2) {
    color: #fff;
    display: flex;
    flex-direction: row;
    align-items: center;
    font-size: 0.9rem;
    span + span {
      margin-left: 5px;
    }
  }
  div:nth-child(3) {
    color: #fff;
    font-size: 0.7rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    div {
      color: var(--fontColorUp);
    }
    i {
      font-size: 1.2rem;
    }
  }
}
</style>
