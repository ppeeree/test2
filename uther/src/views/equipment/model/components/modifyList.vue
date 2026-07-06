<template>
  <el-container style="height: 745px">
    <el-form :model="addFrom" label-width="80px" :rules="rules" ref="ruleFrom">
      <el-form-item label="类型：" prop="type">
        <el-cascader
          ref="typeCascader"
          v-model="addFrom.type"
          clearable
          placeholder="请选择类型"
          :disabled="!isAdd"
          :options="typeList"
          :props="typeProps"
          @change="selectType"
        ></el-cascader>
      </el-form-item>
      <el-form-item label="制造商：" prop="maker">
        <el-select
          v-model="addFrom.maker"
          placeholder="请选择/输入制造商"
          filterable
          allow-create
          clearable
          default-first-option
          :disabled="!isAdd"
          @change="selectMaker"
        >
          <el-option
            v-for="item in addFirmList"
            :key="item.factory"
            :label="item.factory"
            :value="item.factory"
          >
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="型号：" prop="model">
        <el-select
          v-model="addFrom.model"
          placeholder="请选择/输入型号"
          filterable
          allow-create
          clearable
          default-first-option
          :disabled="!isAdd"
        >
          <el-option
            v-for="item in addTypeList"
            :key="item.modelName"
            :label="item.modelName"
            :value="item.modelName"
          >
          </el-option>
        </el-select>
      </el-form-item>
      <div class="space_line"></div>
      <div class="required_param">
        <span>必填参数</span>
        <el-form-item prop="requiredParam" v-for="(item, index) in requiredData" :key="index">
          <template slot="label"
            ><el-input
              style="width: 88%"
              v-model="requiredData[index].name"
              placeholder="参数名称"
              :disabled="true"
            ></el-input
          ></template>

          &nbsp; & &nbsp;
          <el-input v-model="requiredData[index].value" placeholder="参数数值"></el-input>
        </el-form-item>
      </div>

      <div class="space_line"></div>
      <div class="custom_param">
        <span>自定义参数</span>
        <div v-for="(item, index) in paramList" :key="index">
          <el-input v-model="paramList[index].name" placeholder="参数名称"></el-input>
          &nbsp; & &nbsp;
          <el-input v-model="paramList[index].value" placeholder="参数数值"></el-input>
        </div>
        <div class="add_parameter_btn" @click="addComp"><i class="el-icon-plus"></i> 添加参数</div>
      </div>
    </el-form>
    <el-footer
      ><el-button type="primary" @click="handlerSave" style="margin-left: 40%">
        <i class="el-icon-circle-check"></i> 保存</el-button
      ></el-footer
    >
  </el-container>
</template>

<script>
import { getCompApi, getDeviceParamApi, getFactoryModelApi } from '@/api/equipment/component'
import { getPagecomp } from '@/api/equipment/model'
import { findLeafNode } from '../../component/windList'

export default {
  props: {
    isAdd: {
      type: Boolean,
      default: true
    }
  },
  data() {
    return {
      typeList: [],
      addFirmList: [], // 厂商下拉列表
      addTypeList: [], // 类型下拉列表
      entityTypeCode: '', // 选中的类型code
      typeProps: {
        checkStrictly: true,
        label: 'entityName',
        value: 'entityType',
        children: 'child'
      },
      addFrom: {
        type: '',
        maker: '',
        model: ''
      },
      requiredData: [
        {
          name: '',
          key: '',
          value: ''
        }
      ],
      paramList: [
        {
          name: '',
          key: '',
          value: ''
        }
      ],
      rules: {
        maker: [{ required: true, message: '请输入制造商', trigger: 'blur' }],
        model: [{ required: true, message: '请输入型号', trigger: 'blur' }],
        type: [{ required: true, message: '请选择类型', trigger: 'change' }]
        // requiredParam: [{ required: true, message: '请输入参数', trigger: 'change' }]
      }
    }
  },
  watch: {
    addFrom: {
      handler(val) {
        if (this.isAdd) {
          if (val.type !== '' && val.maker !== '' && val.model !== '' && this.isAdd) {
            this.getRequiredList(val)
          } else {
            this.requiredData = [
              {
                name: '',
                key: '',
                value: ''
              }
            ]
          }
        }
      },
      deep: true
    }
  },
  mounted() {
    this.getTypeList()
  },
  methods: {
    addComp() {
      let lastInput = this.paramList[this.paramList.length - 1]
      if (lastInput.name == '' || lastInput.value == '') {
        this.$message({
          type: 'warning',
          message: '请完成上个参数！'
        })
      } else {
        this.paramList.push({
          name: '',
          key: '',
          value: ''
        })
      }
    },

    getTypeList() {
      getPagecomp({ takeWind: true }).then(res => {
        if (res.data.code == 200) {
          this.typeList = res.data.data
          // this.handlerTypeList(this.typeList)

          console.log('处理模型类型', this.typeList, res.data.data)
        }
      })
    },
    handlerTypeList(arr) {
      arr.forEach((item, index) => {
        if (item.children[0] && item.children[0].key == 'comp') {
          this.handlerTypeList(item.children)
        } else {
          delete arr[index].children
        }
      })
    },

    // 选中类型改变
    selectType(val) {
      if (val.length) {
        // this.entityTypeCode = this.$refs['typeCascader'].getCheckedNodes()[0].data.entityType
        // 根据选中的趋势
        let obj = findLeafNode(val[val.length - 1], this.typeList, 'entityType')

        //  let obj= this.typeList.find(j => j.entityName == obj.deviceModelType).

        console.log('选中类型', val, this.typeList, obj)
        this.entityTypeCode = obj.entityType

        getFactoryModelApi({ entityTypeCode: this.entityTypeCode }).then(res => {
          if (res.data.code === 200) {
            this.addFirmList = res.data.data
          }
        })
      } else {
        this.addFirmList = []
      }
    },
    // 选中制造商改变
    selectMaker(val) {
      this.addFrom.model = ''
      this.addTypeList = []
      if (val !== '') {
        let obj = this.addFirmList.find(j => j.factory == val)
        let data = obj ? obj.modelList : []

        data.forEach(item => {
          this.addTypeList.push({
            modelName: item
          })
        })
      } else {
        this.addTypeList = []
      }
    },
    //确定必选参数
    getRequiredList(val) {
      getDeviceParamApi({
        entityTypeCode: this.entityTypeCode,
        factory: val.maker,
        model: val.model
      }).then(res => {
        if (res.data.code === 200) {
          if (res.data.data.length) {
            this.requiredData = res.data.data
          } else {
            this.requiredData = [
              {
                name: '',
                key: '',
                value: ''
              }
            ]
          }
        }
      })
    },

    initData(obj) {
      this.addFrom = {
        maker: obj.manufacturer,
        model: obj.deviceModel
      }

      // 根据设备类型code获取路径
      // this.addFrom.type = this.getParentsById(this.typeList, obj.deviceModelType)

      this.addFrom.type = [this.typeList.find(j => j.entityName == obj.deviceModelType).entityType]

      console.log('初始化时确定选中设备类型', this.typeList, obj.deviceModelType)
      this.selectType(this.addFrom.type)

      this.paramList = obj.propertiesBasic

      if (obj.defaultpropertiesBasic.length) {
        this.requiredData = obj.defaultpropertiesBasic
      } else {
        this.requiredData = [
          {
            name: '',
            key: '',
            value: ''
          }
        ]
      }
    },

    /*
     * el-cascader递归获取父级id
     * @param  list 数据列表
     * @param  id 后端返回的id
     * typeProps 是el-cascader props属性
     **/
    getParentsById(list, id) {
      for (let i in list) {
        if (list[i][this.typeProps.value || 'value'] == id) {
          return [list[i][this.typeProps.value || 'value']]
        }
        if (list[i].children) {
          let node = this.getParentsById(list[i].children, id)
          if (node !== undefined) {
            node.unshift(list[i][this.typeProps.value || 'value'])
            return node
          }
        }
      }
    },

    clearData() {
      this.addFrom = {
        type: '',
        maker: '',
        model: ''
      }
      this.paramList = [
        {
          name: '',
          key: '',
          value: ''
        }
      ]
      this.requiredData = [
        {
          name: '',
          key: '',
          value: ''
        }
      ]
    },

    handlerSave() {
      this.$refs['ruleFrom'].validate(valid => {
        if (valid) {
          this.$emit('handlerListSave')
        }
      })
    }
  }
}
</script>

<style lang="less" scoped>
.el-form {
  height: 100%;
  max-height: 685px;
  overflow-y: auto;
  overflow-x: hidden;
}
.el-cascader {
  width: 100%;
}
.el-select {
  width: 100%;
}

.el-button--primary {
  margin-top: 10px;
}
.space_line {
  width: 100%;
  height: 1px;
  background: #283754;
}
.required_param {
  padding-top: 15px;
  margin-left: 10px;
  max-height: 240px;
  overflow: auto;
  span {
    font-size: 14px;
    line-height: 30px;
  }
  .el-input {
    width: 55%;
  }
  :deep(.el-form-item__label){
    width: 189px !important;
  }
}
.custom_param {
  padding-top: 15px;
  margin-left: 10px;
  max-height: 240px;
  overflow: auto;
  span {
    font-size: 14px;
    line-height: 30px;
  }
  .el-input {
    width: 45%;
    margin: 10px 0;
  }
  .add_parameter_btn {
    height: 40px;
    line-height: 40px;
    text-align: center;
    background: rgba(71, 86, 128, 0.5);
    border-radius: 4px;
    font-size: 12px;
    cursor: pointer;
    margin-top: 20px;
  }
}

:deep(.el-input.is-disabled .el-input__inner){
  border-color: rgba(71, 86, 128, 1);
}
:deep(.el-select .el-input.is-disabled .el-input__inner:hover){
  border-color: rgba(71, 86, 128, 1);
}
</style>
