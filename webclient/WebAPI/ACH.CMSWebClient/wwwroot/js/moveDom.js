var isShowLog = false
var allLogType = {
    DBLog: '数据库下载',
    waveLog: '波形数据下载'
}
let allLodData = {}
let intervalId
let clickLogType = '' // 展示哪种类型的日志

// 1.1、初始化页签
function getLogAll() {
    let titleDom = document.getElementById("logType")
    for (let key in allLogType) {

        let span = document.createElement("span")
        span.id = key
        span.textContent = allLogType[key] + "日志"
        titleDom.appendChild(span)

        getLogByType(key)

        span.addEventListener('click', function () {
            changeLogType(key)
        })
    }

    setTimeout(() => {
        changeLogType(Object.keys(allLogType)[0])
    },200) 
}


// 每隔一段时间调用获取日志接口
function startGetLogTxt() {
    intervalId = setInterval(() => {
        getLogByType(clickLogType)
        document.getElementById("floatLog").textContent = allLodData[clickLogType]
    },500)
}

// 1.2、调用接口，获取日志信息
function getLogByType(fileName) {
    let txtPath = "api/turbineitems/getTxt/" + fileName + "-" + moment().format("YYYY-MM-DD")
    fetch(txtPath)
        .then((response) => {
            if (response.status === 200) {
                response.text().then(res => {
                    allLodData[fileName] = res
                })
            } else if (response.status === 204) {
                allLodData[fileName] = "暂无日志"
            } else if (response.status === 500) {
                allLodData[fileName] = "接口报错"
            }
        })
        .catch((error) => console.error("Unable to get items.", error));
}

// 关闭每隔一段时间的日志接口调用
function endGetLogTxt() {
    if (intervalId) {
        clearInterval(intervalId)
    }
}


// 1.3、点击页签切换日志类型
function changeLogType(key) {
    // console.log('获取日志allLodData', key, allLodData)
    clickLogType = key
    changePageStyle(key)
    document.getElementById("floatLog").textContent = allLodData[key]
}


// 1.4、对选中的日志进行样式设置
function changePageStyle(key) {
    let clickType = document.getElementById(key)
    clickType.style.fontWeight = 'bold'
    for (let j in allLogType) {
        if (j !== key) {
            document.getElementById(j).style.fontWeight = 'normal'
        }
    }
}


// 二、弹框方法
var dragMinWidth = 700;
var dragMinHeight = 500;
var get = {
    byId: function (id) {
        return typeof id === "string" ? document.getElementById(id) : id
    },
    byClass: function (sClass, oParent) {
        var aClass = [];
        var reClass = new RegExp("(^| )" + sClass + "( |$)");
        var aElem = this.byTagName("*", oParent);
        for (var i = 0; i < aElem.length; i++) reClass.test(aElem[i].className) && aClass.push(aElem[i]);
        return aClass
    },
    byTagName: function (elem, obj) {
        return (obj || document).getElementsByTagName(elem)
    }
};

window.onload = window.onresize = function () {
    var oDrag = document.getElementById("dragGable");
    var oTitle = get.byClass("float_title", oDrag)[0];

    drag(oDrag, oTitle);
}

// 2.1、弹框 - 判断弹框展示
function showLog() {
    isShowLog = !isShowLog

    document.getElementById('selectData').style.display = 'none'

    console.log('弹框是否展示', isShowLog)
    if (!isShowLog) {
        endGetLogTxt()
    }

    let oDrag = document.getElementById("dragGable")
    oDrag.style.display = isShowLog ? "block" : "none"
    oDrag.style.left = (document.documentElement.clientWidth - oDrag.offsetWidth) / 2 + "px";
    oDrag.style.top = (document.documentElement.clientHeight - oDrag.offsetHeight) / 2 + "px";
}

// 2.1、弹框 - 拖动弹框
function drag(oDrag, handle) {
    var disX = dixY = 0;
    var oMin = get.byClass("draw_min", oDrag)[0];
    var oMax = get.byClass("draw_max", oDrag)[0];
    var oRevert = get.byClass("draw_min", oDrag)[0];
    var oClose = get.byClass("draw_close", oDrag)[0];
    handle = handle || oDrag;
    handle.style.cursor = "move";

    handle.onmousedown = function (event) {
        var event = event || window.event;
        disX = event.clientX - oDrag.offsetLeft;
        disY = event.clientY - oDrag.offsetTop;
        document.onmousemove = function (event) {
            var event = event || window.event;
            var iL = event.clientX - disX;
            var iT = event.clientY - disY;
            var maxL = document.documentElement.clientWidth - oDrag.offsetWidth;
            var maxT = document.documentElement.clientHeight - oDrag.offsetHeight;
            iL <= 0 && (iL = 0);
            iT <= 0 && (iT = 0);
            iL >= maxL && (iL = maxL);
            iT >= maxT && (iT = maxT);
            oDrag.style.left = iL + "px";
            oDrag.style.top = iT + "px";
            return false
        };
        document.onmouseup = function () {
            document.onmousemove = null;
            document.onmouseup = null;
            this.releaseCapture && this.releaseCapture()
        };
        this.setCapture && this.setCapture();
        return false
    };
    //最大化
    oMax.onclick = function () {
        oDrag.style.top = oDrag.style.left = 0;
        oDrag.style.width = document.documentElement.clientWidth - 2 + "px";
        oDrag.style.height = document.documentElement.clientHeight - 2 + "px";
    };
    // 最小化
    oMin.onclick = oRevert.onclick = function () {
        oDrag.style.width = dragMinWidth + "px";
        oDrag.style.height = dragMinHeight + "px";
        oDrag.style.left = (document.documentElement.clientWidth - oDrag.offsetWidth) / 2 + "px";
        oDrag.style.top = (document.documentElement.clientHeight - oDrag.offsetHeight) / 2 + "px";
    };
    // 关闭
    oClose.onclick = function () {
        showLog()
    };
    //阻止冒泡
    oMin.onmousedown = oMax.onmousedown = oClose.onmousedown = function (event) {
        this.onfocus = function () { this.blur() };
        (event || window.event).cancelBubble = true
    };
}