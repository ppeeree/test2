<template>
  <div class="healthRanking">
    <span style="margin-right: 15px">健康指数</span>
    <span class="health_ranking_num" :style="{ backgroundImage: setHealthNumImg.backgroundImage }">
      <span style="margin-left: 14px; float: left; margin-top: 11px"
        ><i
          :class="['icon', 'local', `local-${gaugeSeretData.currentComp}`]"
          :style="{ color: compLevel, fontSize: '34px' }"
        ></i
      ></span>
      <span v-if="handleHealthValue" class="num" :style="{ color: setHealthNumImg.fontColor }">{{
        handleHealthValue
      }}</span>
      <span v-else class="num" :style="{ color: setHealthNumImg.fontColor }">--</span>
    </span>
    <span class="health_ranking_index">
      环比：
      <span :style="setNumStyle"> {{ handleHealthNum }}</span>
    </span>
  </div>
</template>

<script>
export default {
  props: {
    gaugeSeretData: {
      type: Object,
      require: true,
      default: () => {
        return {
          healthValue: 0,
          indexNum: 2
        }
      }
    }
  },
  data() {
    return {
      healthValue: 86,
      indexNum: 2,
      windHealthBackImg: {
        danger: '/img/healthBackground/4.png',
        warning: '/img/healthBackground/3.png',
        attention: '/img/healthBackground/2.png',
        normal: '/img/healthBackground/1.png',
        nodata: '/img/healthBackground/0.png'
      }
    }
  },
  computed: {
    setHealthNumImg() {
      if (JSON.stringify(this.gaugeSeretData) === '{}') {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['nodata'] + ')',
          fontColor: '#8D8D8D'
        }
      }
      let level = this.gaugeSeretData.healthValue || '--'
      if (level == 100.0) {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['normal'] + ')',
          fontColor: '#36FA3C'
        }
      } else if (level > 60.0) {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['attention'] + ')',
          fontColor: '#FFE604'
        }
      } else if (level > 30.0) {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['warning'] + ')',
          fontColor: '#FF6B0E'
        }
      } else if (level > 0.0) {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['danger'] + ')',
          fontColor: '#FF0F0D'
        }
      } else {
        return {
          backgroundImage: 'url(' + this.windHealthBackImg['nodata'] + ')',
          fontColor: '#8D8D8D'
        }
      }
    },
    handleHealthNum() {
      return this.gaugeSeretData.indexNum === ''
        ? '- -'
        : this.gaugeSeretData.indexNum > 0
        ? `+${this.gaugeSeretData.indexNum}`
        : this.gaugeSeretData.indexNum
    },
    setNumStyle() {
      return {
        color: this.gaugeSeretData.indexNum === '' ? '#909399' : '#fff',
        marginLeft: '0px',
        fontSize: '24px'
      }
    },
    handleHealthValue() {
      return this.gaugeSeretData.healthValue === '' ? '' : this.gaugeSeretData.healthValue
    },
    compLevel() {
      let level = this.gaugeSeretData.healthValue || '--',
        compColor
      if (level == 100.0) {
        compColor = '#2ED133'
      } else if (level > 60.0) {
        compColor = '#FFE604'
      } else if (level > 30.0) {
        compColor = '#F37628'
      } else if (level > 0.0) {
        compColor = '#FF0F0D'
      } else {
        compColor = '#8D8D8D'
      }
      return compColor
    }
    /*  setCompImg() {
      console.log(this.gaugeSeretData)
      console.log(this.compLevel)
      return `/img/healthBackground/${
        (this.gaugeSeretData?.currentComp || 'wind') + this.compLevel
      }.png`
    } */
  }
}
</script>

<style lang="less" scoped>
@font-face {
  font-family: 'DS-DIGIB-2';
  src: url('../../WindTurbine/font/DS-DIGIB-2.ttf') format('truetype');
}
.healthRanking {
  color: white;
  font-size: 18px;
  display: flex;
  flex-direction: row;
  align-items: center;
  .health_ranking_num {
    display: inline-block;
    width: 173px;
    height: 63px;
    text-align: center;
    z-index: 1;
    .num {
      font-family: 'DS-DIGIB-2';
      font-size: 56px;
      font-weight: bold;
      line-height: 60px;
      z-index: 10;
      margin-left: 3px;
      text-shadow: 0px 0px 50px 20px;
    }
    img {
      height: 45px;
    }
  }
  .health_ranking_index {
    display: inline-block;
    color: #abe1ff;
    margin-left: 15px;
    color: #abe1ff;
    i {
      font-size: 18px;
    }
  }
}
</style>
